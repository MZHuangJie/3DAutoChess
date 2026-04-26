using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AutoChess.Core;
using AutoChess.Data;

namespace AutoChess.UI
{
    public partial class UIManager
    {
        private GameObject titlePanel;

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
    }
}
