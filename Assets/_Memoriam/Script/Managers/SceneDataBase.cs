using System.Collections.Generic;
using UnityEngine;

namespace _Memoriam.Script.Managers
{
    [CreateAssetMenu(fileName = "SceneDataBase", menuName = "Databases/SceneDataBase")]
    public class SceneDataBase : ScriptableObject
    {
        [field: SerializeField] public string GameSceneName { get; private set; }
        [field: SerializeField] public string LoadingSceneName { get; private set; }
        [field: SerializeField] public string MainMenuSceneName { get; private set; }
    }
}
