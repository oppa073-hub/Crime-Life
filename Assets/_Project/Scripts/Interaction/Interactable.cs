using UnityEngine;

namespace CrimeLife.Interaction
{
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private InteractionConfig _config;

        public InteractionConfig Config => _config;
    }
}
