using System.Collections.Generic;
using CrimeLife.NPC;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    // NPC 머리 위에 이름을 띄운다. NPC가 계속 나타나고 사라지므로 PopulationManager의 알림에 맞춰 라벨을 만들고 지운다.
    [RequireComponent(typeof(UIDocument))]
    public class NpcNameTags : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private PopulationManager _population;
        [SerializeField] private float _heightOffset = 2.4f;

        private readonly Dictionary<NpcAgent, Label> _labels = new Dictionary<NpcAgent, Label>();
        private readonly List<NpcAgent> _removeBuffer = new List<NpcAgent>();
        private VisualElement _root;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            if (_camera == null) _camera = Camera.main;

            foreach (NpcAgent npc in _population.Active) Add(npc);
            _population.Spawned += Add;
            _population.Despawned += Remove;
        }

        private void OnDestroy()
        {
            if (_population == null) return;
            _population.Spawned -= Add;
            _population.Despawned -= Remove;
        }

        private void Add(NpcAgent npc)
        {
            if (npc == null || _labels.ContainsKey(npc)) return;
            // 주요 NPC는 흰색, 랜덤 NPC는 약간 어둡게 해서 구분한다
            _labels[npc] = WorldLabelUtil.Create(_root, npc.DisplayName, npc.IsMain ? Color.white : new Color(0.85f, 0.85f, 0.85f), 18);
        }

        private void Remove(NpcAgent npc)
        {
            if (npc == null || !_labels.TryGetValue(npc, out Label label)) return;
            label.RemoveFromHierarchy();
            _labels.Remove(npc);
        }

        private void LateUpdate()
        {
            _removeBuffer.Clear();
            foreach (KeyValuePair<NpcAgent, Label> pair in _labels)
            {
                if (pair.Key == null)
                {
                    _removeBuffer.Add(pair.Key);
                    continue;
                }

                Vector3 world = pair.Key.transform.position + Vector3.up * _heightOffset;
                WorldLabelUtil.Place(_root, pair.Value, _camera, world);
            }

            // 알림 없이 사라진 NPC의 라벨이 남지 않게 정리한다
            foreach (NpcAgent gone in _removeBuffer)
            {
                _labels[gone].RemoveFromHierarchy();
                _labels.Remove(gone);
            }
        }
    }
}
