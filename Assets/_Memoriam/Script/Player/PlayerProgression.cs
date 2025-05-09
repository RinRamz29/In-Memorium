using System;
using UnityEngine;

namespace _Memoriam.Script.Player
{
    [Serializable]
    public class PlayerProgression
    {
        [SerializeField] private int maxLevel = 30;
        [SerializeField] private float minXpToLevelUp = 100f;
        [SerializeField] private float maxXpToLevelUp = 1500f;

        [field: SerializeField] public int   Level      { get; private set; } = 1;
        [field: SerializeField] public float CurrentXp  { get; private set; } = 0;

        public float XpToNextLevel
        {
            get
            {
                if (Level >= maxLevel) return Mathf.Infinity;

                float t = (float)(Level - 1) / (maxLevel - 1);  
                return minXpToLevelUp + (maxXpToLevelUp - minXpToLevelUp) * Mathf.Pow(t, 3);
            }
        }
        public event Action<int> OnLevelUp;
        public event Action<float> OnXpGained;

        public void GainXp(float amount)
        {
            if (Level >= maxLevel) return;                

            CurrentXp += amount;
            OnXpGained?.Invoke(amount);

            while (true)
            {
                float xpNeeded = XpToNextLevel;          

                if (CurrentXp >= xpNeeded)
                {
                    CurrentXp -= xpNeeded;
                    Level++;
                    OnLevelUp?.Invoke(Level);

                    if (Level >= maxLevel)            
                    {
                        CurrentXp = 0;
                        break;
                    }
                }
                else break;
            }
        }
    }
}