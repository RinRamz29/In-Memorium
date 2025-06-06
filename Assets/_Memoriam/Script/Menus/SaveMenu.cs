using System;
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
    public class SaveMenu : MonoBehaviour
    {
        [SerializeField] private GameObject firstToSelect;
        [SerializeField] private GameObject[] saveDataPanel = new GameObject[3];
        [SerializeField] private List<LanguagesClass> languages;
        [SerializeField] private SceneDataBase sceneData;
        [SerializeField] private Toggle tutoToggle;
        [SerializeField] private SlotData[] slotData = new SlotData[3];

        
        public TMP_Text TextToTranslateTMP { get; set; }
        private Languages _selectedLanguage;

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(firstToSelect);
            tutoToggle.onValueChanged.AddListener(SetRepeatTutorial);
            tutoToggle.isOn = Loader.Instance.SetTutorial;
            UpdateSlotUI();
        }

        private void UpdateSlotUI()
        {
            for (int i = 0; i < 3; i++)
            {
                if (DataPersistentManager.Instance.DoesSaveExist(i + 1))
                {
                    var data = DataPersistentManager.Instance.GetSlotData(i + 1);

                    if (data != null)
                    {
                        slotData[i].dateText.text = data.playerData.saveDate;
                        slotData[i].hasDoubleJump.isOn = data.playerData.hasDoubleJump;
                        slotData[i].hasDash.isOn = data.playerData.hasDash;
                        slotData[i].playerHealthText.text = data.playerData.playerHealth.ToString("N0");
                    }
                    else
                    {
                        Debug.LogError($"There is no save data for {i + 1}");
                        saveDataPanel[i].SetActive(false);
                        return;
                    }
                }
                else
                {
                    saveDataPanel[i].SetActive(false);
                }
            }
        }

        public void LoadGame(int slot)
        {
            if (DataPersistentManager.Instance.DoesSaveExist(slot))
            {
                DataPersistentManager.Instance.SelectedSlot = slot;
                MenuManager.Instance.LoadGame(slot);
                return;
            }

            MenuManager.Instance.NewGame();
            DataPersistentManager.Instance.SelectedSlot = slot;
        }

        public void DeleteSave(int slot)
        {
            DataPersistentManager.Instance.SelectedSlot = slot;
            DataPersistentManager.Instance.DeleteSave(slot);
            UpdateSlotUI();
        }
        
        private void SetRepeatTutorial(bool value)
        {
            Loader.Instance.SetTutorial = value;
        }
    }

    [Serializable]
    public class SlotData
    {
        [Header("Save Slot Data")]
        public TMP_Text playerHealthText;
        public TMP_Text dateText;
        public Toggle hasDoubleJump;
        public Toggle hasDash;
    }
}