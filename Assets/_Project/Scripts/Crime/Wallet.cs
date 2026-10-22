using System;
using CrimeLife.Core;
using UnityEngine;

namespace CrimeLife.Crime
{
    // 현금과 장물을 따로 둔다: 범죄로 얻은 건 장물이고, 처분소에서 현금으로 바꿔야 쓸 수 있다.
    public class Wallet : MonoBehaviour, ISaveable
    {
        [SerializeField, Min(0)] private int _cash;
        [SerializeField, Min(0)] private int _loot;

        public int Cash => _cash;
        public int Loot => _loot;

        public event Action<int, int> Changed;

        public void AddCash(int amount)
        {
            _cash = Mathf.Max(0, _cash + amount);
            Notify();
        }

        public void AddLoot(int amount)
        {
            _loot = Mathf.Max(0, _loot + amount);
            Notify();
        }

        public bool TrySpend(int amount)
        {
            if (amount > _cash) return false;
            _cash -= amount;
            Notify();
            return true;
        }

        // 장물을 현금으로 바꾼다. 수수료만큼 손해라서 rate는 1 미만이다. 얻은 현금을 돌려준다.
        public int ConvertAllLoot(float rate)
        {
            int gained = Mathf.RoundToInt(_loot * Mathf.Clamp01(rate));
            _loot = 0;
            _cash += gained;
            Notify();
            return gained;
        }

        // 체포 시 압수: 현금은 일부, 장물은 지정 비율만큼 빼앗는다
        public void Confiscate(int cash, int loot)
        {
            _cash = Mathf.Max(0, _cash - cash);
            _loot = Mathf.Max(0, _loot - loot);
            Notify();
        }

        public void Save(SaveData data)
        {
            data.Cash = _cash;
            data.Loot = _loot;
        }

        public void Load(SaveData data)
        {
            _cash = Mathf.Max(0, data.Cash);
            _loot = Mathf.Max(0, data.Loot);
            Notify();
        }

        private void Notify() => Changed?.Invoke(_cash, _loot);
    }
}
