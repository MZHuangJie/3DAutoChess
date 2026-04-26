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
        private GameObject augmentPanel;
        private List<AugmentData> displayedAugmentChoices;

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
            titleRt.anchorMin = new Vector2(0.1f, 0.7f);
            titleRt.anchorMax = new Vector2(0.9f, 0.8f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;
            var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "选择一个海克斯天赋";
            titleTmp.fontSize = 30;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(1f, 0.85f, 0.3f);
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.raycastTarget = false;

            var cardContainer = new GameObject("CardContainer");
            cardContainer.transform.SetParent(augmentPanel.transform, false);
            var ccRt = cardContainer.AddComponent<RectTransform>();
            ccRt.anchorMin = new Vector2(0.15f, 0.25f);
            ccRt.anchorMax = new Vector2(0.85f, 0.68f);
            ccRt.offsetMin = Vector2.zero;
            ccRt.offsetMax = Vector2.zero;
            var ccHlg = cardContainer.AddComponent<HorizontalLayoutGroup>();
            ccHlg.spacing = 20;
            ccHlg.childAlignment = TextAnchor.MiddleCenter;
            ccHlg.childForceExpandWidth = true;
            ccHlg.childForceExpandHeight = true;
            ccHlg.padding = new RectOffset(10, 10, 10, 10);

            for (int i = 0; i < choices.Count; i++)
            {
                var augment = choices[i];
                var cardGO = new GameObject($"AugmentCard_{i}");
                cardGO.transform.SetParent(cardContainer.transform, false);
                var cardLe = cardGO.AddComponent<LayoutElement>();
                cardLe.flexibleWidth = 1;
                cardLe.flexibleHeight = 1;

                var cardImg = cardGO.AddComponent<Image>();
                cardImg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

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
    }
}
