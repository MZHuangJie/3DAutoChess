using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoChess.Data;
using AutoChess.AI;
using AutoChess.UI;

namespace AutoChess.Core
{
    public class GameLoopManager : MonoBehaviour
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
            // Create players: 1 human + 7 AI
            allPlayers.Clear();
            eliminatedPlayers.Clear();
            playerBoards.Clear();

            allPlayers.Add(new PlayerData("玩家", true, gameConfig));
            for (int i = 1; i <= 7; i++)
            {
                allPlayers.Add(new PlayerData($"AI_{i}", false, gameConfig));
            }

            // Initialize subsystems
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

            // Give each player a free random 1-cost hero (not from shop)
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

                // Save board state
                var pb = new PlayerBoard();
                pb.SaveFrom(boardManager, player);
                playerBoards[player] = pb;
            }

            // AI initial placement (on their own invisible boards)
            foreach (var player in allPlayers)
            {
                if (player.isHuman) continue;
                aiManager?.PlaceAIPieces(player, playerBoards[player]);
            }

            uiManager?.UpdateUI();
            StartPreparationPhase();
        }

        void StartPreparationPhase()
        {
            CurrentPhase = GamePhase.Preparation;
            PhaseTimer = CurrentRound == 1 ? 5f : gameConfig.preparationDuration;
            uiManager?.ShowPhase("准备阶段", PhaseTimer);

            bool isFirstRound = CurrentRound == 1;

            // Economy & Shop for all alive players
            foreach (var player in allPlayers)
            {
                if (!player.IsAlive) continue;

                // Grant income (skip round 1 — starting gold already given)
                if (!isFirstRound)
                    economyManager.GrantRoundIncome(player, CurrentRound);

                // Auto +2 exp per round (skip round 1)
                if (!isFirstRound)
                {
                    player.exp += 2;
                    player.CheckLevelUp(gameConfig);
                }

                // Augment per-round effects
                if (!isFirstRound)
                {
                    player.freeRefreshRemaining = player.freeRefreshPerRound;
                    if (augmentManager != null)
                        augmentManager.ApplyExpPerRound(player, gameConfig);
                }

                // Refresh shop (skip round 1 — no shop on first round)
                if (!isFirstRound && !player.shopLocked)
                {
                    shopManager.RefreshShop(player, true);
                }

                // AI decisions
                if (!player.isHuman)
                {
                    aiManager?.MakeDecisions(player, playerBoards.ContainsKey(player) ? playerBoards[player] : null);
                    // Save AI board after decisions
                    var pb = new PlayerBoard();
                    pb.SaveFrom(boardManager, player);
                    playerBoards[player] = pb;
                }
            }

            // Reset human pieces to their saved positions (restore health, not rebuild)
            var human = HumanPlayer;
            if (human != null && human.IsAlive)
            {
                ResetHumanPiecesForPreparation(human);
            }

            uiManager?.UpdateUI();

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            phaseCoroutine = StartCoroutine(PreparationTimer());
        }

        IEnumerator PreparationTimer()
        {
            while (PhaseTimer > 0)
            {
                PhaseTimer -= Time.deltaTime;
                uiManager?.UpdateTimer(PhaseTimer);
                yield return null;
            }
            StartCombatPhase();
        }

        void AutoFillBoard(PlayerData player)
        {
            int currentOnBoard = player.GetCurrentBoardUnitCount();
            int maxOnBoard = player.GetMaxUnitsOnBoard();
            int slotsToFill = maxOnBoard - currentOnBoard;
            if (slotsToFill <= 0 || player.benchPieces.Count == 0) return;

            var benchCopy = new List<ChessPiece>(player.benchPieces);
            // Shuffle for random selection
            for (int i = benchCopy.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (benchCopy[i], benchCopy[j]) = (benchCopy[j], benchCopy[i]);
            }

            int placed = 0;
            foreach (var piece in benchCopy)
            {
                if (placed >= slotsToFill) break;
                if (piece == null || !piece.IsAlive) continue;

                // Find empty player-side slot
                for (int r = 0; r < gameConfig.boardRows; r++)
                {
                    bool found = false;
                    for (int c = 0; c < gameConfig.boardCols; c++)
                    {
                        var slot = boardManager.GetSlot(r, c);
                        if (slot != null && !slot.IsOccupied)
                        {
                            boardManager.PlacePiece(piece, r, c);
                            placed++;
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }

            if (placed > 0)
                Debug.Log($"[AutoFill] {player.playerName}: placed {placed} pieces from bench to board");
        }

        void StartCombatPhase()
        {
            CurrentPhase = GamePhase.Combat;
            uiManager?.HidePieceDetail();
            uiManager?.ShowPhase("战斗阶段", gameConfig.combatMaxDuration);

            // Auto-fill board from bench for human player
            var human = HumanPlayer;
            if (human != null && human.IsAlive)
                AutoFillBoard(human);

            // Save all player boards before combat
            foreach (var player in allPlayers)
            {
                if (!player.IsAlive) continue;
                var pb = new PlayerBoard();
                pb.SaveFrom(boardManager, player);
                playerBoards[player] = pb;
                Debug.Log($"[Combat SaveFrom] {player.playerName}: board={pb.boardInfos.Count}, bench={pb.benchInfos.Count}");
            }

            // Human player's match
            isCreepRound = creepManager != null && creepManager.IsCreepRound(CurrentRound);
            currentCreepData = isCreepRound ? creepManager.GetCreepData(CurrentRound) : null;

            // Apply augment combat buffs to all alive players' board pieces
            if (augmentManager != null)
            {
                foreach (var player in allPlayers)
                {
                    if (!player.IsAlive) continue;
                    augmentManager.ApplyAugmentCombatBuffs(player.boardPieces, player);
                }
            }

            if (human != null && human.IsAlive)
            {
                // Check if human has any pieces on board
                bool hasBoardPieces = human.boardPieces.Count > 0;
                if (!hasBoardPieces)
                {
                    Debug.Log("[Combat] Human has no pieces on board — instant loss.");
                    SimulateAIVersusAI();
                    EndCombat(false);
                    return;
                }

                if (isCreepRound && currentCreepData != null)
                {
                    uiManager?.ShowMatchup(human.playerName, currentCreepData.roundName);
                    creepManager.SpawnCreeps(currentCreepData);
                    combatManager.StartCombat(human, creepManager.GetCreepPlayer());
                }
                else
                {
                    var opponent = SelectOpponent(human);
                    if (opponent != null && opponent.IsAlive)
                    {
                        uiManager?.ShowMatchup(human.playerName, opponent.playerName);
                        uiManager?.UpdateOpponentHealth(opponent);
                        LoadOpponentMirror(opponent);
                        combatManager.StartCombat(human, opponent);
                    }
                    else
                    {
                        EndCombat(true);
                        return;
                    }
                }
            }
            else
            {
                // Human dead, fast-forward AI battles
                SimulateAIVersusAI();
                CurrentPhase = GamePhase.Result;
                if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
                phaseCoroutine = StartCoroutine(ResultThenNextRound());
                return;
            }

            // AI vs AI simplified simulation (run in parallel)
            SimulateAIVersusAI();
        }

        PlayerData SelectOpponent(PlayerData player)
        {
            var candidates = new List<PlayerData>();
            foreach (var p in allPlayers)
            {
                if (p != player && p.IsAlive)
                    candidates.Add(p);
            }

            if (candidates.Count == 0) return null;
            if (candidates.Count == 1) return candidates[0];

            // Try not to pick last opponent
            if (player.lastOpponent != null && candidates.Contains(player.lastOpponent))
            {
                candidates.Remove(player.lastOpponent);
            }

            var picked = candidates[Random.Range(0, candidates.Count)];
            player.lastOpponent = picked;
            picked.lastOpponent = player;
            return picked;
        }

        void LoadOpponentMirror(PlayerData opponent)
        {
            // Clear enemy side (rows 4-7)
            int playerRows = gameConfig.boardRows;
            for (int r = playerRows; r < playerRows * 2; r++)
            {
                for (int c = 0; c < gameConfig.boardCols; c++)
                {
                    var slot = boardManager.GetSlot(r, c);
                    if (slot != null && slot.piece != null)
                    {
                        var piece = slot.piece;
                        if (piece.owner != null)
                            piece.owner.boardPieces.Remove(piece);
                        boardManager.RemoveFromTracking(piece);
                        Destroy(piece.gameObject);
                        slot.piece = null;
                    }
                }
            }

            // Load opponent's board as mirror (flip rows)
            if (!playerBoards.ContainsKey(opponent))
            {
                Debug.LogWarning($"[LoadOpponentMirror] No saved board for {opponent.playerName}");
                return;
            }

            var pb = playerBoards[opponent];
            Debug.Log($"[LoadOpponentMirror] {opponent.playerName}: {pb.boardInfos.Count} board pieces, {pb.benchInfos.Count} bench pieces");

            foreach (var info in pb.boardInfos)
            {
                if (info.heroData == null) continue;
                int mirroredRow = (playerRows * 2 - 1) - info.boardPos.x;
                int col = info.boardPos.y;
                var slot = boardManager.GetSlot(mirroredRow, col);
                if (slot == null)
                {
                    Debug.LogWarning($"[LoadOpponentMirror] Slot({mirroredRow},{col}) is null for {info.heroData.heroName}");
                    continue;
                }
                if (slot.IsOccupied)
                {
                    Debug.LogWarning($"[LoadOpponentMirror] Slot({mirroredRow},{col}) occupied by {slot.piece.heroData.heroName} for {info.heroData.heroName}");
                    continue;
                }

                var piece = boardManager.SpawnPiece(info.heroData, opponent, info.starLevel);
                bool placed = boardManager.PlacePiece(piece, mirroredRow, col);
                if (!placed)
                    Debug.LogWarning($"[LoadOpponentMirror] PlacePiece failed for {info.heroData.heroName} at ({mirroredRow},{col})");
                piece.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }

        void SimulateAIVersusAI()
        {
            var aliveAIs = new List<PlayerData>();
            foreach (var p in allPlayers)
            {
                if (!p.isHuman && p.IsAlive)
                    aliveAIs.Add(p);
            }

            if (aliveAIs.Count <= 1) return;

            // Pair up randomly
            ShuffleList(aliveAIs);
            for (int i = 0; i < aliveAIs.Count - 1; i += 2)
            {
                SimulateMatch(aliveAIs[i], aliveAIs[i + 1]);
            }

            // Odd one out gets a bye (small heal)
            if (aliveAIs.Count % 2 == 1)
            {
                var last = aliveAIs[aliveAIs.Count - 1];
                last.winStreak++;
                last.loseStreak = 0;
                last.health = Mathf.Min(gameConfig.startingHealth, last.health + 1);
            }
        }

        void SimulateMatch(PlayerData p1, PlayerData p2)
        {
            float power1 = CalculatePower(p1);
            float power2 = CalculatePower(p2);

            bool p1Wins = power1 >= power2;
            var winner = p1Wins ? p1 : p2;
            var loser = p1Wins ? p2 : p1;

            int damage = 2 + Random.Range(1, 4);
            loser.health -= damage;
            if (loser.health < 0) loser.health = 0;

            economyManager.UpdateStreaks(winner, loser);

            Debug.Log($"[AI Sim] {p1.playerName}({power1:F0}) vs {p2.playerName}({power2:F0}) -> {winner.playerName} wins, {loser.playerName} takes {damage} dmg");
        }

        float CalculatePower(PlayerData player)
        {
            if (!playerBoards.ContainsKey(player)) return 0;
            var pb = playerBoards[player];
            float power = 0;
            foreach (var info in pb.boardInfos)
            {
                float starMult = info.starLevel == 2 ? 1.8f : (info.starLevel == 3 ? 3.6f : 1f);
                power += info.heroData.maxHealth * info.heroData.attackDamage * starMult * 0.01f;
            }
            power *= Random.Range(0.85f, 1.15f);
            return power;
        }

        public void EndCombat(bool attackerWon)
        {
            CurrentPhase = GamePhase.Result;

            var human = HumanPlayer;

            if (isCreepRound)
            {
                // PvE: grant rewards, no damage on loss (or minimal)
                if (attackerWon && human != null)
                {
                    creepManager?.GrantRewards(human, currentCreepData);
                    // Also grant rewards to AI players (simplified)
                    foreach (var p in allPlayers)
                    {
                        if (!p.isHuman && p.IsAlive)
                            creepManager?.GrantRewards(p, currentCreepData);
                    }
                }
                else if (!attackerWon && human != null)
                {
                    int pveDmg = gameConfig.pveLossDamage;
                    if (pveDmg > 0)
                    {
                        human.health -= pveDmg;
                        if (human.health < 0) human.health = 0;
                    }
                }

                uiManager?.ShowCombatResult(attackerWon, 0);
                uiManager?.UpdateUI();

                combatStatsTracker?.RecordResult(CurrentRound, currentCreepData?.roundName ?? "野怪", attackerWon, 0, true);
                creepManager?.ClearCreeps();
                isCreepRound = false;
                currentCreepData = null;
            }
            else
            {
                var opponent = human?.lastOpponent;
                int damage = 0;
                bool draw = combatManager != null && combatManager.IsDraw;

                if (human != null && opponent != null)
                {
                    if (draw)
                    {
                        // Both sides take damage
                        int humanStars = combatManager.GetAliveStarCount(human);
                        int opponentStars = combatManager.GetAliveStarCount(opponent);
                        int humanDmg = 2 + opponentStars;
                        int opponentDmg = 2 + humanStars;
                        human.health -= humanDmg;
                        opponent.health -= opponentDmg;
                        if (human.health < 0) human.health = 0;
                        if (opponent.health < 0) opponent.health = 0;
                        damage = humanDmg;
                        Debug.Log($"Draw! {human.playerName} takes {humanDmg} dmg, {opponent.playerName} takes {opponentDmg} dmg");
                    }
                    else
                    {
                        var winner = attackerWon ? human : opponent;
                        var loser = attackerWon ? opponent : human;

                        int aliveStars = combatManager.GetAliveStarCount(winner);
                        damage = 2 + aliveStars;
                        loser.health -= damage;
                        if (loser.health < 0) loser.health = 0;

                        economyManager.UpdateStreaks(winner, loser);
                    }
                }

                uiManager?.ShowCombatResult(draw ? true : attackerWon, damage);
                uiManager?.UpdateUI();

                combatStatsTracker?.RecordResult(CurrentRound, opponent?.playerName ?? "未知", draw || attackerWon, damage);

                // Clear opponent mirror pieces from board
                ClearEnemySide();
            }

            // Clear faction bonuses
            var allPieces = boardManager.GetAllPiecesOnBoard();
            factionManager.ClearFactionsFromPieces(allPieces);

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            phaseCoroutine = StartCoroutine(ResultThenNextRound());
        }

        IEnumerator ResultThenNextRound()
        {
            yield return new WaitForSeconds(gameConfig.resultDuration);

            // Check eliminations
            foreach (var player in allPlayers)
            {
                if (player.IsAlive || eliminatedPlayers.Contains(player)) continue;
                eliminatedPlayers.Insert(0, player);
                player.placement = allPlayers.Count - eliminatedPlayers.Count + 1;
                Debug.Log($"{player.playerName} 被淘汰! 排名: {player.placement}");
            }

            // Check game over
            int aliveCount = 0;
            PlayerData lastAlive = null;
            foreach (var player in allPlayers)
            {
                if (player.IsAlive)
                {
                    aliveCount++;
                    lastAlive = player;
                }
            }

            var human = HumanPlayer;
            bool humanAlive = human != null && human.IsAlive;

            if (aliveCount <= 1 || !humanAlive)
            {
                CurrentPhase = GamePhase.GameOver;
                if (lastAlive != null && lastAlive.placement == 0)
                    lastAlive.placement = 1;

                // Ensure all eliminated have placement
                for (int i = 0; i < eliminatedPlayers.Count; i++)
                {
                    if (eliminatedPlayers[i].placement == 0)
                        eliminatedPlayers[i].placement = allPlayers.Count - i;
                }

                uiManager?.ShowGameOver(humanAlive, eliminatedPlayers);
                yield break;
            }

            CurrentRound++;

            // Cleanup dead players' pieces from scene
            CleanupDeadPlayers();

            StartNextPhase();
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

        void StartAugmentPhase()
        {
            CurrentPhase = GamePhase.AugmentSelect;
            PhaseTimer = gameConfig.augmentSelectDuration;

            int tier = augmentManager != null ? augmentManager.GetTierForRound(CurrentRound, gameConfig.augmentRounds) : 1;
            currentAugmentChoices = augmentManager?.GenerateChoices(tier);

            // AI players auto-pick
            foreach (var player in allPlayers)
            {
                if (player.isHuman || !player.IsAlive) continue;
                var aiChoices = augmentManager?.GenerateChoices(tier);
                if (aiChoices != null && aiChoices.Count > 0)
                {
                    var pick = augmentManager.AIChoose(aiChoices);
                    augmentManager.ApplyAugment(player, pick);
                }
            }

            uiManager?.ShowPhase("海克斯选择", PhaseTimer);
            uiManager?.ShowAugmentSelection(currentAugmentChoices);

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            phaseCoroutine = StartCoroutine(AugmentTimer(currentAugmentChoices));
        }

        IEnumerator AugmentTimer(List<AugmentData> choices)
        {
            while (PhaseTimer > 0)
            {
                PhaseTimer -= Time.deltaTime;
                uiManager?.UpdateTimer(PhaseTimer);
                yield return null;
            }
            // Timeout: auto-pick first choice for human
            if (choices != null && choices.Count > 0)
                OnAugmentSelected(0);
        }

        public void OnAugmentSelected(int index)
        {
            if (CurrentPhase != GamePhase.AugmentSelect) return;

            var human = HumanPlayer;
            if (human != null && human.IsAlive && currentAugmentChoices != null && index >= 0 && index < currentAugmentChoices.Count)
            {
                augmentManager?.ApplyAugment(human, currentAugmentChoices[index]);
            }

            currentAugmentChoices = null;
            uiManager?.HideAugmentSelection();

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            StartPreparationPhase();
        }

        void StartCarouselPhase()
        {
            CurrentPhase = GamePhase.Carousel;
            PhaseTimer = gameConfig.carouselSelectDuration;

            var heroes = availableHeroes.ToArray();
            var baseEquips = availableEquipment.FindAll(e => e.equipmentType == EquipmentType.Base).ToArray();
            currentCarouselItems = carouselManager?.GenerateItems(heroes, baseEquips);

            // AI players pick in order (lowest health first)
            var pickOrder = carouselManager?.GetPickOrder(allPlayers);
            if (pickOrder != null && currentCarouselItems != null)
            {
                foreach (var player in pickOrder)
                {
                    if (player.isHuman) continue;
                    int idx = carouselManager.AIChoose(currentCarouselItems);
                    if (idx >= 0) carouselManager.PickItem(player, idx);
                }
            }

            uiManager?.ShowPhase("选秀轮", PhaseTimer);
            uiManager?.ShowCarouselSelection(currentCarouselItems);

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            phaseCoroutine = StartCoroutine(CarouselTimer(currentCarouselItems));
        }

        IEnumerator CarouselTimer(List<CarouselItemData> items)
        {
            while (PhaseTimer > 0)
            {
                PhaseTimer -= Time.deltaTime;
                uiManager?.UpdateTimer(PhaseTimer);
                yield return null;
            }
            // Timeout: auto-pick first available for human
            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (!items[i].picked)
                    {
                        OnCarouselSelected(i);
                        break;
                    }
                }
            }
        }

        public void OnCarouselSelected(int index)
        {
            if (CurrentPhase != GamePhase.Carousel) return;

            var human = HumanPlayer;
            if (human != null && human.IsAlive)
                carouselManager?.PickItem(human, index);

            uiManager?.HideCarouselSelection();

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            StartPreparationPhase();
        }

        void ClearEnemySide()
        {
            int playerRows = gameConfig.boardRows;
            for (int r = playerRows; r < playerRows * 2; r++)
            {
                for (int c = 0; c < gameConfig.boardCols; c++)
                {
                    var slot = boardManager.GetSlot(r, c);
                    if (slot != null && slot.piece != null)
                    {
                        var piece = slot.piece;
                        if (piece.owner != null)
                            piece.owner.boardPieces.Remove(piece);
                        boardManager.RemoveFromTracking(piece);
                        Destroy(piece.gameObject);
                        slot.piece = null;
                    }
                }
            }
        }

        void ResetHumanPiecesForPreparation(PlayerData human)
        {
            // Reset board pieces to their slot positions with full health
            foreach (var piece in human.boardPieces)
            {
                if (piece == null) continue;
                var slot = boardManager.GetSlot(piece.boardPosition);
                if (slot != null)
                {
                    piece.ResetForPreparation(slot.worldPos);
                }
                else
                {
                    piece.ResetForPreparation(piece.transform.position);
                }
            }
            // Reset bench pieces
            foreach (var piece in human.benchPieces)
            {
                if (piece == null) continue;
                piece.gameObject.SetActive(true);
                piece.state = PieceState.Idle;
            }
        }

        void CleanupDeadPlayers()
        {
            foreach (var player in allPlayers)
            {
                if (player.IsAlive) continue;
                var pieces = boardManager.GetPiecesByOwner(player, false);
                foreach (var p in pieces)
                {
                    if (p != null && p.gameObject != null)
                    {
                        boardManager.RemovePieceFromAnywhere(p);
                        boardManager.RemoveFromTracking(p);
                        Destroy(p.gameObject);
                    }
                }
                player.boardPieces.Clear();
                player.benchPieces.Clear();
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
