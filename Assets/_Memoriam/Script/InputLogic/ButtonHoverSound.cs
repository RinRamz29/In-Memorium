using _Memoriam.Script.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Memoriam.Script.InputLogic
{
    public class ButtonHoverSound : MonoBehaviour, ISelectHandler
    {
        public void OnSelect(BaseEventData eventData)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShotSFX("UIButtonHoverSFX");
            }
        }
    }
}