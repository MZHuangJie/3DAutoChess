using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AutoChess.Data;
using AutoChess.Core;

namespace AutoChess.AI
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private StarMergeManager starMergeManager;

        private Dictionary<PlayerData, string> primaryFactions = new Dictionary<PlayerData, string>();

        public void Setup(GameConfig config, BoardManager board, ShopManager shop, StarMergeManager merge)
        {
            gameConfig = config;
            boardManager = board;
            shopManager = shop;
            starMergeManager = merge;
        }

        public void MakeDecisions(PlayerData aiPlayer, PlayerBoard playerBoard)
        {
            if (aiPlayer == null || !aiPlayer.IsAlive) return;

            // 1. Try to upgrade level if bench/board is getting full
            if (aiPlayer.CanUpgradeLevel(gameConfig) && aiPlayer.GetCurrentBoardUnitCount() >= aiPlayer.GetMaxUnitsOnBoard() - 1)
            {
                aiPlayer.BuyExp(gameConfig);
            }

            // 2. Determine primary faction after round 3
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.CurrentRound >= 3)
                UpdatePrimaryFaction(aiPlayer);

            // 3. Buy heroes from shop (prioritize faction synergy + star merge)
            bool boughtSomething = true;
            int buyAttempts = 0;
            while (boughtSomething && buyAttempts < 3)
            {
                boughtSomething = false;
                buyAttempts++;

                int bestSlot = FindBestShopSlot(aiPlayer);
                if (bestSlot >= 0 && shopManager.BuyHero(aiPlayer, bestSlot))
                {
                    boughtSomething = true;
                    starMergeManager.CheckAndMerge(aiPlayer);
                }
            }

            // 4. Refresh shop if gold is plentiful and we want more
            if (aiPlayer.gold >= 10 && Random.value < 0.3f)
            {
                shopManager.RefreshShop(aiPlayer);
                int bestSlot = FindBestShopSlot(aiPlayer);
                if (bestSlot >= 0)
                    shopManager.BuyHero(aiPlayer, bestSlot);
            }

            // 5. Assign equipment to best pieces
            AssignEquipment(aiPlayer);

            // 6. Position pieces on board
            PositionPieces(aiPlayer);

            // 7. Save board state
            if (playerBoard != null)
                playerBoard.SaveFrom(boardManager, aiPlayer);
        }

        void UpdatePrimaryFaction(PlayerData aiPlayer)
        {
            var factions = GetCurrentFactions(aiPlayer);
            if (factions.Count == 0) return;

            string best = null;
            int bestCount = 0;
            foreach (var kvp in factions)
            {
                if (kvp.Value > bestCount)
                {
                    bestCount = kvp.Value;
                    best = kvp.Key;
                }
            }
            if (best != null)
                primaryFactions[aiPlayer] = best;
        }

        int FindBestShopSlot(PlayerData aiPlayer)
        {
            if (aiPlayer.currentShop == null) return -1;

            var currentFactions = GetCurrentFactions(aiPlayer);
            var heroCount = GetHeroNameCounts(aiPlayer);
            string primary = primaryFactions.ContainsKey(aiPlayer) ? primaryFactions[aiPlayer] : null;

            int bestSlot = -1;
            float bestScore = -1;

            for (int i = 0; i < aiPlayer.currentShop.Count; i++)
            {
                var hero = aiPlayer.currentShop[i];
                if (hero == null) continue;
                if (aiPlayer.gold < hero.cost) continue;

                float score = hero.cost;

                if (hero.factions != null)
                {
                    foreach (var f in hero.factions)
                    {
                        if (currentFactions.ContainsKey(f))
                            score += 5f + currentFactions[f] * 2f;
                        if (f == primary)
                            score += 10f;
                    }
                }

                // Near star merge bonus: already have 2 copies
                if (heroCount.ContainsKey(hero.heroName) && heroCount[hero.heroName] >= 2)
                    score += 15f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestSlot = i;
                }
            }

            return bestSlot;
        }

        Dictionary<string, int> GetHeroNameCounts(PlayerData aiPlayer)
        {
            var counts = new Dictionary<string, int>();
            var pieces = boardManager.GetPiecesByOwner(aiPlayer);
            foreach (var p in pieces)
            {
                if (p?.heroData == null || p.starLevel >= 3) continue;
                string key = $"{p.heroData.heroName}_s{p.starLevel}";
                if (!counts.ContainsKey(key)) counts[key] = 0;
                counts[key]++;
            }
            return counts;
        }

        Dictionary<string, int> GetCurrentFactions(PlayerData aiPlayer)
        {
            var factions = new Dictionary<string, int>();
            var pieces = boardManager.GetPiecesByOwner(aiPlayer);
            foreach (var piece in pieces)
            {
                if (piece?.heroData?.factions == null) continue;
                foreach (var f in piece.heroData.factions)
                {
                    if (!factions.ContainsKey(f)) factions[f] = 0;
                    factions[f]++;
                }
            }
            return factions;
        }

        void AssignEquipment(PlayerData aiPlayer)
        {
            if (aiPlayer.equipmentInventory == null || aiPlayer.equipmentInventory.Count == 0) return;
            if (EquipmentManager.Instance == null) return;

            var pieces = boardManager.GetPiecesByOwner(aiPlayer);
            var boardOnly = pieces.Where(p => !p.isOnBench && p.CanEquip).ToList();
            if (boardOnly.Count == 0) return;

            var sortedByAttack = boardOnly.OrderByDescending(p => p.attackDamage).ToList();
            var sortedByHealth = boardOnly.OrderByDescending(p => p.maxHealth).ToList();

            var toAssign = new List<EquipmentData>(aiPlayer.equipmentInventory);
            foreach (var eq in toAssign)
            {
                if (eq == null) continue;
                bool isDefensive = eq.healthBonus > 0 || eq.armorBonus > 0 || eq.magicResistBonus > 0;
                var target = isDefensive ? sortedByHealth.FirstOrDefault(p => p.CanEquip) : sortedByAttack.FirstOrDefault(p => p.CanEquip);
                if (target == null) target = boardOnly.FirstOrDefault(p => p.CanEquip);
                if (target == null) break;

                EquipmentManager.Instance.EquipItem(target, eq, aiPlayer);
            }
        }

        void PositionPieces(PlayerData aiPlayer)
        {
            var pieces = boardManager.GetPiecesByOwner(aiPlayer);
            var boardPieces = pieces.Where(p => !p.isOnBench).ToList();
            var benchPieces = pieces.Where(p => p.isOnBench).ToList();

            int maxBoard = aiPlayer.GetMaxUnitsOnBoard();

            // Move strongest pieces from bench to board
            var sortedBench = benchPieces.OrderByDescending(p => EvaluatePieceStrength(p)).ToList();
            int toPlace = Mathf.Min(sortedBench.Count, maxBoard - boardPieces.Count);

            for (int i = 0; i < toPlace; i++)
            {
                var piece = sortedBench[i];
                // Tanks (high HP, low attack) go front (row 0-1), carries go back (row 2-3)
                bool isTank = piece.heroData != null && piece.heroData.maxHealth > piece.heroData.attackDamage * 5;
                int preferredRow = isTank ? 0 : Mathf.Max(0, gameConfig.boardRows - 1);
                bool placed = false;

                for (int rowOffset = 0; rowOffset < gameConfig.boardRows && !placed; rowOffset++)
                {
                    int row = Mathf.Clamp(preferredRow + (rowOffset % 2 == 0 ? rowOffset / 2 : -(rowOffset / 2 + 1)), 0, gameConfig.boardRows - 1);

                    for (int col = 0; col < gameConfig.boardCols && !placed; col++)
                    {
                        bool posOccupied = false;
                        foreach (var existing in aiPlayer.boardPieces)
                        {
                            if (existing != piece && existing.boardPosition.x == row && existing.boardPosition.y == col)
                            {
                                posOccupied = true;
                                break;
                            }
                        }
                        if (posOccupied) continue;

                        boardManager.RemovePieceFromAnywhere(piece);
                        piece.isOnBench = false;
                        piece.benchIndex = -1;
                        piece.boardPosition = new Vector2Int(row, col);
                        piece.transform.position = new Vector3(col * gameConfig.cellSize, -100, row * gameConfig.cellSize);
                        if (!aiPlayer.boardPieces.Contains(piece))
                            aiPlayer.boardPieces.Add(piece);
                        aiPlayer.benchPieces.Remove(piece);
                        placed = true;
                    }
                }
            }
        }

        float EvaluatePieceStrength(ChessPiece piece)
        {
            if (piece?.heroData == null) return 0;
            float starMult = piece.starLevel == 2 ? 1.8f : (piece.starLevel == 3 ? 3.6f : 1f);
            return (piece.heroData.maxHealth * starMult * 0.01f) + (piece.heroData.attackDamage * starMult * 0.1f);
        }
    }
}
