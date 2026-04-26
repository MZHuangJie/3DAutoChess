using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AutoChess.Data;

namespace AutoChess.Core
{
    public partial class GameLoopManager
    {
        public void EndCombat(bool attackerWon)
        {
            CurrentPhase = GamePhase.Result;

            var human = HumanPlayer;

            if (isCreepRound)
            {
                if (attackerWon && human != null)
                {
                    creepManager?.GrantRewards(human, currentCreepData);
                }
                else if (!attackerWon && human != null)
                {
                    int pveDmg = gameConfig.pveLossDamage;
                    if (pveDmg > 0)
                    {
                        human.health -= pveDmg;
                        if (human.health < 0) human.health = 0;
                    }
                }

                uiManager?.ShowCombatResult(attackerWon, 0);
                uiManager?.UpdateUI();

                combatStatsTracker?.RecordResult(CurrentRound, currentCreepData?.roundName ?? "野怪", attackerWon, 0, true);
                creepManager?.ClearCreeps();
                isCreepRound = false;
                currentCreepData = null;
            }
            else
            {
                var opponent = human?.lastOpponent;
                int damage = 0;
                bool draw = combatManager != null && combatManager.IsDraw;

                if (human != null && opponent != null)
                {
                    if (draw)
                    {
                        int humanStars = combatManager.GetAliveStarCount(human);
                        int opponentStars = combatManager.GetAliveStarCount(opponent);
                        int humanDmg = 2 + opponentStars;
                        int opponentDmg = 2 + humanStars;
                        human.health -= humanDmg;
                        opponent.health -= opponentDmg;
                        if (human.health < 0) human.health = 0;
                        if (opponent.health < 0) opponent.health = 0;
                        damage = humanDmg;
                        Debug.Log($"Draw! {human.playerName} takes {humanDmg} dmg, {opponent.playerName} takes {opponentDmg} dmg");
                    }
                    else
                    {
                        var winner = attackerWon ? human : opponent;
                        var loser = attackerWon ? opponent : human;

                        int aliveStars = combatManager.GetAliveStarCount(winner);
                        damage = 2 + aliveStars;
                        loser.health -= damage;
                        if (loser.health < 0) loser.health = 0;

                        economyManager.UpdateStreaks(winner, loser);
                    }
                }

                uiManager?.ShowCombatResult(draw ? true : attackerWon, damage);
                uiManager?.UpdateUI();

                combatStatsTracker?.RecordResult(CurrentRound, opponent?.playerName ?? "未知", draw || attackerWon, damage);

                ClearEnemySide();
            }

            var allPieces = boardManager.GetAllPiecesOnBoard();
            factionManager.ClearFactionsFromPieces(allPieces);

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            phaseCoroutine = StartCoroutine(ResultThenNextRound());
        }

        IEnumerator ResultThenNextRound()
        {
            yield return new WaitForSeconds(gameConfig.resultDuration);

            foreach (var player in allPlayers)
            {
                if (player.IsAlive || eliminatedPlayers.Contains(player)) continue;
                eliminatedPlayers.Insert(0, player);
                player.placement = allPlayers.Count - eliminatedPlayers.Count + 1;
                Debug.Log($"{player.playerName} 被淘汰! 排名: {player.placement}");
            }

            int aliveCount = 0;
            PlayerData lastAlive = null;
            foreach (var player in allPlayers)
            {
                if (player.IsAlive)
                {
                    aliveCount++;
                    lastAlive = player;
                }
            }

            var human = HumanPlayer;
            bool humanAlive = human != null && human.IsAlive;

            if (aliveCount <= 1 || !humanAlive)
            {
                CurrentPhase = GamePhase.GameOver;
                if (lastAlive != null && lastAlive.placement == 0)
                    lastAlive.placement = 1;

                for (int i = 0; i < eliminatedPlayers.Count; i++)
                {
                    if (eliminatedPlayers[i].placement == 0)
                        eliminatedPlayers[i].placement = allPlayers.Count - i;
                }

                uiManager?.ShowGameOver(humanAlive, eliminatedPlayers);
                yield break;
            }

            CurrentRound++;

            CleanupDeadPlayers();

            StartNextPhase();
        }

        void CleanupDeadPlayers()
        {
            foreach (var player in allPlayers)
            {
                if (player.IsAlive) continue;
                var pieces = boardManager.GetPiecesByOwner(player, false);
                foreach (var p in pieces)
                {
                    if (p != null && p.gameObject != null)
                    {
                        boardManager.RemovePieceFromAnywhere(p);
                        boardManager.RemoveFromTracking(p);
                        Destroy(p.gameObject);
                    }
                }
                player.boardPieces.Clear();
                player.benchPieces.Clear();
            }
        }
    }
}
