using UnityEngine;

namespace AutoChess.Data
{
    [System.Serializable]
    public class CreepInfo
    {
        public string creepName = "野怪";
        public int health = 300;
        public int attackDamage = 30;
        public int armor = 10;
        public float attackSpeed = 0.6f;
        public Color color = Color.gray;
    }

    [CreateAssetMenu(fileName = "CreepRound_", menuName = "AutoChess/CreepRoundData")]
    public class CreepRoundData : ScriptableObject
    {
        public string roundName;
        public CreepInfo[] creeps;
        public int goldReward = 1;
        public int equipmentDropCount = 1;
        public bool dropCombinedEquipment = false;
    }
}
