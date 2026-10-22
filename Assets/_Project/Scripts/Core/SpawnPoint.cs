using UnityEngine;

namespace CrimeLife.Core
{
    // 다른 씬에서 문을 통해 들어왔을 때 플레이어가 서는 자리
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _id;

        public string Id => _id;
    }
}
