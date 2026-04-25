using System.Collections.Generic;
using UnityEngine;
using AutoChess.Core;

namespace AutoChess.Data
{
    public class PlayerData
    {
        public string playerName;
        public bool isHuman;
        public int health;
        public int gold;
        public int level;
        public int exp;
        public int[] expToLevel; // exp needed to reach each level

        public List<ChessPiece> benchPieces = new List<ChessPiece>();
        public List<ChessPiece> boardPieces = new List<ChessPiece>();
        public List<EquipmentData> equipmentInventory = new List<EquipmentData>();
        public int maxEquipmentInventory = 10;

        // Milestone 2 additions
        public int winStreak = 0;
        public int loseStreak = 0;
        public List<HeroData> currentShop = new List<HeroData>();
        public bool shopLocked = false;
        public PlayerData lastOpponent = null;
        public int placement = 0; // final ranking (1 = first place)

        public bool IsAlive => health > 0;

        public PlayerData(string name, bool human, GameConfig config)
        {
            playerName = name;
            isHuman = human;
            health = config.startingHealth;
            gold = config.startingGold;
            level = config.startingLevel;
            exp = 0;
            // EXP to reach NEXT level: index = current level
            expToLevel = new int[] { 0, 0, 2, 4, 8, 12, 20, 32, 48, 80, 100 };
            currentShop = new List<HeroData>();
            for (int i = 0; i < config.shopSlotCount; i++)
                currentShop.Add(null);
        }

        public int GetMaxUnitsOnBoard()
        {
            return Mathf.Min(level, 10);
        }

        public bool CanUpgradeLevel(GameConfig config)
        {
            if (level >= config.maxLevel) return false;
            return gold >= config.expBuyCost;
        }

        public void BuyExp(GameConfig config)
        {
            if (!CanUpgradeLevel(config)) return;
            gold -= config.expBuyCost;
            exp += config.expPerBuy;
            CheckLevelUp(config);
        }

        public void CheckLevelUp(GameConfig config)
        {
            while (level < config.maxLevel && exp >= expToLevel[level])
            {
                exp -= expToLevel[level];
                level++;
            }
        }

        public int GetCurrentBoardUnitCount()
        {
            int count = 0;
            foreach (var p in boardPieces)
                if (p != null && p.IsAlive) count++;
            return count;
        }

        public int GetInterest(GameConfig config)
        {
            return Mathf.Min(gold / 10, config.maxInterest);
        }
    }
}
