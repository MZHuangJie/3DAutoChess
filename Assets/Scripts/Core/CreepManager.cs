using System.Collections.Generic;
using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class CreepManager : MonoBehaviour
    {
        public static CreepManager Instance { get; private set; }

        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private List<CreepRoundData> creepRounds;
        [SerializeField] private List<EquipmentData> allEquipment;

        private PlayerData creepPlayer;
        private List<ChessPiece> activeCreeps = new List<ChessPiece>();

        void Awake()
        {
            Instance = this;
        }

        public void Setup(GameConfig config, BoardManager board, List<EquipmentData> equipment, List<CreepRoundData> rounds = null)
        {
            gameConfig = config;
            boardManager = board;
            allEquipment = equipment;
            if (rounds != null) creepRounds = rounds;
            creepPlayer = new PlayerData("野怪", false, config);
            creepPlayer.health = 9999;
        }

        public bool IsCreepRound(int round)
        {
            if (gameConfig.pveRounds == null) return false;
            foreach (int r in gameConfig.pveRounds)
                if (r == round) return true;
            return false;
        }

        public CreepRoundData GetCreepData(int round)
        {
            if (creepRounds == null) return null;
            int pveIndex = 0;
            for (int i = 0; i < gameConfig.pveRounds.Length; i++)
            {
                if (gameConfig.pveRounds[i] == round)
                {
                    pveIndex = i;
                    break;
                }
            }
            if (pveIndex < creepRounds.Count)
                return creepRounds[pveIndex];
            return creepRounds.Count > 0 ? creepRounds[creepRounds.Count - 1] : null;
        }

        public PlayerData GetCreepPlayer() => creepPlayer;

        public void SpawnCreeps(CreepRoundData data)
        {
            ClearCreeps();
            if (data == null || data.creeps == null) return;

            int playerRows = gameConfig.boardRows;
            int col = gameConfig.boardCols / 2;

            for (int i = 0; i < data.creeps.Length; i++)
            {
                var info = data.creeps[i];
                int row = playerRows + 1 + (i / gameConfig.boardCols);
                int c = col - data.creeps.Length / 2 + i;
                c = Mathf.Clamp(c, 0, gameConfig.boardCols - 1);

                var slot = boardManager.GetSlot(row, c);
                if (slot == null || slot.IsOccupied)
                {
                    for (int tryC = 0; tryC < gameConfig.boardCols; tryC++)
                    {
                        slot = boardManager.GetSlot(row, tryC);
                        if (slot != null && !slot.IsOccupied) break;
                    }
                }
                if (slot == null || slot.IsOccupied) continue;

                var creepPiece = SpawnCreepPiece(info, slot, row, c);
                if (creepPiece != null)
                    activeCreeps.Add(creepPiece);
            }

            Debug.Log($"[PvE] Spawned {activeCreeps.Count} creeps for round: {data.roundName}");
        }

        ChessPiece SpawnCreepPiece(CreepInfo info, BoardSlot slot, int row, int col)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = $"Creep_{info.creepName}";
            go.transform.position = slot.worldPos;
            go.transform.rotation = Quaternion.Euler(0, 180, 0);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(renderer.material);
                mat.color = info.color;
                renderer.material = mat;
            }

            var piece = go.AddComponent<ChessPiece>();
            piece.owner = creepPlayer;
            piece.starLevel = 1;
            piece.maxHealth = info.health;
            piece.currentHealth = info.health;
            piece.attackDamage = info.attackDamage;
            piece.armor = info.armor;
            piece.attackSpeed = info.attackSpeed;
            piece.attackRange = 1.5f;
            piece.magicResist = 10;
            piece.maxMana = 999;
            piece.currentMana = 0;
            piece.boardPosition = new Vector2Int(row, col);
            piece.state = PieceState.Idle;

            slot.piece = piece;
            creepPlayer.boardPieces.Add(piece);

            return piece;
        }

        public void GrantRewards(PlayerData player, CreepRoundData data)
        {
            if (data == null || player == null) return;

            player.gold += data.goldReward;

            for (int i = 0; i < data.equipmentDropCount; i++)
            {
                EquipmentData drop;
                if (data.dropCombinedEquipment)
                    drop = EquipmentManager.Instance?.GetRandomCombinedEquipment();
                else
                    drop = EquipmentManager.Instance?.GetRandomBaseEquipment();

                if (drop != null && player.equipmentInventory.Count < player.maxEquipmentInventory)
                {
                    player.equipmentInventory.Add(drop);
                    Debug.Log($"[PvE] {player.playerName} obtained equipment: {drop.equipmentName}");
                }
            }
        }

        public void ClearCreeps()
        {
            foreach (var creep in activeCreeps)
            {
                if (creep != null && creep.gameObject != null)
                {
                    var slot = boardManager.GetSlot(creep.boardPosition);
                    if (slot != null && slot.piece == creep)
                        slot.piece = null;
                    Destroy(creep.gameObject);
                }
            }
            activeCreeps.Clear();
            creepPlayer.boardPieces.Clear();
        }
    }
}
