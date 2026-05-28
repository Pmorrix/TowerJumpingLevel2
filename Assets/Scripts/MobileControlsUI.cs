using UnityEngine;
using UnityEngine.Rendering;

public sealed class MobileControlsUI : MonoBehaviour
{
    private const string SwapLayoutPrefsKey = "MobileControlsUI.SwappedLayout";

    [Header("Root")]
    [SerializeField] private GameObject controlsRoot;
    [SerializeField] private bool showInEditorForTesting;

    [Header("Player")]
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private PlayerVisualFacing playerVisualFacing;

    [Header("Pause")]
    [SerializeField] private PauseSimpleUI pauseSimpleUI;

    [Header("Swap Controls")]
    [SerializeField] private RectTransform directionControlsRoot;
    [SerializeField] private RectTransform jumpControlRoot;
    [SerializeField] private bool rememberSwappedLayout = true;

    private bool _isSwappedLayout;

    private void Awake()
    {
        if (controlsRoot == null)
            controlsRoot = gameObject;

        ResolveControlRefs();
        LoadSavedLayout();
        ResolvePlayerRefs();
        DisableRuntimeRenderingDebuggerOnMobile();
        ApplyPlatformVisibility();
    }

    private void OnEnable()
    {
        ResolvePlayerRefs();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.C))
            return;

        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            return;

        ToggleControlPositions();
    }

    private void OnDisable()
    {
        ReleaseMove();
        JumpUp();
    }

    public void PressMoveLeft()
    {
        ResolvePlayerRefs();

        if (playerMove != null)
            playerMove.MobileMoveLeft();

        if (playerVisualFacing != null)
            playerVisualFacing.FaceLeft();
    }

    public void PressMoveRight()
    {
        ResolvePlayerRefs();

        if (playerMove != null)
            playerMove.MobileMoveRight();

        if (playerVisualFacing != null)
            playerVisualFacing.FaceRight();
    }

    public void ReleaseMove()
    {
        if (playerMove != null)
            playerMove.MobileStopHorizontal();
    }

    public void LaneForward()
    {
        ResolvePlayerRefs();

        if (playerMove != null)
            playerMove.MobileLaneForward();

        if (playerVisualFacing != null)
            playerVisualFacing.FaceBack();
    }

    public void LaneBack()
    {
        ResolvePlayerRefs();

        if (playerMove != null)
            playerMove.MobileLaneBack();

        if (playerVisualFacing != null)
            playerVisualFacing.FaceForward();
    }

    public void JumpDown()
    {
        ResolvePlayerRefs();

        if (playerJump != null)
            playerJump.MobileJumpDown();
    }

    public void JumpUp()
    {
        if (playerJump != null)
            playerJump.MobileJumpUp();
    }

    public void Pause()
    {
        ResolvePauseRef();

        if (pauseSimpleUI != null)
            pauseSimpleUI.Pause();
    }

    public void SwapControls()
    {
        ToggleControlPositions();
    }

    private void ToggleControlPositions()
    {
        ResolveControlRefs();

        if (directionControlsRoot == null || jumpControlRoot == null)
            return;

        ReleaseMove();
        JumpUp();
        SwapHorizontalLayout(directionControlsRoot, jumpControlRoot);

        _isSwappedLayout = !_isSwappedLayout;

        if (rememberSwappedLayout)
            PlayerPrefs.SetInt(SwapLayoutPrefsKey, _isSwappedLayout ? 1 : 0);
    }

    private void LoadSavedLayout()
    {
        if (!rememberSwappedLayout)
            return;

        _isSwappedLayout = PlayerPrefs.GetInt(SwapLayoutPrefsKey, 0) == 1;

        if (_isSwappedLayout)
            SwapHorizontalLayout(directionControlsRoot, jumpControlRoot);
    }

    private static void SwapHorizontalLayout(RectTransform first, RectTransform second)
    {
        if (first == null || second == null)
            return;

        float firstAnchorMinX = first.anchorMin.x;
        float firstAnchorMaxX = first.anchorMax.x;
        float firstAnchoredPositionX = first.anchoredPosition.x;
        float firstPivotX = first.pivot.x;

        first.anchorMin = new Vector2(second.anchorMin.x, first.anchorMin.y);
        first.anchorMax = new Vector2(second.anchorMax.x, first.anchorMax.y);
        first.anchoredPosition = new Vector2(second.anchoredPosition.x, first.anchoredPosition.y);
        first.pivot = new Vector2(second.pivot.x, first.pivot.y);

        second.anchorMin = new Vector2(firstAnchorMinX, second.anchorMin.y);
        second.anchorMax = new Vector2(firstAnchorMaxX, second.anchorMax.y);
        second.anchoredPosition = new Vector2(firstAnchoredPositionX, second.anchoredPosition.y);
        second.pivot = new Vector2(firstPivotX, second.pivot.y);
    }

    private void ApplyPlatformVisibility()
    {
        if (controlsRoot == null)
            return;

        controlsRoot.SetActive(ShouldShowControls());
    }

    private bool ShouldShowControls()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return true;
#elif UNITY_EDITOR
        return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android
            || showInEditorForTesting;
#else
        return showInEditorForTesting;
#endif
    }

    private static void DisableRuntimeRenderingDebuggerOnMobile()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        DebugManager.instance.displayRuntimeUI = false;
        DebugManager.instance.displayPersistentRuntimeUI = false;
        DebugManager.instance.enableRuntimeUI = false;
#endif
    }

    private void ResolveControlRefs()
    {
        if (directionControlsRoot == null)
            directionControlsRoot = transform.Find("LeftControlsRoot") as RectTransform;

        if (jumpControlRoot == null)
            jumpControlRoot = transform.Find("MobileJumpButton") as RectTransform;
    }

    private void ResolvePlayerRefs()
    {
        if (playerMove != null && playerJump != null && playerVisualFacing != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        if (playerMove == null)
            playerMove = player.GetComponent<PlayerMove>();

        if (playerJump == null)
            playerJump = player.GetComponent<PlayerJump>();

        if (playerVisualFacing == null)
            playerVisualFacing = player.GetComponent<PlayerVisualFacing>();
    }

    private void ResolvePauseRef()
    {
        if (pauseSimpleUI != null)
            return;

        pauseSimpleUI = FindAnyObjectByType<PauseSimpleUI>();
    }
}
