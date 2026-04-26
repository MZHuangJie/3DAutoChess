using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [SerializeField] private GameConfig gameConfig;

        void Awake()
        {
            Instance = this;
        }

        public void Setup(GameConfig config)
        {
            gameConfig = config;
        }

        public void GrantRoundIncome(PlayerData player, int currentRound)
        {
            if (player == null || !player.IsAlive) return;

            int baseIncome = currentRound == 1 ? 1 : (currentRound == 2 ? 2 : gameConfig.baseIncomePerRound);
            int interest = player.GetInterest(gameConfig);
            int streakBonus = GetStreakBonus(player);

            int total = baseIncome + interest + streakBonus + player.bonusGoldPerRound;
            player.gold += total;

            Debug.Log($"{player.playerName} income: Base={baseIncome}, Interest={interest}, Streak={streakBonus}, Augment={player.bonusGoldPerRound}, Total={total}. Now has {player.gold} gold.");
        }

        int GetStreakBonus(PlayerData player)
        {
            int streak = Mathf.Max(player.winStreak, player.loseStreak);
            if (streak >= gameConfig.streakBonus.Length)
                return gameConfig.streakBonus[gameConfig.streakBonus.Length - 1];
            return gameConfig.streakBonus[streak];
        }

        public void UpdateStreaks(PlayerData winner, PlayerData loser)
        {
            winner.winStreak++;
            winner.loseStreak = 0;
            loser.loseStreak++;
            loser.winStreak = 0;
        }
    }
}
