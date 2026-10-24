using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FullscreenToggle : Toggle
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left ||
            !IsActive() ||
            !IsInteractable())
            return;

        base.OnPointerDown(eventData);
        isOn = !isOn;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {}
}