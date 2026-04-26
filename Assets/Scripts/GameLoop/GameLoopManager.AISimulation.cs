using UnityEngine;
using System.Collections.Generic;
using AutoChess.Data;

namespace AutoChess.Core
{
    public partial class GameLoopManager
    {
        void SimulateAIVersusAI()
        {
            var aliveAIs = new List<PlayerData>();
            foreach (var p in allPlayers)
            {
                if (!p.isHuman && p.IsAlive)
                    aliveAIs.Add(p);
            }

            if (aliveAIs.Count <= 1) return;

            ShuffleList(aliveAIs);
            for (int i = 0; i < aliveAIs.Count - 1; i += 2)
            {
                SimulateMatch(aliveAIs[i], aliveAIs[i + 1]);
            }

            if (aliveAIs.Count % 2 == 1)
            {
                var last = aliveAIs[aliveAIs.Count - 1];
                last.winStreak++;
                last.loseStreak = 0;
                last.health = Mathf.Min(gameConfig.startingHealth, last.health + 1);
            }
        }

        void SimulateAICreepRounds()
        {
            if (currentCreepData == null) return;

            float creepPower = 0f;
            if (currentCreepData.creeps != null)
            {
                foreach (var c in currentCreepData.creeps)
                    creepPower += c.health * c.attackDamage * 0.01f;
            }

            foreach (var ai in allPlayers)
            {
                if (ai.isHuman || !ai.IsAlive) continue;

                float aiPower = CalculatePower(ai) * Random.Range(0.85f, 1.15f);
                bool won = aiPower >= creepPower;

                if (won)
                {
                    creepManager?.GrantRewards(ai, currentCreepData);
                    ai.winStreak++;
                    ai.loseStreak = 0;
                    Debug.Log($"[AI PvE] {ai.playerName} beat {currentCreepData.roundName}");
                }
                else
                {
                    int pveDmg = gameConfig.pveLossDamage;
                    if (pveDmg > 0)
                    {
                        ai.health -= pveDmg;
                        if (ai.health < 0) ai.health = 0;
                    }
                    ai.loseStreak++;
                    ai.winStreak = 0;
                    Debug.Log($"[AI PvE] {ai.playerName} lost to {currentCreepData.roundName}, took {pveDmg} dmg");
                }
            }
        }

        void SimulateMatch(PlayerData p1, PlayerData p2)
        {
            float power1 = CalculatePower(p1);
            float power2 = CalculatePower(p2);

            bool p1Wins = power1 >= power2;
            var winner = p1Wins ? p1 : p2;
            var loser = p1Wins ? p2 : p1;

            int damage = 2 + Random.Range(1, 4);
            loser.health -= damage;
            if (loser.health < 0) loser.health = 0;

            economyManager.UpdateStreaks(winner, loser);

            Debug.Log($"[AI Sim] {p1.playerName}({power1:F0}) vs {p2.playerName}({power2:F0}) -> {winner.playerName} wins, {loser.playerName} takes {damage} dmg");
        }

        float CalculatePower(PlayerData player)
        {
            if (!playerBoards.ContainsKey(player)) return 0;
            var pb = playerBoards[player];
            float power = 0;
            foreach (var info in pb.boardInfos)
            {
                float starMult = info.starLevel == 2 ? 1.8f : (info.starLevel == 3 ? 3.6f : 1f);
                power += info.heroData.maxHealth * info.heroData.attackDamage * starMult * 0.01f;
            }
            power *= Random.Range(0.85f, 1.15f);
            return power;
        }
    }
}
