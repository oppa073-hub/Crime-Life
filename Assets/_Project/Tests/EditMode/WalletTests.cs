using CrimeLife.Crime;
using NUnit.Framework;
using UnityEngine;

namespace CrimeLife.Tests
{
    public class WalletTests
    {
        private GameObject _go;
        private Wallet _wallet;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("WalletTest");
            _wallet = _go.AddComponent<Wallet>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        [Test]
        public void ConvertAllLoot_PaysRateAndEmptiesLoot()
        {
            _wallet.AddLoot(100);
            int gained = _wallet.ConvertAllLoot(0.7f);
            Assert.AreEqual(70, gained);
            Assert.AreEqual(70, _wallet.Cash);
            Assert.AreEqual(0, _wallet.Loot);
        }

        [Test]
        public void TrySpend_FailsWithoutChangingCash_WhenNotEnough()
        {
            _wallet.AddCash(10);
            Assert.IsFalse(_wallet.TrySpend(20));
            Assert.AreEqual(10, _wallet.Cash);
            Assert.IsTrue(_wallet.TrySpend(10));
            Assert.AreEqual(0, _wallet.Cash);
        }

        [Test]
        public void Confiscate_NeverGoesBelowZero()
        {
            _wallet.AddCash(30);
            _wallet.AddLoot(40);
            _wallet.Confiscate(100, 15);
            Assert.AreEqual(0, _wallet.Cash);
            Assert.AreEqual(25, _wallet.Loot);
        }

        [Test]
        public void Changed_FiresWithCurrentValues()
        {
            int cash = -1, loot = -1;
            _wallet.Changed += (c, l) => { cash = c; loot = l; };
            _wallet.AddLoot(5);
            Assert.AreEqual(0, cash);
            Assert.AreEqual(5, loot);
        }
    }
}
