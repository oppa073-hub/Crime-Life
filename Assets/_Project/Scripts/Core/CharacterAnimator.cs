using UnityEngine;
using UnityEngine.AI;

namespace CrimeLife.Core
{
    // NavMeshAgent의 이동 속도를 애니메이터의 Speed 값으로 넘겨 대기/걷기/달리기를 블렌딩한다.
    // 플레이어, NPC, 경찰이 같은 컨트롤러를 쓰므로 캐릭터 모델은 이 컴포넌트가 붙은 프리팹만 갈아 끼우면 된다.
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        [Tooltip("값이 바뀌는 속도. 클수록 대기↔걷기 전환이 빠르다")]
        [SerializeField] private float _damp = 0.1f;

        private Animator _animator;
        private NavMeshAgent _agent;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _agent = GetComponentInParent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_agent == null) return;
            _animator.SetFloat(SpeedHash, _agent.velocity.magnitude, _damp, Time.deltaTime);
        }
    }
}
