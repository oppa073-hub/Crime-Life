using UnityEngine;

namespace CrimeLife.Core
{
    // 캐릭터의 겉모습 슬롯. 모델 프리팹이 연결돼 있으면 그걸 쓰고, 없으면 색 캡슐로 대체한다.
    // 외부 에셋(_External)은 저장소에 올라가지 않아서, 에셋이 없는 환경에서도 게임이 똑같이 돌아가게 하려는 구조다.
    // 모델/충돌체는 런타임에 만든다: 편집 시점에 NavMesh 베이크에 섞여 들어가지 않게 하려는 목적도 있다.
    public class CharacterVisual : MonoBehaviour
    {
        [Tooltip("비어 있거나 외부 에셋이 없으면 캡슐로 대체된다")]
        [SerializeField] private GameObject _modelPrefab;
        [SerializeField] private RuntimeAnimatorController _controller;
        [Tooltip("모델이 없을 때 캡슐 색 (플레이어/경찰/NPC 구분용)")]
        [SerializeField] private Color _placeholderColor = new Color(0.6f, 0.6f, 0.6f);
        [Tooltip("모델의 앞면이 루트의 앞(+Z)과 어긋난 만큼 보정하는 각도. 이 에셋 팩의 캐릭터는 옆(+X)을 보고 만들어져 있어 -90이 필요하다")]
        [SerializeField] private float _modelYaw = -90f;
        [Tooltip("켜면 Awake에서 만들지 않고, 나중에 Build()로 외형을 정한다 (런타임에 생성되는 NPC용)")]
        [SerializeField] private bool _deferBuild;

        [Header("이동 느낌")]
        [Tooltip("NavMeshAgent 기본값(120)은 너무 느려서, 이미 걷기 시작한 뒤에야 방향을 돌려 엉뚱한 쪽을 보며 걷는다")]
        [SerializeField] private float _turnSpeed = 720f;
        [Tooltip("기본값(8)이면 방향을 다 돌리기 전에 최고 속도가 된다")]
        [SerializeField] private float _acceleration = 20f;

        private const float Height = 1.8f;
        private const float Radius = 0.35f;

        private GameObject _current;

        private void Awake()
        {
            // 모델이 있든 캡슐이든 같은 이동 느낌이 되도록 에이전트 설정을 여기서 맞춘다
            var agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.angularSpeed = _turnSpeed;
                agent.acceleration = _acceleration;
            }

            if (!_deferBuild) Build(_modelPrefab, _placeholderColor);

            // NPC/플레이어를 클릭할 때 레이캐스트가 맞는 충돌체
            var collider = gameObject.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, Height * 0.5f, 0f);
            collider.height = Height;
            collider.radius = Radius;
        }

        // 외형을 (다시) 만든다. 모델 프리팹이 null이면 색 캡슐로 대체한다.
        public void Build(GameObject modelPrefab, Color placeholderColor)
        {
            if (_current != null) Destroy(_current);

            _current = modelPrefab != null ? BuildModel(modelPrefab) : BuildPlaceholder(placeholderColor);
        }

        private GameObject BuildModel(GameObject prefab)
        {
            GameObject model = Instantiate(prefab, transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, _modelYaw, 0f);

            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = _controller;
            animator.applyRootMotion = false;

            model.AddComponent<CharacterAnimator>();
            return model;
        }

        private GameObject BuildPlaceholder(Color color)
        {
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Placeholder";
            capsule.transform.SetParent(transform, false);
            capsule.transform.localPosition = new Vector3(0f, 1f, 0f);
            // 충돌체는 이 컴포넌트가 따로 달기 때문에 기본 것은 없앤다
            Destroy(capsule.GetComponent<Collider>());
            capsule.GetComponent<Renderer>().material.color = color;
            return capsule;
        }
    }
}
