using CrimeLife.NPC;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    // NPC 머리 위에 이름을 띄운다.
    [RequireComponent(typeof(UIDocument))]
    public class NpcNameTags : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private NpcAgent[] _npcs;
        [SerializeField] private float _heightOffset = 2.4f;

        private Label[] _labels;
        private VisualElement _root;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            if (_camera == null) _camera = Camera.main;

            _labels = new Label[_npcs.Length];
            for (int i = 0; i < _npcs.Length; i++)
                _labels[i] = WorldLabelUtil.Create(_root, _npcs[i].DisplayName, Color.white, 18);
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _labels.Length; i++)
            {
                Vector3 world = _npcs[i].transform.position + Vector3.up * _heightOffset;
                WorldLabelUtil.Place(_root, _labels[i], _camera, world);
            }
        }
    }
}
