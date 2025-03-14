using System;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Player;
using _Memoriam.Script.SaveLoad;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveMenu : MonoBehaviour
{
    public GameObject saveMenu;
    InputAction saveAction;
    

    private void Start()
    {
        saveAction = InputReader.Instance.PlayerActions.FindAction("SaveMenu");
    }


    public void Save()
    {
        DataPersistentManager.Instance.SaveGame();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        saveMenu.SetActive(false);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            if (saveAction.IsPressed())
            {
                saveMenu.SetActive(true);
                player.LastCheckPoint = transform.position;
            }
        }
    }
}
