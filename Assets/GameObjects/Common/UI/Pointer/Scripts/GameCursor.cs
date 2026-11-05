using UnityEngine;

namespace GameObjects.Common.UI.Pointer.Scripts
{
    // Swaps the mouse cursor for the Tiny Swords arrow in every scene. The texture is loaded from Resources,
    // so no scene needs a component for it
    public static class GameCursor
    {
        private const string TexturePath = "UI/GameCursor";
        // The arrow's tip, in pixels from the texture's top-left corner
        private static readonly Vector2 Hotspot = new(23f, 18f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            var texture = Resources.Load<Texture2D>(TexturePath);
            if (texture == null)
            {
                Debug.LogWarning($"GameCursor: No cursor texture at Resources/{TexturePath}.");
                return;
            }
            Cursor.SetCursor(texture, Hotspot, CursorMode.Auto);
        }
    }
}
