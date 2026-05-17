using UnityEngine;
using UnityEngine.UI;

namespace GameObjects.Common.UI.Scripts
{
    [RequireComponent(typeof(Button))]
    public class QuitButton : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() => Application.Quit());
        }
    }
}
