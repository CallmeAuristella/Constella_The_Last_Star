using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler
{
    public int index;
    public MainMenuSelectionController controller;

    public void OnPointerEnter(PointerEventData eventData)
    {
        controller.SetIndex(index);
    }
}