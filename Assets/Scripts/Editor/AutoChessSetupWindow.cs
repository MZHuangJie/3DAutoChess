#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;
using TMPro;
using AutoChess.Data;
using AutoChess.Core;
using AutoChess.AI;
using AutoChess.UI;

namespace AutoChess.Editor
{
    public class AutoChessSetupWindow : EditorWindow
    {
        [MenuItem("AutoChess/Setup Complete Scene")]
        public static void SetupCompleteScene()
        {
            // 1. Create ScriptableObjects
            var gameConfig = CreateGameConfig();
            var heroes = CreateHeroes();
            var factions = CreateFactions();
            var equipment = CreateEquipment();
            var creepRounds = CreateCreepRounds();
            var augments = CreateAugments();

            // 2. Create Materials
            var mat = CreatePieceMaterial();

            // 3. Create Piece Prefab
            var piecePrefab = CreatePiecePrefab(mat);

            // 4. Setup Scene
            SetupScene(gameConfig, heroes, factions, equipment, creepRounds, augments, piecePrefab);

            // Save
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Setup Complete",
                "无限恐怖自走棋 scene has been set up!\n\n" +
                "Press Play to start the game.", "OK");
        }

        static GameConfig CreateGameConfig()
        {
            EnsureDirectory("Assets/ScriptableObjects");
            string path = "Assets/ScriptableObjects/GameConfig.asset";
            var config = ScriptableObject.CreateInstance<GameConfig>();
            config.boardRows = 4;
            config.boardCols = 7;
            config.benchSlots = 9;
            config.cellSize = 1.2f;
            config.boardSpacing = 2f;
            config.preparationDuration = 30f;
            config.combatMaxDuration = 30f;
            config.resultDuration = 3f;
            config.startingHealth = 100;
            config.startingGold = 5;
            config.startingLevel = 1;
            config.maxLevel = 10;
            config.baseIncomePerRound = 5;
            config.maxInterest = 5;
            config.interestPer10Gold = 1;
            config.streakBonus = new int[] { 0, 0, 1, 2, 3 };
            config.shopSlotCount = 5;
            config.refreshCost = 2;
            config.expCost = new int[] { 0, 0, 2, 6, 10, 14, 20, 36, 76, 84 };
            config.expBuyCost = 4;
            config.expPerBuy = 4;
            config.poolCountByCost = new int[] { 0, 39, 26, 18, 13, 10 };
            config.level1Prob = new int[] { 100, 0, 0, 0, 0 };
            config.level2Prob = new int[] { 75, 25, 0, 0, 0 };
            config.level3Prob = new int[] { 55, 30, 15, 0, 0 };
            config.level4Prob = new int[] { 45, 33, 20, 2, 0 };
            config.level5Prob = new int[] { 35, 35, 25, 5, 0 };
            config.level6Prob = new int[] { 22, 35, 30, 10, 3 };
            config.level7Prob = new int[] { 15, 25, 35, 20, 5 };
            config.level8Prob = new int[] { 12, 20, 30, 28, 10 };
            config.level9Prob = new int[] { 10, 15, 22, 35, 18 };
            config.level10Prob = new int[] { 5, 10, 18, 35, 32 };
            config.attackTickRate = 0.1f;
            config.moveSpeed = 3f;
            config.pveRounds = new int[] { 1, 2, 3, 10, 17, 24 };
            config.pveLossDamage = 0;

            AssetDatabase.CreateAsset(config, path);
            return config;
        }

