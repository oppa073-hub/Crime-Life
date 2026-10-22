using UnityEngine;
using UnityEngine.AI;

namespace CrimeLife.Core
{
    // 씬이 시작될 때, 문으로 들어왔다면 해당 SpawnPoint로 플레이어를 옮긴다.
    public class SceneSpawner : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent _player;
        [SerializeField] private SpawnPoint[] _points;

        private void Start()
        {
            string id = GameSession.Instance.ConsumeSpawnId();
            if (string.IsNullOrEmpty(id)) return;

            foreach (SpawnPoint point in _points)
            {
                if (point.Id != id) continue;

                // Warp는 에이전트가 NavMesh 위에 있어야 해서 가장 가까운 바닥으로 보정한다
                if (NavMesh.SamplePosition(point.transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    _player.Warp(hit.position);
                return;
            }
            Debug.LogWarning($"SpawnPoint '{id}'를 찾을 수 없다.", this);
        }
    }
}
