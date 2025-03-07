using System;
using System.Text.RegularExpressions;
using _Memoriam.Script.General;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private InputManager InputManager { get; set; }
        public PlayerActionsScript PlayerActions { get; private set; }

        public Action<ControlType> OnControlTypeChanged;
        
        //Regex Pattern
        private const string PatternForController = @"Control";

        protected override void Awake()
        {
            base.Awake();
            PlayerActions = new PlayerActionsScript();
            InputManager = new InputManager(PlayerActions);
        }

        //Subscribe
        private void OnEnable()
        {
            InputManager.TypeOfController += GetTypeOfController;
            InputManager.OnButtonPress += ChangeControllerType;
        }

        //Unsubscribe
        private void OnDisable()
        {
            InputManager.TypeOfController -= GetTypeOfController;
            InputManager.OnButtonPress -= ChangeControllerType;
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