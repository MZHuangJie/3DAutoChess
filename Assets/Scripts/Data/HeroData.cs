using UnityEngine;

namespace AutoChess.Data
{
    public enum AttackType { Melee, Ranged }
    public enum SkillType { None, Damage, AreaDamage, Heal, Stun }
    public enum SkillTargetType { NearestEnemy, LowestHpAlly, Self, AllEnemiesInRange }

    [CreateAssetMenu(fileName = "HeroData", menuName = "AutoChess/HeroData")]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        public string heroName = "Hero";
        public int cost = 1;
        public Color displayColor = Color.white;

        [Header("Stats (Base at 1-Star)")]
        public int maxHealth = 500;
        public int attackDamage = 50;
        public float attackSpeed = 0.7f;
        public int armor = 20;
        public int magicResist = 20;
        public float attackRange = 1f;
        public int maxMana = 100;
        public int startingMana = 0;
        public AttackType attackType = AttackType.Melee;

        [Header("Factions")]
        public string[] factions;

        [Header("Skill")]
        public string skillName = "";
        public SkillType skillType = SkillType.None;
        public SkillTargetType skillTargetType = SkillTargetType.NearestEnemy;
        public int skillDamage = 0;
        public float skillRange = 2f;
        public float skillStunDuration = 0f;
        public bool skillIsMagic = true;
    }
}
