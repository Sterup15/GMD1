using UnityEngine;

namespace GameObjects.Common.Progression.Scripts
{
    // The currency kept between runs, earned by converting run points at the end of each run
    public static class Glory
    {
        private const string BalanceKey = "progress.glory";

        public static int Balance => PlayerPrefs.GetInt(BalanceKey, 0);

        public static void Add(int amount)
        {
            if (amount <= 0) return;
            PlayerPrefs.SetInt(BalanceKey, Balance + amount);
            // The web build only writes PlayerPrefs to the browser's storage on Save
            PlayerPrefs.Save();
        }
    }
}
