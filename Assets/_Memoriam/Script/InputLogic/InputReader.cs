using System;
using System.Text.RegularExpressions;
using _Memoriam.Script.General;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

namespace _Memoriam.Script.InputLogic
{
    [Preserve]
    public class InputReader : Singleton<InputReader>
    {
        //Enum for control type
        public enum ControlType
        {
            Control,
            KeyboardMouse,
        }

        [field: SerializeField] public ControlType ControlTypes { get; set; }
        public PlayerActionsScript PlayerActions { get; private set; }

        public Action<ControlType> OnControlTypeChanged;
        
        //Regex Pattern
        private const string PatternForController = @"Control";
        
        public Action<InputControl> OnButtonPress;
        public Action<InputDevice, InputDeviceChange> TypeOfController;

        //Check for connection/disconnection of an input device
        private void SetController(InputDevice device, InputDeviceChange change)
        {
            TypeOfController.Invoke(device, change);
        }

        //Subscribe
        protected override void Awake()
        {
            base.Awake();
            PlayerActions = new PlayerActionsScript();
            
            TypeOfController += GetTypeOfController;
            OnButtonPress += ChangeControllerType;
            InputSystem.onDeviceChange += SetController;
            InputSystem.onAnyButtonPress.Call(control => { OnButtonPress?.Invoke(control); });
        }

        //Unsubscribe
        protected override void OnDestroy()
        {
            base.OnDestroy();
            TypeOfController -= GetTypeOfController;
            OnButtonPress -= ChangeControllerType;
            InputSystem.onDeviceChange -= SetController;
        }

        //Get type of controller from change of device Delegate
        private void GetTypeOfController(InputDevice inputDevice, InputDeviceChange change)
        {
            if (inputDevice == null)
                return;

            var device = inputDevice.device;

            if (change == InputDeviceChange.Added)
            {
                var regex = new Regex(PatternForController);

                var match = regex.Match(device.name);

                if (match.Success)
                {
                    ControlTypes = ControlType.Control;
                }
            }
        }

        // Change the controller type based on button pressed on it
        private void ChangeControllerType(InputControl inputControl)
        {
            if (inputControl == null)
                return;

            var regex = new Regex(PatternForController);
            var device = inputControl.device;
            var match = regex.Match(device.name);

            if (device.name.Contains("Keyboard") || device.name.Contains("Mouse"))
            {
                ControlTypes = ControlType.KeyboardMouse;
                OnControlTypeChanged?.Invoke(ControlTypes);
                Debug.Log(device.name);
            }

            if (match.Success)
            {
                ControlTypes = ControlType.Control;
                OnControlTypeChanged?.Invoke(ControlTypes);
                Debug.Log(device.name);
            }
        }
    }
}