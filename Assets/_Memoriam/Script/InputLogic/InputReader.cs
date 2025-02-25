using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Zenject;

namespace _Memoriam.Script.InputLogic
{
    public class InputReader : MonoBehaviour
    {
        //Enum for control type
        public enum ControlType
        {
            Control,
            KeyboardMouse,
        }

        [field: SerializeField] public ControlType ControlTypes { get; set; }
        [Inject] private InputManager _inputManager;

        //Regex Pattern
        private const string PatternForController = @"Control";

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
                Debug.Log(device.name);
            }

            if (match.Success)
            {
                ControlTypes = ControlType.Control;
                Debug.Log(device.name);
            }
        }
    }
}