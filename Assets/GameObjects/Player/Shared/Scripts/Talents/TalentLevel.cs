using System;
using UnityEngine;

namespace GameObjects.Player.Shared.Scripts.Talents
{
    // One level of a talent above level 1: unlocked by its character's mastery, then bought with Glory
    [Serializable]
    public class TalentLevel
    {
        [Tooltip("Mastery level of the character that makes this level buyable")]
        [SerializeField, Min(1)] private int masteryLevel = 1;
        [SerializeField, Min(0)] private int gloryCost;
        [Tooltip("What the talent does at this level, shown instead of the level 1 description")]
        [TextArea] [SerializeField] private string description;

        public int MasteryLevel   => masteryLevel;
        public int GloryCost      => gloryCost;
        public string Description => description;
    }
}
