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
        private GameObject carouselPanel;
        private List<CarouselItemData> displayedCarouselItems;

        public void ShowCarouselSelection(List<CarouselItemData> items, bool humanCanPick = false)
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
            titleTmp.text = humanCanPick ? "选秀轮 - 选择一个英雄" : "选秀轮 - 等待其他玩家选择...";
            titleTmp.fontSize = 28;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = humanCanPick ? new Color(0.4f, 0.9f, 1f) : new Color(0.6f, 0.6f, 0.6f);
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
    }
}
