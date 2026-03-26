using System;
using UnityEngine;

namespace Actors.Common
{
    [Serializable]
    public class Stat
    {
        [SerializeField] private float baseValue;
        private float _bonus;

        public float Value => baseValue + _bonus;

        public void AddBonus(float amount) => _bonus += amount;
        public void RemoveBonus(float amount) => _bonus -= amount;
    }
}