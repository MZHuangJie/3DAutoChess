using System.Collections.Generic;
using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class CarouselManager : MonoBehaviour
    {
        public static CarouselManager Instance { get; private set; }

        [SerializeField] private BoardManager boardManager;
        [SerializeField] private GameConfig gameConfig;

        private List<CarouselItemData> currentItems = new List<CarouselItemData>();

        void Awake()
        {
            Instance = this;
        }

        public void Setup(BoardManager board, GameConfig config)
        {
            boardManager = board;
            gameConfig = config;
        }

        public List<CarouselItemData> GenerateItems(HeroData[] heroes, EquipmentData[] baseEquipments)
        {
            currentItems.Clear();
            int count = 8;
            for (int i = 0; i < count; i++)
            {
                var item = new CarouselItemData
                {
                    heroData = heroes[Random.Range(0, heroes.Length)],
                    equipmentData = baseEquipments.Length > 0 ? baseEquipments[Random.Range(0, baseEquipments.Length)] : null,
                    picked = false
                };
                currentItems.Add(item);
            }
            return currentItems;
        }

        public List<PlayerData> GetPickOrder(List<PlayerData> players)
        {
            var alive = players.FindAll(p => p.IsAlive);
            alive.Sort((a, b) => a.health.CompareTo(b.health));
            return alive;
        }

        public bool PickItem(PlayerData player, int index)
        {
            if (index < 0 || index >= currentItems.Count) return false;
            var item = currentItems[index];
            if (item.picked) return false;

            item.picked = true;
            item.pickedByName = player.playerName;

            int benchIdx = GetAvailableBenchIndex(player);
            if (benchIdx >= 0)
            {
                var piece = boardManager.SpawnPiece(item.heroData, player, 1);
                if (player.isHuman)
                {
                    boardManager.PlacePieceOnBench(piece, benchIdx);
                }
                else
                {
                    piece.isOnBench = true;
                    piece.benchIndex = benchIdx;
                    piece.boardPosition = new Vector2Int(-1, benchIdx);
                    piece.transform.position = new Vector3(0, -100, 0);
                    player.benchPieces.Add(piece);
                }

                if (item.equipmentData != null)
                {
                    if (player.equipmentInventory.Count < player.maxEquipmentInventory)
                        player.equipmentInventory.Add(item.equipmentData);
                }

                StarMergeManager.Instance?.CheckAndMerge(player);
                Debug.Log($"[Carousel] {player.playerName} picked {item.heroData.heroName} + {item.equipmentData?.equipmentName ?? "无装备"}");
                return true;
            }

            Debug.Log($"[Carousel] {player.playerName} bench full, cannot pick");
            return false;
        }

        public int AIChoose(List<CarouselItemData> items)
        {
            var available = new List<int>();
            for (int i = 0; i < items.Count; i++)
                if (!items[i].picked) available.Add(i);
            if (available.Count == 0) return -1;
            return available[Random.Range(0, available.Count)];
        }

        int GetAvailableBenchIndex(PlayerData player)
        {
            int totalSlots = gameConfig.benchSlots + player.bonusBenchSlots;
            bool[] occupied = new bool[totalSlots];
            foreach (var p in player.benchPieces)
            {
                if (p != null && p.benchIndex >= 0 && p.benchIndex < totalSlots)
                    occupied[p.benchIndex] = true;
            }
            if (player.isHuman)
            {
                var benchPieces = boardManager.GetPiecesOnBench();
                foreach (var p in benchPieces)
                {
                    if (p.owner == player && p.benchIndex >= 0 && p.benchIndex < totalSlots)
                        occupied[p.benchIndex] = true;
                }
            }
            for (int i = 0; i < totalSlots; i++)
                if (!occupied[i]) return i;
            return -1;
        }
    }
}
