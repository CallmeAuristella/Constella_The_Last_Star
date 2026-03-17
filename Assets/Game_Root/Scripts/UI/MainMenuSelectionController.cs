using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuSelectionController : MonoBehaviour
{
    [Header("Menu Items")]
    [SerializeField] private RectTransform[] menuItems;

    [Header("Buttons")]
    [SerializeField] private Button[] menuButtons;

    [Header("Star Indicator")]
    [SerializeField] private RectTransform starIndicator;

    [Header("Animation")]
    [SerializeField] private float selectedOffset = 22f;
    [SerializeField] private float moveSpeed = 10f;

    [Header("Star Position")]
    [SerializeField] private float starOffsetX = -120f;

    [Header("Star Pulse")]
    [SerializeField] private float starPulseScale = 1.25f;
    [SerializeField] private float starPulseSpeed = 12f;

    private Vector3 starBaseScale;

    private int currentIndex = 0;

    private Vector2[] originalPositions;

    void Start()
    {
        originalPositions = new Vector2[menuItems.Length];

        for (int i = 0; i < menuItems.Length; i++)
        {
            originalPositions[i] = menuItems[i].anchoredPosition;
        }

        starBaseScale = starIndicator.localScale;

        UpdateSelectionImmediate();
    }

    void Update()
    {
        HandleKeyboardInput();
        UpdateVisual();
    }

    void HandleKeyboardInput()
    {
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            ChangeSelection(1);
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            ChangeSelection(-1);
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ConfirmSelection();
        }
    }

    void ChangeSelection(int direction)
    {
        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = menuItems.Length - 1;

        if (currentIndex >= menuItems.Length)
            currentIndex = 0;

        TriggerStarPulse();
    }

    void UpdateVisual()
    {
        // TEXT MOVEMENT
        for (int i = 0; i < menuItems.Length; i++)
        {
            Vector2 target = originalPositions[i];

            if (i == currentIndex)
                target.x += selectedOffset;

            menuItems[i].anchoredPosition = Vector2.Lerp(
                menuItems[i].anchoredPosition,
                target,
                Time.deltaTime * moveSpeed
            );
        }

        // STAR FOLLOW
        RectTransform currentItem = menuItems[currentIndex];

        Vector3 worldPos = currentItem.position;

        Vector3 starTarget = worldPos;
        starTarget.x += starOffsetX;

        starIndicator.position = Vector3.Lerp(
            starIndicator.position,
            starTarget,
            Time.deltaTime * moveSpeed
        );

        // STAR SCALE RETURN
        starIndicator.localScale = Vector3.Lerp(
            starIndicator.localScale,
            starBaseScale,
            Time.deltaTime * starPulseSpeed
        );
    }

    void UpdateSelectionImmediate()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            Vector2 pos = originalPositions[i];

            if (i == currentIndex)
                pos.x += selectedOffset;

            menuItems[i].anchoredPosition = pos;
        }

        Vector2 starTarget = originalPositions[currentIndex];
        starTarget.x += starOffsetX;

        starIndicator.anchoredPosition = starTarget;
    }

    void ConfirmSelection()
    {
        if (menuButtons[currentIndex] != null)
        {
            menuButtons[currentIndex].onClick.Invoke();
        }
    }

    void TriggerStarPulse()
    {
        starIndicator.localScale = starBaseScale * starPulseScale;
    }

    public void SetIndex(int newIndex)
    {
        currentIndex = newIndex;
        TriggerStarPulse();
    }
}