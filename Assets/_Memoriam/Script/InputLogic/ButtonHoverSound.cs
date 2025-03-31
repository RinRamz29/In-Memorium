using TerrorConsole;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonHoverSFX");
        }
    }
}