using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class StarMergeManager : MonoBehaviour
    {
        public static StarMergeManager Instance { get; private set; }

        [SerializeField] private BoardManager boardManager;

        void Awake()
        {
            Instance = this;
        }

        public void Setup(BoardManager board)
        {
            boardManager = board;
        }

        public void CheckAndMerge(PlayerData player)
        {
            if (player == null) return;

            bool merged = true;
            while (merged)
            {
                merged = TryMergeOnce(player);
            }
        }

        bool TryMergeOnce(PlayerData player)
        {
            var allPieces = boardManager.GetPiecesByOwner(player);
            var grouped = new Dictionary<string, List<ChessPiece>>();

            foreach (var piece in allPieces)
            {
                if (piece == null || !piece.IsAlive) continue;
                if (piece.starLevel >= 3) continue; // Max star

                string key = $"{piece.heroData.heroName}_⭐{piece.starLevel}";
                if (!grouped.ContainsKey(key))
                    grouped[key] = new List<ChessPiece>();
                grouped[key].Add(piece);
            }

            foreach (var kvp in grouped)
            {
                if (kvp.Value.Count >= 3)
                {
                    MergeThree(player, kvp.Value);
                    return true;
                }
            }
            return false;
        }

        void MergeThree(PlayerData player, List<ChessPiece> pieces)
        {
            // Take first 3
            var toMerge = pieces.Take(3).ToList();
            if (toMerge.Count < 3) return;

            // Keep the one with the best position (board preferred over bench, then lowest index)
            ChessPiece keeper = toMerge[0];
            int bestPriority = GetPositionPriority(keeper);
            foreach (var p in toMerge.Skip(1))
            {
                int priority = GetPositionPriority(p);
                if (priority < bestPriority)
                {
                    bestPriority = priority;
                    keeper = p;
                }
            }

            // Collect equipment from consumed pieces before destroying them
            var extraEquipment = new List<EquipmentData>();
            foreach (var p in toMerge)
            {
                if (p == keeper) continue;
                var items = p.UnequipAll();
                extraEquipment.AddRange(items);
            }

            // Remove others
            foreach (var p in toMerge)
            {
                if (p == keeper) continue;
                boardManager.RemovePieceFromAnywhere(p);
                boardManager.RemoveFromTracking(p);
                player.boardPieces.Remove(p);
                player.benchPieces.Remove(p);
                if (p.gameObject != null)
                    Destroy(p.gameObject);
            }

            // Upgrade keeper
            keeper.starLevel++;
            keeper.Initialize(keeper.heroData, player, keeper.starLevel);

            // Transfer collected equipment to keeper (overflow goes to player inventory)
            foreach (var item in extraEquipment)
            {
                if (keeper.CanEquip)
                    keeper.Equip(item);
                else
                    player.equipmentInventory.Add(item);
            }

            // Re-place at original position (only for human player to avoid slot conflicts)
            if (player.isHuman)
            {
                if (keeper.isOnBench && keeper.benchIndex >= 0)
                {
                    boardManager.PlacePieceOnBench(keeper, keeper.benchIndex);
                }
                else if (keeper.boardPosition.x >= 0)
                {
                    boardManager.PlacePiece(keeper, keeper.boardPosition.x, keeper.boardPosition.y);
                }
            }
            else
            {
                // AI piece stays hidden
                keeper.transform.position = new Vector3(keeper.transform.position.x, -100, keeper.transform.position.z);
            }

            Debug.Log($"Merged 3 {keeper.heroData.heroName} into ⭐{keeper.starLevel}!");
        }

        int GetPositionPriority(ChessPiece piece)
        {
            // Lower number = better (keep this one)
            // Board pieces preferred over bench
            if (!piece.isOnBench) return piece.boardPosition.x * 100 + piece.boardPosition.y;
            return 10000 + piece.benchIndex;
        }
    }
}
