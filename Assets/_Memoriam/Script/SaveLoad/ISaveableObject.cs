using _Memoriam.Script.SaveLoad.Data;

namespace _Memoriam.Script.SaveLoad
{
    public interface ISaveableObject
    {
        void LoadData(GameData data);
        
        void SaveData(ref GameData data);
    }
}