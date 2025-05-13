using System.Threading.Tasks;
using _Memoriam.Script.Audio;
using _Memoriam.Script.Managers;
using _Memoriam.Script.SaveLoad;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Memoriam.Script.Menus
{
    public class SaveMenu : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI[] slotTexts;
        [SerializeField] private GameObject firstToSelect;
        [SerializeField] private SceneDataBase sceneData;
        [SerializeField] public GameObject errorCanva;
        [SerializeField] public GameObject errorButton;

        private void OnEnable()
        {
            UpdateSlotUI();
            EventSystem.current.SetSelectedGameObject(firstToSelect);
        }

        private void UpdateSlotUI()
        {
            for (int i = 0; i < 3; i++)
            {
                if (DataPersistentManager.Instance.DoesSaveExist(i + 1))
                {
                    slotTexts[i].text = $"Save Slot {i + 1}\nExists";
                }
                else
                {
                    slotTexts[i].text = $"Save Slot {i + 1}\nEmpty";
                }
            }
        }
        
        public void LoadGame(int slot)
        {
            if (MenuManager.Instance.NoSave)
            {
                errorCanva.SetActive(true);
                EventSystem.current.SetSelectedGameObject(errorButton);
                return;
            }

            MenuManager.Instance.LoadGame(slot);
        }
    }
}
