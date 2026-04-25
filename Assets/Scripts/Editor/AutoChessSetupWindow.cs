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

            // 2. Create Materials
            var mat = CreatePieceMaterial();

            // 3. Create Piece Prefab
            var piecePrefab = CreatePiecePrefab(mat);

            // 4. Setup Scene
            SetupScene(gameConfig, heroes, factions, equipment, creepRounds, piecePrefab);

            // Save
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Setup Complete",
                "AutoChess Milestone 3 scene has been set up!\n\n" +
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
            config.startingLevel = 3;
            config.maxLevel = 10;
            config.baseIncomePerRound = 5;
            config.maxInterest = 5;
            config.interestPer10Gold = 1;
            config.streakBonus = new int[] { 0, 0, 1, 2, 3 };
            config.shopSlotCount = 5;
            config.refreshCost = 2;
            config.expCost = new int[] { 0, 0, 2, 4, 8, 12, 20, 32, 48, 80 };
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

            // Warrior - Red
            var h1 = ScriptableObject.CreateInstance<HeroData>();
            h1.heroName = "狂战士";
            h1.cost = 1;
            h1.displayColor = new Color(0.9f, 0.2f, 0.2f);
            h1.maxHealth = 700;
            h1.attackDamage = 60;
            h1.attackSpeed = 0.65f;
            h1.armor = 35;
            h1.magicResist = 20;
            h1.attackRange = 1f;
            h1.maxMana = 100;
            h1.startingMana = 0;
            h1.attackType = AttackType.Melee;
            h1.factions = new string[] { "战士", "诺克萨斯" };
            h1.skillName = "狂暴斩击";
            h1.skillType = SkillType.Damage;
            h1.skillTargetType = SkillTargetType.NearestEnemy;
            h1.skillDamage = 150;
            h1.skillIsMagic = false;
            AssetDatabase.CreateAsset(h1, "Assets/ScriptableObjects/Heroes/Hero_Berserker.asset");
            heroes.Add(h1);

            // Archer - Green
            var h2 = ScriptableObject.CreateInstance<HeroData>();
            h2.heroName = "神射手";
            h2.cost = 2;
            h2.displayColor = new Color(0.2f, 0.8f, 0.2f);
            h2.maxHealth = 450;
            h2.attackDamage = 70;
            h2.attackSpeed = 0.85f;
            h2.armor = 15;
            h2.magicResist = 15;
            h2.attackRange = 4f;
            h2.maxMana = 90;
            h2.startingMana = 0;
            h2.attackType = AttackType.Ranged;
            h2.factions = new string[] { "射手", "艾欧尼亚" };
            h2.skillName = "箭雨";
            h2.skillType = SkillType.AreaDamage;
            h2.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h2.skillDamage = 80;
            h2.skillRange = 2f;
            h2.skillIsMagic = false;
            AssetDatabase.CreateAsset(h2, "Assets/ScriptableObjects/Heroes/Hero_Archer.asset");
            heroes.Add(h2);

            // Mage - Blue
            var h3 = ScriptableObject.CreateInstance<HeroData>();
            h3.heroName = "元素法师";
            h3.cost = 3;
            h3.displayColor = new Color(0.2f, 0.4f, 0.9f);
            h3.maxHealth = 500;
            h3.attackDamage = 55;
            h3.attackSpeed = 0.6f;
            h3.armor = 18;
            h3.magicResist = 30;
            h3.attackRange = 4f;
            h3.maxMana = 120;
            h3.startingMana = 30;
            h3.attackType = AttackType.Ranged;
            h3.factions = new string[] { "法师", "虚空" };
            h3.skillName = "元素风暴";
            h3.skillType = SkillType.AreaDamage;
            h3.skillTargetType = SkillTargetType.AllEnemiesInRange;
            h3.skillDamage = 120;
            h3.skillRange = 2.5f;
            h3.skillIsMagic = true;
            AssetDatabase.CreateAsset(h3, "Assets/ScriptableObjects/Heroes/Hero_Mage.asset");
            heroes.Add(h3);

            // Tank - Yellow
            var h4 = ScriptableObject.CreateInstance<HeroData>();
            h4.heroName = "铁壁守卫";
            h4.cost = 1;
            h4.displayColor = new Color(0.9f, 0.8f, 0.2f);
            h4.maxHealth = 900;
            h4.attackDamage = 40;
            h4.attackSpeed = 0.55f;
            h4.armor = 45;
            h4.magicResist = 35;
            h4.attackRange = 1f;
            h4.maxMana = 150;
            h4.startingMana = 50;
            h4.attackType = AttackType.Melee;
            h4.factions = new string[] { "护卫", "德玛西亚" };
            h4.skillName = "震地猛击";
            h4.skillType = SkillType.Stun;
            h4.skillTargetType = SkillTargetType.NearestEnemy;
            h4.skillDamage = 40;
            h4.skillStunDuration = 1.5f;
            h4.skillIsMagic = true;
            AssetDatabase.CreateAsset(h4, "Assets/ScriptableObjects/Heroes/Hero_Tank.asset");
            heroes.Add(h4);

            // Assassin - Purple
            var h5 = ScriptableObject.CreateInstance<HeroData>();
            h5.heroName = "暗影刺客";
            h5.cost = 2;
            h5.displayColor = new Color(0.7f, 0.2f, 0.8f);
            h5.maxHealth = 550;
            h5.attackDamage = 75;
            h5.attackSpeed = 0.9f;
            h5.armor = 20;
            h5.magicResist = 20;
            h5.attackRange = 1f;
            h5.maxMana = 80;
            h5.startingMana = 0;
            h5.attackType = AttackType.Melee;
            h5.factions = new string[] { "刺客", "诺克萨斯" };
            h5.skillName = "暗影突袭";
            h5.skillType = SkillType.Damage;
            h5.skillTargetType = SkillTargetType.NearestEnemy;
            h5.skillDamage = 200;
            h5.skillIsMagic = false;
            AssetDatabase.CreateAsset(h5, "Assets/ScriptableObjects/Heroes/Hero_Assassin.asset");
            heroes.Add(h5);

            // Support - Cyan
            var h6 = ScriptableObject.CreateInstance<HeroData>();
            h6.heroName = "圣光祭司";
            h6.cost = 2;
            h6.displayColor = new Color(0.2f, 0.9f, 0.9f);
            h6.maxHealth = 500;
            h6.attackDamage = 35;
            h6.attackSpeed = 0.6f;
            h6.armor = 20;
            h6.magicResist = 25;
            h6.attackRange = 3f;
            h6.maxMana = 100;
            h6.startingMana = 0;
            h6.attackType = AttackType.Ranged;
            h6.factions = new string[] { "神谕", "德玛西亚" };
            h6.skillName = "圣光治愈";
            h6.skillType = SkillType.Heal;
            h6.skillTargetType = SkillTargetType.LowestHpAlly;
            h6.skillDamage = 150;
            h6.skillIsMagic = true;
            AssetDatabase.CreateAsset(h6, "Assets/ScriptableObjects/Heroes/Hero_Support.asset");
            heroes.Add(h6);

            return heroes;
        }

        static System.Collections.Generic.List<FactionData> CreateFactions()
        {
            EnsureDirectory("Assets/ScriptableObjects/Factions");
            var factions = new System.Collections.Generic.List<FactionData>();
            factions.Add(CreateFactionAsset("Faction_Warrior", "战士", new Color(0.8f, 0.2f, 0.2f),
                new (int, string, int, int, float)[] { (2, "战士+150HP", 150, 0, 0), (4, "战士+350HP/+25攻击", 350, 25, 0) }));
            factions.Add(CreateFactionAsset("Faction_Ranger", "射手", new Color(0.2f, 0.8f, 0.2f),
                new (int, string, int, int, float)[] { (2, "射手+20%攻速", 0, 0, 0.2f), (4, "射手+40%攻速/+20攻击", 0, 20, 0.4f) }));
            factions.Add(CreateFactionAsset("Faction_Mage", "法师", new Color(0.2f, 0.4f, 0.9f),
                new (int, string, int, int, float)[] { (2, "法师+30魔抗/+20%技能", 0, 30, 0), (4, "法师+60魔抗/+50%技能", 0, 60, 0) }));
            factions.Add(CreateFactionAsset("Faction_Guardian", "护卫", new Color(0.9f, 0.8f, 0.2f),
                new (int, string, int, int, float)[] { (2, "护卫+25护甲", 0, 25, 0), (4, "护卫+60护甲/+200HP", 200, 60, 0) }));
            factions.Add(CreateFactionAsset("Faction_Assassin", "刺客", new Color(0.7f, 0.2f, 0.8f),
                new (int, string, int, int, float)[] { (2, "刺客+15%攻击", 0, 15, 0), (4, "刺客+40%攻击/+15%攻速", 0, 40, 0.15f) }));
            factions.Add(CreateFactionAsset("Faction_Oracle", "神谕", new Color(0.2f, 0.9f, 0.9f),
                new (int, string, int, int, float)[] { (2, "神谕+15%HP/+10魔抗", 15, 10, 0), (4, "神谕+30%HP/+25魔抗", 30, 25, 0) }));
            factions.Add(CreateFactionAsset("Faction_Noxus", "诺克萨斯", new Color(0.6f, 0.1f, 0.1f),
                new (int, string, int, int, float)[] { (2, "诺克萨斯+20攻击", 0, 20, 0), (4, "诺克萨斯+50攻击/+150HP", 150, 50, 0) }));
            factions.Add(CreateFactionAsset("Faction_Demacia", "德玛西亚", new Color(0.9f, 0.8f, 0.3f),
                new (int, string, int, int, float)[] { (2, "德玛西亚+25护甲/+15魔抗", 0, 25, 0), (4, "德玛西亚+50护甲/+35魔抗/+100HP", 100, 50, 0) }));
            factions.Add(CreateFactionAsset("Faction_Ionia", "艾欧尼亚", new Color(0.2f, 0.7f, 0.3f),
                new (int, string, int, int, float)[] { (2, "艾欧尼亚+10%攻速", 0, 0, 0.1f), (4, "艾欧尼亚+25%攻速/+15%HP", 15, 0, 0.25f) }));
            factions.Add(CreateFactionAsset("Faction_Void", "虚空", new Color(0.4f, 0.1f, 0.6f),
                new (int, string, int, int, float)[] { (2, "虚空+20攻击/+10魔抗", 0, 20, 0), (4, "虚空+50攻击/+30魔抗/+10%HP", 10, 50, 0) }));
            return factions;
        }

        static FactionData CreateFactionAsset(string fileName, string name, Color color,
            (int count, string desc, int hp, int atk, float spd)[] thresholds)
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
                    attackSpeedBonus = thresholds[i].spd
                };
            }
            AssetDatabase.CreateAsset(f, $"Assets/ScriptableObjects/Factions/{fileName}.asset");
            return f;
        }

        static System.Collections.Generic.List<EquipmentData> CreateEquipment()
        {
            EnsureDirectory("Assets/ScriptableObjects/Equipment");
            var all = new System.Collections.Generic.List<EquipmentData>();

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

            var ie = CreateEquipmentAsset("无尽之刃", EquipmentType.Combined, new Color(1f, 0.4f, 0.2f), attackBonus: 40);
            ie.recipe1 = sword; ie.recipe2 = sword;
            EditorUtility.SetDirty(ie);

            var warmog = CreateEquipmentAsset("狂徒铠甲", EquipmentType.Combined, new Color(0.9f, 0.7f, 0.2f), healthBonus: 500);
            warmog.recipe1 = belt; warmog.recipe2 = belt;
            EditorUtility.SetDirty(warmog);

            var guinsoo = CreateEquipmentAsset("鬼索的狂暴之刃", EquipmentType.Combined, new Color(0.3f, 0.9f, 0.4f), attackSpeedBonus: 0.5f);
            guinsoo.recipe1 = bow; guinsoo.recipe2 = bow;
            EditorUtility.SetDirty(guinsoo);

            var archangel = CreateEquipmentAsset("大天使之杖", EquipmentType.Combined, new Color(0.3f, 0.5f, 1f), manaBonus: 30, spellDamageBonus: 40);
            archangel.recipe1 = tear; archangel.recipe2 = rod;
            EditorUtility.SetDirty(archangel);

            all.Add(ie); all.Add(warmog); all.Add(guinsoo); all.Add(archangel);
            return all;
        }

        static EquipmentData CreateEquipmentAsset(string name, EquipmentType type, Color color,
            int healthBonus = 0, int attackBonus = 0, int armorBonus = 0, int magicResistBonus = 0,
            float attackSpeedBonus = 0f, int manaBonus = 0, float lifestealPercent = 0f, int spellDamageBonus = 0)
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
            string safeName = name.Replace(" ", "_");
            AssetDatabase.CreateAsset(eq, $"Assets/ScriptableObjects/Equipment/Eq_{safeName}.asset");
            return eq;
        }

        static System.Collections.Generic.List<CreepRoundData> CreateCreepRounds()
        {
            EnsureDirectory("Assets/ScriptableObjects/CreepRounds");
            var rounds = new System.Collections.Generic.List<CreepRoundData>();

            rounds.Add(CreateCreepRoundAsset("Round1_Beetles", "石甲虫", 1, 1, false,
                new CreepInfo[] {
                    new CreepInfo { creepName = "石甲虫", health = 200, attackDamage = 20, armor = 5, attackSpeed = 0.5f, color = new Color(0.5f, 0.4f, 0.3f) },
                    new CreepInfo { creepName = "石甲虫", health = 200, attackDamage = 20, armor = 5, attackSpeed = 0.5f, color = new Color(0.5f, 0.4f, 0.3f) },
                    new CreepInfo { creepName = "石甲虫", health = 200, attackDamage = 20, armor = 5, attackSpeed = 0.5f, color = new Color(0.5f, 0.4f, 0.3f) }
                }));

            rounds.Add(CreateCreepRoundAsset("Round2_Wolves", "灰狼", 1, 1, false,
                new CreepInfo[] {
                    new CreepInfo { creepName = "灰狼", health = 350, attackDamage = 35, armor = 10, attackSpeed = 0.7f, color = new Color(0.4f, 0.4f, 0.45f) },
                    new CreepInfo { creepName = "灰狼", health = 350, attackDamage = 35, armor = 10, attackSpeed = 0.7f, color = new Color(0.4f, 0.4f, 0.45f) },
                    new CreepInfo { creepName = "灰狼", health = 350, attackDamage = 35, armor = 10, attackSpeed = 0.7f, color = new Color(0.4f, 0.4f, 0.45f) }
                }));

            rounds.Add(CreateCreepRoundAsset("Round3_Gargoyle", "石像鬼", 2, 1, false,
                new CreepInfo[] {
                    new CreepInfo { creepName = "石像鬼", health = 800, attackDamage = 50, armor = 25, attackSpeed = 0.6f, color = new Color(0.35f, 0.35f, 0.4f) }
                }));

            rounds.Add(CreateCreepRoundAsset("Round10_Ghosts", "幽灵", 2, 2, false,
                new CreepInfo[] {
                    new CreepInfo { creepName = "幽灵", health = 500, attackDamage = 45, armor = 10, attackSpeed = 0.65f, color = new Color(0.6f, 0.7f, 0.9f) },
                    new CreepInfo { creepName = "幽灵", health = 500, attackDamage = 45, armor = 10, attackSpeed = 0.65f, color = new Color(0.6f, 0.7f, 0.9f) },
                    new CreepInfo { creepName = "幽灵", health = 500, attackDamage = 45, armor = 10, attackSpeed = 0.65f, color = new Color(0.6f, 0.7f, 0.9f) },
                    new CreepInfo { creepName = "幽灵", health = 500, attackDamage = 45, armor = 10, attackSpeed = 0.65f, color = new Color(0.6f, 0.7f, 0.9f) }
                }));

            rounds.Add(CreateCreepRoundAsset("Round17_Dragon", "巨龙", 3, 1, true,
                new CreepInfo[] {
                    new CreepInfo { creepName = "巨龙", health = 2000, attackDamage = 80, armor = 40, attackSpeed = 0.5f, color = new Color(0.8f, 0.2f, 0.1f) }
                }));

            rounds.Add(CreateCreepRoundAsset("Round24_AncientDragon", "远古龙", 5, 1, true,
                new CreepInfo[] {
                    new CreepInfo { creepName = "远古龙", health = 3500, attackDamage = 120, armor = 60, attackSpeed = 0.55f, color = new Color(0.9f, 0.6f, 0.1f) }
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
                    go.GetComponent<CreepManager>() != null)
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

            // Round Text (top left)
            var roundGO = CreateText(canvasGO.transform, "RoundText", "回合: 1", 24, TextAnchor.MiddleLeft);
            roundGO.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1f);
            roundGO.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1f);
            roundGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, -30);
            uiSO.FindProperty("roundText").objectReferenceValue = roundGO.GetComponent<TextMeshProUGUI>();

            // Player Health (bottom left)
            var hpGO = CreateText(canvasGO.transform, "PlayerHealth", "玩家: 100 HP", 24, TextAnchor.MiddleLeft);
            hpGO.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            hpGO.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
            hpGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, 160);
            uiSO.FindProperty("playerHealthText").objectReferenceValue = hpGO.GetComponent<TextMeshProUGUI>();

            // AI Health (bottom right, above shop)
            var aiHpGO = CreateText(canvasGO.transform, "AIHealth", "对手: 100 HP", 24, TextAnchor.MiddleRight);
            aiHpGO.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
            aiHpGO.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
            aiHpGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, 160);
            uiSO.FindProperty("aiHealthText").objectReferenceValue = aiHpGO.GetComponent<TextMeshProUGUI>();

            // Result Text (center, hidden by default)
            var resultGO = CreateText(canvasGO.transform, "ResultText", "", 36, TextAnchor.MiddleCenter);
            resultGO.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            resultGO.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            resultGO.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
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
