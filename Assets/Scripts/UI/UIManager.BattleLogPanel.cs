using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using AutoChess.Core;
using AutoChess.Data;

namespace AutoChess.UI
{
    public partial class UIManager
    {
        private GameObject battleLogPanel;
        private UnityEngine.UI.ScrollRect battleLogScrollRect;
        private TextMeshProUGUI battleLogText;

        void CreateBattleLogPanel()
        {
            if (battleLogPanel != null) return;

            battleLogPanel = new GameObject("BattleLogPanel");
            battleLogPanel.transform.SetParent(transform, false);
            var rt = battleLogPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.85f, 0.15f);
            rt.anchorMax = new Vector2(1, 0.45f);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = Vector2.zero;

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
            var logCsf = logGO.AddComponent<ContentSizeFitter>();
            logCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

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
    }
}
