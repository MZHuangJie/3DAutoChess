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
        // Shop panel fields
        private GameObject shopPanel;
        private List<GameObject> shopSlots = new List<GameObject>();
        private Button refreshButton;
        private Button lockButton;
        private Button upgradeButton;
        private Button shopToggleButton;
        private bool shopExpanded = true;
        public bool IsShopExpanded => shopExpanded;

        void CreateShopPanel()
        {
            if (shopPanel != null) return;

            shopPanel = new GameObject("ShopPanel");
            shopPanel.transform.SetParent(transform, false);
            var shopRt = shopPanel.AddComponent<RectTransform>();
            shopRt.anchorMin = new Vector2(0.15f, 0);
            shopRt.anchorMax = new Vector2(0.85f, 0);
            shopRt.pivot = new Vector2(0.5f, 0);
            shopRt.sizeDelta = new Vector2(0, 100);
            shopRt.anchoredPosition = Vector2.zero;

            var shopBg = shopPanel.AddComponent<Image>();
            shopBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            var slotContainer = new GameObject("SlotContainer");
            slotContainer.transform.SetParent(shopPanel.transform, false);
            var containerRt = slotContainer.AddComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.1f, 0.08f);
            containerRt.anchorMax = new Vector2(0.9f, 0.92f);
            containerRt.offsetMin = Vector2.zero;
            containerRt.offsetMax = Vector2.zero;
            var hlg = slotContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(4, 4, 4, 4);

            for (int i = 0; i < 5; i++)
            {
                var slot = new GameObject($"ShopSlot_{i}");
                slot.transform.SetParent(slotContainer.transform, false);
                var le = slot.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                le.preferredHeight = 100;

                var img = slot.AddComponent<Image>();
                img.color = new Color(0.2f, 0.2f, 0.25f, 1f);

                var nameGO = new GameObject("Name");
                nameGO.transform.SetParent(slot.transform, false);
                var nameRt = nameGO.AddComponent<RectTransform>();
                nameRt.anchorMin = Vector2.zero;
                nameRt.anchorMax = Vector2.one;
                nameRt.offsetMin = Vector2.zero;
                nameRt.offsetMax = Vector2.zero;
                var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
                nameTmp.text = "";
                nameTmp.fontSize = 16;
                nameTmp.alignment = TextAlignmentOptions.Center;
                nameTmp.color = Color.white;

                var costGO = new GameObject("Cost");
                costGO.transform.SetParent(slot.transform, false);
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

                var factionGO = new GameObject("Factions");
                factionGO.transform.SetParent(slot.transform, false);
                var factionRt = factionGO.AddComponent<RectTransform>();
                factionRt.anchorMin = new Vector2(0, 0);
                factionRt.anchorMax = new Vector2(1, 0);
                factionRt.sizeDelta = new Vector2(-4, 30);
                factionRt.anchoredPosition = new Vector2(0, 18);
                var factionTmp = factionGO.AddComponent<TextMeshProUGUI>();
                factionTmp.text = "";
                factionTmp.fontSize = 10;
                factionTmp.alignment = TextAlignmentOptions.Center;
                factionTmp.color = new Color(0.5f, 0.82f, 1f);
                factionTmp.enableWordWrapping = true;

                var btn = slot.AddComponent<Button>();
                int index = i;
                btn.onClick.AddListener(() => OnShopSlotClicked(index));

                shopSlots.Add(slot);
            }

            var leftBtnContainer = new GameObject("LeftButtons");
            leftBtnContainer.transform.SetParent(shopPanel.transform, false);
            var leftRt = leftBtnContainer.AddComponent<RectTransform>();
            leftRt.anchorMin = new Vector2(0, 0);
            leftRt.anchorMax = new Vector2(0.1f, 1);
            leftRt.offsetMin = new Vector2(4, 8);
            leftRt.offsetMax = new Vector2(-2, -8);
            var leftVlg = leftBtnContainer.AddComponent<VerticalLayoutGroup>();
            leftVlg.spacing = 6;
            leftVlg.childAlignment = TextAnchor.MiddleCenter;
            leftVlg.childForceExpandWidth = true;
            leftVlg.childForceExpandHeight = true;

            upgradeButton = CreateShopButtonLayout("升级 (4金)", OnUpgradeClicked, leftBtnContainer);
            refreshButton = CreateShopButtonLayout("刷新 (2金)", OnRefreshClicked, leftBtnContainer);

            var rightBtnContainer = new GameObject("RightButtons");
            rightBtnContainer.transform.SetParent(shopPanel.transform, false);
            var rightRt = rightBtnContainer.AddComponent<RectTransform>();
            rightRt.anchorMin = new Vector2(0.9f, 0);
            rightRt.anchorMax = new Vector2(1, 1);
            rightRt.offsetMin = new Vector2(2, 8);
            rightRt.offsetMax = new Vector2(-4, -8);
            var rightVlg = rightBtnContainer.AddComponent<VerticalLayoutGroup>();
            rightVlg.spacing = 6;
            rightVlg.childAlignment = TextAnchor.MiddleCenter;
            rightVlg.childForceExpandWidth = true;
            rightVlg.childForceExpandHeight = true;

            lockButton = CreateShopButtonLayout("锁定", OnLockClicked, rightBtnContainer);

            var toggleGO = new GameObject("ShopToggle");
            toggleGO.transform.SetParent(transform, false);
            var toggleRt = toggleGO.AddComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(0.5f, 0);
            toggleRt.anchorMax = new Vector2(0.5f, 0);
            toggleRt.pivot = new Vector2(0.5f, 0);
            toggleRt.sizeDelta = new Vector2(100, 28);
            toggleRt.anchoredPosition = new Vector2(0, 100);

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

        Button CreateShopButtonLayout(string label, UnityEngine.Events.UnityAction action, GameObject parent)
        {
            var btnGO = new GameObject(label.Replace(" ", ""));
            btnGO.transform.SetParent(parent.transform, false);
            var le = btnGO.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.flexibleHeight = 1;

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
            txt.fontSize = 13;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;

            return btn;
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
                var factionTmp = slot.transform.Find("Factions")?.GetComponent<TextMeshProUGUI>();
                var img = slot.GetComponent<Image>();

                if (hero != null)
                {
                    if (nameTmp != null) nameTmp.text = hero.heroName;
                    if (costTmp != null) costTmp.text = $"{hero.cost} 金";
                    if (factionTmp != null)
                        factionTmp.text = hero.factions != null && hero.factions.Length > 0
                            ? string.Join(" / ", hero.factions) : "";
                    img.color = hero.displayColor;
                }
                else
                {
                    if (nameTmp != null) nameTmp.text = "";
                    if (costTmp != null) costTmp.text = "";
                    if (factionTmp != null) factionTmp.text = "";
                    img.color = new Color(0.2f, 0.2f, 0.25f, 1f);
                }
            }

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
                toggleRt.anchoredPosition = new Vector2(0, shopExpanded ? 100 : 0);
        }
    }
}
