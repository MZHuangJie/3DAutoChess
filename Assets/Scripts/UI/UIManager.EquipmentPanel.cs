using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using AutoChess.Core;
using AutoChess.Data;

namespace AutoChess.UI
{
    public partial class UIManager
    {
        private GameObject equipmentPanel;
        private List<GameObject> equipmentSlots = new List<GameObject>();
        private TextMeshProUGUI equipmentTitleText;
        private int equipmentPage = 0;
        private const int equipmentSlotsPerPage = 10;
        private const int equipmentTotalSlots = 20;
        private TextMeshProUGUI equipPageText;

        void CreateEquipmentPanel()
        {
            if (equipmentPanel != null) return;

            equipmentPanel = new GameObject("EquipmentPanel");
            equipmentPanel.transform.SetParent(transform, false);
            var rt = equipmentPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.45f);
            rt.anchorMax = new Vector2(0.08f, 0.98f);
            rt.offsetMin = new Vector2(4, 0);
            rt.offsetMax = Vector2.zero;

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

            var slotContainer = new GameObject("SlotContainer");
            slotContainer.transform.SetParent(equipmentPanel.transform, false);
            var scRt = slotContainer.AddComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0, 0.06f);
            scRt.anchorMax = Vector2.one;
            scRt.offsetMin = new Vector2(2, 0);
            scRt.offsetMax = new Vector2(-2, -26);
            var eqVlg = slotContainer.AddComponent<VerticalLayoutGroup>();
            eqVlg.spacing = 2;
            eqVlg.childAlignment = TextAnchor.UpperCenter;
            eqVlg.childForceExpandWidth = true;
            eqVlg.childForceExpandHeight = false;
            eqVlg.padding = new RectOffset(2, 2, 2, 2);

            for (int i = 0; i < equipmentTotalSlots; i++)
            {
                var slot = new GameObject($"EqSlot_{i}");
                slot.transform.SetParent(slotContainer.transform, false);
                var slotLe = slot.AddComponent<LayoutElement>();
                slotLe.preferredHeight = 28;
                slotLe.flexibleWidth = 1;

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

            var navContainer = new GameObject("NavBar");
            navContainer.transform.SetParent(equipmentPanel.transform, false);
            var navRt = navContainer.AddComponent<RectTransform>();
            navRt.anchorMin = new Vector2(0, 0);
            navRt.anchorMax = new Vector2(1, 0.06f);
            navRt.offsetMin = Vector2.zero;
            navRt.offsetMax = Vector2.zero;
            var navHlg = navContainer.AddComponent<HorizontalLayoutGroup>();
            navHlg.spacing = 2;
            navHlg.childAlignment = TextAnchor.MiddleCenter;
            navHlg.childForceExpandWidth = false;
            navHlg.childForceExpandHeight = true;

            var prevGO = new GameObject("PrevPage");
            prevGO.transform.SetParent(navContainer.transform, false);
            var prevLe = prevGO.AddComponent<LayoutElement>();
            prevLe.preferredWidth = 28;
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
            pageGO.transform.SetParent(navContainer.transform, false);
            var pageLe = pageGO.AddComponent<LayoutElement>();
            pageLe.flexibleWidth = 1;
            equipPageText = pageGO.AddComponent<TextMeshProUGUI>();
            equipPageText.text = "1/2";
            equipPageText.fontSize = 12;
            equipPageText.alignment = TextAlignmentOptions.Center;
            equipPageText.color = Color.white;

            var nextGO = new GameObject("NextPage");
            nextGO.transform.SetParent(navContainer.transform, false);
            var nextLe = nextGO.AddComponent<LayoutElement>();
            nextLe.preferredWidth = 28;
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
    }
}
