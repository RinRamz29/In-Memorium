using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace _Memoriam.Script.InputLogic
{
    public class InputManager 
    {
        private PlayerActionsScript _playerActions;
        
        //Subscribe
        public InputManager(PlayerActionsScript playerActions)
        {
            _playerActions = playerActions;
            _playerActions.Enable();

            InputSystem.onDeviceChange += SetController;
            InputSystem.onAnyButtonPress.Call(control => { OnButtonPress?.Invoke(control); });
        }

        public Action<InputControl> OnButtonPress;
        public Action<InputDevice, InputDeviceChange> TypeOfController;

        //Check for connection/disconnection of an input device
        private void SetController(InputDevice device, InputDeviceChange change)
        {
            TypeOfController.Invoke(device, change);
        }

        //Unsubscribe
        ~InputManager()
        {
            _playerActions.Disable();

            InputSystem.onDeviceChange -= SetController;
        }
    }
}