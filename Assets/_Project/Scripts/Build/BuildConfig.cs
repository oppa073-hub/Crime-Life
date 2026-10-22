using UnityEngine;

namespace CrimeLife.Build
{
    [CreateAssetMenu(fileName = "BuildConfig", menuName = "CrimeLife/Build Config")]
    public class BuildConfig : ScriptableObject
    {
        [Tooltip("가구를 팔 때 돌려받는 가격 비율")]
        [SerializeField, Range(0f, 1f)] private float _sellRefundFraction = 0.5f;

        public float SellRefundFraction => _sellRefundFraction;
    }
}
