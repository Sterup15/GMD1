using System.Collections.Generic;
using Actors.Common;
using Actors.Common.Upgrades;
using Actors.Player.Scripts.Gold;
using UnityEngine;

namespace UI.Upgrade
{
    public class UpgradeScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private UpgradeCard[] cards;
        [SerializeField] private List<UpgradeDefinition> upgradePool;

        private Stats _playerStats;
        private readonly List<UpgradeDefinition> _available = new();

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _playerStats = playerObj.GetComponent<Stats>();

            _available.AddRange(upgradePool);
            panel.SetActive(false);
        }

        private void OnEnable()  => PlayerGold.OnLevelUp += Show;
        private void OnDisable() => PlayerGold.OnLevelUp -= Show;

        private void Show()
        {
            if (_available.Count == 0) return;

            List<UpgradeDefinition> drawn = DrawUpgrades(Mathf.Min(3, _available.Count));

            for (int i = 0; i < cards.Length; i++)
            {
                if (i < drawn.Count)
                {
                    int index = i; // capture for lambda
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
        }

        private void Pick(UpgradeDefinition definition)
        {
            if (_playerStats != null)
            {
                var stat = _playerStats.GetStat(definition.statType);
                stat?.AddBonus(definition.bonusAmount);
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
                int index = Random.Range(0, pool.Count);
                drawn.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return drawn;
        }
    }
}
