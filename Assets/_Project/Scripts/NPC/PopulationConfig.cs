using UnityEngine;

namespace CrimeLife.NPC
{
    [CreateAssetMenu(fileName = "PopulationConfig", menuName = "CrimeLife/Population Config")]
    public class PopulationConfig : ScriptableObject
    {
        [Header("랜덤 NPC")]
        [SerializeField, Min(0)] private int _randomCount = 20;
        [Tooltip("게임 시간으로 이만큼마다 일부가 떠나고 새 사람이 온다")]
        [SerializeField, Min(0.5f)] private float _rotateIntervalHours = 3f;
        [SerializeField, Min(0)] private int _rotateCount = 2;
        [Tooltip("플레이어와 이보다 가까운 NPC는 눈앞에서 사라지지 않게 내보내지 않는다")]
        [SerializeField] private float _minDespawnDistance = 14f;
        [Tooltip("새로 나타나는 NPC는 플레이어와 이보다 멀리 떨어진 곳에서 시작한다")]
        [SerializeField] private float _minSpawnDistance = 12f;

        [Header("아는 사람")]
        [Tooltip("도시에 처음 들어올 때, 아는 사람 한 명 한 명이 이미 와 있을 확률")]
        [SerializeField, Range(0f, 1f)] private float _knownInitialChance = 0.35f;
        [Tooltip("순환할 때마다, 도시에 없는 아는 사람 한 명 한 명이 새로 나타날 확률")]
        [SerializeField, Range(0f, 1f)] private float _knownAppearChance = 0.06f;
        [Tooltip("부름을 받고 플레이어에게 오는 시간(현실 초)")]
        [SerializeField, Min(10f)] private float _summonSeconds = 90f;

        public int RandomCount => _randomCount;
        public float RotateIntervalHours => _rotateIntervalHours;
        public int RotateCount => _rotateCount;
        public float MinDespawnDistance => _minDespawnDistance;
        public float MinSpawnDistance => _minSpawnDistance;
        public float KnownInitialChance => _knownInitialChance;
        public float KnownAppearChance => _knownAppearChance;
        public float SummonSeconds => _summonSeconds;
    }
}
