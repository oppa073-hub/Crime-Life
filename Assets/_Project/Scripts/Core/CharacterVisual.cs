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

        private const float Height = 1.8f;
        private const float Radius = 0.35f;

        private void Awake()
        {
            if (_modelPrefab != null) BuildModel();
            else BuildPlaceholder();

            // NPC/플레이어를 클릭할 때 레이캐스트가 맞는 충돌체
            var collider = gameObject.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, Height * 0.5f, 0f);
            collider.height = Height;
            collider.radius = Radius;
        }

        private void BuildModel()
        {
            GameObject model = Instantiate(_modelPrefab, transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = _controller;
            animator.applyRootMotion = false;

            model.AddComponent<CharacterAnimator>();
        }

        private void BuildPlaceholder()
        {
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Placeholder";
            capsule.transform.SetParent(transform, false);
            capsule.transform.localPosition = new Vector3(0f, 1f, 0f);
            // 충돌체는 이 컴포넌트가 따로 달기 때문에 기본 것은 없앤다
            Destroy(capsule.GetComponent<Collider>());
            capsule.GetComponent<Renderer>().material.color = _placeholderColor;
        }
    }
}
