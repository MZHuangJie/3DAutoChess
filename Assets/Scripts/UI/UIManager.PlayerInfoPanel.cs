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
        private GameObject playerInfoPanel;
        private TextMeshProUGUI goldText;
        private TextMeshProUGUI populationText;
        private TextMeshProUGUI expText;
        private TextMeshProUGUI streakText;
        private TextMeshProUGUI interestText;
        private List<TextMeshProUGUI> playerListTexts = new List<TextMeshProUGUI>();
        private TextMeshProUGUI matchupText;

        void CreatePlayerInfoPanel()
        {
            if (playerInfoPanel != null) return;

            playerInfoPanel = new GameObject("PlayerInfoPanel");
            playerInfoPanel.transform.SetParent(transform, false);
            var rt = playerInfoPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.85f, 0.45f);
            rt.anchorMax = new Vector2(1, 0.98f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = playerInfoPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            var vlg = playerInfoPanel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(8, 8, 4, 4);

            if (playerHealthText == null)
            {
                playerHealthText = CreateLayoutText(playerInfoPanel, "HealthText", "玩家: 100 HP", 20);
            }
            else
            {
                playerHealthText.transform.SetParent(playerInfoPanel.transform, false);
                var le = playerHealthText.gameObject.GetComponent<LayoutElement>() ?? playerHealthText.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 28;
                le.flexibleWidth = 1;
            }

            if (goldText == null)
            {
                goldText = CreateLayoutText(playerInfoPanel, "GoldText", "金币: 5", 18);
                goldText.color = new Color(1f, 0.85f, 0.2f);
            }

            if (populationText == null)
            {
                populationText = CreateLayoutText(playerInfoPanel, "PopText", "人口: 1/3", 16);
            }

            if (expText == null)
            {
                expText = CreateLayoutText(playerInfoPanel, "ExpText", "经验: 0/2", 15);
                expText.color = new Color(0.6f, 0.8f, 1f);
            }

            if (interestText == null)
            {
                interestText = CreateLayoutText(playerInfoPanel, "InterestText", "利息: +0", 15);
                interestText.color = new Color(0.8f, 0.9f, 0.5f);
            }

            if (streakText == null)
            {
                streakText = CreateLayoutText(playerInfoPanel, "StreakText", "", 15);
            }

            var sep = new GameObject("Separator");
            sep.transform.SetParent(playerInfoPanel.transform, false);
            var sepLe = sep.AddComponent<LayoutElement>();
            sepLe.preferredHeight = 2;
            sepLe.flexibleWidth = 1;
            var sepImg = sep.AddComponent<Image>();
            sepImg.color = new Color(0.3f, 0.3f, 0.4f);

            if (aiHealthText == null)
            {
                aiHealthText = CreateLayoutText(playerInfoPanel, "OpponentText", "", 15);
                aiHealthText.color = new Color(1f, 0.5f, 0.5f);
            }
            else
            {
                aiHealthText.transform.SetParent(playerInfoPanel.transform, false);
                var le = aiHealthText.gameObject.GetComponent<LayoutElement>() ?? aiHealthText.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 23;
                le.flexibleWidth = 1;
            }

            playerListTexts.Clear();
            for (int i = 0; i < 8; i++)
            {
                var txt = CreateLayoutText(playerInfoPanel, $"PlayerList_{i}", "", 13);
                playerListTexts.Add(txt);
            }
        }

        void UpdatePlayerList()
        {
            if (playerListTexts == null) return;

            var allPlayers = GameLoopManager.Instance?.AllPlayers;
            if (allPlayers == null) return;

            var sorted = new List<PlayerData>(allPlayers);
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
    }
}
