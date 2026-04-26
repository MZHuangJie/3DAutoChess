using UnityEngine;

namespace AutoChess.Data
{
    public enum AugmentEffectType
    {
        AllAttackPercent,
        AllHealthPercent,
        AllAttackSpeedPercent,
        AllArmorFlat,
        AllMagicResistFlat,
        BonusGoldPerRound,
        InterestCapBonus,
        ShopSlotBonus,
        FreeRefreshPerRound,
        ExpPerRound,
        BenchSlotBonus,
        StartingGoldBonus
    }

    [CreateAssetMenu(fileName = "NewAugment", menuName = "AutoChess/AugmentData")]
    public class AugmentData : ScriptableObject
    {
        public string augmentName;
        [TextArea] public string description;
        public int tier = 1;
        public Color iconColor = Color.white;
        public AugmentEffectType effectType;
        public float effectValue;
    }
}
