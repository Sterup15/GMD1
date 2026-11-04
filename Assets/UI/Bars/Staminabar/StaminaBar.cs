using GameObjects.Player.Scripts.Dash;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Bars.Staminabar
{
    // Shows the dash cooldown, plus one pip per charge when the player has more than one
    public class StaminaBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private GameObject chargeRow;
        [Tooltip("Pre-made pips, one per possible charge. Extra pips stay hidden")]
        [SerializeField] private Image[] pips;
        [SerializeField] private Color readyColor = new(0.35f, 0.85f, 0.3f, 1f);
        [SerializeField] private Color emptyColor = new(0.25f, 0.25f, 0.25f, 0.8f);

        private PlayerDash _dash;

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _dash = playerObj.GetComponent<PlayerDash>();

            if (_dash == null)
            {
                Debug.LogWarning("StaminaBar: No PlayerDash found on the player.");
                return;
            }

            _dash.OnChargesChanged += UpdatePips;
            UpdatePips(_dash.Charges, _dash.MaxCharges);
        }

        // The player is only found in Start, so the first subscription happens there
        private void OnEnable()
        {
            if (_dash != null) _dash.OnChargesChanged += UpdatePips;
        }

        private void OnDisable()
        {
            if (_dash != null) _dash.OnChargesChanged -= UpdatePips;
        }

        private void Update()
        {
            if (_dash != null) slider.value = _dash.CooldownProgress;
        }

        private void UpdatePips(int charges, int maxCharges)
        {
            chargeRow.SetActive(maxCharges > 1);

            for (int i = 0; i < pips.Length; i++)
            {
                pips[i].gameObject.SetActive(i < maxCharges);
                pips[i].color = i < charges ? readyColor : emptyColor;
            }
        }
    }
}
