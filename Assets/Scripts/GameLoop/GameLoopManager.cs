using UnityEngine;
using System.Collections.Generic;
using AutoChess.Data;
using AutoChess.AI;
using AutoChess.UI;

namespace AutoChess.Core
{
    public partial class GameLoopManager : MonoBehaviour
    {
        public static GameLoopManager Instance { get; private set; }

        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private CombatManager combatManager;
        [SerializeField] private AIManager aiManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private HeroPool heroPool;
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private StarMergeManager starMergeManager;
        [SerializeField] private FactionManager factionManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private EquipmentManager equipmentManager;
        [SerializeField] private CreepManager creepManager;
        [SerializeField] private AugmentManager augmentManager;
        [SerializeField] private CarouselManager carouselManager;
        [SerializeField] private CombatStatsTracker combatStatsTracker;

        [Header("Data Assets")]
        [SerializeField] private List<HeroData> availableHeroes;
        [SerializeField] private List<FactionData> availableFactions;
        [SerializeField] private List<EquipmentData> availableEquipment;
        [SerializeField] private List<CreepRoundData> creepRoundData;
        [SerializeField] private List<AugmentData> availableAugments;

        public GamePhase CurrentPhase { get; private set; } = GamePhase.Preparation;
        public int CurrentRound { get; private set; } = 1;
        public float PhaseTimer { get; private set; } = 0f;
        public PlayerData HumanPlayer => allPlayers.Count > 0 ? allPlayers[0] : null;
        public List<PlayerData> AllPlayers => allPlayers;
        public List<PlayerData> EliminatedPlayers => eliminatedPlayers;
        public GameConfig Config => gameConfig;

        private List<PlayerData> allPlayers = new List<PlayerData>();
        private List<PlayerData> eliminatedPlayers = new List<PlayerData>();
        private Dictionary<PlayerData, PlayerBoard> playerBoards = new Dictionary<PlayerData, PlayerBoard>();
        private Coroutine phaseCoroutine;
        private bool isCreepRound = false;
        private CreepRoundData currentCreepData = null;
        private List<AugmentData> currentAugmentChoices = null;
        private List<CarouselItemData> currentCarouselItems = null;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            CurrentPhase = GamePhase.Title;
            uiManager?.ShowTitleScreen();
        }

        public void StartGame()
        {
            uiManager?.HideTitleScreen();
            InitializeGame();
        }

        void Update()
        {
            if (CurrentPhase == GamePhase.Combat && combatManager != null && combatManager.IsCombatActive)
            {
                uiManager?.UpdateTimer(combatManager.RemainingTime);
                if (combatManager.IsOvertime)
                    uiManager?.ShowPhase("加时赛", gameConfig.combatOvertimeDuration);
            }
        }

        void InitializeGame()
        {
            allPlayers.Clear();
            eliminatedPlayers.Clear();
            playerBoards.Clear();

            allPlayers.Add(new PlayerData("玩家", true, gameConfig));
            for (int i = 1; i <= 7; i++)
            {
                allPlayers.Add(new PlayerData($"AI_{i}", false, gameConfig));
            }

            heroPool.Initialize(availableHeroes, gameConfig);
            shopManager.Setup(gameConfig, boardManager, heroPool);
            starMergeManager.Setup(boardManager);
            factionManager.Setup(availableFactions);
            economyManager.Setup(gameConfig);
            if (equipmentManager != null)
                equipmentManager.Setup(availableEquipment);
            if (creepManager != null)
                creepManager.Setup(gameConfig, boardManager, availableEquipment, creepRoundData);
            if (augmentManager != null)
                augmentManager.Setup(availableAugments);
            if (carouselManager != null)
                carouselManager.Setup(boardManager, gameConfig);
            if (combatStatsTracker != null)
                combatStatsTracker.Clear();

            foreach (var player in allPlayers)
            {
                var freeHero = heroPool.DrawHeroByCost(1);
                if (freeHero != null)
                {
                    var piece = boardManager.SpawnPiece(freeHero, player, 1);
                    if (player.isHuman)
                    {
                        boardManager.PlacePieceOnBench(piece, 0);
                    }
                    else
                    {
                        piece.isOnBench = true;
                        piece.benchIndex = 0;
                        piece.boardPosition = new Vector2Int(-1, 0);
                        piece.transform.position = new Vector3(0, -100, 0);
                        player.benchPieces.Add(piece);
                    }
                }

                var pb = new PlayerBoard();
                pb.SaveFrom(boardManager, player);
                playerBoards[player] = pb;
            }

            foreach (var player in allPlayers)
            {
                if (player.isHuman) continue;
                aiManager?.PlaceAIPieces(player, playerBoards[player]);
            }

            uiManager?.UpdateUI();
            StartPreparationPhase();
        }

        void StartNextPhase()
        {
            if (System.Array.IndexOf(gameConfig.augmentRounds, CurrentRound) >= 0)
            {
                StartAugmentPhase();
            }
            else if (System.Array.IndexOf(gameConfig.carouselRounds, CurrentRound) >= 0)
            {
                StartCarouselPhase();
            }
            else
            {
                StartPreparationPhase();
            }
        }

        void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public void RestartGame()
        {
            if (phaseCoroutine != null)
            {
                StopCoroutine(phaseCoroutine);
                phaseCoroutine = null;
            }
            StopAllCoroutines();

            var allPieces = FindObjectsByType<ChessPiece>(FindObjectsSortMode.None);
            foreach (var p in allPieces) Destroy(p.gameObject);

            boardManager.ClearAllTracking();
            allPlayers.Clear();
            eliminatedPlayers.Clear();
            playerBoards.Clear();
            CurrentRound = 1;
            InitializeGame();
        }
    }
}
