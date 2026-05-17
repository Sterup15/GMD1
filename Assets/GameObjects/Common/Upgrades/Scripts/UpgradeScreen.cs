using System;
using System.Collections.Generic;
using GameObjects.Common.Events;
using GameObjects.Common.Stats.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameObjects.Common.Upgrades.Scripts
{
    [Serializable]
    public struct RarityWeight
    {
        public UpgradeRarity rarity;
        public float weight;
    }

    public class UpgradeScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private UpgradeCard[] cards;
        [SerializeField] private float bonusScale = 1f;
        [SerializeField] private List<RarityWeight> rarityWeights = new()
        {
            new RarityWeight { rarity = UpgradeRarity.Common,    weight = 60f },
            new RarityWeight { rarity = UpgradeRarity.Uncommon,  weight = 25f },
            new RarityWeight { rarity = UpgradeRarity.Rare,      weight = 12f },
            new RarityWeight { rarity = UpgradeRarity.Legendary, weight = 3f  },
        };

        private Stats.Scripts.Stats _playerStats;
        private readonly List<UpgradeDefinition> _available = new();
        private int _selectedIndex;
        private int _activeCardCount;

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _playerStats = playerObj.GetComponent<Stats.Scripts.Stats>();

            _available.AddRange(Resources.LoadAll<UpgradeDefinition>("Upgrades"));
            panel.SetActive(false);
        }

        private void OnEnable()  => GlobalEvents.OnLevelUp += OnLevelUp;
        private void OnDisable() => GlobalEvents.OnLevelUp -= OnLevelUp;

        private void OnLevelUp(int _) => Show();

        private void Update()
        {
            if (!panel.activeSelf) return;

            if (GameInput.NavigateLeft)
            {
                _selectedIndex = (_selectedIndex + 1) % _activeCardCount;
                cards[_selectedIndex].Select();
            }
            else if (GameInput.NavigateRight)
            {
                _selectedIndex = (_selectedIndex - 1 + _activeCardCount) % _activeCardCount;
                cards[_selectedIndex].Select();
            }

            if (GameInput.InteractPressed)
                cards[_selectedIndex].Confirm();
        }

        private void Show()
        {
            if (_available.Count == 0) return;

            List<UpgradeDefinition> drawn = DrawUpgrades(Mathf.Min(3, _available.Count));
            _activeCardCount = drawn.Count;

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

            _selectedIndex = 0;
            cards[0].Select();
        }

        private void Pick(UpgradeDefinition definition)
        {
            if (_playerStats != null)
            {
                var stat = _playerStats.GetStat(definition.statType);
                float amount = definition.bonusAmount * (definition.applyBonusScale ? bonusScale : 1f);
                stat?.AddBonus(amount);
            }

            if (!definition.canRepeat)
                _available.Remove(definition);

            panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private List<UpgradeDefinition> DrawUpgrades(int count)
        {
            List<UpgradeDefinition> pool = new(_available);
            List<UpgradeDefinition> drawn = new();

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                var picked = PickWeighted(pool);
                drawn.Add(picked);
                pool.Remove(picked);
            }

            return drawn;
        }

        private UpgradeDefinition PickWeighted(List<UpgradeDefinition> pool)
        {
            float totalWeight = 0f;
            foreach (var rw in rarityWeights)
            {
                if (pool.Exists(u => u.rarity == rw.rarity))
                    totalWeight += rw.weight;
            }

            if (totalWeight <= 0f)
                return pool[Random.Range(0, pool.Count)];

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var rw in rarityWeights)
            {
                var candidates = pool.FindAll(u => u.rarity == rw.rarity);
                if (candidates.Count == 0) continue;
                cumulative += rw.weight;
                if (roll < cumulative)
                    return candidates[Random.Range(0, candidates.Count)];
            }

            return pool[Random.Range(0, pool.Count)];
        }
    }
}
