using UnityEngine;

namespace AutoChess.Data
{
    [System.Serializable]
    public class FactionThreshold
    {
        public int count;
        public string description;
        public int healthBonus;
        public int attackBonus;
        public float attackSpeedBonus;
        public int armorBonus;
        public int magicResistBonus;
    }

    [CreateAssetMenu(fileName = "FactionData", menuName = "AutoChess/FactionData")]
    public class FactionData : ScriptableObject
    {
        public string factionName = "Faction";
        public Color factionColor = Color.white;
        public FactionThreshold[] thresholds;
    }
}
