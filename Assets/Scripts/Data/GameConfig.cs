using UnityEngine;

namespace AutoChess.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "AutoChess/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Board")]
        public int boardRows = 4;
        public int boardCols = 7;
        public int benchSlots = 9;
        public float cellSize = 1.2f;
        public float boardSpacing = 2f;

        [Header("Phase Durations")]
        public float preparationDuration = 30f;
        public float combatMaxDuration = 30f;
        public float combatOvertimeDuration = 15f;
        public float resultDuration = 3f;

        [Header("Player")]
        public int startingHealth = 100;
        public int startingGold = 5;
        public int startingLevel = 1;
        public int maxLevel = 10;

        [Header("Economy")]
        public int baseIncomePerRound = 5;
        public int maxInterest = 5;
        public int interestPer10Gold = 1;
        public int[] streakBonus = new int[] { 0, 0, 1, 2, 3 }; // index = streak count

        [Header("Shop")]
        public int shopSlotCount = 5;
        public int refreshCost = 2;
        public int[] expCost = new int[] { 0, 0, 2, 6, 10, 14, 20, 36, 76, 84 }; // exp needed to reach next level (index = current level)
        public int expBuyCost = 4;
        public int expPerBuy = 4;

        [Header("Hero Pool")]
        public int[] poolCountByCost = new int[] { 0, 39, 26, 18, 13, 10 }; // index = cost

        [Header("Roll Probability (level, cost)")]
        // Roll probability table: rows = player level (1-10), columns = cost (1-5)
        public int[] level1Prob = new int[] { 100, 0, 0, 0, 0 };
        public int[] level2Prob = new int[] { 75, 25, 0, 0, 0 };
        public int[] level3Prob = new int[] { 55, 30, 15, 0, 0 };
        public int[] level4Prob = new int[] { 45, 33, 20, 2, 0 };
        public int[] level5Prob = new int[] { 35, 35, 25, 5, 0 };
        public int[] level6Prob = new int[] { 22, 35, 30, 10, 3 };
        public int[] level7Prob = new int[] { 15, 25, 35, 20, 5 };
        public int[] level8Prob = new int[] { 12, 20, 30, 28, 10 };
        public int[] level9Prob = new int[] { 10, 15, 22, 35, 18 };
        public int[] level10Prob = new int[] { 5, 10, 18, 35, 32 };

        [Header("Combat")]
        public float attackTickRate = 0.1f;
        public float moveSpeed = 3f;

        [Header("PvE")]
        public int[] pveRounds = { 1, 2, 3, 10, 17, 24 };
        public int pveLossDamage = 0;

        [Header("Augment")]
        public int[] augmentRounds = { 4, 9, 15 };
        public float augmentSelectDuration = 30f;

        [Header("Carousel")]
        public int[] carouselRounds = { 5, 12, 19 };
        public float carouselSelectDuration = 20f;

        public int[] GetRollProbability(int level)
        {
            return level switch
            {
                1 => level1Prob,
                2 => level2Prob,
                3 => level3Prob,
                4 => level4Prob,
                5 => level5Prob,
                6 => level6Prob,
                7 => level7Prob,
                8 => level8Prob,
                9 => level9Prob,
                10 => level10Prob,
                _ => level10Prob,
            };
        }
    }
}
