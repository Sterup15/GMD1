using GameObjects.Common.Events;
using GameObjects.Common.Progression.Scripts;
using TMPro;
using UnityEngine;

namespace GameObjects.Common.UI.EndScreen.Scripts
{
    // Fills the end screen's results with what the run earned. Sits on the end screen's root, which stays active
    // while the panel is hidden, so it hears OnRunEnded
    public class RunResultsText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private void OnEnable()  => GlobalEvents.OnRunEnded += Show;
        private void OnDisable() => GlobalEvents.OnRunEnded -= Show;

        private void Show(RunResult result)
        {
            int minutes = Mathf.FloorToInt(result.Seconds / 60f);
            int seconds = Mathf.FloorToInt(result.Seconds % 60f);

            string lines = $"Kills {result.Kills}  +{result.KillPoints}      Time {minutes}:{seconds:00}  +{result.TimePoints}\n";
            if (result.Won)
                lines += $"Victory  +{result.VictoryPoints}\n";
            lines += $"<b>+{result.GloryEarned} Glory</b>  ({result.GloryTotal} total)";

            if (!string.IsNullOrEmpty(result.CharacterName))
            {
                lines += $"\n+{result.MasteryXpEarned} {result.CharacterName} XP  (Mastery {result.MasteryLevelAfter})";
                if (result.MasteryLevelAfter > result.MasteryLevelBefore)
                    lines += "  <color=#B35C00>Level up!</color>";
            }

            text.text = lines;
        }
    }
}
