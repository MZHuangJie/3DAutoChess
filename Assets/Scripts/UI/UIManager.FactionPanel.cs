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
        private GameObject factionPanel;
        private GameObject factionContent;

        void CreateFactionPanel()
        {
            if (factionPanel != null) return;

            factionPanel = new GameObject("FactionPanel");
            factionPanel.transform.SetParent(transform, false);
            var rt = factionPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.08f, 0.45f);
            rt.anchorMax = new Vector2(0.2f, 0.98f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

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

            factionContent = new GameObject("Content");
            factionContent.transform.SetParent(scrollGO.transform, false);
            var contentRt = factionContent.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0, 1);
            contentRt.sizeDelta = new Vector2(0, 0);
            var vlg = factionContent.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(4, 4, 2, 2);
            var csf = factionContent.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
        }

        void UpdateFactionUI(PlayerData human)
        {
            if (factionContent == null) return;

            foreach (Transform child in factionContent.transform)
                Destroy(child.gameObject);

            var pieces = BoardManager.Instance?.GetPiecesByOwner(human);
            if (pieces == null) return;

            var boardPieces = new List<ChessPiece>();
            foreach (var p in pieces)
            {
                if (p != null && !p.isOnBench)
                    boardPieces.Add(p);
            }
            if (boardPieces.Count == 0) return;

            var factionCounts = new Dictionary<string, int>();
            var countedHeroes = new HashSet<string>();
            foreach (var piece in boardPieces)
            {
                if (piece.heroData?.factions == null) continue;
                if (!countedHeroes.Add(piece.heroData.heroName)) continue;
                foreach (var f in piece.heroData.factions)
                {
                    if (!factionCounts.ContainsKey(f)) factionCounts[f] = 0;
                    factionCounts[f]++;
                }
            }

            if (factionCounts.Count == 0) return;

            var allFactions = FactionManager.Instance?.AllFactions;

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
                var le = go.AddComponent<LayoutElement>();
                le.preferredHeight = 26;
                le.flexibleWidth = 1;

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
            }
        }
    }
}
