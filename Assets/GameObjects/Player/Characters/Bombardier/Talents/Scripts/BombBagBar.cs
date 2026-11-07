using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Player.Characters.Bombardier.Talents.Scripts
{
    // HUD bar of the Bomb Bag talent: fills as the next bomb goes into the bag, with "bombs/capacity" above it.
    // It lives in the talent prefab, so it only shows when the talent is equipped
    public class BombBagBar : MonoBehaviour
    {
        [SerializeField] private BombBagTalent talent;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text countText;

        private int _shownBombs = -1;
        private int _shownCapacity = -1;

        private void Update()
        {
            if (talent == null) return;
            slider.value = talent.Progress;

            int bombs = talent.Bombs;
            int capacity = talent.Capacity;
            if (bombs == _shownBombs && capacity == _shownCapacity) return;
            _shownBombs = bombs;
            _shownCapacity = capacity;
            countText.text = $"{bombs}/{capacity}";
        }
    }
}
