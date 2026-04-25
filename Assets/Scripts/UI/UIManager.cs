using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using AutoChess.Core;
using AutoChess.Data;
using System.Linq;

namespace AutoChess.UI
{
    public class UIManager : MonoBehaviour
    {
        // Existing UI elements
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI playerHealthText;
        [SerializeField] private TextMeshProUGUI aiHealthText;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;

        // New Milestone 2 UI elements
        private TextMeshProUGUI goldText;
        private TextMeshProUGUI populationText;
        private TextMeshProUGUI expText;
        private TextMeshProUGUI streakText;
        private TextMeshProUGUI interestText;
        private GameObject shopPanel;
        private List<GameObject> shopSlots = new List<GameObject>();
        private Button refreshButton;
        private Button lockButton;
        private Button upgradeButton;
        private GameObject factionPanel;
        private GameObject playerListPanel;
        private List<TextMeshProUGUI> playerListTexts = new List<TextMeshProUGUI>();
        private TextMeshProUGUI matchupText;

        // Equipment UI
        private GameObject equipmentPanel;
        private List<GameObject> equipmentSlots = new List<GameObject>();
        private TextMeshProUGUI equipmentTitleText;

        private Canvas canvas;
        private bool initialized = false;

        void Start()
        {
            EnsureCanvas();
            CreateMissingUI();
            initialized = true;

            if (resultText != null) resultText.gameObject.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (matchupText != null) matchupText.gameObject.SetActive(false);
        }

        void EnsureCanvas()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameObject.AddComponent<CanvasScaler>();
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        void CreateMissingUI()
        {
            // Gold text (top right)
            if (goldText == null)
            {
                var go = CreateTextGO("GoldText", "💰 5", 28, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-20, -30));
                goldText = go.GetComponent<TextMeshProUGUI>();
                goldText.color = new Color(1f, 0.85f, 0.2f);
            }

            // Population text (below gold)
            if (populationText == null)
            {
                var go = CreateTextGO("PopText", "人口: 1/3", 22, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-20, -65));
                populationText = go.GetComponent<TextMeshProUGUI>();
            }

