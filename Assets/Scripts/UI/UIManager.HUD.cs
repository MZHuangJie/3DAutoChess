using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using AutoChess.Core;
using AutoChess.Data;

namespace AutoChess.UI
{
    public partial class UIManager
    {
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

        IEnumerator HideMatchupAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (matchupText != null)
                matchupText.gameObject.SetActive(false);
        }

        public void OnRestartClicked()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (resultText != null) resultText.gameObject.SetActive(false);
            GameLoopManager.Instance?.RestartGame();
        }
    }
}
