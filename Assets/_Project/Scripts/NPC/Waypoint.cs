using UnityEngine;

namespace CrimeLife.NPC
{
    // 일과표의 장소 이름과 씬 위치를 이어 주는 표식.
    // ScriptableObject는 씬 오브젝트를 직접 참조할 수 없어서 문자열 Id로 연결한다.
    public class Waypoint : MonoBehaviour
    {
        [Tooltip("일과표가 가리키는 이름. 같은 이름의 장소가 여러 개면 NPC마다 하나씩 정해서 간다")]
        [SerializeField] private string _id;
        [Tooltip("장소의 종류(예: Residence). 랜덤 NPC의 집을 고를 때 쓴다. 비우면 Id를 쓴다")]
        [SerializeField] private string _role;

        public string Id => _id;
        public string Role => string.IsNullOrEmpty(_role) ? _id : _role;
    }
}
