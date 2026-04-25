using UnityEngine;

namespace AutoChess.Data
{
    public enum EquipmentType { Base, Combined }

    [CreateAssetMenu(fileName = "Equipment_", menuName = "AutoChess/EquipmentData")]
    public class EquipmentData : ScriptableObject
    {
        [Header("Identity")]
        public string equipmentName;
        public EquipmentType equipmentType;
        public Color displayColor = Color.white;

        [Header("Stat Bonuses")]
        public int healthBonus;
        public int attackBonus;
        public int armorBonus;
        public int magicResistBonus;
        public float attackSpeedBonus;
        public int manaBonus;

        [Header("Special Effects")]
        public float lifestealPercent;
        public int spellDamageBonus;
        public float dodgeChance;

        [Header("Combine Recipe (Combined only)")]
        public EquipmentData recipe1;
        public EquipmentData recipe2;
    }
}
