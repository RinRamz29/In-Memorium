using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Memoriam.Script.Managers
{
    public class MainMenuButtonSelector : MonoBehaviour
    {
        [SerializeField] private GameObject PlayButton;

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(PlayButton);
        }

        public void Play()
        {
            MenuManager.Instance.NewGame();
        }
    }
}