            // EXP text (below population)
            if (expText == null)
            {
                var go = CreateTextGO("ExpText", "经验: 0/2", 18, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-20, -95));
                expText = go.GetComponent<TextMeshProUGUI>();
                expText.color = new Color(0.6f, 0.8f, 1f);
            }

            // Interest text
            if (interestText == null)
            {
                var go = CreateTextGO("InterestText", "利息: +0", 18, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-20, -120));
                interestText = go.GetComponent<TextMeshProUGUI>();
                interestText.color = new Color(0.8f, 0.9f, 0.5f);
            }

            // Streak text
            if (streakText == null)
            {
                var go = CreateTextGO("StreakText", "", 20, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-20, -150));
                streakText = go.GetComponent<TextMeshProUGUI>();
            }

            // Matchup banner (center top)
            if (matchupText == null)
            {
                var go = CreateTextGO("MatchupText", "", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -120));
                matchupText = go.GetComponent<TextMeshProUGUI>();
                matchupText.fontStyle = FontStyles.Bold;
            }

            // Shop panel (bottom)
            CreateShopPanel();

            // Faction panel (right side)
            CreateFactionPanel();

            // Player list (top left, below round)
            CreatePlayerListPanel();

            // Equipment inventory panel (left side)
            CreateEquipmentPanel();
        }

        void CreateShopPanel()
        {
            if (shopPanel != null) return;

            shopPanel = new GameObject("ShopPanel");
            shopPanel.transform.SetParent(transform);
            var shopRt = shopPanel.AddComponent<RectTransform>();
            shopRt.anchorMin = new Vector2(0, 0);
            shopRt.anchorMax = new Vector2(1, 0);
            shopRt.pivot = new Vector2(0.5f, 0);
            shopRt.sizeDelta = new Vector2(0, 140);
            shopRt.anchoredPosition = Vector2.zero;

            var shopBg = shopPanel.AddComponent<Image>();
            shopBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // 5 Shop slots
            float slotWidth = 120;
            float slotHeight = 100;
            float startX = -((5 * slotWidth) + (4 * 10)) / 2f + slotWidth / 2f;

            for (int i = 0; i < 5; i++)
            {
                var slot = new GameObject($"ShopSlot_{i}");
                slot.transform.SetParent(shopPanel.transform);
                var rt = slot.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(slotWidth, slotHeight);
                rt.anchoredPosition = new Vector2(startX + i * (slotWidth + 10), 10);

                var img = slot.AddComponent<Image>();
                img.color = new Color(0.2f, 0.2f, 0.25f, 1f);

                // Hero name text
                var nameGO = new GameObject("Name");
                nameGO.transform.SetParent(slot.transform);
                var nameRt = nameGO.AddComponent<RectTransform>();
                nameRt.anchorMin = Vector2.zero;
                nameRt.anchorMax = Vector2.one;
                nameRt.sizeDelta = Vector2.zero;
                nameRt.anchoredPosition = Vector2.zero;
                var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
                nameTmp.text = "";
                nameTmp.fontSize = 16;
                nameTmp.alignment = TextAlignmentOptions.Center;
                nameTmp.color = Color.white;

                // Cost text
                var costGO = new GameObject("Cost");
                costGO.transform.SetParent(slot.transform);
                var costRt = costGO.AddComponent<RectTransform>();
                costRt.anchorMin = new Vector2(0, 1);
                costRt.anchorMax = new Vector2(1, 1);
                costRt.sizeDelta = new Vector2(0, 24);
                costRt.anchoredPosition = new Vector2(0, -12);
                var costTmp = costGO.AddComponent<TextMeshProUGUI>();
                costTmp.text = "";
                costTmp.fontSize = 14;
                costTmp.alignment = TextAlignmentOptions.Center;
                costTmp.color = new Color(1f, 0.85f, 0.2f);

                // Button for buying
                var btn = slot.AddComponent<Button>();
                int index = i;
                btn.onClick.AddListener(() => OnShopSlotClicked(index));

                shopSlots.Add(slot);
            }

            // Refresh button
            refreshButton = CreateShopButton("刷新 (2金)", new Vector2(340, -40), OnRefreshClicked);
            refreshButton.transform.SetParent(shopPanel.transform);

            // Lock button
            lockButton = CreateShopButton("锁定", new Vector2(340, -75), OnLockClicked);
            lockButton.transform.SetParent(shopPanel.transform);

            // Upgrade button
            upgradeButton = CreateShopButton("升级 (4金)", new Vector2(-340, -40), OnUpgradeClicked);
            upgradeButton.transform.SetParent(shopPanel.transform);
        }

        Button CreateShopButton(string label, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            var btnGO = new GameObject(label.Replace(" ", ""));
            btnGO.transform.SetParent(shopPanel.transform);
            var rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(110, 32);
            rt.anchoredPosition = pos;

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.25f, 0.35f, 0.45f);

            var btn = btnGO.AddComponent<Button>();
            btn.onClick.AddListener(action);

            var txtGO = new GameObject("Text");
            txtGO.transform.SetParent(btnGO.transform);
            var txtRt = txtGO.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;
            var txt = txtGO.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = 14;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;

            return btn;
        }

        void CreateFactionPanel()
        {
            if (factionPanel != null) return;

            factionPanel = new GameObject("FactionPanel");
            factionPanel.transform.SetParent(transform);
            var rt = factionPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(160, 300);
            rt.anchoredPosition = new Vector2(-10, 0);
        }

        void CreatePlayerListPanel()
        {
            if (playerListPanel != null) return;

            playerListPanel = new GameObject("PlayerListPanel");
            playerListPanel.transform.SetParent(transform);
            var rt = playerListPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(180, 220);
            rt.anchoredPosition = new Vector2(10, -70);

            for (int i = 0; i < 8; i++)
            {
                var go = CreateTextGO($"PlayerList_{i}", "", 16, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(10, -25 * i));
                go.transform.SetParent(playerListPanel.transform);
                var tmp = go.GetComponent<TextMeshProUGUI>();
                playerListTexts.Add(tmp);
            }
        }

        void CreateEquipmentPanel()
        {
            if (equipmentPanel != null) return;

            equipmentPanel = new GameObject("EquipmentPanel");
            equipmentPanel.transform.SetParent(transform, false);
            var rt = equipmentPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(200, 340);
            rt.anchoredPosition = new Vector2(10, 150);

            var bg = equipmentPanel.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.1f, 0.18f, 0.85f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(equipmentPanel.transform, false);
            var titleRt = titleGO.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.sizeDelta = new Vector2(0, 28);
            titleRt.anchoredPosition = new Vector2(0, 0);
            equipmentTitleText = titleGO.AddComponent<TextMeshProUGUI>();
            equipmentTitleText.text = "装备背包";
            equipmentTitleText.fontSize = 16;
            equipmentTitleText.alignment = TextAlignmentOptions.Center;
            equipmentTitleText.color = new Color(1f, 0.8f, 0.4f);

            for (int i = 0; i < 10; i++)
            {
                var slot = new GameObject($"EqSlot_{i}");
                slot.transform.SetParent(equipmentPanel.transform, false);
                var slotRt = slot.AddComponent<RectTransform>();
                int col = i % 2;
                int row = i / 2;
                slotRt.anchorMin = new Vector2(0, 1);
                slotRt.anchorMax = new Vector2(0, 1);
                slotRt.pivot = new Vector2(0, 1);
                slotRt.sizeDelta = new Vector2(90, 50);
                slotRt.anchoredPosition = new Vector2(5 + col * 95, -32 - row * 55);

                var slotImg = slot.AddComponent<Image>();
                slotImg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

                var nameGO = new GameObject("Name");
                nameGO.transform.SetParent(slot.transform, false);
                var nameRt = nameGO.AddComponent<RectTransform>();
                nameRt.anchorMin = Vector2.zero;
                nameRt.anchorMax = Vector2.one;
                nameRt.pivot = new Vector2(0.5f, 0.5f);
                nameRt.sizeDelta = Vector2.zero;
                nameRt.anchoredPosition = Vector2.zero;
                var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
                nameTmp.text = "";
                nameTmp.fontSize = 12;
                nameTmp.alignment = TextAlignmentOptions.Center;
                nameTmp.color = Color.white;

                var btn = slot.AddComponent<Button>();
                int index = i;
                btn.onClick.AddListener(() => OnEquipmentSlotClicked(index));

                equipmentSlots.Add(slot);
            }
        }

        GameObject CreateTextGO(string name, string text, int fontSize, TextAnchor anchor, Vector2 anchorMinMax, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMinMax;
            rt.anchorMax = anchorMinMax;
            rt.sizeDelta = new Vector2(300, 30);
            rt.anchoredPosition = anchoredPos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = GetTMPAlignment(anchor);
            tmp.color = Color.white;
            return go;
        }

        static TextAlignmentOptions GetTMPAlignment(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.MiddleRight => TextAlignmentOptions.Right,
                _ => TextAlignmentOptions.Center,
            };
        }

        // ========== UI Updates ==========

        public void ShowPhase(string phaseName, float timer)
        {
            if (phaseText != null)
                phaseText.text = phaseName;
            UpdateTimer(timer);

            if (resultText != null) resultText.gameObject.SetActive(false);
            if (matchupText != null) matchupText.gameObject.SetActive(false);
        }

        public void UpdateTimer(float timeRemaining)
        {
            if (timerText != null)
                timerText.text = Mathf.Max(0, timeRemaining).ToString("F1") + "s";
        }

        public void UpdateUI()
        {
            if (GameLoopManager.Instance == null) return;

            var human = GameLoopManager.Instance.HumanPlayer;
            if (human == null) return;
            var config = GameLoopManager.Instance.Config;

            if (roundText != null)
                roundText.text = $"回合: {GameLoopManager.Instance.CurrentRound}";

            // Health
            if (playerHealthText != null)
                playerHealthText.text = $"玩家: {human.health} HP";

            // Opponent health (show last opponent if any)
            if (aiHealthText != null)
            {
                var opponent = human.lastOpponent;
                if (opponent != null && opponent.IsAlive)
                    aiHealthText.text = $"对手: {opponent.health} HP";
                else
                    aiHealthText.text = "";
            }

            // Gold
            if (goldText != null)
                goldText.text = $"💰 {human.gold}";

            // Population
            if (populationText != null)
            {
                int current = human.GetCurrentBoardUnitCount();
                int max = human.GetMaxUnitsOnBoard();
                populationText.text = $"人口: {current}/{max}";
                populationText.color = current >= max ? Color.red : Color.white;
            }

            // EXP
            if (expText != null)
            {
                if (human.level >= 10)
                    expText.text = "等级: MAX";
                else
                    expText.text = $"经验: {human.exp}/{human.expToLevel[human.level]}";
            }

            // Interest
            if (interestText != null)
            {
                int interest = config != null ? human.GetInterest(config) : Mathf.Min(human.gold / 10, 5);
                interestText.text = $"利息: +{interest}";
            }

            // Streak
            if (streakText != null)
            {
                if (human.winStreak >= 2)
                {
                    streakText.text = $"🔥 连胜 x{human.winStreak}";
                    streakText.color = new Color(1f, 0.6f, 0.2f);
                }
                else if (human.loseStreak >= 2)
                {
                    streakText.text = $"❄️ 连败 x{human.loseStreak}";
                    streakText.color = new Color(0.4f, 0.7f, 1f);
                }
                else
                {
                    streakText.text = "";
                }
            }

            // Shop
            UpdateShopUI(human, config);

            // Factions
            UpdateFactionUI(human);

            // Equipment inventory
            UpdateEquipmentUI(human);

            // Player list
            UpdatePlayerList();
        }

        void UpdateShopUI(PlayerData human, GameConfig config)
        {
            if (shopPanel != null)
            {
                bool showShop = GameLoopManager.Instance == null || GameLoopManager.Instance.CurrentRound > 1;
                shopPanel.SetActive(showShop);
            }

            if (shopSlots == null || shopSlots.Count == 0) return;

            for (int i = 0; i < shopSlots.Count; i++)
            {
                var slot = shopSlots[i];
                var hero = i < human.currentShop.Count ? human.currentShop[i] : null;

                var nameTmp = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                var costTmp = slot.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
                var img = slot.GetComponent<Image>();

                if (hero != null)
                {
                    if (nameTmp != null) nameTmp.text = hero.heroName;
                    if (costTmp != null) costTmp.text = $"{hero.cost} 💰";
                    img.color = hero.displayColor;
                }
                else
                {
                    if (nameTmp != null) nameTmp.text = "";
                    if (costTmp != null) costTmp.text = "";
                    img.color = new Color(0.2f, 0.2f, 0.25f, 1f);
                }
            }

            // Update button colors based on affordability
            if (refreshButton != null)
            {
                refreshButton.GetComponent<Image>().color = human.gold >= 2 ? new Color(0.25f, 0.45f, 0.35f) : new Color(0.35f, 0.25f, 0.25f);
            }
            if (upgradeButton != null)
            {
                upgradeButton.GetComponent<Image>().color = config != null && human.CanUpgradeLevel(config)
                    ? new Color(0.25f, 0.35f, 0.55f) : new Color(0.35f, 0.25f, 0.25f);
            }
            if (lockButton != null)
            {
                lockButton.GetComponentInChildren<TextMeshProUGUI>().text = human.shopLocked ? "🔒 已锁定" : "锁定";
                lockButton.GetComponent<Image>().color = human.shopLocked ? new Color(0.6f, 0.4f, 0.2f) : new Color(0.25f, 0.35f, 0.45f);
            }
        }

        void UpdateFactionUI(PlayerData human)
        {
            if (factionPanel == null) return;

            foreach (Transform child in factionPanel.transform)
                Destroy(child.gameObject);

            var pieces = BoardManager.Instance?.GetPiecesByOwner(human);
            if (pieces == null) return;

            var boardPieces = new System.Collections.Generic.List<ChessPiece>();
            foreach (var p in pieces)
            {
                if (p != null && !p.isOnBench)
                    boardPieces.Add(p);
            }
            if (boardPieces.Count == 0) return;

            var factionCounts = new Dictionary<string, int>();
            foreach (var piece in boardPieces)
            {
                if (piece.heroData?.factions == null) continue;
                foreach (var f in piece.heroData.factions)
                {
                    if (!factionCounts.ContainsKey(f)) factionCounts[f] = 0;
                    factionCounts[f]++;
                }
            }

            if (factionCounts.Count == 0) return;

            var allFactions = FactionManager.Instance?.AllFactions;
            int yOffset = 0;
            foreach (var kvp in factionCounts)
            {
                int count = kvp.Value;
                int nextThreshold = 0;
                bool active = false;

                if (allFactions != null)
                {
                    foreach (var fd in allFactions)
                    {
                        if (fd == null || fd.factionName != kvp.Key) continue;
                        if (fd.thresholds != null && fd.thresholds.Length > 0)
                        {
                            foreach (var t in fd.thresholds)
                            {
                                if (count >= t.count) active = true;
                                if (t.count > count && (nextThreshold == 0 || t.count < nextThreshold))
                                    nextThreshold = t.count;
                            }
                            if (nextThreshold == 0)
                                nextThreshold = fd.thresholds[fd.thresholds.Length - 1].count;
                        }
                        break;
                    }
                }

                if (nextThreshold == 0) nextThreshold = count;

                var go = new GameObject($"Faction_{kvp.Key}");
                go.transform.SetParent(factionPanel.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0, 1);
                rt.sizeDelta = new Vector2(0, 28);
                rt.anchoredPosition = new Vector2(0, -yOffset);

                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{kvp.Key} ({count}/{nextThreshold})";
                tmp.fontSize = 16;
                tmp.alignment = TextAlignmentOptions.Left;
                tmp.color = active ? Color.cyan : new Color(0.6f, 0.6f, 0.6f);

                yOffset += 28;
            }
        }
        }

        void UpdateEquipmentUI(PlayerData human)
        {
            if (equipmentSlots == null || equipmentSlots.Count == 0) return;

            bool hasEquipment = human.equipmentInventory != null && human.equipmentInventory.Count > 0;
            if (equipmentPanel != null)
                equipmentPanel.SetActive(hasEquipment);

            if (!hasEquipment) return;

            if (equipmentTitleText != null)
                equipmentTitleText.text = $"装备背包 ({human.equipmentInventory.Count}/{human.maxEquipmentInventory})";

            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                var slot = equipmentSlots[i];
                var eq = i < human.equipmentInventory.Count ? human.equipmentInventory[i] : null;
                var nameTmp = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                var img = slot.GetComponent<Image>();

                if (eq != null)
                {
                    if (nameTmp != null) nameTmp.text = eq.equipmentName;
                    img.color = eq.displayColor * 0.6f + new Color(0.1f, 0.1f, 0.1f, 0.9f);
                }
                else
                {
                    if (nameTmp != null) nameTmp.text = "";
                    img.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
                }
            }
        }

        void OnEquipmentSlotClicked(int index)
        {
            if (GameLoopManager.Instance == null || GameLoopManager.Instance.CurrentPhase != GamePhase.Preparation)
                return;

            var dragController = FindFirstObjectByType<DragController>();
            if (dragController != null)
            {
                dragController.StartEquipmentDrag(index);
                Debug.Log($"[Equipment] Started dragging equipment slot {index}");
            }
        }

        void UpdatePlayerList()
        {
            if (playerListTexts == null) return;

            var allPlayers = GameLoopManager.Instance?.AllPlayers;
            if (allPlayers == null) return;

            var sorted = new System.Collections.Generic.List<PlayerData>(allPlayers);
            sorted.Sort((a, b) =>
            {
                if (a.IsAlive != b.IsAlive) return a.IsAlive ? -1 : 1;
                return b.health.CompareTo(a.health);
            });

            for (int i = 0; i < playerListTexts.Count; i++)
            {
                var txt = playerListTexts[i];
                if (i >= sorted.Count) { txt.text = ""; continue; }

                var p = sorted[i];
                string prefix = p.isHuman ? "► " : "  ";
                if (p.IsAlive)
                    txt.text = $"{prefix}{p.playerName}: {p.health} HP";
                else
                    txt.text = $"<color=grey>{prefix}{p.playerName}: 淘汰 #{p.placement}</color>";
            }
        }

        public void ShowCombatResult(bool playerWon, int damage)
        {
            if (resultText == null) return;
            resultText.gameObject.SetActive(true);
            if (playerWon)
                resultText.text = $"<color=green>胜利!</color> 对对手造成 {damage} 点伤害";
            else
                resultText.text = $"<color=red>失败!</color> 受到 {damage} 点伤害";
        }

        public void UpdateOpponentHealth(PlayerData opponent)
        {
            if (aiHealthText != null && opponent != null)
                aiHealthText.text = $"对手: {opponent.health} HP";
        }

        public void ShowGameOver(bool humanWon, List<PlayerData> eliminated)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                string result = humanWon ? "<color=yellow>🎉 你赢了! 排名第1</color>" : $"<color=red>💀 你输了</color>\n排名: #{GameLoopManager.Instance.HumanPlayer?.placement ?? 8}";
                gameOverText.text = result;
            }
        }

        public void ShowMatchup(string attacker, string defender)
        {
            if (matchupText == null) return;
            matchupText.gameObject.SetActive(true);
            matchupText.text = $"{attacker}  VS  {defender}";
            StartCoroutine(HideMatchupAfterDelay(2f));
        }

        System.Collections.IEnumerator HideMatchupAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (matchupText != null)
                matchupText.gameObject.SetActive(false);
        }

        // ========== Button Handlers ==========

        void OnShopSlotClicked(int index)
        {
            if (GameLoopManager.Instance == null || GameLoopManager.Instance.CurrentPhase != GamePhase.Preparation)
                return;
            var human = GameLoopManager.Instance.HumanPlayer;
            if (human == null) return;
            if (ShopManager.Instance == null)
            {
                Debug.LogWarning("[Shop] ShopManager.Instance is null!");
                return;
            }
            bool bought = ShopManager.Instance.BuyHero(human, index);
            Debug.Log($"[Shop] Slot {index} clicked, bought={bought}, gold={human.gold}");
            if (bought) UpdateUI();
        }

        void OnRefreshClicked()
        {
            if (GameLoopManager.Instance == null || GameLoopManager.Instance.CurrentPhase != GamePhase.Preparation)
                return;
            var human = GameLoopManager.Instance.HumanPlayer;
            if (human == null) return;
            ShopManager.Instance?.RefreshShop(human);
            UpdateUI();
        }

        void OnLockClicked()
        {
            var human = GameLoopManager.Instance?.HumanPlayer;
            if (human == null) return;
            ShopManager.Instance?.ToggleShopLock(human);
            UpdateUI();
        }

        void OnUpgradeClicked()
        {
            if (GameLoopManager.Instance == null || GameLoopManager.Instance.CurrentPhase != GamePhase.Preparation)
                return;
            var human = GameLoopManager.Instance?.HumanPlayer;
            var config = GameLoopManager.Instance?.Config;
            if (human == null || config == null) return;
            human.BuyExp(config);
            UpdateUI();
        }

        public void OnRestartClicked()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (resultText != null) resultText.gameObject.SetActive(false);
            GameLoopManager.Instance?.RestartGame();
        }
    }
}
