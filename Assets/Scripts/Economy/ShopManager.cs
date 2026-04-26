using System.Collections.Generic;
using UnityEngine;
using AutoChess.Data;
using AutoChess.UI;

namespace AutoChess.Core
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private HeroPool heroPool;

        void Awake()
        {
            Instance = this;
        }

        public void Setup(GameConfig config, BoardManager board, HeroPool pool)
        {
            gameConfig = config;
            boardManager = board;
            heroPool = pool;
        }

        // ========== Shop Operations ==========

        public void RefreshShop(PlayerData player, bool free = false)
        {
            if (player == null) return;
            if (!free)
            {
                if (player.freeRefreshRemaining > 0)
                {
                    player.freeRefreshRemaining--;
                }
                else
                {
                    if (player.gold < gameConfig.refreshCost) return;
                    player.gold -= gameConfig.refreshCost;
                }
            }

            // Return current shop heroes to pool
            foreach (var hero in player.currentShop)
            {
                if (hero != null)
                    heroPool.ReturnHero(hero);
            }

            // Draw new heroes (base + augment bonus slots)
            int totalSlots = gameConfig.shopSlotCount + player.bonusShopSlots;
            while (player.currentShop.Count < totalSlots)
                player.currentShop.Add(null);
            for (int i = 0; i < totalSlots; i++)
            {
                player.currentShop[i] = heroPool.DrawHero(player.level);
            }
        }

        public bool BuyHero(PlayerData player, int shopIndex)
        {
            if (player == null) return false;

            // Round 1: no buying allowed
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.CurrentRound == 1) return false;

            if (shopIndex < 0 || shopIndex >= player.currentShop.Count) return false;

            var hero = player.currentShop[shopIndex];
            if (hero == null) return false;
            if (player.gold < hero.cost) return false;

            // Check bench space
            int benchSpace = GetAvailableBenchIndex(player);
            if (benchSpace < 0)
            {
                Debug.Log("Bench is full!");
                return false;
            }

            // Deduct gold
            player.gold -= hero.cost;

            // Create piece on bench
            var piece = boardManager.SpawnPiece(hero, player, 1);

            if (player.isHuman)
            {
                boardManager.PlacePieceOnBench(piece, benchSpace);
            }
            else
            {
                // For AI: don't occupy bench slot, just track in player.benchPieces
                piece.isOnBench = true;
                piece.benchIndex = benchSpace;
                piece.boardPosition = new Vector2Int(-1, benchSpace);
                piece.transform.position = new Vector3(0, -100, 0);
                player.benchPieces.Add(piece);
            }

            // Remove from shop
            player.currentShop[shopIndex] = null;

            // Check star merge
            StarMergeManager.Instance?.CheckAndMerge(player);

            Debug.Log($"Bought {hero.heroName} for {hero.cost} gold. Remaining: {player.gold}");
            return true;
        }

        public void SellPiece(ChessPiece piece, bool fromDrag = false)
        {
            if (piece == null || piece.owner == null) return;

            // Round 1: no selling allowed
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.CurrentRound == 1) return;

            if (!fromDrag)
            {
                var uiManager = Object.FindFirstObjectByType<UIManager>();
                if (uiManager != null && !uiManager.IsShopExpanded) return;
            }

            var player = piece.owner;
            int refund = piece.heroData.cost;
            if (piece.starLevel == 2) refund = piece.heroData.cost * 3;
            else if (piece.starLevel == 3) refund = piece.heroData.cost * 9;

            player.gold += refund;

            // Return to pool (2星=3张, 3星=9张)
            int returnCount = piece.starLevel == 3 ? 9 : (piece.starLevel == 2 ? 3 : 1);
            for (int i = 0; i < returnCount; i++)
                heroPool.ReturnHero(piece.heroData);

            // Remove from board/bench
            boardManager.RemovePieceFromAnywhere(piece);
            boardManager.RemoveFromTracking(piece);
            player.boardPieces.Remove(piece);
            player.benchPieces.Remove(piece);

            // Return equipment to owner
            if (EquipmentManager.Instance != null)
                EquipmentManager.Instance.ReturnEquipmentToOwner(piece);

            // Destroy
            if (piece.gameObject != null)
                Destroy(piece.gameObject);

            Debug.Log($"Sold {piece.heroData.heroName} (⭐{piece.starLevel}) for {refund} gold.");

            boardManager.RefreshFactionBonuses(player);

            var uiMgr = Object.FindFirstObjectByType<UIManager>();
            uiMgr?.UpdateUI();
        }

        public void ToggleShopLock(PlayerData player)
        {
            if (player == null) return;
            player.shopLocked = !player.shopLocked;
        }

        // ========== Helpers ==========

        int GetAvailableBenchIndex(PlayerData player)
        {
            bool[] occupied = new bool[gameConfig.benchSlots];
            if (player.isHuman)
            {
                var benchPieces = boardManager.GetPiecesOnBench();
                foreach (var p in benchPieces)
                {
                    if (p.owner == player && p.benchIndex >= 0 && p.benchIndex < occupied.Length)
                        occupied[p.benchIndex] = true;
                }
            }
            else
            {
                foreach (var p in player.benchPieces)
                {
                    if (p != null && p.benchIndex >= 0 && p.benchIndex < occupied.Length)
                        occupied[p.benchIndex] = true;
                }
            }
            for (int i = 0; i < occupied.Length; i++)
                if (!occupied[i]) return i;
            return -1;
        }

        public bool CanPlaceOnBoard(PlayerData player)
        {
            return player.GetCurrentBoardUnitCount() < player.GetMaxUnitsOnBoard();
        }
    }
}
