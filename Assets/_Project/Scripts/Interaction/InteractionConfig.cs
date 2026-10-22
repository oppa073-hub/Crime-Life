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

        public string DisplayName => _displayName;
        public float Duration => _duration;
        public NeedEffect[] Effects => _effects;
        public int CashCost => _cashCost;
        public float LootConversionRate => _lootConversionRate;
    }
}
