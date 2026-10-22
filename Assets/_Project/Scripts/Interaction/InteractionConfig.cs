using System;
using CrimeLife.Needs;
using UnityEngine;

namespace CrimeLife.Interaction
{
    [Serializable]
    public struct NeedEffect
    {
        public NeedType Type;
        [Tooltip("상호작용이 끝났을 때 누적되는 총 변화량 (음수면 감소)")]
        public float TotalAmount;
    }

    [CreateAssetMenu(fileName = "InteractionConfig", menuName = "CrimeLife/Interaction Config")]
    public class InteractionConfig : ScriptableObject
    {
        [SerializeField] private string _displayName = "Interaction";
        [Tooltip("실시간 초 단위")]
        [SerializeField, Min(0.1f)] private float _duration = 5f;
        [SerializeField] private NeedEffect[] _effects;

        [Header("비용/정산")]
        [Tooltip("시작할 때 내는 현금")]
        [SerializeField, Min(0)] private int _cashCost;
        [Tooltip("0보다 크면 끝날 때 가진 장물을 이 비율로 현금화한다 (장물 처분소용)")]
        [SerializeField, Range(0f, 1f)] private float _lootConversionRate;

        [Tooltip("켜면 끝날 때 미납 청구서를 전부 납부한다 (우편함용)")]
        [SerializeField] private bool _payBills;

        [Header("씬 이동 (문)")]
        [Tooltip("비어 있지 않으면 끝날 때 이 씬으로 이동한다. Build Settings에 등록된 씬 이름")]
        [SerializeField] private string _loadScene;
        [Tooltip("도착한 씬에서 플레이어가 설 SpawnPoint의 Id")]
        [SerializeField] private string _spawnId;

        public bool PayBills => _payBills;
        public string LoadScene => _loadScene;
        public string SpawnId => _spawnId;
        public string DisplayName => _displayName;
        public float Duration => _duration;
        public NeedEffect[] Effects => _effects;
        public int CashCost => _cashCost;
        public float LootConversionRate => _lootConversionRate;
    }
}
