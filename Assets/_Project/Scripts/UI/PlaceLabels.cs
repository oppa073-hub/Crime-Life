using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    // 건물/장소 이름표. 에셋을 입히기 전 블록아웃 단계에서 어느 박스가 무엇인지 알아보게 한다.
    [RequireComponent(typeof(UIDocument))]
    public class PlaceLabels : MonoBehaviour
    {
        [Serializable]
        public struct Entry
        {
            public Transform Anchor;
            public string Text;
        }

        [SerializeField] private Camera _camera;
        [SerializeField] private Entry[] _entries;

        private Label[] _labels;
        private VisualElement _root;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            if (_camera == null) _camera = Camera.main;

            var color = new Color(1f, 0.9f, 0.5f);
            _labels = new Label[_entries.Length];
            for (int i = 0; i < _entries.Length; i++)
                _labels[i] = WorldLabelUtil.Create(_root, _entries[i].Text, color, 20);
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _labels.Length; i++)
                WorldLabelUtil.Place(_root, _labels[i], _camera, _entries[i].Anchor.position);
        }
    }
}
