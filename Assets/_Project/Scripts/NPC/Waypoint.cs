using UnityEngine;

namespace CrimeLife.NPC
{
    // 일과표의 장소 이름과 씬 위치를 이어 주는 표식.
    // ScriptableObject는 씬 오브젝트를 직접 참조할 수 없어서 문자열 Id로 연결한다.
    public class Waypoint : MonoBehaviour
    {
        [SerializeField] private string _id;

        public string Id => _id;
    }
}
