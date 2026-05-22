using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MobileControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private enum MobileAction
    {
        LaneForward,
        MoveLeft,
        MoveRight,
        LaneBack,
        Jump,
        Pause,
        SwapControls
    }

    [SerializeField] private MobileControlsUI controls;
    [SerializeField] private MobileAction action;

    private bool _pressed;

    private void Reset()
    {
        controls = GetComponentInParent<MobileControlsUI>();
    }

    private void Awake()
    {
        if (controls == null)
            controls = GetComponentInParent<MobileControlsUI>();
    }

    private void OnDisable()
    {
        ReleaseIfNeeded();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;

        switch (action)
        {
            case MobileAction.LaneForward:
                controls?.LaneForward();
                break;
            case MobileAction.MoveLeft:
                controls?.PressMoveLeft();
                break;
            case MobileAction.MoveRight:
                controls?.PressMoveRight();
                break;
            case MobileAction.LaneBack:
                controls?.LaneBack();
                break;
            case MobileAction.Jump:
                controls?.JumpDown();
                break;
            case MobileAction.Pause:
                controls?.Pause();
                break;
            case MobileAction.SwapControls:
                controls?.SwapControls();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReleaseIfNeeded();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ReleaseIfNeeded();
    }

    private void ReleaseIfNeeded()
    {
        if (!_pressed)
            return;

        _pressed = false;

        switch (action)
        {
            case MobileAction.MoveLeft:
            case MobileAction.MoveRight:
                controls?.ReleaseMove();
                break;
            case MobileAction.Jump:
                controls?.JumpUp();
                break;
        }
    }
}
