using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset controls;

    public static Vector2 Move          { get; private set; }
    public static bool InteractPressed  { get; private set; }
    public static bool NavigateLeft     { get; private set; }
    public static bool NavigateRight    { get; private set; }
    public static bool NavigateUp       { get; private set; }
    public static bool NavigateDown     { get; private set; }

    private float _prevMoveY;

    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction quitAction;
    private InputAction navLeftAction;
    private InputAction navRightAction;

    private void Awake()
    {
        var player = controls.FindActionMap("Player", throwIfNotFound: true);
        moveAction     = player.FindAction("Move",          throwIfNotFound: true);
        interactAction = player.FindAction("Interact",      throwIfNotFound: true);
        quitAction     = player.FindAction("Quit",          throwIfNotFound: true);
        navLeftAction  = player.FindAction("NavigateLeft",  throwIfNotFound: true);
        navRightAction = player.FindAction("NavigateRight", throwIfNotFound: true);
    }

    private void OnEnable()
    {
        moveAction.Enable();
        interactAction.Enable();
        quitAction.Enable();
        navLeftAction.Enable();
        navRightAction.Enable();
        quitAction.performed += OnQuit;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        interactAction.Disable();
        quitAction.Disable();
        navLeftAction.Disable();
        navRightAction.Disable();
        quitAction.performed -= OnQuit;
    }

    private void Update()
    {
        Move            = moveAction.ReadValue<Vector2>();
        InteractPressed = interactAction.WasPressedThisFrame();
        NavigateLeft    = navLeftAction.WasPressedThisFrame();
        NavigateRight   = navRightAction.WasPressedThisFrame();
        NavigateUp      = Move.y >  0.5f && _prevMoveY <=  0.5f;
        NavigateDown    = Move.y < -0.5f && _prevMoveY >= -0.5f;
        _prevMoveY      = Move.y;
    }

    private static void OnQuit(InputAction.CallbackContext _)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
