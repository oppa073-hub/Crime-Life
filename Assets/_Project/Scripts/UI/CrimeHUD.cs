using CrimeLife.Core;
using CrimeLife.Crime;
using CrimeLife.Interaction;
using CrimeLife.NPC;
using CrimeLife.Social;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class CrimeHUD : MonoBehaviour
    {
        [SerializeField] private Wallet _wallet;
        [SerializeField] private HeatTracker _heat;
        [SerializeField] private PlayerCrimeController _crime;
        [SerializeField] private PoliceOfficer _police;
        [SerializeField] private PlayerInteractor _interactor;
        [Tooltip("관계 관련은 NPC가 없는 씬(집)에서는 비워 둔다")]
        [SerializeField] private PlayerSocialController _social;
        [SerializeField] private RelationshipBook _book;
        [SerializeField] private RelationshipConfig _relationshipConfig;

        private Label _money;
        private Label _heatLabel;
        private Label _message;
        private Label _relation;

        // 다른 컴포넌트의 Awake 이후에 초기값을 읽도록 Start에서 초기화한다
        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _money = root.Q<Label>("money");
            _heatLabel = root.Q<Label>("heat");
            _message = root.Q<Label>("message");
            _relation = root.Q<Label>("relation");

            SetMoney(_wallet.Cash, _wallet.Loot);
            SetHeat(Mathf.CeilToInt(_heat.Heat));

            _wallet.Changed += SetMoney;
            _heat.HeatChanged += SetHeat;
            _crime.CrimeResolved += ShowResult;
            // 집 씬에는 경찰이 없다
            if (_police != null)
            {
                _police.ChasingChanged += ShowChase;
                _police.PlayerArrested += ShowArrest;
            }
            GameSession.Instance.Notice += ShowMessage;
            _interactor.Rejected += ShowMessage;
            _interactor.LootSold += ShowSold;
            _crime.Rejected += ShowMessage;
            _crime.WitnessReported += ShowWitness;

            // 관계 쪽은 NPC가 있는 씬(도시)에만 있다
            if (_social != null)
            {
                _social.Rejected += ShowMessage;
                _social.Performed += ShowSocial;
            }
            if (_book != null) _book.Changed += ShowRelation;
        }

        private void OnDestroy()
        {
            if (_social != null)
            {
                _social.Rejected -= ShowMessage;
                _social.Performed -= ShowSocial;
            }
            if (_book != null) _book.Changed -= ShowRelation;
            if (GameSession.Current != null) GameSession.Current.Notice -= ShowMessage;
            if (_wallet != null) _wallet.Changed -= SetMoney;
            if (_heat != null) _heat.HeatChanged -= SetHeat;
            if (_crime != null)
            {
                _crime.CrimeResolved -= ShowResult;
                _crime.Rejected -= ShowMessage;
                _crime.WitnessReported -= ShowWitness;
            }
            if (_police != null)
            {
                _police.ChasingChanged -= ShowChase;
                _police.PlayerArrested -= ShowArrest;
            }
            if (_interactor != null)
            {
                _interactor.Rejected -= ShowMessage;
                _interactor.LootSold -= ShowSold;
            }
        }

        private void SetMoney(int cash, int loot) => _money.text = $"현금 {cash}   장물 {loot}";

        private void SetHeat(int heat) => _heatLabel.text = $"수배 열기 {heat}";

        private void ShowMessage(string text) => _message.text = text;

        private void ShowSocial(SocialKind kind, NpcAgent npc) =>
            _message.text = kind == SocialKind.Gift ? $"선물: {npc.DisplayName}" : $"대화: {npc.DisplayName}";

        private void ShowRelation(NpcAgent npc, float value, float delta)
        {
            RelationshipTier tier = RelationshipLogic.GetTier(value, _relationshipConfig.Settings);
            string sign = delta >= 0f ? "+" : "";
            _relation.text = $"{npc.DisplayName} 친밀도 {Mathf.RoundToInt(value)} ({sign}{Mathf.RoundToInt(delta)}, {RelationshipText.TierName(tier)})";
        }

        private void ShowWitness(NpcAgent witness) => _message.text = $"목격자 {witness.DisplayName}: 신고했다!";

        private void ShowSold(int cash) => _message.text = $"장물 처분: 현금 {cash} 획득";

        private void ShowChase(bool chasing)
        {
            if (chasing) _message.text = "경찰이 추격 중! 도망쳐!";
        }

        private void ShowArrest(int fine, int seized) =>
            _message.text = $"체포됨! 벌금 {fine}, 장물 {seized} 압수";

        private void ShowResult(ICrimeAction action, CrimeOutcome outcome, NpcAgent target)
        {
            _message.text = outcome.Success
                ? $"{action.DisplayName} 성공: {target.DisplayName}에게서 장물 {outcome.Loot} 획득"
                : $"{action.DisplayName} 실패: {target.DisplayName}";
        }
    }
}
