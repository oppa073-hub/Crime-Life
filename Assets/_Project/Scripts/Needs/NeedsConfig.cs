using UnityEngine;

namespace CrimeLife.Needs
{
    [CreateAssetMenu(fileName = "NeedsConfig", menuName = "CrimeLife/Needs Config")]
    public class NeedsConfig : ScriptableObject
    {
        [Header("시간")]
        [Tooltip("실시간 1초당 흐르는 게임 시간(시간 단위). 1이면 현실 1초 = 게임 1시간")]
        [SerializeField] private float _gameHoursPerRealSecond = 0.05f;

        [Header("욕구")]
        [SerializeField] private NeedSettings _satiety = new NeedSettings(80f, 4f, 25f);
        [SerializeField] private NeedSettings _energy = new NeedSettings(80f, 3f, 20f);
        [SerializeField] private NeedSettings _calm = new NeedSettings(80f, 1.5f, 25f);

        [Header("범죄 연동")]
        [Tooltip("욕구 하나가 바닥(0)일 때 범죄 성공률 배율에서 깎이는 양")]
        [SerializeField, Range(0f, 1f)] private float _maxCrimePenaltyPerNeed = 0.2f;

        public float GameHoursPerRealSecond => _gameHoursPerRealSecond;

        public NeedsModel CreateModel()
        {
            var settings = new[] { _satiety, _energy, _calm };
            return new NeedsModel(settings, _maxCrimePenaltyPerNeed);
        }
    }
}
