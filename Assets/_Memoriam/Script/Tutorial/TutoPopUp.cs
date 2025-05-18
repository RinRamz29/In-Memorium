using System;
using _Memoriam.Script.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Memoriam.Script.Tutorial
{
    public class TutoPopUp : MonoBehaviour
    {
        [SerializeField] private GameObject firsToSelect;
        [SerializeField] private Toggle tutoToggle;

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(firsToSelect);

            tutoToggle.onValueChanged.AddListener(SetRepeatTutorial);
            tutoToggle.isOn = Loader.Instance.SetTutorial;
        }

        private void SetRepeatTutorial(bool value)
        {
            Loader.Instance.SetTutorial = value;
        }
    }
}