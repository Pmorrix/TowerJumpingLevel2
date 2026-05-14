using UnityEngine;

public sealed class MobileControlsUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject controlsRoot;
    [SerializeField] private bool showInEditorForTesting;

    [Header("Player")]
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private PlayerVisualFacing playerVisualFacing;

    [Header("Pause")]
    [SerializeField] private PauseSimpleUI pauseSimpleUI;

    private void Awake()
    {
        if (controlsRoot == null)
            controlsRoot = gameObject;

        ResolvePlayerRefs();
        ApplyPlatformVisibility();
    }

    private void OnEnable()
    {
        ResolvePlayerRefs();
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
