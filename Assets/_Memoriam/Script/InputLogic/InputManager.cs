using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Zenject;

namespace _Memoriam.Script.InputLogic
{
    public class InputManager : MonoBehaviour
    {
        [Inject] private PlayerActionsScript _playerActions;

        public Action<InputControl> OnButtonPress;
        public Action<InputDevice, InputDeviceChange> TypeOfController;

        //Check for connection/disconnection of an input device
        private void SetController(InputDevice device, InputDeviceChange change)
        {
            TypeOfController.Invoke(device, change);
        }

        //Subscribe
        private void Awake()
        {
            InputSystem.onDeviceChange += SetController;

            InputSystem.onAnyButtonPress.Call(control => { OnButtonPress?.Invoke(control); });
        }

        //Unsubscribe
        private void OnDisable()
        {
            InputSystem.onDeviceChange -= SetController;
        }
    }
}