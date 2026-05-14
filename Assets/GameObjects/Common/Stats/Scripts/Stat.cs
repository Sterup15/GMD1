using System;
using UnityEngine;

namespace GameObjects.Common.Stats.Scripts
{
    [Serializable]
    public class Stat
    {
        [SerializeField] private float baseValue;
        private float _bonus;

        public float Value => baseValue + _bonus;

        public event Action OnValueChanged;

        public void SetBaseValue(float value) { baseValue = value; OnValueChanged?.Invoke(); }
        public void AddBonus(float amount)    { _bonus += amount;  OnValueChanged?.Invoke(); }
        public void RemoveBonus(float amount) { _bonus -= amount;  OnValueChanged?.Invoke(); }
    }
}