using System;
using _Memoriam.Script.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Memoriam.Script.General
{
    public class MoreComing : MonoBehaviour
    {
        [SerializeField] private GameObject moreComingUI;
        [SerializeField] private GameObject selectedButton;
        [SerializeField] private GameObject playerUI;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            playerUI.SetActive(false);
            GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnPause);
            moreComingUI.SetActive(true);
            EventSystem.current.SetSelectedGameObject(selectedButton);
        }
    }
}