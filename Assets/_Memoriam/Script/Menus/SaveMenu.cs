using System.Threading.Tasks;
using _Memoriam.Script.Audio;
using _Memoriam.Script.Managers;
using _Memoriam.Script.SaveLoad;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Memoriam.Script.Menus
{
    public class SaveMenu : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI[] slotTexts;
        [SerializeField] private GameObject firstToSelect;
        [SerializeField] private SceneDataBase sceneData;

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
        
        public async void LoadGame(int slot)
        {
            await Task.Delay(150);
            AudioManager.Instance.PlayMusic("GameplayMusic");
            GameLoader.newGame = false;
            GameLoader.slotIndex = slot;
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData.LoadingSceneName);
        }
    }
}
