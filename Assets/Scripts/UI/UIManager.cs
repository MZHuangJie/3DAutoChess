using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using AutoChess.Core;
using AutoChess.Data;

namespace AutoChess.UI
{
    public partial class UIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI playerHealthText;
        [SerializeField] private TextMeshProUGUI aiHealthText;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private TMP_FontAsset chineseFont;

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
                gameObject.AddComponent<GraphicRaycaster>();
            }

            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        void CreateMissingUI()
        {
            if (matchupText == null)
            {
                var go = CreateTextGO("MatchupText", "", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -120));
                matchupText = go.GetComponent<TextMeshProUGUI>();
                matchupText.fontStyle = FontStyles.Bold;
            }

            CreateShopPanel();
            CreateEquipmentPanel();
            CreateFactionPanel();
            CreatePlayerInfoPanel();
            CreateBattleLogPanel();
            CreatePieceDetailPanel();
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

            if (playerHealthText != null)
                playerHealthText.text = $"玩家: {human.health} HP";

            if (aiHealthText != null)
            {
                var opponent = human.lastOpponent;
                if (opponent != null && opponent.IsAlive)
                    aiHealthText.text = $"对手: {opponent.health} HP";
                else
                    aiHealthText.text = "";
            }

            if (goldText != null)
                goldText.text = $"金币: {human.gold}";

            if (populationText != null)
            {
                int current = human.GetCurrentBoardUnitCount();
                int max = human.GetMaxUnitsOnBoard();
                populationText.text = $"人口: {current}/{max}";
                populationText.color = current >= max ? Color.red : Color.white;
            }

            if (expText != null)
            {
                if (human.level >= 10)
                    expText.text = "等级: MAX";
                else
                    expText.text = $"经验: {human.exp}/{human.expToLevel[human.level]}";
            }

            if (interestText != null)
            {
                int interest = config != null ? human.GetInterest(config) : Mathf.Min(human.gold / 10, 5);
                interestText.text = $"利息: +{interest}";
            }

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

            UpdateShopUI(human, config);
            UpdateFactionUI(human);
            UpdateEquipmentUI(human);
            UpdatePlayerList();
            UpdateBattleLog();
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

        TextMeshProUGUI CreateLayoutText(GameObject parent, string name, string text, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = fontSize + 10;
            le.flexibleWidth = 1;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 8;
            tmp.fontSizeMax = fontSize;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;
            return tmp;
        }

        GameObject CreateTextGO(string name, string text, int fontSize, TextAnchor anchor, Vector2 anchorMinMax, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
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
    }
}
