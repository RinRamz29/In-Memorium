using System;
using _Memoriam.Script.Enemies.BasicEnemy;
using _Memoriam.Script.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpManager : MonoBehaviour
{
   public int level = 1;
   public int currentExp;
   public int expToLevel = 100;
   public float expGrowMultiplier = 1.2f;
   public Slider expSlider;
   public TMP_Text expText;

   private void Start()
   {
      UpdateUI();
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.P))
      {
         GainExp(25);
      }
   }

   private void OnEnable()
   {
      BasicEnemy.OnMonsterDefeated += GainExp;
      RangedEnemy.OnMonsterDefeated += GainExp;
      FlyingEnemy.OnMonsterDefeated += GainExp;
   }
   
   private void OnDisable()
   {
      BasicEnemy.OnMonsterDefeated -= GainExp;
      RangedEnemy.OnMonsterDefeated -= GainExp;
      FlyingEnemy.OnMonsterDefeated -= GainExp;
   }

   public void GainExp(int amount)
   {
      currentExp += amount;
      if (currentExp >= expToLevel)
      {
         LevelUp();
      }
      UpdateUI();
   }

   private void LevelUp()
   {
      level++;
      currentExp -= expToLevel;
      expToLevel = Mathf.RoundToInt(expToLevel * expGrowMultiplier);
   }

   public void UpdateUI()
   {
      expSlider.maxValue = expToLevel;
      expSlider.value = currentExp;
      expText.text = "Lvl: " + level;
   }
}
