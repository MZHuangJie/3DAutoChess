using System.Collections.Generic;
using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class AugmentManager : MonoBehaviour
    {
        public static AugmentManager Instance { get; private set; }

        [SerializeField] private List<AugmentData> allAugments = new List<AugmentData>();

        void Awake()
        {
            Instance = this;
        }

        public void Setup(List<AugmentData> augments)
        {
            allAugments = augments;
        }

        public List<AugmentData> GenerateChoices(int tier)
        {
            var pool = allAugments.FindAll(a => a.tier == tier);
            var choices = new List<AugmentData>();
            var shuffled = new List<AugmentData>(pool);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            for (int i = 0; i < Mathf.Min(3, shuffled.Count); i++)
                choices.Add(shuffled[i]);
            return choices;
        }

        public int GetTierForRound(int round, int[] augmentRounds)
        {
            for (int i = 0; i < augmentRounds.Length; i++)
            {
                if (augmentRounds[i] == round)
                    return i + 1;
            }
            return 1;
        }

        public void ApplyAugment(PlayerData player, AugmentData augment)
        {
            if (player == null || augment == null) return;
            player.augments.Add(augment);

            switch (augment.effectType)
            {
                case AugmentEffectType.BonusGoldPerRound:
                    player.bonusGoldPerRound += Mathf.RoundToInt(augment.effectValue);
                    break;
                case AugmentEffectType.InterestCapBonus:
                    player.bonusInterestCap += Mathf.RoundToInt(augment.effectValue);
                    break;
                case AugmentEffectType.ShopSlotBonus:
                    player.bonusShopSlots += Mathf.RoundToInt(augment.effectValue);
                    break;
                case AugmentEffectType.FreeRefreshPerRound:
                    player.freeRefreshPerRound += Mathf.RoundToInt(augment.effectValue);
                    break;
                case AugmentEffectType.BenchSlotBonus:
                    player.bonusBenchSlots += Mathf.RoundToInt(augment.effectValue);
                    break;
                case AugmentEffectType.StartingGoldBonus:
                    player.gold += Mathf.RoundToInt(augment.effectValue);
                    break;
            }

            Debug.Log($"[Augment] {player.playerName} selected: {augment.augmentName}");
        }

        public void ApplyAugmentCombatBuffs(List<ChessPiece> pieces, PlayerData player)
        {
            if (player == null) return;
            foreach (var augment in player.augments)
            {
                foreach (var piece in pieces)
                {
                    if (piece == null || !piece.IsAlive) continue;
                    switch (augment.effectType)
                    {
                        case AugmentEffectType.AllAttackPercent:
                            piece.attackDamage += Mathf.RoundToInt(piece.attackDamage * augment.effectValue / 100f);
                            break;
                        case AugmentEffectType.AllHealthPercent:
                            int hpBonus = Mathf.RoundToInt(piece.maxHealth * augment.effectValue / 100f);
                            piece.maxHealth += hpBonus;
                            piece.currentHealth += hpBonus;
                            break;
                        case AugmentEffectType.AllAttackSpeedPercent:
                            piece.attackSpeed *= (1f + augment.effectValue / 100f);
                            break;
                        case AugmentEffectType.AllArmorFlat:
                            piece.armor += Mathf.RoundToInt(augment.effectValue);
                            break;
                        case AugmentEffectType.AllMagicResistFlat:
                            piece.magicResist += Mathf.RoundToInt(augment.effectValue);
                            break;
                    }
                }
            }
        }

        public void ApplyExpPerRound(PlayerData player, GameConfig config)
        {
            foreach (var augment in player.augments)
            {
                if (augment.effectType == AugmentEffectType.ExpPerRound)
                {
                    player.exp += Mathf.RoundToInt(augment.effectValue);
                    player.CheckLevelUp(config);
                }
            }
        }

        public AugmentData AIChoose(List<AugmentData> choices)
        {
            if (choices == null || choices.Count == 0) return null;
            return choices[Random.Range(0, choices.Count)];
        }
    }
}
