using System;
using CrimeLife.Core;
using UnityEngine;

namespace CrimeLife.Needs
{
    public class NeedsController : MonoBehaviour, ISaveable
    {
        [SerializeField] private NeedsConfig _config;

        private NeedsModel _model;

        public event Action<NeedType, float> ValueChanged;
        public event Action<NeedType> BecameCritical;

        public NeedsModel Model => _model;

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("NeedsConfig가 연결되지 않았다.", this);
                enabled = false;
                return;
            }

            _model = _config.CreateModel();
            _model.ValueChanged += (type, value) => ValueChanged?.Invoke(type, value);
            _model.BecameCritical += type => BecameCritical?.Invoke(type);
        }

        private void Update()
        {
            _model.Tick(Time.deltaTime * _config.GameHoursPerRealSecond);
        }

        public void Save(SaveData data) => data.Needs = _model.ToArray();

        public void Load(SaveData data) => _model.Restore(data.Needs);

        public float Get(NeedType type) => _model.Get(type);
        public void Add(NeedType type, float delta) => _model.Add(type, delta);
        public float GetCrimeModifier() => _model.GetCrimeModifier();
    }
}
