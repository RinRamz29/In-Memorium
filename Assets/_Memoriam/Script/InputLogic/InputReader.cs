using System;
using System.Text.RegularExpressions;
using _Memoriam.Script.General;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Memoriam.Script.InputLogic
{
    public class InputReader : MonoSingleton<InputReader>
    {
        //Enum for control type
        public enum ControlType
        {
            Control,
            KeyboardMouse,
        }

        [field: SerializeField] public ControlType ControlTypes { get; set; }
        public InputManager _inputManager { get; private set; }
        public PlayerActionsScript PlayerActions { get; private set; }

        public Action<ControlType> OnControlTypeChanged;
        
        //Regex Pattern
        private const string PatternForController = @"Control";

        protected override void Awake()
        {
            base.Awake();
            PlayerActions = new PlayerActionsScript();
            _inputManager = new InputManager(PlayerActions);
        }

        //Subscribe
        private void OnEnable()
        {
            _inputManager.TypeOfController += GetTypeOfController;
            _inputManager.OnButtonPress += ChangeControllerType;
        }

        //Unsubscribe
        private void OnDisable()
        {
            _inputManager.TypeOfController -= GetTypeOfController;
            _inputManager.OnButtonPress -= ChangeControllerType;
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