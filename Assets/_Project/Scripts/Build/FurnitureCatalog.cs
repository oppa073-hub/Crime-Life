using UnityEngine;

namespace CrimeLife.Build
{
    [CreateAssetMenu(fileName = "FurnitureCatalog", menuName = "CrimeLife/Furniture Catalog")]
    public class FurnitureCatalog : ScriptableObject
    {
        [SerializeField] private FurnitureDefinition[] _items;

        public FurnitureDefinition[] Items => _items;

        public FurnitureDefinition Find(string id)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i].Id == id) return _items[i];
            }
            return null;
        }
    }
}
