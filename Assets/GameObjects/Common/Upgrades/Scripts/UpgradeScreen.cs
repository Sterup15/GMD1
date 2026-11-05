using System;
using System.Collections.Generic;
using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using GameObjects.Common.UI.Navigation.Scripts;
using GameObjects.Common.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GameObjects.Common.Upgrades.Scripts
{
    [Serializable]
    public struct RarityWeight
    {
        public UpgradeRarity rarity;
        [Tooltip("Chance in percent when the weights add up to 100")]
        public float weight;
        [Tooltip("Percentage points added per point of Luck (negative takes them away, e.g. from Common)")]
        public float luckBonus;
    }

    public class UpgradeScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private UpgradeCard[] cards;
        [Header("Balance")]
        [Tooltip("What one card point is worth for each stat. Stats missing here get 1 flat per point")]
        [SerializeField] private List<StatPointValue> pointValues = new()
        {
            new StatPointValue(StatType.Damage,      1f),
            new StatPointValue(StatType.MaxHealth,   1f),
            new StatPointValue(StatType.FireRate,    40f,      true),
            new StatPointValue(StatType.MoveSpeed,   13.3333f, true),
            new StatPointValue(StatType.ShootRange,  8f,       true),
            new StatPointValue(StatType.PickupRange, 40f,      true),
            new StatPointValue(StatType.Area,        10f,      true),
            new StatPointValue(StatType.Haste,       10f),
            new StatPointValue(StatType.Luck,        1f),
        };
        [Tooltip("Luck stops counting above this, and Luck cards stop showing up once it's reached")]
        [SerializeField, Min(0f)] private float maxLuck = 10f;
        [SerializeField] private List<RarityWeight> rarityWeights = new()
        {
            new RarityWeight { rarity = UpgradeRarity.Common,    weight = 60f, luckBonus = -6f },
            new RarityWeight { rarity = UpgradeRarity.Uncommon,  weight = 25f, luckBonus = 3f  },
            new RarityWeight { rarity = UpgradeRarity.Rare,      weight = 12f, luckBonus = 2f  },
            new RarityWeight { rarity = UpgradeRarity.Legendary, weight = 3f,  luckBonus = 1f  },
        };

        private Stats.Scripts.Stats _playerStats;
        private UpgradeEligibility _eligibility;
        private UpgradeScaling _scaling;
        private readonly List<UpgradeDefinition> _available = new();
        private MenuNavigator _navigator;

        private void Start()
        {
            var playerObj = Players.Local;
            if (playerObj != null)
            {
                _playerStats = playerObj.GetComponent<Stats.Scripts.Stats>();
                _eligibility = playerObj.GetComponent<UpgradeEligibility>();
                _scaling = playerObj.GetComponent<UpgradeScaling>();
            }

            _available.AddRange(Resources.LoadAll<UpgradeDefinition>("Upgrades"));

            // The cards are listed right to left, the navigator wants them in on-screen order
            var buttons = new Button[cards.Length];
            for (int i = 0; i < cards.Length; i++)
                buttons[i] = cards[cards.Length - 1 - i].Button;
            _navigator = new MenuNavigator(buttons);

            panel.SetActive(false);
        }

        private void OnEnable()  => GlobalEvents.OnLevelUp += OnLevelUp;
        private void OnDisable() => GlobalEvents.OnLevelUp -= OnLevelUp;

        private void OnLevelUp(int _) => Show();

        private void Update()
        {
            if (!panel.activeSelf || _navigator.InputBlocked) return;

            _navigator.Navigate();
            if (GameInput.InteractPressed) _navigator.Submit();
        }

        private void Show()
        {
            if (_available.Count == 0) return;

            List<UpgradeDefinition> drawn = DrawUpgrades(Mathf.Min(3, _available.Count));

            for (int i = 0; i < cards.Length; i++)
            {
                if (i < drawn.Count)
                {
                    int index = i;
                    cards[i].gameObject.SetActive(true);
                    cards[i].Setup(drawn[i], () => Pick(drawn[index]));
                }
                else
                {
                    cards[i].gameObject.SetActive(false);
                }
            }

            panel.SetActive(true);
            Time.timeScale = 0f;

            _navigator.BlockInputThisFrame();
            _navigator.Select(cards[0].Button);
        }

        private void Pick(UpgradeDefinition definition)
        {
            if (_playerStats != null)
            {
                var stat = _playerStats.GetStat(definition.statType);
                if (stat != null) stat.AddBonus(BonusFor(definition, stat));
            }

            if (!definition.canRepeat)
                _available.Remove(definition);

            panel.SetActive(false);
            Time.timeScale = 1f;
        }

        // Percentage stats scale with the character's own base value, so one card set works for every character,
        // and the character's UpgradeScaling tunes how much it gets out of each stat
        private float BonusFor(UpgradeDefinition definition, Stat stat)
        {
            float multiplier = _scaling != null ? _scaling.For(definition.statType) : 1f;

            int index = pointValues.FindIndex(p => p.stat == definition.statType);
            if (index < 0) return definition.points * multiplier;

            var value = pointValues[index];
            float perPoint = value.percentOfBase ? stat.BaseValue * value.perPoint / 100f : value.perPoint;
            return perPoint * definition.points * multiplier;
        }

        private List<UpgradeDefinition> DrawUpgrades(int count)
        {
            List<UpgradeDefinition> pool = new(_available);
            // Checked on every level-up, so cards unlocked mid-run (e.g. by an item) show up from the next draw
            if (_eligibility != null)
                pool.RemoveAll(u => !_eligibility.Allows(u.requirement));
            if (Luck >= maxLuck)
                pool.RemoveAll(u => u.statType == StatType.Luck);
            List<UpgradeDefinition> drawn = new();

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                var picked = PickWeighted(pool);
                drawn.Add(picked);
                pool.RemoveAll(u => u.statType == picked.statType);
            }

            return drawn;
        }

        private UpgradeDefinition PickWeighted(List<UpgradeDefinition> pool)
        {
            float totalWeight = 0f;
            foreach (var rw in rarityWeights)
            {
                if (pool.Exists(u => u.rarity == rw.rarity))
                    totalWeight += WeightOf(rw);
            }

            if (totalWeight <= 0f)
                return pool[Random.Range(0, pool.Count)];

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var rw in rarityWeights)
            {
                var candidates = pool.FindAll(u => u.rarity == rw.rarity);
                if (candidates.Count == 0) continue;
                cumulative += WeightOf(rw);
                if (roll < cumulative)
                    return candidates[Random.Range(0, candidates.Count)];
            }

            return pool[Random.Range(0, pool.Count)];
        }

        // Each point of Luck moves luckBonus percentage points onto a rarity (by default +3 Uncommon, +2 Rare,
        // +1 Legendary, taken from Common). Luck is capped at maxLuck
        private float WeightOf(RarityWeight rw)
        {
            return Mathf.Max(0f, rw.weight + Mathf.Min(Luck, maxLuck) * rw.luckBonus);
        }

        private float Luck => _playerStats != null ? Mathf.Max(0f, _playerStats.Luck.Value) : 0f;
    }
}
