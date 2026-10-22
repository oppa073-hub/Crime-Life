using System.Collections.Generic;
using UnityEngine;

namespace CrimeLife.NPC
{
    // 씬의 장소(Waypoint)를 Id/역할로 찾아 주는 목록. NPC가 늘고 줄어도 장소는 한 번만 모은다.
    public class WaypointDirectory : MonoBehaviour
    {
        private readonly Dictionary<string, List<Waypoint>> _byId = new Dictionary<string, List<Waypoint>>();
        private readonly Dictionary<string, List<string>> _idsByRole = new Dictionary<string, List<string>>();

        private void Awake()
        {
            foreach (Waypoint waypoint in FindObjectsByType<Waypoint>(FindObjectsSortMode.None))
            {
                if (!_byId.TryGetValue(waypoint.Id, out List<Waypoint> list))
                {
                    list = new List<Waypoint>();
                    _byId[waypoint.Id] = list;

                    if (!_idsByRole.TryGetValue(waypoint.Role, out List<string> ids))
                    {
                        ids = new List<string>();
                        _idsByRole[waypoint.Role] = ids;
                    }
                    ids.Add(waypoint.Id);
                }
                list.Add(waypoint);
            }
        }

        // 같은 Id의 장소가 여러 개면 seed로 하나를 고정해서 고른다 (같은 NPC는 항상 같은 장소로 간다)
        public bool TryGet(string id, int seed, out Waypoint waypoint)
        {
            waypoint = null;
            if (string.IsNullOrEmpty(id) || !_byId.TryGetValue(id, out List<Waypoint> list) || list.Count == 0) return false;

            waypoint = list[((seed % list.Count) + list.Count) % list.Count];
            return true;
        }

        // 해당 역할을 가진 장소 Id들 (랜덤 NPC의 집 후보)
        public List<string> GetIdsByRole(string role) =>
            _idsByRole.TryGetValue(role, out List<string> ids) ? ids : new List<string>();
    }
}
