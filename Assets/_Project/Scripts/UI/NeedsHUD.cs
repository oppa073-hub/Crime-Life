using CrimeLife.Needs;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    // 값이 바뀔 때만 갱신한다. 매 프레임 폴링하지 않으려고 컨트롤러 이벤트를 구독한다.
    [RequireComponent(typeof(UIDocument))]
    public class NeedsHUD : MonoBehaviour
    {
        [SerializeField] private NeedsController _needs;

        private ProgressBar[] _bars;

        // OnEnable에서 하면 NeedsController.Awake(모델 생성)보다 먼저 돌 수 있어 Start에서 초기화한다.
        private void Start()
        {
            if (_needs == null)
            {
                Debug.LogError("NeedsController가 연결되지 않았다.", this);
                return;
            }

            var root = GetComponent<UIDocument>().rootVisualElement;
            _bars = new[]
            {
                root.Q<ProgressBar>("satiety"),
                root.Q<ProgressBar>("energy"),
                root.Q<ProgressBar>("calm"),
            };

            for (int i = 0; i < _bars.Length; i++)
                Refresh((NeedType)i, _needs.Get((NeedType)i));

            _needs.ValueChanged += Refresh;
        }

        private void OnEnable()
        {
            // 비활성 → 활성 재진입 시에만 해당(첫 구독은 Start에서 한다)
            if (_bars != null) _needs.ValueChanged += Refresh;
        }

        private void OnDisable()
        {
            if (_needs != null) _needs.ValueChanged -= Refresh;
        }

        private void Refresh(NeedType type, float value)
        {
            var bar = _bars[(int)type];
            bar.value = value;
            bar.EnableInClassList("critical", _needs.Model.IsCritical(type));
        }
    }
}