        static System.Collections.Generic.List<HeroData> CreateHeroes()
        {
            EnsureDirectory("Assets/ScriptableObjects/Heroes");
            var heroes = new System.Collections.Generic.List<HeroData>();

            // === 1费 (7个) ===
            var h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "萧宏吕"; h.cost = 1;
            h.displayColor = new Color(0.2f, 0.6f, 0.9f);
            h.maxHealth = 700; h.attackDamage = 55; h.attackSpeed = 0.65f;
            h.armor = 35; h.magicResist = 20; h.attackRange = 1f;
            h.maxMana = 100; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "中洲队", "武术" };
            h.skillName = "铁拳"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 140; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_XiaoHongLv.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "洛丽"; h.cost = 1;
            h.displayColor = new Color(0.6f, 0.2f, 0.8f);
            h.maxHealth = 500; h.attackDamage = 35; h.attackSpeed = 0.60f;
            h.armor = 20; h.magicResist = 30; h.attackRange = 3f;
            h.maxMana = 100; h.startingMana = 0;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "中洲队", "精神力" };
            h.skillName = "精神治愈"; h.skillType = SkillType.Heal;
            h.skillTargetType = SkillTargetType.LowestHpAlly;
            h.skillDamage = 150; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_Luoli.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "丧尸步兵"; h.cost = 1;
            h.displayColor = new Color(0.7f, 0.3f, 0.2f);
            h.maxHealth = 750; h.attackDamage = 50; h.attackSpeed = 0.55f;
            h.armor = 30; h.magicResist = 15; h.attackRange = 1f;
            h.maxMana = 120; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "生化危机", "血族" };
            h.skillName = "撕咬"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 120; h.skillRange = 1.5f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_ZombieSoldier.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "半兽人"; h.cost = 1;
            h.displayColor = new Color(0.9f, 0.8f, 0.2f);
            h.maxHealth = 800; h.attackDamage = 45; h.attackSpeed = 0.55f;
            h.armor = 40; h.magicResist = 20; h.attackRange = 1f;
            h.maxMana = 150; h.startingMana = 50;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "指环王", "血族" };
            h.skillName = "战吼"; h.skillType = SkillType.Stun;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 40; h.skillRange = 2f; h.skillStunDuration = 1.5f; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_Orc.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "抱脸虫"; h.cost = 1;
            h.displayColor = new Color(0.3f, 0.6f, 0.3f);
            h.maxHealth = 550; h.attackDamage = 65; h.attackSpeed = 0.85f;
            h.armor = 15; h.magicResist = 15; h.attackRange = 1f;
            h.maxMana = 80; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "异形", "武术" };
            h.skillName = "寄生突袭"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 160; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_Facehugger.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "安布雷拉士兵"; h.cost = 1;
            h.displayColor = new Color(0.7f, 0.3f, 0.2f);
            h.maxHealth = 450; h.attackDamage = 60; h.attackSpeed = 0.75f;
            h.armor = 20; h.magicResist = 15; h.attackRange = 3f;
            h.maxMana = 90; h.startingMana = 0;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "生化危机", "科技" };
            h.skillName = "扫射"; h.skillType = SkillType.AreaDamage;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 70; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_UmbrellaSoldier.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "哈比人"; h.cost = 1;
            h.displayColor = new Color(0.9f, 0.8f, 0.2f);
            h.maxHealth = 500; h.attackDamage = 40; h.attackSpeed = 0.60f;
            h.armor = 15; h.magicResist = 25; h.attackRange = 3f;
            h.maxMana = 100; h.startingMana = 0;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "指环王", "修真" };
            h.skillName = "星光瓶"; h.skillType = SkillType.Heal;
            h.skillTargetType = SkillTargetType.LowestHpAlly;
            h.skillDamage = 120; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_Hobbit.asset");
            heroes.Add(h);

            // === 2费 (6) ===
            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "张恒"; h.cost = 2;
            h.displayColor = new Color(0.3f, 0.8f, 0.9f);
            h.maxHealth = 600; h.attackDamage = 80; h.attackSpeed = 0.70f;
            h.armor = 20; h.magicResist = 20; h.attackRange = 4f;
            h.maxMana = 80; h.startingMana = 0;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "中洲队", "科技" };
            h.skillName = "精准狙击"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 200; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_ZhangHeng.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "刘宇"; h.cost = 2;
            h.displayColor = new Color(0.4f, 0.3f, 0.9f);
            h.maxHealth = 550; h.attackDamage = 60; h.attackSpeed = 0.65f;
            h.armor = 20; h.magicResist = 30; h.attackRange = 3f;
            h.maxMana = 70; h.startingMana = 0;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "中洲队", "修真" };
            h.skillName = "御剑术"; h.skillType = SkillType.AreaDamage;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 90; h.skillRange = 2.5f; h.skillStunDuration = 0; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_LiuYu.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "王侠"; h.cost = 2;
            h.displayColor = new Color(0.3f, 0.8f, 0.9f);
            h.maxHealth = 650; h.attackDamage = 65; h.attackSpeed = 0.60f;
            h.armor = 25; h.magicResist = 20; h.attackRange = 2f;
            h.maxMana = 90; h.startingMana = 0;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "中洲队", "科技" };
            h.skillName = "C4炸弹"; h.skillType = SkillType.AreaDamage;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 130; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_WangXia.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "莱戈拉斯"; h.cost = 2;
            h.displayColor = new Color(0.9f, 0.8f, 0.2f);
            h.maxHealth = 500; h.attackDamage = 75; h.attackSpeed = 0.85f;
            h.armor = 15; h.magicResist = 15; h.attackRange = 4f;
            h.maxMana = 90; h.startingMana = 0;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "指环王", "科技" };
            h.skillName = "精灵连射"; h.skillType = SkillType.AreaDamage;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 80; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_Legolas.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "暴君T-002"; h.cost = 2;
            h.displayColor = new Color(0.7f, 0.3f, 0.2f);
            h.maxHealth = 850; h.attackDamage = 55; h.attackSpeed = 0.50f;
            h.armor = 40; h.magicResist = 25; h.attackRange = 1f;
            h.maxMana = 120; h.startingMana = 30;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "生化危机", "血族" };
            h.skillName = "暴怒冲锋"; h.skillType = SkillType.Stun;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 60; h.skillRange = 2f; h.skillStunDuration = 1.2f; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_TyrantT002.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "异形潜伏者"; h.cost = 2;
            h.displayColor = new Color(0.3f, 0.6f, 0.3f);
            h.maxHealth = 600; h.attackDamage = 80; h.attackSpeed = 0.90f;
            h.armor = 20; h.magicResist = 20; h.attackRange = 1f;
            h.maxMana = 80; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "异形", "武术" };
            h.skillName = "尾刺穿刺"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 200; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_AlienLurker.asset");
            heroes.Add(h);

            // === 3费 (5) ===
            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "赵樱空"; h.cost = 3;
            h.displayColor = new Color(0.9f, 0.5f, 0.1f);
            h.maxHealth = 700; h.attackDamage = 90; h.attackSpeed = 0.85f;
            h.armor = 25; h.magicResist = 25; h.attackRange = 1f;
            h.maxMana = 60; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "中洲队", "武术" };
            h.skillName = "暗影突袭"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 280; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_ZhaoYingkong.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "阿拉贡"; h.cost = 3;
            h.displayColor = new Color(0.9f, 0.8f, 0.2f);
            h.maxHealth = 750; h.attackDamage = 75; h.attackSpeed = 0.70f;
            h.armor = 35; h.magicResist = 25; h.attackRange = 1f;
            h.maxMana = 80; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "指环王", "武术" };
            h.skillName = "安都瑞尔"; h.skillType = SkillType.AreaDamage;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 150; h.skillRange = 2.5f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_Aragorn.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "艾丽丝"; h.cost = 3;
            h.displayColor = new Color(0.7f, 0.3f, 0.2f);
            h.maxHealth = 700; h.attackDamage = 70; h.attackSpeed = 0.75f;
            h.armor = 25; h.magicResist = 20; h.attackRange = 1f;
            h.maxMana = 60; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "生化危机", "血族" };
            h.skillName = "T病毒强化"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 180; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_Alice.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "异形战士"; h.cost = 3;
            h.displayColor = new Color(0.3f, 0.6f, 0.3f);
            h.maxHealth = 800; h.attackDamage = 65; h.attackSpeed = 0.65f;
            h.armor = 30; h.magicResist = 15; h.attackRange = 1f;
            h.maxMana = 100; h.startingMana = 50;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "异形", "血族" };
            h.skillName = "酸血喷射"; h.skillType = SkillType.AreaDamage;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 120; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_AlienWarrior.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "复制体樱空"; h.cost = 3;
            h.displayColor = new Color(0.6f, 0.1f, 0.1f);
            h.maxHealth = 650; h.attackDamage = 95; h.attackSpeed = 0.90f;
            h.armor = 20; h.magicResist = 20; h.attackRange = 1f;
            h.maxMana = 50; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "魔鬼队", "武术" };
            h.skillName = "嗜血之舞"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 250; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_CopyYingkong.asset");
            heroes.Add(h);

            // === 4费 (5) ===
            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "零"; h.cost = 4;
            h.displayColor = new Color(0.6f, 0.2f, 0.8f);
            h.maxHealth = 600; h.attackDamage = 50; h.attackSpeed = 0.60f;
            h.armor = 20; h.magicResist = 35; h.attackRange = 3f;
            h.maxMana = 80; h.startingMana = 0;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "中洲队", "精神力" };
            h.skillName = "精神风暴"; h.skillType = SkillType.Stun;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 100; h.skillRange = 2.5f; h.skillStunDuration = 1.5f; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_Ling.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "甘道夫"; h.cost = 4;
            h.displayColor = new Color(0.9f, 0.8f, 0.2f);
            h.maxHealth = 700; h.attackDamage = 60; h.attackSpeed = 0.55f;
            h.armor = 30; h.magicResist = 40; h.attackRange = 3f;
            h.maxMana = 90; h.startingMana = 20;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "指环王", "修真" };
            h.skillName = "你不可通过"; h.skillType = SkillType.AreaDamage;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 200; h.skillRange = 3f; h.skillStunDuration = 0; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_Gandalf.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "异形女王"; h.cost = 4;
            h.displayColor = new Color(0.3f, 0.6f, 0.3f);
            h.maxHealth = 900; h.attackDamage = 55; h.attackSpeed = 0.50f;
            h.armor = 35; h.magicResist = 30; h.attackRange = 1f;
            h.maxMana = 100; h.startingMana = 30;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "异形", "精神力" };
            h.skillName = "虫巢召唤"; h.skillType = SkillType.AreaDamage;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 150; h.skillRange = 2.5f; h.skillStunDuration = 0; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_AlienQueen.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "威斯克"; h.cost = 4;
            h.displayColor = new Color(0.7f, 0.3f, 0.2f);
            h.maxHealth = 750; h.attackDamage = 85; h.attackSpeed = 0.80f;
            h.armor = 30; h.magicResist = 25; h.attackRange = 1f;
            h.maxMana = 70; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "生化危机", "科技" };
            h.skillName = "闪现突击"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 300; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_Wesker.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "复制体张恒"; h.cost = 4;
            h.displayColor = new Color(0.6f, 0.1f, 0.1f);
            h.maxHealth = 550; h.attackDamage = 90; h.attackSpeed = 0.75f;
            h.armor = 20; h.magicResist = 20; h.attackRange = 4f;
            h.maxMana = 80; h.startingMana = 0;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "魔鬼队", "科技" };
            h.skillName = "暗能狙击"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 350; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_CopyZhangHeng.asset");
            heroes.Add(h);

            // === 5费 (4) ===
            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "郑吒"; h.cost = 5;
            h.displayColor = new Color(1f, 0.4f, 0.1f);
            h.maxHealth = 1000; h.attackDamage = 100; h.attackSpeed = 0.75f;
            h.armor = 35; h.magicResist = 30; h.attackRange = 1f;
            h.maxMana = 80; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "中洲队", "血族", "洪荒·开天辟地" };
            h.skillName = "暗焰斩"; h.skillType = SkillType.AreaDamage;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 300; h.skillRange = 2.5f; h.skillStunDuration = 0; h.skillIsMagic = false;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_ZhengZha.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "复制体郑吒"; h.cost = 5;
            h.displayColor = new Color(0.2f, 0f, 0.3f);
            h.maxHealth = 1100; h.attackDamage = 95; h.attackSpeed = 0.70f;
            h.armor = 40; h.magicResist = 35; h.attackRange = 1f;
            h.maxMana = 100; h.startingMana = 0;
            h.attackType = AttackType.Melee;
            h.factions = new string[] { "魔鬼队", "血族", "原暗·宇宙终结" };
            h.skillName = "黑洞吞噬"; h.skillType = SkillType.AreaDamage;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 250; h.skillRange = 3f; h.skillStunDuration = 0; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_CopyZhengZha.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "楚轩"; h.cost = 5;
            h.displayColor = new Color(0.9f, 0.9f, 0.5f);
            h.maxHealth = 700; h.attackDamage = 60; h.attackSpeed = 0.60f;
            h.armor = 25; h.magicResist = 35; h.attackRange = 4f;
            h.maxMana = 100; h.startingMana = 30;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "中洲队", "科技", "天机·万象推演" };
            h.skillName = "万象推演"; h.skillType = SkillType.Stun;
            h.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h.skillDamage = 80; h.skillRange = 3f; h.skillStunDuration = 2.0f; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_ChuXuan.asset");
            heroes.Add(h);

            h = ScriptableObject.CreateInstance<HeroData>();
            h.heroName = "复制体楚轩"; h.cost = 5;
            h.displayColor = new Color(0.3f, 0f, 0.4f);
            h.maxHealth = 650; h.attackDamage = 40; h.attackSpeed = 0.50f;
            h.armor = 20; h.magicResist = 40; h.attackRange = 5f;
            h.maxMana = 120; h.startingMana = 40;
            h.attackType = AttackType.Ranged;
            h.factions = new string[] { "魔鬼队", "精神力", "原暗·因果律武器" };
            h.skillName = "因果律攻击"; h.skillType = SkillType.Damage;
            h.skillTargetType = SkillTargetType.NearestEnemy;
            h.skillDamage = 400; h.skillRange = 2f; h.skillStunDuration = 0; h.skillIsMagic = true;
            AssetDatabase.CreateAsset(h, "Assets/ScriptableObjects/Heroes/Hero_CopyChuXuan.asset");
            heroes.Add(h);

            return heroes;
        }

        static System.Collections.Generic.List<FactionData> CreateFactions()
        {
            EnsureDirectory("Assets/ScriptableObjects/Factions");
            var factions = new System.Collections.Generic.List<FactionData>();
            // Origin 阵营
            factions.Add(CreateFactionAsset("Faction_Zhongzhou", "中洲队", new Color(0.2f, 0.6f, 0.9f),
                new (int, string, int, int, float, int, int)[] { (2, "中洲队+150HP/+10护甲", 150, 0, 0, 10, 0), (4, "中洲队+350HP/+25护甲/+15魔抗", 350, 0, 0, 25, 15) }));
            factions.Add(CreateFactionAsset("Faction_Devil", "魔鬼队", new Color(0.6f, 0.1f, 0.1f),
                new (int, string, int, int, float, int, int)[] { (2, "魔鬼队+25攻击/+10%攻速", 0, 25, 0.1f, 0, 0), (4, "魔鬼队+60攻击/+25%攻速", 0, 60, 0.25f, 0, 0) }));
            factions.Add(CreateFactionAsset("Faction_LOTR", "指环王", new Color(0.9f, 0.8f, 0.2f),
                new (int, string, int, int, float, int, int)[] { (2, "指环王+20攻击/+15护甲", 0, 20, 0, 15, 0), (4, "指环王+50攻击/+35护甲/+150HP", 150, 50, 0, 35, 0) }));
            factions.Add(CreateFactionAsset("Faction_Alien", "异形", new Color(0.3f, 0.6f, 0.3f),
                new (int, string, int, int, float, int, int)[] { (2, "异形+200HP/+10%攻速", 200, 0, 0.1f, 0, 0), (4, "异形+400HP/+20%攻速/+15护甲", 400, 0, 0.2f, 15, 0) }));
            factions.Add(CreateFactionAsset("Faction_RE", "生化危机", new Color(0.7f, 0.3f, 0.2f),
                new (int, string, int, int, float, int, int)[] { (2, "生化危机+15攻击/+200HP", 200, 15, 0, 0, 0), (4, "生化危机+35攻击/+400HP", 400, 35, 0, 0, 0) }));
            // Class 职业
            factions.Add(CreateFactionAsset("Faction_Blood", "血族", new Color(0.8f, 0.1f, 0.15f),
                new (int, string, int, int, float, int, int)[] { (2, "血族+20攻击/+150HP", 150, 20, 0, 0, 0), (4, "血族+50攻击/+350HP", 350, 50, 0, 0, 0) }));
            factions.Add(CreateFactionAsset("Faction_Xiuzhen", "修真", new Color(0.4f, 0.3f, 0.9f),
                new (int, string, int, int, float, int, int)[] { (2, "修真+30魔抗/+15攻击", 0, 15, 0, 0, 30), (4, "修真+60魔抗/+40攻击", 0, 40, 0, 0, 60) }));
            factions.Add(CreateFactionAsset("Faction_Tech", "科技", new Color(0.3f, 0.8f, 0.9f),
                new (int, string, int, int, float, int, int)[] { (2, "科技+20%攻速", 0, 0, 0.2f, 0, 0), (4, "科技+40%攻速/+20攻击", 0, 20, 0.4f, 0, 0) }));
            factions.Add(CreateFactionAsset("Faction_Psychic", "精神力", new Color(0.6f, 0.2f, 0.8f),
                new (int, string, int, int, float, int, int)[] { (2, "精神力+25魔抗/+100HP", 100, 0, 0, 0, 25), (4, "精神力+50魔抗/+250HP", 250, 0, 0, 0, 50) }));
            factions.Add(CreateFactionAsset("Faction_Martial", "武术", new Color(0.9f, 0.5f, 0.1f),
                new (int, string, int, int, float, int, int)[] { (2, "武术+15攻击/+10护甲", 0, 15, 0, 10, 0), (4, "武术+40攻击/+25护甲/+10%攻速", 0, 40, 0.1f, 25, 0) }));
            // 5费独特羁绊（1人激活）
            factions.Add(CreateFactionAsset("Faction_Honghuang", "洪荒·开天辟地", new Color(1f, 0.4f, 0.1f),
                new (int, string, int, int, float, int, int)[] { (1, "洪荒·开天辟地: +50攻击/+300HP/+20护甲", 300, 50, 0, 20, 0) }));
            factions.Add(CreateFactionAsset("Faction_DarkEnd", "原暗·宇宙终结", new Color(0.2f, 0f, 0.3f),
                new (int, string, int, int, float, int, int)[] { (1, "原暗·宇宙终结: +40攻击/+400HP/+25魔抗", 400, 40, 0, 0, 25) }));
            factions.Add(CreateFactionAsset("Faction_Tianji", "天机·万象推演", new Color(0.9f, 0.9f, 0.5f),
                new (int, string, int, int, float, int, int)[] { (1, "天机·万象推演: +30攻击/+20%攻速/+200HP", 200, 30, 0.2f, 0, 0) }));
            factions.Add(CreateFactionAsset("Faction_Causal", "原暗·因果律武器", new Color(0.3f, 0f, 0.4f),
                new (int, string, int, int, float, int, int)[] { (1, "原暗·因果律武器: +20攻击/+300HP/+30魔抗", 300, 20, 0, 0, 30) }));
            return factions;
        }

        static FactionData CreateFactionAsset(string fileName, string name, Color color,
            (int count, string desc, int hp, int atk, float spd, int armor, int mr)[] thresholds)
        {
            var f = ScriptableObject.CreateInstance<FactionData>();
            f.factionName = name;
            f.factionColor = color;
            f.thresholds = new FactionThreshold[thresholds.Length];
            for (int i = 0; i < thresholds.Length; i++)
            {
                f.thresholds[i] = new FactionThreshold
                {
                    count = thresholds[i].count,
                    description = thresholds[i].desc,
                    healthBonus = thresholds[i].hp,
                    attackBonus = thresholds[i].atk,
                    attackSpeedBonus = thresholds[i].spd,
                    armorBonus = thresholds[i].armor,
                    magicResistBonus = thresholds[i].mr
                };
            }
            AssetDatabase.CreateAsset(f, $"Assets/ScriptableObjects/Factions/{fileName}.asset");
            return f;
        }

        static System.Collections.Generic.List<EquipmentData> CreateEquipment()
        {
            EnsureDirectory("Assets/ScriptableObjects/Equipment");
            var all = new System.Collections.Generic.List<EquipmentData>();

            // 8 base items
            var sword = CreateEquipmentAsset("暴风大剑", EquipmentType.Base, new Color(0.9f, 0.3f, 0.2f), attackBonus: 15);
            var bow = CreateEquipmentAsset("反曲之弓", EquipmentType.Base, new Color(0.2f, 0.8f, 0.3f), attackSpeedBonus: 0.2f);
            var rod = CreateEquipmentAsset("无用大棒", EquipmentType.Base, new Color(0.3f, 0.3f, 0.9f), spellDamageBonus: 20);
            var chain = CreateEquipmentAsset("锁子甲", EquipmentType.Base, new Color(0.6f, 0.6f, 0.6f), armorBonus: 20);
            var cloak = CreateEquipmentAsset("负极斗篷", EquipmentType.Base, new Color(0.5f, 0.2f, 0.7f), magicResistBonus: 20);
            var belt = CreateEquipmentAsset("巨人腰带", EquipmentType.Base, new Color(0.8f, 0.6f, 0.2f), healthBonus: 200);
            var tear = CreateEquipmentAsset("女神之泪", EquipmentType.Base, new Color(0.2f, 0.6f, 0.9f), manaBonus: 15);
            var vamp = CreateEquipmentAsset("吸血鬼权杖", EquipmentType.Base, new Color(0.7f, 0.1f, 0.2f), lifestealPercent: 0.1f);

            all.Add(sword); all.Add(bow); all.Add(rod); all.Add(chain);
            all.Add(cloak); all.Add(belt); all.Add(tear); all.Add(vamp);

            // --- Combined items: all 36 pairwise combinations ---
            // sword + sword
            var ie = CreateEquipmentAsset("无尽之刃", EquipmentType.Combined, new Color(1f, 0.4f, 0.2f), attackBonus: 40);
            ie.recipe1 = sword; ie.recipe2 = sword; EditorUtility.SetDirty(ie);
            // sword + bow
            var giantSlayer = CreateEquipmentAsset("巨人杀手", EquipmentType.Combined, new Color(0.9f, 0.5f, 0.3f), attackBonus: 15, attackSpeedBonus: 0.2f);
            giantSlayer.recipe1 = sword; giantSlayer.recipe2 = bow; EditorUtility.SetDirty(giantSlayer);
            // sword + rod
            var hextech = CreateEquipmentAsset("海克斯科技枪刃", EquipmentType.Combined, new Color(0.6f, 0.3f, 0.8f), attackBonus: 15, spellDamageBonus: 20);
            hextech.recipe1 = sword; hextech.recipe2 = rod; EditorUtility.SetDirty(hextech);
            // sword + chain
            var ga = CreateEquipmentAsset("守护天使", EquipmentType.Combined, new Color(0.9f, 0.85f, 0.3f), attackBonus: 15, armorBonus: 20);
            ga.recipe1 = sword; ga.recipe2 = chain; EditorUtility.SetDirty(ga);
            // sword + cloak
            var bt = CreateEquipmentAsset("饮血剑", EquipmentType.Combined, new Color(0.8f, 0.2f, 0.3f), attackBonus: 15, magicResistBonus: 20);
            bt.recipe1 = sword; bt.recipe2 = cloak; EditorUtility.SetDirty(bt);
            // sword + belt
            var zeke = CreateEquipmentAsset("泽克先驱之令", EquipmentType.Combined, new Color(0.85f, 0.5f, 0.2f), attackBonus: 15, healthBonus: 200);
            zeke.recipe1 = sword; zeke.recipe2 = belt; EditorUtility.SetDirty(zeke);
            // sword + tear
            var shojin = CreateEquipmentAsset("破败王者之刃", EquipmentType.Combined, new Color(0.4f, 0.5f, 0.9f), attackBonus: 15, manaBonus: 15);
            shojin.recipe1 = sword; shojin.recipe2 = tear; EditorUtility.SetDirty(shojin);
            // sword + vamp
            var deathblade = CreateEquipmentAsset("死亡之刃", EquipmentType.Combined, new Color(0.6f, 0.1f, 0.15f), attackBonus: 25, lifestealPercent: 0.1f);
            deathblade.recipe1 = sword; deathblade.recipe2 = vamp; EditorUtility.SetDirty(deathblade);

            // bow + bow
            var guinsoo = CreateEquipmentAsset("鬼索的狂暴之刃", EquipmentType.Combined, new Color(0.3f, 0.9f, 0.4f), attackSpeedBonus: 0.5f);
            guinsoo.recipe1 = bow; guinsoo.recipe2 = bow; EditorUtility.SetDirty(guinsoo);
            // bow + rod
            var statikk = CreateEquipmentAsset("电刃", EquipmentType.Combined, new Color(0.4f, 0.8f, 0.9f), attackSpeedBonus: 0.2f, spellDamageBonus: 20);
            statikk.recipe1 = bow; statikk.recipe2 = rod; EditorUtility.SetDirty(statikk);
            // bow + chain
            var phantom = CreateEquipmentAsset("幻影之舞", EquipmentType.Combined, new Color(0.5f, 0.7f, 0.6f), attackSpeedBonus: 0.2f, armorBonus: 20, dodgeChance: 0.2f);
            phantom.recipe1 = bow; phantom.recipe2 = chain; EditorUtility.SetDirty(phantom);
            // bow + cloak
            var runaanHurricane = CreateEquipmentAsset("卢安娜的飓风", EquipmentType.Combined, new Color(0.3f, 0.7f, 0.5f), attackSpeedBonus: 0.3f, magicResistBonus: 20);
            runaanHurricane.recipe1 = bow; runaanHurricane.recipe2 = cloak; EditorUtility.SetDirty(runaanHurricane);
            // bow + belt
            var titanResolve = CreateEquipmentAsset("泰坦的坚决", EquipmentType.Combined, new Color(0.7f, 0.6f, 0.3f), attackSpeedBonus: 0.2f, healthBonus: 200);
            titanResolve.recipe1 = bow; titanResolve.recipe2 = belt; EditorUtility.SetDirty(titanResolve);
            // bow + tear
            var shiv = CreateEquipmentAsset("离子火花", EquipmentType.Combined, new Color(0.3f, 0.6f, 0.8f), attackSpeedBonus: 0.2f, manaBonus: 15);
            shiv.recipe1 = bow; shiv.recipe2 = tear; EditorUtility.SetDirty(shiv);
            // bow + vamp
            var rageknife = CreateEquipmentAsset("嗜血弯刀", EquipmentType.Combined, new Color(0.6f, 0.3f, 0.3f), attackSpeedBonus: 0.3f, lifestealPercent: 0.1f);
            rageknife.recipe1 = bow; rageknife.recipe2 = vamp; EditorUtility.SetDirty(rageknife);

            // rod + rod
            var rabadons = CreateEquipmentAsset("灭世者的死亡之帽", EquipmentType.Combined, new Color(0.4f, 0.2f, 0.9f), spellDamageBonus: 50);
            rabadons.recipe1 = rod; rabadons.recipe2 = rod; EditorUtility.SetDirty(rabadons);
            // rod + chain
            var locket = CreateEquipmentAsset("钢铁烈阳之匣", EquipmentType.Combined, new Color(0.5f, 0.5f, 0.7f), spellDamageBonus: 20, armorBonus: 20);
            locket.recipe1 = rod; locket.recipe2 = chain; EditorUtility.SetDirty(locket);
            // rod + cloak
            var ionicSpark = CreateEquipmentAsset("珠光护手", EquipmentType.Combined, new Color(0.4f, 0.3f, 0.8f), spellDamageBonus: 20, magicResistBonus: 20);
            ionicSpark.recipe1 = rod; ionicSpark.recipe2 = cloak; EditorUtility.SetDirty(ionicSpark);
            // rod + belt
            var morello = CreateEquipmentAsset("莫雷洛秘典", EquipmentType.Combined, new Color(0.7f, 0.3f, 0.5f), spellDamageBonus: 20, healthBonus: 200);
            morello.recipe1 = rod; morello.recipe2 = belt; EditorUtility.SetDirty(morello);
            // rod + tear
            var archangel = CreateEquipmentAsset("大天使之杖", EquipmentType.Combined, new Color(0.3f, 0.5f, 1f), manaBonus: 30, spellDamageBonus: 40);
            archangel.recipe1 = tear; archangel.recipe2 = rod; EditorUtility.SetDirty(archangel);
            // rod + vamp
            var gunblade = CreateEquipmentAsset("灵风", EquipmentType.Combined, new Color(0.6f, 0.2f, 0.6f), spellDamageBonus: 20, lifestealPercent: 0.1f);
            gunblade.recipe1 = rod; gunblade.recipe2 = vamp; EditorUtility.SetDirty(gunblade);

            // chain + chain
            var brambleVest = CreateEquipmentAsset("棘刺背心", EquipmentType.Combined, new Color(0.5f, 0.6f, 0.5f), armorBonus: 50);
            brambleVest.recipe1 = chain; brambleVest.recipe2 = chain; EditorUtility.SetDirty(brambleVest);
            // chain + cloak
            var gargoyle = CreateEquipmentAsset("石像鬼板甲", EquipmentType.Combined, new Color(0.5f, 0.4f, 0.6f), armorBonus: 20, magicResistBonus: 20);
            gargoyle.recipe1 = chain; gargoyle.recipe2 = cloak; EditorUtility.SetDirty(gargoyle);
            // chain + belt
            var sunfire = CreateEquipmentAsset("日炎斗篷", EquipmentType.Combined, new Color(0.8f, 0.4f, 0.2f), armorBonus: 20, healthBonus: 200);
            sunfire.recipe1 = chain; sunfire.recipe2 = belt; EditorUtility.SetDirty(sunfire);
            // chain + tear
            var frozenHeart = CreateEquipmentAsset("冰霜之心", EquipmentType.Combined, new Color(0.3f, 0.5f, 0.8f), armorBonus: 20, manaBonus: 15);
            frozenHeart.recipe1 = chain; frozenHeart.recipe2 = tear; EditorUtility.SetDirty(frozenHeart);
            // chain + vamp
            var stoneplate = CreateEquipmentAsset("锁子血甲", EquipmentType.Combined, new Color(0.6f, 0.5f, 0.4f), armorBonus: 20, lifestealPercent: 0.1f);
            stoneplate.recipe1 = chain; stoneplate.recipe2 = vamp; EditorUtility.SetDirty(stoneplate);

            // cloak + cloak
            var dragonClaw = CreateEquipmentAsset("龙爪", EquipmentType.Combined, new Color(0.4f, 0.2f, 0.6f), magicResistBonus: 50);
            dragonClaw.recipe1 = cloak; dragonClaw.recipe2 = cloak; EditorUtility.SetDirty(dragonClaw);
            // cloak + belt
            var zephyr = CreateEquipmentAsset("和风", EquipmentType.Combined, new Color(0.5f, 0.6f, 0.7f), magicResistBonus: 20, healthBonus: 200);
            zephyr.recipe1 = cloak; zephyr.recipe2 = belt; EditorUtility.SetDirty(zephyr);
            // cloak + tear
            var hush = CreateEquipmentAsset("静止法衣", EquipmentType.Combined, new Color(0.4f, 0.4f, 0.7f), magicResistBonus: 20, manaBonus: 15);
            hush.recipe1 = cloak; hush.recipe2 = tear; EditorUtility.SetDirty(hush);
            // cloak + vamp
            var quicksilver = CreateEquipmentAsset("水银", EquipmentType.Combined, new Color(0.6f, 0.3f, 0.5f), magicResistBonus: 20, lifestealPercent: 0.1f);
            quicksilver.recipe1 = cloak; quicksilver.recipe2 = vamp; EditorUtility.SetDirty(quicksilver);

            // belt + belt
            var warmog = CreateEquipmentAsset("狂徒铠甲", EquipmentType.Combined, new Color(0.9f, 0.7f, 0.2f), healthBonus: 500);
            warmog.recipe1 = belt; warmog.recipe2 = belt; EditorUtility.SetDirty(warmog);
            // belt + tear
            var redemption = CreateEquipmentAsset("救赎", EquipmentType.Combined, new Color(0.6f, 0.7f, 0.4f), healthBonus: 200, manaBonus: 15);
            redemption.recipe1 = belt; redemption.recipe2 = tear; EditorUtility.SetDirty(redemption);
            // belt + vamp
            var bloodthirster = CreateEquipmentAsset("嗜血之力", EquipmentType.Combined, new Color(0.7f, 0.2f, 0.3f), healthBonus: 200, lifestealPercent: 0.15f);
            bloodthirster.recipe1 = belt; bloodthirster.recipe2 = vamp; EditorUtility.SetDirty(bloodthirster);

            // tear + tear
            var blueBuff = CreateEquipmentAsset("蓝霸符", EquipmentType.Combined, new Color(0.2f, 0.4f, 0.9f), manaBonus: 40);
            blueBuff.recipe1 = tear; blueBuff.recipe2 = tear; EditorUtility.SetDirty(blueBuff);
            // tear + vamp
            var handOfJustice = CreateEquipmentAsset("正义之手", EquipmentType.Combined, new Color(0.5f, 0.4f, 0.6f), manaBonus: 15, lifestealPercent: 0.15f);
            handOfJustice.recipe1 = tear; handOfJustice.recipe2 = vamp; EditorUtility.SetDirty(handOfJustice);

            // vamp + vamp
            var bloodletter = CreateEquipmentAsset("嗜血者", EquipmentType.Combined, new Color(0.6f, 0.1f, 0.1f), lifestealPercent: 0.25f);
            bloodletter.recipe1 = vamp; bloodletter.recipe2 = vamp; EditorUtility.SetDirty(bloodletter);

            all.Add(ie); all.Add(giantSlayer); all.Add(hextech); all.Add(ga);
            all.Add(bt); all.Add(zeke); all.Add(shojin); all.Add(deathblade);
            all.Add(guinsoo); all.Add(statikk); all.Add(phantom); all.Add(runaanHurricane);
            all.Add(titanResolve); all.Add(shiv); all.Add(rageknife);
            all.Add(rabadons); all.Add(locket); all.Add(ionicSpark); all.Add(morello);
            all.Add(archangel); all.Add(gunblade);
            all.Add(brambleVest); all.Add(gargoyle); all.Add(sunfire); all.Add(frozenHeart); all.Add(stoneplate);
            all.Add(dragonClaw); all.Add(zephyr); all.Add(hush); all.Add(quicksilver);
            all.Add(warmog); all.Add(redemption); all.Add(bloodthirster);
            all.Add(blueBuff); all.Add(handOfJustice);
            all.Add(bloodletter);

            AssetDatabase.SaveAssets();
            return all;
        }

        static EquipmentData CreateEquipmentAsset(string name, EquipmentType type, Color color,
            int healthBonus = 0, int attackBonus = 0, int armorBonus = 0, int magicResistBonus = 0,
            float attackSpeedBonus = 0f, int manaBonus = 0, float lifestealPercent = 0f, int spellDamageBonus = 0,
            float dodgeChance = 0f)
        {
            var eq = ScriptableObject.CreateInstance<EquipmentData>();
            eq.equipmentName = name;
            eq.equipmentType = type;
            eq.displayColor = color;
            eq.healthBonus = healthBonus;
            eq.attackBonus = attackBonus;
            eq.armorBonus = armorBonus;
            eq.magicResistBonus = magicResistBonus;
            eq.attackSpeedBonus = attackSpeedBonus;
            eq.manaBonus = manaBonus;
            eq.lifestealPercent = lifestealPercent;
            eq.spellDamageBonus = spellDamageBonus;
            eq.dodgeChance = dodgeChance;
            string safeName = name.Replace(" ", "_");
            string path = $"Assets/ScriptableObjects/Equipment/Eq_{safeName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EquipmentData>(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(eq, path);
            return eq;
        }

        static System.Collections.Generic.List<CreepRoundData> CreateCreepRounds()
        {
            EnsureDirectory("Assets/ScriptableObjects/CreepRounds");
            var rounds = new System.Collections.Generic.List<CreepRoundData>();

            rounds.Add(CreateCreepRoundAsset("Round1_Beetles", "丧尸", 1, 1, false,
                new CreepInfo[] {
                    new CreepInfo { creepName = "丧尸", health = 200, attackDamage = 20, armor = 5, attackSpeed = 0.5f, color = new Color(0.4f, 0.5f, 0.3f) },
                    new CreepInfo { creepName = "丧尸", health = 200, attackDamage = 20, armor = 5, attackSpeed = 0.5f, color = new Color(0.4f, 0.5f, 0.3f) },
                    new CreepInfo { creepName = "丧尸", health = 200, attackDamage = 20, armor = 5, attackSpeed = 0.5f, color = new Color(0.4f, 0.5f, 0.3f) }
                }));

            rounds.Add(CreateCreepRoundAsset("Round2_Wolves", "舔食者", 1, 1, false,
                new CreepInfo[] {
                    new CreepInfo { creepName = "舔食者", health = 350, attackDamage = 35, armor = 10, attackSpeed = 0.7f, color = new Color(0.6f, 0.2f, 0.2f) },
                    new CreepInfo { creepName = "舔食者", health = 350, attackDamage = 35, armor = 10, attackSpeed = 0.7f, color = new Color(0.6f, 0.2f, 0.2f) },
                    new CreepInfo { creepName = "舔食者", health = 350, attackDamage = 35, armor = 10, attackSpeed = 0.7f, color = new Color(0.6f, 0.2f, 0.2f) }
                }));

            rounds.Add(CreateCreepRoundAsset("Round3_Gargoyle", "猎犬", 2, 1, false,
                new CreepInfo[] {
                    new CreepInfo { creepName = "猎犬", health = 800, attackDamage = 50, armor = 25, attackSpeed = 0.6f, color = new Color(0.3f, 0.3f, 0.35f) }
                }));

            rounds.Add(CreateCreepRoundAsset("Round10_Ghosts", "抱脸体", 2, 2, false,
                new CreepInfo[] {
                    new CreepInfo { creepName = "抱脸体", health = 500, attackDamage = 45, armor = 10, attackSpeed = 0.65f, color = new Color(0.3f, 0.4f, 0.3f) },
                    new CreepInfo { creepName = "抱脸体", health = 500, attackDamage = 45, armor = 10, attackSpeed = 0.65f, color = new Color(0.3f, 0.4f, 0.3f) },
                    new CreepInfo { creepName = "抱脸体", health = 500, attackDamage = 45, armor = 10, attackSpeed = 0.65f, color = new Color(0.3f, 0.4f, 0.3f) },
                    new CreepInfo { creepName = "抱脸体", health = 500, attackDamage = 45, armor = 10, attackSpeed = 0.65f, color = new Color(0.3f, 0.4f, 0.3f) }
                }));

            rounds.Add(CreateCreepRoundAsset("Round17_Dragon", "异形皇后", 3, 1, true,
                new CreepInfo[] {
                    new CreepInfo { creepName = "异形皇后", health = 2000, attackDamage = 80, armor = 40, attackSpeed = 0.5f, color = new Color(0.2f, 0.2f, 0.3f) }
                }));

            rounds.Add(CreateCreepRoundAsset("Round24_AncientDragon", "主神守卫", 5, 1, true,
                new CreepInfo[] {
                    new CreepInfo { creepName = "主神守卫", health = 3500, attackDamage = 120, armor = 60, attackSpeed = 0.55f, color = new Color(0.9f, 0.8f, 0.3f) }
                }));

            return rounds;
        }

        static CreepRoundData CreateCreepRoundAsset(string fileName, string roundName, int goldReward, int equipDrop, bool dropCombined, CreepInfo[] creeps)
        {
            var data = ScriptableObject.CreateInstance<CreepRoundData>();
            data.roundName = roundName;
            data.goldReward = goldReward;
            data.equipmentDropCount = equipDrop;
            data.dropCombinedEquipment = dropCombined;
            data.creeps = creeps;
            AssetDatabase.CreateAsset(data, $"Assets/ScriptableObjects/CreepRounds/{fileName}.asset");
            return data;
        }

        static System.Collections.Generic.List<AugmentData> CreateAugments()
        {
            EnsureDirectory("Assets/ScriptableObjects/Hexes");
            var list = new System.Collections.Generic.List<AugmentData>();

            // Tier 1 (6)
            list.Add(CreateAugmentAsset("Hex_T1_Attack", "利刃之力", "所有棋子攻击力+10%", 1,
                new Color(0.9f, 0.3f, 0.2f), AugmentEffectType.AllAttackPercent, 10));
            list.Add(CreateAugmentAsset("Hex_T1_Health", "坚韧体魄", "所有棋子生命值+10%", 1,
                new Color(0.2f, 0.8f, 0.3f), AugmentEffectType.AllHealthPercent, 10));
            list.Add(CreateAugmentAsset("Hex_T1_Gold", "聚宝盆", "每回合额外获得1金币", 1,
                new Color(0.9f, 0.8f, 0.2f), AugmentEffectType.BonusGoldPerRound, 1));
            list.Add(CreateAugmentAsset("Hex_T1_Refresh", "慧眼识珠", "每回合1次免费刷新", 1,
                new Color(0.3f, 0.6f, 0.9f), AugmentEffectType.FreeRefreshPerRound, 1));
            list.Add(CreateAugmentAsset("Hex_T1_Armor", "铁壁", "所有棋子护甲+10", 1,
                new Color(0.6f, 0.6f, 0.6f), AugmentEffectType.AllArmorFlat, 10));
            list.Add(CreateAugmentAsset("Hex_T1_MR", "魔法屏障", "所有棋子魔抗+10", 1,
                new Color(0.5f, 0.3f, 0.7f), AugmentEffectType.AllMagicResistFlat, 10));

            // Tier 2 (6)
            list.Add(CreateAugmentAsset("Hex_T2_Attack", "战争狂热", "所有棋子攻击力+20%", 2,
                new Color(1f, 0.4f, 0.2f), AugmentEffectType.AllAttackPercent, 20));
            list.Add(CreateAugmentAsset("Hex_T2_AtkSpd", "疾风之力", "所有棋子攻速+15%", 2,
                new Color(0.3f, 0.9f, 0.5f), AugmentEffectType.AllAttackSpeedPercent, 15));
            list.Add(CreateAugmentAsset("Hex_T2_Interest", "复利投资", "利息上限+3", 2,
                new Color(0.9f, 0.7f, 0.1f), AugmentEffectType.InterestCapBonus, 3));
            list.Add(CreateAugmentAsset("Hex_T2_Shop", "扩展商店", "商店格子+1", 2,
                new Color(0.4f, 0.7f, 0.9f), AugmentEffectType.ShopSlotBonus, 1));
            list.Add(CreateAugmentAsset("Hex_T2_Exp", "知识渊博", "每回合额外获得2经验", 2,
                new Color(0.6f, 0.4f, 0.9f), AugmentEffectType.ExpPerRound, 2));
            list.Add(CreateAugmentAsset("Hex_T2_Bench", "扩展备战席", "备战席+1", 2,
                new Color(0.5f, 0.8f, 0.4f), AugmentEffectType.BenchSlotBonus, 1));

            // Tier 3 (6)
            list.Add(CreateAugmentAsset("Hex_T3_Attack", "毁灭之力", "所有棋子攻击力+35%", 3,
                new Color(1f, 0.2f, 0.1f), AugmentEffectType.AllAttackPercent, 35));
            list.Add(CreateAugmentAsset("Hex_T3_Health", "不朽意志", "所有棋子生命值+25%", 3,
                new Color(0.1f, 0.9f, 0.2f), AugmentEffectType.AllHealthPercent, 25));
            list.Add(CreateAugmentAsset("Hex_T3_Gold", "点石成金", "立即获得20金币", 3,
                new Color(1f, 0.9f, 0.1f), AugmentEffectType.StartingGoldBonus, 20));
            list.Add(CreateAugmentAsset("Hex_T3_AtkSpd", "狂暴风暴", "所有棋子攻速+30%", 3,
                new Color(0.2f, 1f, 0.6f), AugmentEffectType.AllAttackSpeedPercent, 30));
            list.Add(CreateAugmentAsset("Hex_T3_Refresh", "无尽刷新", "每回合3次免费刷新", 3,
                new Color(0.2f, 0.5f, 1f), AugmentEffectType.FreeRefreshPerRound, 3));
            list.Add(CreateAugmentAsset("Hex_T3_Armor", "钢铁堡垒", "所有棋子护甲+25", 3,
                new Color(0.7f, 0.7f, 0.7f), AugmentEffectType.AllArmorFlat, 25));

            return list;
        }

        static AugmentData CreateAugmentAsset(string fileName, string name, string desc, int tier,
            Color color, AugmentEffectType effectType, float effectValue)
        {
            var aug = ScriptableObject.CreateInstance<AugmentData>();
            aug.augmentName = name;
            aug.description = desc;
            aug.tier = tier;
            aug.iconColor = color;
            aug.effectType = effectType;
            aug.effectValue = effectValue;
            AssetDatabase.CreateAsset(aug, $"Assets/ScriptableObjects/Hexes/{fileName}.asset");
            return aug;
        }

        static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }

        static Material CreatePieceMaterial()
        {
            EnsureDirectory("Assets/Materials");
            EnsureDirectory("Assets/Prefabs");

            var mat = new Material(FindSafeShader());
            mat.color = Color.white;
            AssetDatabase.CreateAsset(mat, "Assets/Materials/PieceBase.mat");
            return mat;
        }

        static Shader FindSafeShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null) return shader;
            shader = Shader.Find("Standard");
            return shader;
        }

        static GameObject CreatePiecePrefab(Material baseMat)
        {
            var go = new GameObject("ChessPiecePrefab");
            go.SetActive(false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(go.transform);
            body.transform.localPosition = Vector3.up * 0.5f;
            body.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
            body.GetComponent<Renderer>().material = baseMat;

            var collider = body.GetComponent<Collider>();
            collider.isTrigger = false;

            go.AddComponent<ChessPiece>();
            go.layer = LayerMask.NameToLayer("Default");

            PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/ChessPiece.prefab");
            DestroyImmediate(go);

            return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ChessPiece.prefab");
        }

        static void SetupScene(GameConfig config, System.Collections.Generic.List<HeroData> heroes,
            System.Collections.Generic.List<FactionData> factions,
            System.Collections.Generic.List<EquipmentData> equipment,
            System.Collections.Generic.List<CreepRoundData> creepRounds,
            System.Collections.Generic.List<AugmentData> augments,
            GameObject piecePrefab)
        {
            var scene = EditorSceneManager.GetActiveScene();

            // Clean existing managers
            var existing = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var toDestroy = new System.Collections.Generic.HashSet<GameObject>();
            foreach (var go in existing)
            {
                if (go == null) continue;
                if (go.GetComponent<GameLoopManager>() != null ||
                    go.GetComponent<BoardManager>() != null ||
                    go.GetComponent<CombatManager>() != null ||
                    go.GetComponent<AIManager>() != null ||
                    go.GetComponent<UIManager>() != null ||
                    go.GetComponent<DragController>() != null ||
                    go.GetComponent<CameraController>() != null ||
                    go.GetComponent<HeroPool>() != null ||
                    go.GetComponent<ShopManager>() != null ||
                    go.GetComponent<StarMergeManager>() != null ||
                    go.GetComponent<FactionManager>() != null ||
                    go.GetComponent<EconomyManager>() != null ||
                    go.GetComponent<AIController>() != null ||
                    go.GetComponent<EquipmentManager>() != null ||
                    go.GetComponent<CreepManager>() != null ||
                    go.GetComponent<AugmentManager>() != null ||
                    go.GetComponent<CarouselManager>() != null ||
                    go.GetComponent<CombatStatsTracker>() != null)
                {
                    // Collect root object to avoid destroying children separately
                    var root = go.transform.root.gameObject;
                    toDestroy.Add(root);
                }
            }
            foreach (var go in toDestroy)
            {
                if (go != null) DestroyImmediate(go);
            }

            // Clean standalone scene objects from previous setup
            string[] standaloneNames = { "BoardOrigin", "BenchOrigin", "MainCamera", "CameraTarget", "DirectionalLight", "Ground", "Divider", "EventSystem" };
            foreach (var name in standaloneNames)
            {
                var obj = GameObject.Find(name);
                if (obj != null) DestroyImmediate(obj);
            }

            // Main Manager object
            var managerGO = new GameObject("=== GAME MANAGERS ===");

            // ====== GameLoopManager ======
            var gameLoop = managerGO.AddComponent<GameLoopManager>();
            var so = new SerializedObject(gameLoop);
            so.FindProperty("gameConfig").objectReferenceValue = config;
            so.FindProperty("availableHeroes").ClearArray();
            so.FindProperty("availableHeroes").arraySize = heroes.Count;
            for (int i = 0; i < heroes.Count; i++)
                so.FindProperty("availableHeroes").GetArrayElementAtIndex(i).objectReferenceValue = heroes[i];
            so.FindProperty("availableFactions").ClearArray();
            so.FindProperty("availableFactions").arraySize = factions.Count;
            for (int i = 0; i < factions.Count; i++)
                so.FindProperty("availableFactions").GetArrayElementAtIndex(i).objectReferenceValue = factions[i];
            so.ApplyModifiedProperties();

            // ====== BoardManager ======
            var boardGO = new GameObject("BoardManager");
            boardGO.transform.SetParent(managerGO.transform);
            var board = boardGO.AddComponent<BoardManager>();
            var boardSO = new SerializedObject(board);
            boardSO.FindProperty("gameConfig").objectReferenceValue = config;
            boardSO.FindProperty("piecePrefab").objectReferenceValue = piecePrefab;

            var boardOrigin = new GameObject("BoardOrigin");
            boardOrigin.transform.position = new Vector3(-3.6f, 0, -1.5f);
            boardSO.FindProperty("boardOrigin").objectReferenceValue = boardOrigin.transform;

            var benchOrigin = new GameObject("BenchOrigin");
            benchOrigin.transform.position = new Vector3(-3.6f, 0, -3.2f);
            boardSO.FindProperty("benchOrigin").objectReferenceValue = benchOrigin.transform;
            boardSO.ApplyModifiedProperties();

            // HexGridRenderer (auto-attached via RequireComponent, but add explicitly)
            boardGO.AddComponent<HexGridRenderer>();

            // ====== CombatManager ======
            var combatGO = new GameObject("CombatManager");
            combatGO.transform.SetParent(managerGO.transform);
            var combat = combatGO.AddComponent<CombatManager>();
            var combatSO = new SerializedObject(combat);
            combatSO.FindProperty("gameConfig").objectReferenceValue = config;
            combatSO.ApplyModifiedProperties();

            // ====== HeroPool ======
            var poolGO = new GameObject("HeroPool");
            poolGO.transform.SetParent(managerGO.transform);
            var pool = poolGO.AddComponent<HeroPool>();

            // ====== ShopManager ======
            var shopGO = new GameObject("ShopManager");
            shopGO.transform.SetParent(managerGO.transform);
            var shop = shopGO.AddComponent<ShopManager>();

            // ====== StarMergeManager ======
            var mergeGO = new GameObject("StarMergeManager");
            mergeGO.transform.SetParent(managerGO.transform);
            var merge = mergeGO.AddComponent<StarMergeManager>();

            // ====== FactionManager ======
            var factionGO = new GameObject("FactionManager");
            factionGO.transform.SetParent(managerGO.transform);
            var factionMgr = factionGO.AddComponent<FactionManager>();

            // ====== EconomyManager ======
            var econGO = new GameObject("EconomyManager");
            econGO.transform.SetParent(managerGO.transform);
            var econ = econGO.AddComponent<EconomyManager>();

            // ====== AIController ======
            var aiCtrlGO = new GameObject("AIController");
            aiCtrlGO.transform.SetParent(managerGO.transform);
            var aiCtrl = aiCtrlGO.AddComponent<AIController>();

            // ====== AIManager ======
            var aiGO = new GameObject("AIManager");
            aiGO.transform.SetParent(managerGO.transform);
            var ai = aiGO.AddComponent<AIManager>();
            var aiSO = new SerializedObject(ai);
            aiSO.FindProperty("gameConfig").objectReferenceValue = config;
            aiSO.FindProperty("boardManager").objectReferenceValue = board;
            aiSO.FindProperty("aiController").objectReferenceValue = aiCtrl;
            aiSO.ApplyModifiedProperties();

            // ====== EquipmentManager ======
            var eqMgrGO = new GameObject("EquipmentManager");
            eqMgrGO.transform.SetParent(managerGO.transform);
            var eqMgr = eqMgrGO.AddComponent<EquipmentManager>();

            // ====== CreepManager ======
            var creepMgrGO = new GameObject("CreepManager");
            creepMgrGO.transform.SetParent(managerGO.transform);
            var creepMgr = creepMgrGO.AddComponent<CreepManager>();
            var creepMgrSO = new SerializedObject(creepMgr);
            creepMgrSO.FindProperty("gameConfig").objectReferenceValue = config;
            creepMgrSO.FindProperty("boardManager").objectReferenceValue = board;
            creepMgrSO.FindProperty("creepRounds").ClearArray();
            creepMgrSO.FindProperty("creepRounds").arraySize = creepRounds.Count;
            for (int i = 0; i < creepRounds.Count; i++)
                creepMgrSO.FindProperty("creepRounds").GetArrayElementAtIndex(i).objectReferenceValue = creepRounds[i];
            creepMgrSO.ApplyModifiedProperties();

            // ====== AugmentManager ======
            var augMgrGO = new GameObject("AugmentManager");
            augMgrGO.transform.SetParent(managerGO.transform);
            var augMgr = augMgrGO.AddComponent<AugmentManager>();

            // ====== CarouselManager ======
            var carouselMgrGO = new GameObject("CarouselManager");
            carouselMgrGO.transform.SetParent(managerGO.transform);
            var carouselMgr = carouselMgrGO.AddComponent<CarouselManager>();

            // ====== CombatStatsTracker ======
            var statsGO = new GameObject("CombatStatsTracker");
            statsGO.transform.SetParent(managerGO.transform);
            var statsTracker = statsGO.AddComponent<CombatStatsTracker>();

            // Link GameLoopManager references
            var glSO = new SerializedObject(gameLoop);
            glSO.FindProperty("boardManager").objectReferenceValue = board;
            glSO.FindProperty("combatManager").objectReferenceValue = combat;
            glSO.FindProperty("aiManager").objectReferenceValue = ai;
            glSO.FindProperty("heroPool").objectReferenceValue = pool;
            glSO.FindProperty("shopManager").objectReferenceValue = shop;
            glSO.FindProperty("starMergeManager").objectReferenceValue = merge;
            glSO.FindProperty("factionManager").objectReferenceValue = factionMgr;
            glSO.FindProperty("economyManager").objectReferenceValue = econ;
            glSO.FindProperty("equipmentManager").objectReferenceValue = eqMgr;
            glSO.FindProperty("creepManager").objectReferenceValue = creepMgr;
            glSO.FindProperty("augmentManager").objectReferenceValue = augMgr;
            glSO.FindProperty("carouselManager").objectReferenceValue = carouselMgr;
            glSO.FindProperty("combatStatsTracker").objectReferenceValue = statsTracker;

            // Equipment list
            glSO.FindProperty("availableEquipment").ClearArray();
            glSO.FindProperty("availableEquipment").arraySize = equipment.Count;
            for (int i = 0; i < equipment.Count; i++)
                glSO.FindProperty("availableEquipment").GetArrayElementAtIndex(i).objectReferenceValue = equipment[i];

            // Creep round data
            glSO.FindProperty("creepRoundData").ClearArray();
            glSO.FindProperty("creepRoundData").arraySize = creepRounds.Count;
            for (int i = 0; i < creepRounds.Count; i++)
                glSO.FindProperty("creepRoundData").GetArrayElementAtIndex(i).objectReferenceValue = creepRounds[i];

            // Augment data
            glSO.FindProperty("availableAugments").ClearArray();
            glSO.FindProperty("availableAugments").arraySize = augments.Count;
            for (int i = 0; i < augments.Count; i++)
                glSO.FindProperty("availableAugments").GetArrayElementAtIndex(i).objectReferenceValue = augments[i];

            glSO.ApplyModifiedProperties();

            // Link AIController references
            var ctrlSO = new SerializedObject(aiCtrl);
            ctrlSO.FindProperty("gameConfig").objectReferenceValue = config;
            ctrlSO.FindProperty("boardManager").objectReferenceValue = board;
            ctrlSO.FindProperty("shopManager").objectReferenceValue = shop;
            ctrlSO.FindProperty("starMergeManager").objectReferenceValue = merge;
            ctrlSO.ApplyModifiedProperties();

            // Camera setup
            var camGO = new GameObject("MainCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0, 10, -8);
            cam.transform.rotation = Quaternion.Euler(55, 0, 0);
            cam.orthographic = false;
            cam.fieldOfView = 50;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);

            var camCtrl = camGO.AddComponent<CameraController>();
            var camTarget = new GameObject("CameraTarget");
            camTarget.transform.position = new Vector3(0, 0, 2.0f);
            var camCtrlSO = new SerializedObject(camCtrl);
            camCtrlSO.FindProperty("target").objectReferenceValue = camTarget.transform;
            camCtrlSO.FindProperty("distance").floatValue = 12f;
            camCtrlSO.FindProperty("height").floatValue = 10f;
            camCtrlSO.ApplyModifiedProperties();

            // Light
            var lightGO = new GameObject("DirectionalLight");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Ground plane (dark background under the hex grid)
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0, -0.02f, 2.0f);
            ground.transform.localScale = new Vector3(2f, 1, 1.5f);
            var groundMat = new Material(FindSafeShader());
            groundMat.color = new Color(0.08f, 0.08f, 0.1f);
            ground.GetComponent<Renderer>().material = groundMat;
            ground.GetComponent<Collider>().enabled = false;
            AssetDatabase.CreateAsset(groundMat, "Assets/Materials/Ground.mat");

            // HexGridRenderer draws the divider, no need for a separate Divider object

            // Setup UI
            SetupUI(managerGO, board, cam);

            // Setup DragController
            var dragGO = new GameObject("DragController");
            dragGO.transform.SetParent(managerGO.transform);
            var drag = dragGO.AddComponent<DragController>();
            var dragSO = new SerializedObject(drag);
            dragSO.FindProperty("boardManager").objectReferenceValue = board;
            dragSO.FindProperty("gameCamera").objectReferenceValue = cam;
            dragSO.FindProperty("pieceLayer").intValue = 1 << 0;
            dragSO.FindProperty("groundLayer").intValue = 1 << 0;
            dragSO.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
        }

        static void SetupUI(GameObject parent, BoardManager board, Camera cam)
        {
            // Canvas
            var canvasGO = new GameObject("Canvas");
            canvasGO.transform.SetParent(parent.transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem (required for UI click events)
            if (FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 0)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }

            // UIManager
            var ui = canvasGO.AddComponent<UIManager>();
            var uiSO = new SerializedObject(ui);

            // Assign Chinese font if available
            var chFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/ChineseFont SDF.asset");
            if (chFont != null)
                uiSO.FindProperty("chineseFont").objectReferenceValue = chFont;

            // Phase Text (top center)
            var phaseGO = CreateText(canvasGO.transform, "PhaseText", "准备阶段", 32, TextAnchor.MiddleCenter);
            phaseGO.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
            phaseGO.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
            phaseGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);
            uiSO.FindProperty("phaseText").objectReferenceValue = phaseGO.GetComponent<TextMeshProUGUI>();

            // Timer Text
            var timerGO = CreateText(canvasGO.transform, "TimerText", "30.0s", 28, TextAnchor.MiddleCenter);
            timerGO.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
            timerGO.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
            timerGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);
            uiSO.FindProperty("timerText").objectReferenceValue = timerGO.GetComponent<TextMeshProUGUI>();

            // Round Text (top center, left of phase)
            var roundGO = CreateText(canvasGO.transform, "RoundText", "回合: 1", 24, TextAnchor.MiddleCenter);
            roundGO.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
            roundGO.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
            roundGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(-120, -40);
            uiSO.FindProperty("roundText").objectReferenceValue = roundGO.GetComponent<TextMeshProUGUI>();

            // Player Health - hidden, managed by UIManager.CreatePlayerInfoPanel
            var hpGO = CreateText(canvasGO.transform, "PlayerHealth", "", 1, TextAnchor.MiddleLeft);
            hpGO.SetActive(false);
            uiSO.FindProperty("playerHealthText").objectReferenceValue = null;

            // AI Health - hidden, managed by UIManager.CreatePlayerInfoPanel
            var aiHpGO = CreateText(canvasGO.transform, "AIHealth", "", 1, TextAnchor.MiddleRight);
            aiHpGO.SetActive(false);
            uiSO.FindProperty("aiHealthText").objectReferenceValue = null;

            // Result Text (center, hidden by default)
            var resultGO = CreateText(canvasGO.transform, "ResultText", "", 36, TextAnchor.MiddleCenter);
            var resultRt = resultGO.GetComponent<RectTransform>();
            resultRt.anchorMin = new Vector2(0.5f, 0.5f);
            resultRt.anchorMax = new Vector2(0.5f, 0.5f);
            resultRt.anchoredPosition = Vector2.zero;
            resultRt.sizeDelta = new Vector2(800, 60);
            resultGO.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
            resultGO.SetActive(false);
            uiSO.FindProperty("resultText").objectReferenceValue = resultGO.GetComponent<TextMeshProUGUI>();

            // Game Over Panel
            var goPanel = new GameObject("GameOverPanel");
            goPanel.transform.SetParent(canvasGO.transform);
            var goPanelRt = goPanel.AddComponent<RectTransform>();
            goPanelRt.anchorMin = Vector2.zero;
            goPanelRt.anchorMax = Vector2.one;
            goPanelRt.sizeDelta = Vector2.zero;
            var goPanelImg = goPanel.AddComponent<Image>();
            goPanelImg.color = new Color(0, 0, 0, 0.85f);
            goPanel.SetActive(false);

            var goTextGO = CreateText(goPanel.transform, "GameOverText", "", 48, TextAnchor.MiddleCenter);
            goTextGO.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            goTextGO.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            goTextGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
            uiSO.FindProperty("gameOverText").objectReferenceValue = goTextGO.GetComponent<TextMeshProUGUI>();

            // Restart Button
            var btnGO = new GameObject("RestartButton");
            btnGO.transform.SetParent(goPanel.transform);
            var btnRt = btnGO.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0.5f);
            btnRt.anchorMax = new Vector2(0.5f, 0.5f);
            btnRt.sizeDelta = new Vector2(200, 60);
            btnRt.anchoredPosition = new Vector2(0, -50);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.3f);
            var btn = btnGO.AddComponent<Button>();
            var btnTextGO = CreateText(btnGO.transform, "BtnText", "重新开始", 24, TextAnchor.MiddleCenter);
            btnTextGO.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            btnTextGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
            btnTextGO.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            btn.onClick.AddListener(ui.OnRestartClicked);

            uiSO.FindProperty("gameOverPanel").objectReferenceValue = goPanel;
            uiSO.ApplyModifiedProperties();

            // Link UIManager to GameLoopManager
            var gameLoop = parent.GetComponentInChildren<GameLoopManager>();
            if (gameLoop != null)
            {
                var glSO = new SerializedObject(gameLoop);
                glSO.FindProperty("uiManager").objectReferenceValue = ui;
                glSO.ApplyModifiedProperties();
            }
        }

        static GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 50);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = GetTMPAlignment(anchor);
            tmp.color = Color.white;
            return go;
        }

        static TextAlignmentOptions GetTMPAlignment(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.MiddleRight => TextAlignmentOptions.Right,
                _ => TextAlignmentOptions.Center,
            };
        }
    }
}
#endif
