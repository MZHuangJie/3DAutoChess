using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        [SerializeField] private TMP_FontAsset chineseFont;

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
        private Button shopToggleButton;
        private bool shopExpanded = true;
        public bool IsShopExpanded => shopExpanded;
        private GameObject factionPanel;
        private List<TextMeshProUGUI> playerListTexts = new List<TextMeshProUGUI>();
        private TextMeshProUGUI matchupText;

        // Equipment UI
        private GameObject equipmentPanel;
        private List<GameObject> equipmentSlots = new List<GameObject>();
        private TextMeshProUGUI equipmentTitleText;
        private int equipmentPage = 0;
        private const int equipmentSlotsPerPage = 10;
        private const int equipmentTotalSlots = 20;
        private TextMeshProUGUI equipPageText;

        // Title screen
        private GameObject titlePanel;

        // Augment selection
        private GameObject augmentPanel;
        private List<AugmentData> displayedAugmentChoices;

        // Carousel selection
        private GameObject carouselPanel;
        private List<CarouselItemData> displayedCarouselItems;

        // Battle log
        private GameObject battleLogPanel;
        private UnityEngine.UI.ScrollRect battleLogScrollRect;
        private TextMeshProUGUI battleLogText;

        // Piece detail panel
        private GameObject pieceDetailPanel;
        private TextMeshProUGUI pieceDetailText;
        private GameObject pieceDetailEquipContainer;
        private ChessPiece currentDetailPiece;
        private int lastDetailEquipHash = -1;
        private GameObject equipDetailPanel;
        private TextMeshProUGUI equipDetailText;

        private Canvas canvas;
        private bool initialized = false;

        void Start()
        {
            EnsureCanvas();
            LoadChineseFont();
            CreateMissingUI();
            ApplyFontToAll();
            initialized = true;

            if (resultText != null) resultText.gameObject.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (matchupText != null) matchupText.gameObject.SetActive(false);
        }

        void LoadChineseFont()
        {
            if (chineseFont != null) return;
            chineseFont = Resources.Load<TMP_FontAsset>("Fonts/ChineseFont SDF");
            if (chineseFont == null)
            {
                var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                foreach (var f in fonts)
                {
                    if (f.name.Contains("Chinese"))
                    {
                        chineseFont = f;
                        break;
                    }
                }
            }
            if (chineseFont != null)
            {
                var defaultFont = TMP_Settings.defaultFontAsset;
                if (defaultFont != null && !defaultFont.fallbackFontAssetTable.Contains(chineseFont))
                    defaultFont.fallbackFontAssetTable.Add(chineseFont);
            }
        }

        void ApplyFontToAll()
        {
            if (chineseFont == null) return;
            foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
                tmp.font = chineseFont;
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
            // Matchup banner (center top)
            if (matchupText == null)
            {
                var go = CreateTextGO("MatchupText", "", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -120));
                matchupText = go.GetComponent<TextMeshProUGUI>();
                matchupText.fontStyle = FontStyles.Bold;
            }

            // Shop panel (bottom center)
            CreateShopPanel();

            // Equipment panel (left side, upper)
            CreateEquipmentPanel();

            // Faction panel (left side, below equipment)
            CreateFactionPanel();

            // Player info panel (right side: name, health, gold, pop, exp, streak, interest, player list)
            CreatePlayerInfoPanel();

            // Battle log panel (right side, below player info)
            CreateBattleLogPanel();

            // Piece detail panel (center, hidden by default)
            CreatePieceDetailPanel();
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

            // Left side: upgrade (top) + refresh (bottom), stacked
            upgradeButton = CreateShopButton("升级 (4金)", new Vector2(-400, 25), OnUpgradeClicked);
            upgradeButton.transform.SetParent(shopPanel.transform);

            refreshButton = CreateShopButton("刷新 (2金)", new Vector2(-400, -12), OnRefreshClicked);
            refreshButton.transform.SetParent(shopPanel.transform);

            // Right side: lock button
            lockButton = CreateShopButton("锁定", new Vector2(400, 10), OnLockClicked);
            lockButton.transform.SetParent(shopPanel.transform);

            // Toggle button (always visible, sits above shop panel)
            var toggleGO = new GameObject("ShopToggle");
            toggleGO.transform.SetParent(transform);
            var toggleRt = toggleGO.AddComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(0.5f, 0);
            toggleRt.anchorMax = new Vector2(0.5f, 0);
            toggleRt.pivot = new Vector2(0.5f, 0);
            toggleRt.sizeDelta = new Vector2(100, 28);
            toggleRt.anchoredPosition = new Vector2(0, 140);

            var toggleImg = toggleGO.AddComponent<Image>();
            toggleImg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            var toggleBtn = toggleGO.AddComponent<Button>();
            toggleBtn.onClick.AddListener(OnShopToggleClicked);
            shopToggleButton = toggleBtn;

            var toggleTxtGO = new GameObject("Text");
            toggleTxtGO.transform.SetParent(toggleGO.transform, false);
            var toggleTxtRt = toggleTxtGO.AddComponent<RectTransform>();
            toggleTxtRt.anchorMin = Vector2.zero;
            toggleTxtRt.anchorMax = Vector2.one;
            toggleTxtRt.offsetMin = Vector2.zero;
            toggleTxtRt.offsetMax = Vector2.zero;
            var toggleTxt = toggleTxtGO.AddComponent<TextMeshProUGUI>();
            toggleTxt.text = "▼ 商店";
            toggleTxt.fontSize = 14;
            toggleTxt.alignment = TextAlignmentOptions.Center;
            toggleTxt.color = Color.white;
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
            txtGO.transform.SetParent(btnGO.transform, false);
            var txtRt = txtGO.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            var txt = txtGO.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = 14;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;

            return btn;
        }

        private GameObject factionContent;

        void CreateFactionPanel()
        {
            if (factionPanel != null) return;

            factionPanel = new GameObject("FactionPanel");
            factionPanel.transform.SetParent(transform, false);
            var rt = factionPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(180, 380);
            rt.anchoredPosition = new Vector2(115, -10);

            // Title (outside scroll)
            var titleGO = new GameObject("FactionTitle");
            titleGO.transform.SetParent(factionPanel.transform, false);
            var titleRt = titleGO.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.sizeDelta = new Vector2(0, 24);
            titleRt.anchoredPosition = Vector2.zero;
            var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "羁绊";
            titleTmp.fontSize = 14;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(1f, 0.8f, 0.4f);

            // Scroll view
            var scrollGO = new GameObject("Scroll");
            scrollGO.transform.SetParent(factionPanel.transform, false);
            var scrollRt = scrollGO.AddComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(0, 0);
            scrollRt.offsetMax = new Vector2(0, -26);

            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20f;

            var scrollMask = scrollGO.AddComponent<RectMask2D>();

            // Content container
            factionContent = new GameObject("Content");
            factionContent.transform.SetParent(scrollGO.transform, false);
            var contentRt = factionContent.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0, 1);
            contentRt.sizeDelta = new Vector2(0, 0);

            scrollRect.content = contentRt;
        }

        private GameObject playerInfoPanel;

        void CreatePlayerInfoPanel()
        {
            if (playerInfoPanel != null) return;

            playerInfoPanel = new GameObject("PlayerInfoPanel");
            playerInfoPanel.transform.SetParent(transform, false);
            var rt = playerInfoPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(200, 380);
            rt.anchoredPosition = new Vector2(0, -10);

            var bg = playerInfoPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            int y = 0;
            // Player name + health
            if (playerHealthText == null)
            {
                var go = CreatePanelText(playerInfoPanel, "HealthText", "玩家: 100 HP", 22, y);
                playerHealthText = go;
            }
            y -= 30;

            if (goldText == null)
            {
                goldText = CreatePanelText(playerInfoPanel, "GoldText", "金币: 5", 20, y);
                goldText.color = new Color(1f, 0.85f, 0.2f);
            }
            y -= 28;

            if (populationText == null)
            {
                populationText = CreatePanelText(playerInfoPanel, "PopText", "人口: 1/3", 18, y);
            }
            y -= 26;

            if (expText == null)
            {
                expText = CreatePanelText(playerInfoPanel, "ExpText", "经验: 0/2", 16, y);
                expText.color = new Color(0.6f, 0.8f, 1f);
            }
            y -= 26;

            if (interestText == null)
            {
                interestText = CreatePanelText(playerInfoPanel, "InterestText", "利息: +0", 16, y);
                interestText.color = new Color(0.8f, 0.9f, 0.5f);
            }
            y -= 26;

            if (streakText == null)
            {
                streakText = CreatePanelText(playerInfoPanel, "StreakText", "", 16, y);
            }
            y -= 30;

            // Separator
            var sep = new GameObject("Separator");
            sep.transform.SetParent(playerInfoPanel.transform, false);
            var sepRt = sep.AddComponent<RectTransform>();
            sepRt.anchorMin = new Vector2(0, 1);
            sepRt.anchorMax = new Vector2(1, 1);
            sepRt.pivot = new Vector2(0.5f, 1);
            sepRt.sizeDelta = new Vector2(-20, 2);
            sepRt.anchoredPosition = new Vector2(0, y);
            var sepImg = sep.AddComponent<Image>();
            sepImg.color = new Color(0.3f, 0.3f, 0.4f);
            y -= 10;

            // Opponent info
            if (aiHealthText == null)
            {
                aiHealthText = CreatePanelText(playerInfoPanel, "OpponentText", "", 16, y);
                aiHealthText.color = new Color(1f, 0.5f, 0.5f);
            }
            y -= 30;

            // Player list
            playerListTexts.Clear();
            for (int i = 0; i < 8; i++)
            {
                var txt = CreatePanelText(playerInfoPanel, $"PlayerList_{i}", "", 14, y);
                playerListTexts.Add(txt);
                y -= 22;
            }
        }

        TextMeshProUGUI CreatePanelText(GameObject parent, string name, string text, int fontSize, int yOffset)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(-16, 26);
            rt.anchoredPosition = new Vector2(8, yOffset);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;
            return tmp;
        }

        void CreateBattleLogPanel()
        {
            if (battleLogPanel != null) return;

            battleLogPanel = new GameObject("BattleLogPanel");
            battleLogPanel.transform.SetParent(transform, false);
            var rt = battleLogPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(200, 160);
            rt.anchoredPosition = new Vector2(0, -395);

            var bg = battleLogPanel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(battleLogPanel.transform, false);
            var titleRt = titleGO.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.sizeDelta = new Vector2(0, 20);
            titleRt.anchoredPosition = Vector2.zero;
            var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "对战记录";
            titleTmp.fontSize = 12;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(1f, 0.8f, 0.4f);

            var scrollGO = new GameObject("Scroll");
            scrollGO.transform.SetParent(battleLogPanel.transform, false);
            var scrollRt = scrollGO.AddComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(0, 0);
            scrollRt.offsetMax = new Vector2(0, -22);
            var scrollRect = scrollGO.AddComponent<UnityEngine.UI.ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20f;
            var scrollMask = scrollGO.AddComponent<UnityEngine.UI.Mask>();
            scrollMask.showMaskGraphic = false;
            var scrollImg = scrollGO.AddComponent<Image>();
            scrollImg.color = Color.clear;

            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(scrollGO.transform, false);
            var contentRt = contentGO.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 0);
            var csf = contentGO.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRt;
            battleLogScrollRect = scrollRect;
            var logGO = new GameObject("LogText");
            logGO.transform.SetParent(contentGO.transform, false);
            var logRt = logGO.AddComponent<RectTransform>();
            logRt.anchorMin = new Vector2(0, 1);
            logRt.anchorMax = new Vector2(1, 1);
            logRt.pivot = new Vector2(0, 1);
            logRt.sizeDelta = new Vector2(-12, 0);
            logRt.anchoredPosition = new Vector2(6, 0);
            battleLogText = logGO.AddComponent<TextMeshProUGUI>();
            battleLogText.text = "";
            battleLogText.fontSize = 11;
            battleLogText.alignment = TextAlignmentOptions.TopLeft;
            battleLogText.color = Color.white;
            battleLogText.enableWordWrapping = true;
        }

        void CreatePieceDetailPanel()
        {
            if (pieceDetailPanel != null) return;

            pieceDetailPanel = new GameObject("PieceDetailPanel");
            pieceDetailPanel.transform.SetParent(transform, false);
            var rt = pieceDetailPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(320, 450);

            var bg = pieceDetailPanel.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.06f, 0.1f, 0.92f);

            var textGO = new GameObject("DetailText");
            textGO.transform.SetParent(pieceDetailPanel.transform, false);
            var textRt = textGO.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(14, 10);
            textRt.offsetMax = new Vector2(-14, -10);
            pieceDetailText = textGO.AddComponent<TextMeshProUGUI>();
            pieceDetailText.fontSize = 14;
            pieceDetailText.alignment = TextAlignmentOptions.TopLeft;
            pieceDetailText.color = Color.white;
            pieceDetailText.lineSpacing = 4;

            pieceDetailEquipContainer = new GameObject("EquipIcons");
            pieceDetailEquipContainer.transform.SetParent(pieceDetailPanel.transform, false);
            var eqRt = pieceDetailEquipContainer.AddComponent<RectTransform>();
            eqRt.anchorMin = new Vector2(0, 0);
            eqRt.anchorMax = new Vector2(1, 0);
            eqRt.pivot = new Vector2(0, 0);
            eqRt.offsetMin = new Vector2(14, 10);
            eqRt.offsetMax = new Vector2(-14, 46);

            equipDetailPanel = new GameObject("EquipDetailPanel");
            equipDetailPanel.transform.SetParent(transform, false);
            var edRt = equipDetailPanel.AddComponent<RectTransform>();
            edRt.anchorMin = Vector2.zero;
            edRt.anchorMax = Vector2.zero;
            edRt.pivot = new Vector2(0, 0.5f);
            edRt.sizeDelta = new Vector2(220, 300);

            var edBg = equipDetailPanel.AddComponent<Image>();
            edBg.color = new Color(0.08f, 0.08f, 0.14f, 0.95f);

            var edTextGO = new GameObject("EquipDetailText");
            edTextGO.transform.SetParent(equipDetailPanel.transform, false);
            var edTextRt = edTextGO.AddComponent<RectTransform>();
            edTextRt.anchorMin = Vector2.zero;
            edTextRt.anchorMax = Vector2.one;
            edTextRt.offsetMin = new Vector2(10, 8);
            edTextRt.offsetMax = new Vector2(-10, -8);
            equipDetailText = edTextGO.AddComponent<TextMeshProUGUI>();
            equipDetailText.fontSize = 13;
            equipDetailText.alignment = TextAlignmentOptions.TopLeft;
            equipDetailText.color = Color.white;
            equipDetailText.lineSpacing = 3;
            equipDetailPanel.SetActive(false);

            pieceDetailPanel.SetActive(false);
        }

        void PositionPanelNearScreenPoint(RectTransform panelRt, Vector2 screenPoint, float gap = 10f)
        {
            var canvasRt = GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRt.sizeDelta;
            float scaleX = canvasSize.x / Screen.width;
            float scaleY = canvasSize.y / Screen.height;
            Vector2 panelSize = panelRt.sizeDelta;

            float canvasX = screenPoint.x * scaleX;
            float canvasY = screenPoint.y * scaleY;

            float rightX = canvasX + gap;
            float leftX = canvasX - gap - panelSize.x;

            float posX;
            if (rightX + panelSize.x <= canvasSize.x)
                posX = rightX;
            else if (leftX >= 0)
                posX = leftX;
            else
                posX = Mathf.Clamp(canvasX - panelSize.x * 0.5f, 0, canvasSize.x - panelSize.x);

            float posY = Mathf.Clamp(canvasY, panelSize.y * 0.5f, canvasSize.y - panelSize.y * 0.5f);

            panelRt.anchoredPosition = new Vector2(posX, posY - panelSize.y * 0.5f);
            panelRt.pivot = new Vector2(0, 0);
        }

        public void ShowPieceDetail(ChessPiece piece)
        {
            if (piece == null) return;
            if (pieceDetailPanel == null) CreatePieceDetailPanel();

            currentDetailPiece = piece;
            lastDetailEquipHash = -1;
            RefreshPieceDetailContent(piece);

            pieceDetailPanel.SetActive(true);

            Vector2 screenPos = Input.mousePosition;
            PositionPanelNearScreenPoint(pieceDetailPanel.GetComponent<RectTransform>(), screenPos, 20f);
        }

        public void HidePieceDetail()
        {
            currentDetailPiece = null;
            lastDetailEquipHash = -1;
            if (pieceDetailPanel != null)
                pieceDetailPanel.SetActive(false);
            if (equipDetailPanel != null)
                equipDetailPanel.SetActive(false);
        }

        void LateUpdate()
        {
            if (currentDetailPiece != null && pieceDetailPanel != null && pieceDetailPanel.activeSelf)
            {
                if (currentDetailPiece.IsAlive)
                    RefreshPieceDetailContent(currentDetailPiece);
                else
                    HidePieceDetail();
            }
        }

        void RefreshPieceDetailContent(ChessPiece piece)
        {
            var h = piece.heroData;
            var sb = new System.Text.StringBuilder();

            string pieceName = h != null ? h.heroName : piece.gameObject.name.Replace("Creep_", "");
            int starLevel = piece.starLevel;
            var stars = new string('★', starLevel);

            if (h != null)
            {
                int sellValue = h.cost * (int)Mathf.Pow(3, starLevel - 1);
                sb.AppendLine($"<size=20><b>{pieceName}</b></size>  {stars}    <color=#FFD700>{sellValue}金币</color>");
            }
            else
            {
                sb.AppendLine($"<size=20><b>{pieceName}</b></size>  <color=#FF6060>野怪</color>");
            }
            sb.AppendLine();

            sb.AppendLine($"血量: {piece.currentHealth}/{piece.maxHealth}");
            sb.AppendLine($"蓝量: {piece.currentMana}/{piece.maxMana}");
            sb.AppendLine();

            if (h != null && h.factions != null && h.factions.Length > 0)
            {
                sb.AppendLine($"<color=#80D0FF>羁绊: {string.Join(" / ", h.factions)}</color>");
                sb.AppendLine();
            }

            if (h != null && h.skillType != SkillType.None && !string.IsNullOrEmpty(h.skillName))
            {
                sb.AppendLine($"<color=#FFA040>技能: {h.skillName}</color>");
                string typeStr = h.skillType switch
                {
                    SkillType.Damage => "单体伤害",
                    SkillType.AreaDamage => "范围伤害",
                    SkillType.Heal => "治疗",
                    SkillType.Stun => "眩晕",
                    _ => h.skillType.ToString()
                };
                sb.Append($"类型: {typeStr}");
                if (h.skillDamage > 0) sb.Append($"  伤害: {h.skillDamage}");
                sb.AppendLine();
                sb.Append($"范围: {h.skillRange}");
                if (h.skillStunDuration > 0) sb.Append($"  眩晕: {h.skillStunDuration}s");
                sb.AppendLine();
                sb.AppendLine();
            }

            string atkType = (h != null ? h.attackType : AttackType.Melee) == AttackType.Melee ? "近战" : "远程";
            sb.AppendLine($"攻击力: {piece.attackDamage}  攻速: {piece.attackSpeed:F2}");
            sb.AppendLine($"护甲: {piece.armor}  魔抗: {piece.magicResist}");
            sb.AppendLine($"攻击距离: {piece.attackRange} ({atkType})");
            sb.AppendLine();

            int equipHash = ComputeEquipHash(piece);
            bool equipChanged = equipHash != lastDetailEquipHash;

            if (equipChanged)
            {
                lastDetailEquipHash = equipHash;
                foreach (Transform child in pieceDetailEquipContainer.transform)
                    Destroy(child.gameObject);
            }

            if (piece.equipment.Count > 0)
            {
                sb.AppendLine("<color=#40FF80>装备:</color>");
                pieceDetailEquipContainer.SetActive(true);
                if (equipChanged)
                {
                    for (int i = 0; i < piece.equipment.Count; i++)
                    {
                        var eq = piece.equipment[i];
                        if (eq == null) continue;

                        var iconGO = new GameObject($"EqIcon_{i}");
                        iconGO.transform.SetParent(pieceDetailEquipContainer.transform, false);
                        var iconRt = iconGO.AddComponent<RectTransform>();
                        iconRt.anchorMin = new Vector2(0, 0.5f);
                        iconRt.anchorMax = new Vector2(0, 0.5f);
                        iconRt.pivot = new Vector2(0, 0.5f);
                        iconRt.sizeDelta = new Vector2(32, 32);
                        iconRt.anchoredPosition = new Vector2(i * 38, 0);

                        var iconImg = iconGO.AddComponent<Image>();
                        iconImg.color = eq.displayColor;

                        var btn = iconGO.AddComponent<Button>();
                        var capturedEq = eq;
                        btn.onClick.AddListener(() => ShowEquipmentDetail(capturedEq));

                        var nameGO = new GameObject("Name");
                        nameGO.transform.SetParent(iconGO.transform, false);
                        var nameRt = nameGO.AddComponent<RectTransform>();
                        nameRt.anchorMin = Vector2.zero;
                        nameRt.anchorMax = Vector2.one;
                        nameRt.offsetMin = Vector2.zero;
                    nameRt.offsetMax = Vector2.zero;
                    var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
                    nameTmp.text = eq.equipmentName.Substring(0, 1);
                    nameTmp.fontSize = 14;
                    nameTmp.alignment = TextAlignmentOptions.Center;
                    nameTmp.color = Color.white;
                    nameTmp.raycastTarget = false;
                    }
                }
            }
            else
            {
                sb.AppendLine("<color=#808080>装备: 无</color>");
                pieceDetailEquipContainer.SetActive(false);
            }

            pieceDetailText.text = sb.ToString();
        }

        int ComputeEquipHash(ChessPiece piece)
        {
            int hash = piece.equipment.Count;
            for (int i = 0; i < piece.equipment.Count; i++)
            {
                if (piece.equipment[i] != null)
                    hash = hash * 31 + piece.equipment[i].GetInstanceID();
            }
            return hash;
        }

        public void ShowFactionDetail(string factionName)
        {
            var allFactions = FactionManager.Instance?.AllFactions;
            if (allFactions == null) return;

            FactionData factionData = null;
            foreach (var fd in allFactions)
            {
                if (fd != null && fd.factionName == factionName)
                { factionData = fd; break; }
            }
            if (factionData == null) return;

            if (pieceDetailPanel == null) CreatePieceDetailPanel();

            int currentCount = 0;
            var human = GameLoopManager.Instance?.HumanPlayer;
            if (human != null)
            {
                var pieces = BoardManager.Instance?.GetPiecesByOwner(human);
                if (pieces != null)
                {
                    foreach (var p in pieces)
                    {
                        if (p != null && !p.isOnBench && p.heroData?.factions != null)
                        {
                            foreach (var f in p.heroData.factions)
                                if (f == factionName) currentCount++;
                        }
                    }
                }
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<size=20><b><color=#80D0FF>{factionName}</color></b></size>");
            sb.AppendLine($"当前: {currentCount} 个棋子");
            sb.AppendLine();

            if (factionData.thresholds != null)
            {
                sb.AppendLine("<color=#FFA040>羁绊效果:</color>");
                foreach (var t in factionData.thresholds)
                {
                    bool reached = currentCount >= t.count;
                    string mark = reached ? "<color=#40FF80>✓</color>" : " ";
                    sb.Append($"{mark} ({t.count}) ");
                    if (!string.IsNullOrEmpty(t.description))
                        sb.Append(t.description);
                    else
                    {
                        var parts = new System.Collections.Generic.List<string>();
                        if (t.healthBonus > 0) parts.Add($"生命+{t.healthBonus}");
                        if (t.attackBonus > 0) parts.Add($"攻击+{t.attackBonus}");
                        if (t.attackSpeedBonus > 0) parts.Add($"攻速+{t.attackSpeedBonus:P0}");
                        sb.Append(string.Join(", ", parts));
                    }
                    sb.AppendLine();
                }
            }

            pieceDetailText.text = sb.ToString();
            pieceDetailPanel.SetActive(true);

            Vector2 screenPos = Input.mousePosition;
            PositionPanelNearScreenPoint(pieceDetailPanel.GetComponent<RectTransform>(), screenPos, 20f);
        }

        public void ShowEquipmentDetail(EquipmentData eq)
        {
            if (eq == null) return;
            if (equipDetailPanel == null) CreatePieceDetailPanel();

            var sb = new System.Text.StringBuilder();
            string typeStr = eq.equipmentType == EquipmentType.Base ? "基础装备" : "合成装备";
            sb.AppendLine($"<size=18><b><color=#40FF80>{eq.equipmentName}</color></b></size>");
            sb.AppendLine($"<color=#808080>{typeStr}</color>");
            sb.AppendLine();

            sb.AppendLine("<color=#FFA040>属性加成:</color>");
            if (eq.healthBonus != 0) sb.AppendLine($"  生命 +{eq.healthBonus}");
            if (eq.attackBonus != 0) sb.AppendLine($"  攻击力 +{eq.attackBonus}");
            if (eq.armorBonus != 0) sb.AppendLine($"  护甲 +{eq.armorBonus}");
            if (eq.magicResistBonus != 0) sb.AppendLine($"  魔抗 +{eq.magicResistBonus}");
            if (eq.attackSpeedBonus != 0) sb.AppendLine($"  攻速 +{eq.attackSpeedBonus:P0}");
            if (eq.manaBonus != 0) sb.AppendLine($"  法力 +{eq.manaBonus}");

            if (eq.lifestealPercent > 0 || eq.spellDamageBonus > 0 || eq.dodgeChance > 0)
            {
                sb.AppendLine();
                sb.AppendLine("<color=#FFA040>特殊效果:</color>");
                if (eq.lifestealPercent > 0) sb.AppendLine($"  吸血 {eq.lifestealPercent:P0}");
                if (eq.spellDamageBonus > 0) sb.AppendLine($"  法术伤害 +{eq.spellDamageBonus}");
                if (eq.dodgeChance > 0) sb.AppendLine($"  闪避 {eq.dodgeChance:P0}");
            }

            if (eq.equipmentType == EquipmentType.Combined && eq.recipe1 != null && eq.recipe2 != null)
            {
                sb.AppendLine();
                sb.AppendLine($"<color=#808080>合成: {eq.recipe1.equipmentName} + {eq.recipe2.equipmentName}</color>");
            }

            equipDetailText.text = sb.ToString();
            equipDetailPanel.SetActive(true);

            Vector2 screenPos = Input.mousePosition;
            PositionPanelNearScreenPoint(equipDetailPanel.GetComponent<RectTransform>(), screenPos, 10f);
        }

        void CreateEquipmentPanel()
        {
            if (equipmentPanel != null) return;

            equipmentPanel = new GameObject("EquipmentPanel");
            equipmentPanel.transform.SetParent(transform, false);
            var rt = equipmentPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(100, 380);
            rt.anchoredPosition = new Vector2(10, -10);

            var bg = equipmentPanel.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.1f, 0.18f, 0.85f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(equipmentPanel.transform, false);
            var titleRt = titleGO.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.sizeDelta = new Vector2(0, 24);
            titleRt.anchoredPosition = Vector2.zero;
            equipmentTitleText = titleGO.AddComponent<TextMeshProUGUI>();
            equipmentTitleText.text = "装备";
            equipmentTitleText.fontSize = 14;
            equipmentTitleText.alignment = TextAlignmentOptions.Center;
            equipmentTitleText.color = new Color(1f, 0.8f, 0.4f);

            for (int i = 0; i < equipmentTotalSlots; i++)
            {
                var slot = new GameObject($"EqSlot_{i}");
                slot.transform.SetParent(equipmentPanel.transform, false);
                var slotRt = slot.AddComponent<RectTransform>();
                slotRt.anchorMin = new Vector2(0, 1);
                slotRt.anchorMax = new Vector2(1, 1);
                slotRt.pivot = new Vector2(0.5f, 1);
                int localIndex = i % equipmentSlotsPerPage;
                slotRt.sizeDelta = new Vector2(-10, 28);
                slotRt.anchoredPosition = new Vector2(0, -26 - localIndex * 30);

                var slotImg = slot.AddComponent<Image>();
                slotImg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

                var nameGO = new GameObject("Name");
                nameGO.transform.SetParent(slot.transform, false);
                var nameRt = nameGO.AddComponent<RectTransform>();
                nameRt.anchorMin = Vector2.zero;
                nameRt.anchorMax = Vector2.one;
                nameRt.offsetMin = new Vector2(4, 0);
                nameRt.offsetMax = new Vector2(-4, 0);
                var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
                nameTmp.text = "";
                nameTmp.fontSize = 12;
                nameTmp.alignment = TextAlignmentOptions.Center;
                nameTmp.color = Color.white;

                var trigger = slot.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                int index = i;
                entry.callback.AddListener((_) => OnEquipmentSlotClicked(index));
                trigger.triggers.Add(entry);

                equipmentSlots.Add(slot);
            }

            // Page navigation bar at bottom
            float navY = -26 - equipmentSlotsPerPage * 30 - 4;

            var prevGO = new GameObject("PrevPage");
            prevGO.transform.SetParent(equipmentPanel.transform, false);
            var prevRt = prevGO.AddComponent<RectTransform>();
            prevRt.anchorMin = new Vector2(0, 1);
            prevRt.anchorMax = new Vector2(0, 1);
            prevRt.pivot = new Vector2(0, 1);
            prevRt.sizeDelta = new Vector2(28, 24);
            prevRt.anchoredPosition = new Vector2(5, navY);
            var prevImg = prevGO.AddComponent<Image>();
            prevImg.color = new Color(0.25f, 0.3f, 0.4f);
            var prevBtn = prevGO.AddComponent<Button>();
            prevBtn.onClick.AddListener(OnEquipPrevPage);
            var prevTxtGO = new GameObject("Text");
            prevTxtGO.transform.SetParent(prevGO.transform, false);
            var prevTxtRt = prevTxtGO.AddComponent<RectTransform>();
            prevTxtRt.anchorMin = Vector2.zero;
            prevTxtRt.anchorMax = Vector2.one;
            prevTxtRt.offsetMin = Vector2.zero;
            prevTxtRt.offsetMax = Vector2.zero;
            var prevTxt = prevTxtGO.AddComponent<TextMeshProUGUI>();
            prevTxt.text = "<";
            prevTxt.fontSize = 14;
            prevTxt.alignment = TextAlignmentOptions.Center;
            prevTxt.color = Color.white;

            var pageGO = new GameObject("PageText");
            pageGO.transform.SetParent(equipmentPanel.transform, false);
            var pageRt = pageGO.AddComponent<RectTransform>();
            pageRt.anchorMin = new Vector2(0, 1);
            pageRt.anchorMax = new Vector2(1, 1);
            pageRt.pivot = new Vector2(0.5f, 1);
            pageRt.sizeDelta = new Vector2(-66, 24);
            pageRt.anchoredPosition = new Vector2(0, navY);
            equipPageText = pageGO.AddComponent<TextMeshProUGUI>();
            equipPageText.text = "1/2";
            equipPageText.fontSize = 12;
            equipPageText.alignment = TextAlignmentOptions.Center;
            equipPageText.color = Color.white;

            var nextGO = new GameObject("NextPage");
            nextGO.transform.SetParent(equipmentPanel.transform, false);
            var nextRt = nextGO.AddComponent<RectTransform>();
            nextRt.anchorMin = new Vector2(1, 1);
            nextRt.anchorMax = new Vector2(1, 1);
            nextRt.pivot = new Vector2(1, 1);
            nextRt.sizeDelta = new Vector2(28, 24);
            nextRt.anchoredPosition = new Vector2(-5, navY);
            var nextImg = nextGO.AddComponent<Image>();
            nextImg.color = new Color(0.25f, 0.3f, 0.4f);
            var nextBtn = nextGO.AddComponent<Button>();
            nextBtn.onClick.AddListener(OnEquipNextPage);
            var nextTxtGO = new GameObject("Text");
            nextTxtGO.transform.SetParent(nextGO.transform, false);
            var nextTxtRt = nextTxtGO.AddComponent<RectTransform>();
            nextTxtRt.anchorMin = Vector2.zero;
            nextTxtRt.anchorMax = Vector2.one;
            nextTxtRt.offsetMin = Vector2.zero;
            nextTxtRt.offsetMax = Vector2.zero;
            var nextTxt = nextTxtGO.AddComponent<TextMeshProUGUI>();
            nextTxt.text = ">";
            nextTxt.fontSize = 14;
            nextTxt.alignment = TextAlignmentOptions.Center;
            nextTxt.color = Color.white;

            RefreshEquipmentPageVisibility();
        }

        void OnEquipPrevPage()
        {
            if (equipmentPage > 0)
            {
                equipmentPage--;
                RefreshEquipmentPageVisibility();
                UpdateUI();
            }
        }

        void OnEquipNextPage()
        {
            if (equipmentPage < 1)
            {
                equipmentPage++;
                RefreshEquipmentPageVisibility();
                UpdateUI();
            }
        }

        void RefreshEquipmentPageVisibility()
        {
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                int page = i / equipmentSlotsPerPage;
                equipmentSlots[i].SetActive(page == equipmentPage);
            }
            if (equipPageText != null)
                equipPageText.text = $"{equipmentPage + 1}/2";
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
                timerText.text = Mathf.CeilToInt(Mathf.Max(0, timeRemaining)) + "s";
        }

        public void UpdateUI()
        {
            if (GameLoopManager.Instance == null) return;
            ApplyFontToAll();

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
                goldText.text = $"金币: {human.gold}";

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
                    streakText.text = $"连胜 x{human.winStreak}";
                    streakText.color = new Color(1f, 0.6f, 0.2f);
                }
                else if (human.loseStreak >= 2)
                {
                    streakText.text = $"连败 x{human.loseStreak}";
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

            // Battle log
            UpdateBattleLog();
        }

        void UpdateShopUI(PlayerData human, GameConfig config)
        {
            if (shopPanel != null)
            {
                bool showShop = GameLoopManager.Instance == null || GameLoopManager.Instance.CurrentRound > 1;
                shopPanel.SetActive(showShop && shopExpanded);

                if (shopToggleButton != null)
                    shopToggleButton.gameObject.SetActive(showShop);
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
                    if (costTmp != null) costTmp.text = $"{hero.cost} 金";
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
                lockButton.GetComponentInChildren<TextMeshProUGUI>().text = human.shopLocked ? "[已锁定]" : "锁定";
                lockButton.GetComponent<Image>().color = human.shopLocked ? new Color(0.6f, 0.4f, 0.2f) : new Color(0.25f, 0.35f, 0.45f);
            }
        }

        void UpdateFactionUI(PlayerData human)
        {
            if (factionContent == null) return;

            foreach (Transform child in factionContent.transform)
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
                go.transform.SetParent(factionContent.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0, 1);
                rt.sizeDelta = new Vector2(-12, 26);
                rt.anchoredPosition = new Vector2(6, -yOffset);

                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = $"{kvp.Key} ({count}/{nextThreshold})";
                tmp.fontSize = 14;
                tmp.alignment = TextAlignmentOptions.Left;
                tmp.color = active ? Color.cyan : new Color(0.6f, 0.6f, 0.6f);
                tmp.raycastTarget = true;

                var btn = go.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                string fName = kvp.Key;
                btn.onClick.AddListener(() => ShowFactionDetail(fName));

                yOffset += 26;
            }

            var contentRt = factionContent.GetComponent<RectTransform>();
            if (contentRt != null)
                contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, yOffset);
        }

        void UpdateEquipmentUI(PlayerData human)
        {
            if (equipmentSlots == null || equipmentSlots.Count == 0) return;

            if (equipmentTitleText != null)
            {
                int count = human.equipmentInventory != null ? human.equipmentInventory.Count : 0;
                equipmentTitleText.text = $"装备 ({count})";
            }

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

            RefreshEquipmentPageVisibility();
        }

        void OnEquipmentSlotClicked(int index)
        {
            var human = GameLoopManager.Instance?.HumanPlayer;
            if (human == null) return;
            if (index < 0 || index >= human.equipmentInventory.Count) return;
            var eq = human.equipmentInventory[index];
            if (eq == null) return;

            var dragCtrl = Object.FindFirstObjectByType<DragController>();
            if (dragCtrl != null)
                dragCtrl.SetPendingEquipment(index);
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
            resultText.enableWordWrapping = false;
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
                var human = GameLoopManager.Instance?.HumanPlayer;
                string result = humanWon ? "<color=yellow>你赢了! 排名第1</color>" : $"<color=red>你输了</color>\n排名: #{human?.placement ?? 8}";

                var sb = new System.Text.StringBuilder();
                sb.AppendLine(result);
                sb.AppendLine();
                sb.AppendLine("--- 最终排名 ---");

                var allPlayers = GameLoopManager.Instance?.AllPlayers;
                if (allPlayers != null)
                {
                    var sorted = new List<PlayerData>(allPlayers);
                    sorted.Sort((a, b) =>
                    {
                        if (a.placement == 0 && b.placement == 0) return b.health.CompareTo(a.health);
                        if (a.placement == 0) return -1;
                        if (b.placement == 0) return 1;
                        return a.placement.CompareTo(b.placement);
                    });
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        var p = sorted[i];
                        int rank = p.placement > 0 ? p.placement : 1;
                        string name = p.isHuman ? $"<color=yellow>{p.playerName}</color>" : p.playerName;
                        string augStr = "";
                        if (p.augments.Count > 0)
                        {
                            var names = p.augments.ConvertAll(a => a.augmentName);
                            augStr = $" [{string.Join(", ", names)}]";
                        }
                        sb.AppendLine($"#{rank} {name} Lv{p.level}{augStr}");
                    }
                }

                gameOverText.text = sb.ToString();
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

        void OnShopToggleClicked()
        {
            shopExpanded = !shopExpanded;
            if (shopPanel != null)
                shopPanel.SetActive(shopExpanded);

            var toggleTxt = shopToggleButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (toggleTxt != null)
                toggleTxt.text = shopExpanded ? "▼ 商店" : "▲ 商店";

            var toggleRt = shopToggleButton?.GetComponent<RectTransform>();
            if (toggleRt != null)
                toggleRt.anchoredPosition = new Vector2(0, shopExpanded ? 140 : 0);
        }

        // ========== Title Screen ==========

        public void ShowTitleScreen()
        {
            if (titlePanel != null) return;

            SetGamePanelsVisible(false);

            titlePanel = new GameObject("TitlePanel");
            titlePanel.transform.SetParent(transform, false);
            var rt = titlePanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = titlePanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 1f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(titlePanel.transform, false);
            var titleRt = titleGO.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.sizeDelta = new Vector2(600, 80);
            titleRt.anchoredPosition = new Vector2(0, 100);
            var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "无限恐怖自走棋";
            titleTmp.fontSize = 60;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(1f, 0.85f, 0.3f);
            titleTmp.fontStyle = FontStyles.Bold;

            var startGO = new GameObject("StartButton");
            startGO.transform.SetParent(titlePanel.transform, false);
            var startRt = startGO.AddComponent<RectTransform>();
            startRt.anchorMin = new Vector2(0.5f, 0.5f);
            startRt.anchorMax = new Vector2(0.5f, 0.5f);
            startRt.sizeDelta = new Vector2(200, 50);
            startRt.anchoredPosition = new Vector2(0, -30);

            var startImg = startGO.AddComponent<Image>();
            startImg.color = new Color(0.2f, 0.4f, 0.6f);

            var startBtn = startGO.AddComponent<Button>();
            startBtn.onClick.AddListener(() => GameLoopManager.Instance?.StartGame());

            var startTxtGO = new GameObject("Text");
            startTxtGO.transform.SetParent(startGO.transform, false);
            var startTxtRt = startTxtGO.AddComponent<RectTransform>();
            startTxtRt.anchorMin = Vector2.zero;
            startTxtRt.anchorMax = Vector2.one;
            startTxtRt.offsetMin = Vector2.zero;
            startTxtRt.offsetMax = Vector2.zero;
            var startTxt = startTxtGO.AddComponent<TextMeshProUGUI>();
            startTxt.text = "开始游戏";
            startTxt.fontSize = 28;
            startTxt.alignment = TextAlignmentOptions.Center;
            startTxt.color = Color.white;

            ApplyFontToAll();
        }

        public void HideTitleScreen()
        {
            if (titlePanel != null)
            {
                Destroy(titlePanel);
                titlePanel = null;
            }
            SetGamePanelsVisible(true);
        }

        void SetGamePanelsVisible(bool visible)
        {
            if (shopPanel != null) shopPanel.SetActive(visible);
            if (factionPanel != null) factionPanel.SetActive(visible);
            if (equipmentPanel != null) equipmentPanel.SetActive(visible);
            if (playerInfoPanel != null) playerInfoPanel.SetActive(visible);
            if (battleLogPanel != null) battleLogPanel.SetActive(visible);
            if (phaseText != null) phaseText.gameObject.SetActive(visible);
            if (timerText != null) timerText.gameObject.SetActive(visible);
            if (roundText != null) roundText.gameObject.SetActive(visible);
            if (matchupText != null) matchupText.gameObject.SetActive(visible);
        }

        // ========== Augment Selection ==========

        public void ShowAugmentSelection(List<AugmentData> choices)
        {
            HideAugmentSelection();
            if (choices == null || choices.Count == 0) return;
            displayedAugmentChoices = choices;

            augmentPanel = new GameObject("AugmentPanel");
            augmentPanel.transform.SetParent(transform, false);
            var rt = augmentPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = augmentPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(augmentPanel.transform, false);
            var titleRt = titleGO.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.sizeDelta = new Vector2(500, 50);
            titleRt.anchoredPosition = new Vector2(0, 160);
            var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "选择一个海克斯天赋";
            titleTmp.fontSize = 30;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(1f, 0.85f, 0.3f);
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.raycastTarget = false;

            float cardWidth = 200;
            float spacing = 20;
            float totalWidth = choices.Count * cardWidth + (choices.Count - 1) * spacing;
            float startX = -totalWidth / 2f + cardWidth / 2f;

            for (int i = 0; i < choices.Count; i++)
            {
                var augment = choices[i];
                var cardGO = new GameObject($"AugmentCard_{i}");
                cardGO.transform.SetParent(augmentPanel.transform, false);
                var cardRt = cardGO.AddComponent<RectTransform>();
                cardRt.anchorMin = new Vector2(0.5f, 0.5f);
                cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                cardRt.sizeDelta = new Vector2(cardWidth, 220);
                cardRt.anchoredPosition = new Vector2(startX + i * (cardWidth + spacing), 0);

                var cardImg = cardGO.AddComponent<Image>();
                cardImg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

                // Color bar
                var barGO = new GameObject("ColorBar");
                barGO.transform.SetParent(cardGO.transform, false);
                var barRt = barGO.AddComponent<RectTransform>();
                barRt.anchorMin = new Vector2(0, 1);
                barRt.anchorMax = new Vector2(1, 1);
                barRt.pivot = new Vector2(0.5f, 1);
                barRt.sizeDelta = new Vector2(0, 6);
                barRt.anchoredPosition = Vector2.zero;
                var barImg = barGO.AddComponent<Image>();
                barImg.color = augment.iconColor;
                barImg.raycastTarget = false;

                // Name
                var nameGO = new GameObject("Name");
                nameGO.transform.SetParent(cardGO.transform, false);
                var nameRt = nameGO.AddComponent<RectTransform>();
                nameRt.anchorMin = new Vector2(0, 1);
                nameRt.anchorMax = new Vector2(1, 1);
                nameRt.pivot = new Vector2(0.5f, 1);
                nameRt.sizeDelta = new Vector2(-16, 36);
                nameRt.anchoredPosition = new Vector2(0, -12);
                var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
                nameTmp.text = augment.augmentName;
                nameTmp.fontSize = 18;
                nameTmp.alignment = TextAlignmentOptions.Center;
                nameTmp.color = Color.white;
                nameTmp.fontStyle = FontStyles.Bold;
                nameTmp.raycastTarget = false;

                // Tier
                var tierGO = new GameObject("Tier");
                tierGO.transform.SetParent(cardGO.transform, false);
                var tierRt = tierGO.AddComponent<RectTransform>();
                tierRt.anchorMin = new Vector2(0, 1);
                tierRt.anchorMax = new Vector2(1, 1);
                tierRt.pivot = new Vector2(0.5f, 1);
                tierRt.sizeDelta = new Vector2(-16, 20);
                tierRt.anchoredPosition = new Vector2(0, -50);
                var tierTmp = tierGO.AddComponent<TextMeshProUGUI>();
                string tierStr = augment.tier == 1 ? "银" : (augment.tier == 2 ? "金" : "棱彩");
                tierTmp.text = $"[{tierStr}]";
                tierTmp.fontSize = 14;
                tierTmp.alignment = TextAlignmentOptions.Center;
                tierTmp.color = augment.tier == 1 ? Color.gray : (augment.tier == 2 ? new Color(1f, 0.85f, 0.3f) : Color.magenta);
                tierTmp.raycastTarget = false;

                // Description
                var descGO = new GameObject("Desc");
                descGO.transform.SetParent(cardGO.transform, false);
                var descRt = descGO.AddComponent<RectTransform>();
                descRt.anchorMin = new Vector2(0, 0);
                descRt.anchorMax = new Vector2(1, 1);
                descRt.offsetMin = new Vector2(10, 10);
                descRt.offsetMax = new Vector2(-10, -76);
                var descTmp = descGO.AddComponent<TextMeshProUGUI>();
                descTmp.text = augment.description;
                descTmp.fontSize = 14;
                descTmp.alignment = TextAlignmentOptions.Center;
                descTmp.color = new Color(0.8f, 0.8f, 0.8f);
                descTmp.raycastTarget = false;

                var btn = cardGO.AddComponent<Button>();
                int idx = i;
                btn.onClick.AddListener(() => OnAugmentCardClicked(idx));
            }

            ApplyFontToAll();
        }

        void OnAugmentCardClicked(int index)
        {
            if (displayedAugmentChoices == null || index < 0 || index >= displayedAugmentChoices.Count) return;
            HideAugmentSelection();
            GameLoopManager.Instance?.OnAugmentSelected(index);
        }

        public void HideAugmentSelection()
        {
            if (augmentPanel != null)
            {
                Destroy(augmentPanel);
                augmentPanel = null;
            }
            displayedAugmentChoices = null;
        }

        // ========== Carousel Selection ==========

        public void ShowCarouselSelection(List<CarouselItemData> items)
        {
            HideCarouselSelection();
            if (items == null || items.Count == 0) return;
            displayedCarouselItems = items;

            carouselPanel = new GameObject("CarouselPanel");
            carouselPanel.transform.SetParent(transform, false);
            var rt = carouselPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = carouselPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(carouselPanel.transform, false);
            var titleRt = titleGO.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.sizeDelta = new Vector2(500, 50);
            titleRt.anchoredPosition = new Vector2(0, 140);
            var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "选秀轮 - 选择一个英雄";
            titleTmp.fontSize = 28;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.4f, 0.9f, 1f);
            titleTmp.raycastTarget = false;
            titleTmp.fontStyle = FontStyles.Bold;

            float cardWidth = 120;
            float spacing = 10;
            float totalWidth = items.Count * cardWidth + (items.Count - 1) * spacing;
            float startX = -totalWidth / 2f + cardWidth / 2f;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var cardGO = new GameObject($"CarouselCard_{i}");
                cardGO.transform.SetParent(carouselPanel.transform, false);
                var cardRt = cardGO.AddComponent<RectTransform>();
                cardRt.anchorMin = new Vector2(0.5f, 0.5f);
                cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                cardRt.sizeDelta = new Vector2(cardWidth, 160);
                cardRt.anchoredPosition = new Vector2(startX + i * (cardWidth + spacing), 0);

                var cardImg = cardGO.AddComponent<Image>();
                cardImg.color = item.picked ? new Color(0.3f, 0.3f, 0.3f, 0.5f) : new Color(0.15f, 0.2f, 0.25f, 0.95f);

                // Hero name
                var nameGO = new GameObject("Name");
                nameGO.transform.SetParent(cardGO.transform, false);
                var nameRt = nameGO.AddComponent<RectTransform>();
                nameRt.anchorMin = new Vector2(0, 1);
                nameRt.anchorMax = new Vector2(1, 1);
                nameRt.pivot = new Vector2(0.5f, 1);
                nameRt.sizeDelta = new Vector2(-8, 30);
                nameRt.anchoredPosition = new Vector2(0, -8);
                var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
                nameTmp.text = item.heroData?.heroName ?? "???";
                nameTmp.fontSize = 16;
                nameTmp.alignment = TextAlignmentOptions.Center;
                nameTmp.color = item.picked ? Color.gray : Color.white;
                nameTmp.fontStyle = FontStyles.Bold;
                nameTmp.raycastTarget = false;

                // Hero color indicator
                var colorGO = new GameObject("Color");
                colorGO.transform.SetParent(cardGO.transform, false);
                var colorRt = colorGO.AddComponent<RectTransform>();
                colorRt.anchorMin = new Vector2(0.5f, 0.5f);
                colorRt.anchorMax = new Vector2(0.5f, 0.5f);
                colorRt.sizeDelta = new Vector2(40, 40);
                colorRt.anchoredPosition = new Vector2(0, 10);
                var colorImg = colorGO.AddComponent<Image>();
                colorImg.color = item.heroData?.displayColor ?? Color.white;
                colorImg.raycastTarget = false;

                // Equipment name
                var eqGO = new GameObject("Equipment");
                eqGO.transform.SetParent(cardGO.transform, false);
                var eqRt = eqGO.AddComponent<RectTransform>();
                eqRt.anchorMin = new Vector2(0, 0);
                eqRt.anchorMax = new Vector2(1, 0);
                eqRt.pivot = new Vector2(0.5f, 0);
                eqRt.sizeDelta = new Vector2(-8, 40);
                eqRt.anchoredPosition = new Vector2(0, 8);
                var eqTmp = eqGO.AddComponent<TextMeshProUGUI>();
                eqTmp.text = item.equipmentData?.equipmentName ?? "无装备";
                eqTmp.fontSize = 13;
                eqTmp.alignment = TextAlignmentOptions.Center;
                eqTmp.color = item.picked ? Color.gray : new Color(0.6f, 0.9f, 1f);
                eqTmp.raycastTarget = false;

                if (item.picked && !string.IsNullOrEmpty(item.pickedByName))
                {
                    var pickerGO = new GameObject("Picker");
                    pickerGO.transform.SetParent(cardGO.transform, false);
                    var pickerRt = pickerGO.AddComponent<RectTransform>();
                    pickerRt.anchorMin = new Vector2(0, 0.5f);
                    pickerRt.anchorMax = new Vector2(1, 0.5f);
                    pickerRt.sizeDelta = new Vector2(-8, 20);
                    pickerRt.anchoredPosition = new Vector2(0, -25);
                    var pickerTmp = pickerGO.AddComponent<TextMeshProUGUI>();
                    pickerTmp.text = item.pickedByName;
                    pickerTmp.fontSize = 12;
                    pickerTmp.alignment = TextAlignmentOptions.Center;
                    pickerTmp.color = new Color(1f, 0.6f, 0.3f);
                    pickerTmp.fontStyle = FontStyles.Italic;
                    pickerTmp.raycastTarget = false;
                }

                if (!item.picked)
                {
                    var btn = cardGO.AddComponent<Button>();
                    int idx = i;
                    btn.onClick.AddListener(() => OnCarouselCardClicked(idx));
                }
            }

            ApplyFontToAll();
        }

        void OnCarouselCardClicked(int index)
        {
            if (displayedCarouselItems == null || index < 0 || index >= displayedCarouselItems.Count) return;
            if (displayedCarouselItems[index].picked) return;
            GameLoopManager.Instance?.OnCarouselSelected(index);
        }

        public void HideCarouselSelection()
        {
            if (carouselPanel != null)
            {
                Destroy(carouselPanel);
                carouselPanel = null;
            }
            displayedCarouselItems = null;
        }

        // ========== Battle Log ==========

        public void UpdateBattleLog()
        {
            if (battleLogText == null || CombatStatsTracker.Instance == null) return;
            var history = CombatStatsTracker.Instance.GetRecentHistory(50);
            var sb = new System.Text.StringBuilder();
            foreach (var record in history)
            {
                string result = record.won ? "<color=green>胜</color>" : "<color=red>负</color>";
                string dmgStr = record.damage > 0 ? $" -{record.damage}" : "";
                sb.AppendLine($"R{record.round} vs {record.opponentName} {result}{dmgStr}");
            }
            battleLogText.text = sb.ToString();
            if (battleLogScrollRect != null)
                Canvas.ForceUpdateCanvases();
            if (battleLogScrollRect != null)
                battleLogScrollRect.verticalNormalizedPosition = 0f;
        }

        public void OnRestartClicked()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (resultText != null) resultText.gameObject.SetActive(false);
            GameLoopManager.Instance?.RestartGame();
        }
    }
}
