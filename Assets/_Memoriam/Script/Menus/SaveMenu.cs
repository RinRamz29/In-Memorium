using System.Collections.Generic;
using System.Threading.Tasks;
using _Memoriam.Script.Audio;
using _Memoriam.Script.Localization;
using _Memoriam.Script.Managers;
using _Memoriam.Script.SaveLoad;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Memoriam.Script.Menus
{
    public class SaveMenu : MonoBehaviour, ILocalization
    {
        [SerializeField] private TMPro.TextMeshProUGUI[] slotTexts;
        [SerializeField] private GameObject firstToSelect;
        [SerializeField] private List<LanguagesClass> languages;
        [SerializeField] private SceneDataBase sceneData;
        [SerializeField] public GameObject errorCanva;
        [SerializeField] public GameObject errorButton;
        [SerializeField] private Toggle tutoToggle;
        public TMP_Text TextToTranslateTMP { get; set; }
        private int _slot;

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(firstToSelect);
            tutoToggle.onValueChanged.AddListener(SetRepeatTutorial);
            tutoToggle.isOn = Loader.Instance.SetTutorial;
        }

        private void UpdateSlotUI(string text, string textNoSave)
        {
            for (int i = 0; i < 3; i++)
            {
                if (DataPersistentManager.Instance.DoesSaveExist(i + 1))
                {
                    slotTexts[i].text = text;
                }
                else
                {
                    slotTexts[i].text = textNoSave;
                }
            }
        }

        public void Translate(Languages language)
        {
            foreach (var lang in languages)
            {
                if (lang.TryGetText(language, out var txt))
                {
                    var splitted = txt.Split("/");
                    UpdateSlotUI(splitted[0], splitted[1]);
                    break;
                }
            }
        }

        public void LoadGame(int slot)
        {
            if (DataPersistentManager.Instance.DoesSaveExist(slot))
            {
                ConfirmLoad(slot);
                return;
            }

            MenuManager.Instance.NewGame();
            DataPersistentManager.Instance.SelectedSlot = slot;
        }


        public void ConfirmSave()
        {
            DataPersistentManager.Instance.SelectedSlot = _slot;
            DataPersistentManager.Instance.OverWriteSave(_slot);
            MenuManager.Instance.NewGame();
        }

        public void ConfirmLoad(int slot)
        {
            _slot = slot;
            DataPersistentManager.Instance.SelectedSlot = slot;
            MenuManager.Instance.LoadGame(_slot);
        }
        
        private void SetRepeatTutorial(bool value)
        {
            Loader.Instance.SetTutorial = value;
        }
    }
}