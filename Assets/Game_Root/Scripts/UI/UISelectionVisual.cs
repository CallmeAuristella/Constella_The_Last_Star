using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UISelectionVisual : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler {
    [SerializeField] private GameObject selectionOverlay;

    private bool isHovered;

    private static bool usingController;

    void Update() {
        DetectInputMode();

        GameObject current =
            EventSystem.current.currentSelectedGameObject;

        bool isSelected = current == gameObject;

        bool show = false;

        if (usingController) {
            show = isSelected;
        } else {
            show = isHovered;
        }

        selectionOverlay.SetActive(show);
    }

    void DetectInputMode() {
        // Controller / Keyboard
        if (
            Keyboard.current.anyKey.wasPressedThisFrame ||
            Gamepad.current != null &&
            (
                Gamepad.current.leftStick.ReadValue().magnitude > 0.2f ||
                Gamepad.current.dpad.ReadValue() != Vector2.zero
            )
        ) {
            usingController = true;
        }

        // Mouse
        if (
            Mouse.current.delta.ReadValue() != Vector2.zero
        ) {
            usingController = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        ForceMouseMode();

        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovered = false;
    }
    public static void ForceControllerMode() {
        usingController = true;
    }

    public static void ForceMouseMode() {
        usingController = false;
    }
}