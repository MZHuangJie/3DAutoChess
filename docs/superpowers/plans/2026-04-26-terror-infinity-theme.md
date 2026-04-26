# 《无限恐怖》主题替换 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将 3D 自走棋项目的全部阵营、职业、棋子、野怪替换为《无限恐怖》主题设定

**Architecture:** 纯数据层替换 — 修改 ScriptableObject .asset 文件和 Editor 脚本中的数据定义。C# 运行时代码无需改动（数据模型已经是泛化的）。羁绊通过 string 名称匹配，所以只需保证 HeroData.factions[] 中的字符串与 FactionData.factionName 一致即可。

**Tech Stack:** Unity 6 + URP, ScriptableObject 数据驱动, TextMeshPro

---

## 设计总览

### 阵营（Origin）— 5 个

| 阵营 | factionName | 颜色 | 2人效果 | 4人效果 |
|------|------------|------|---------|---------|
| 中洲队 | 中洲队 | (0.2, 0.6, 0.9) 蓝 | +150HP/+10护甲 | +350HP/+25护甲/+15魔抗 |
| 魔鬼队 | 魔鬼队 | (0.6, 0.1, 0.1) 暗红 | +25攻击/+10%攻速 | +60攻击/+25%攻速 |
| 指环王 | 指环王 | (0.9, 0.8, 0.2) 金 | +20攻击/+15护甲 | +50攻击/+35护甲/+150HP |
| 异形 | 异形 | (0.3, 0.6, 0.3) 暗绿 | +200HP/+10%攻速 | +400HP/+20%攻速/+15护甲 |
| 生化危机 | 生化危机 | (0.7, 0.3, 0.2) 锈红 | +15攻击/+10%吸血(描述) | +35攻击/+200HP |

### 职业（Class）— 5 个

| 职业 | factionName | 颜色 | 2人效果 | 4人效果 |
|------|------------|------|---------|---------|
| 血族 | 血族 | (0.8, 0.1, 0.15) 血红 | +20攻击/+150HP | +50攻击/+350HP |
| 修真 | 修真 | (0.4, 0.3, 0.9) 紫蓝 | +30魔抗/+15攻击 | +60魔抗/+40攻击 |
| 科技 | 科技 | (0.3, 0.8, 0.9) 青 | +20%攻速 | +40%攻速/+20攻击 |
| 精神力 | 精神力 | (0.6, 0.2, 0.8) 紫 | +25魔抗/+100HP | +50魔抗/+250HP |
| 武术 | 武术 | (0.9, 0.5, 0.1) 橙 | +15攻击/+10护甲 | +40攻击/+25护甲/+10%攻速 |

### 棋子池（27 个）

**1 费（7 个）：**

| 棋子 | 阵营 | 职业 | 类型 | HP | 攻击 | 攻速 | 护甲 | 魔抗 | 射程 | 蓝量 | 初始蓝 | 技能 | 技能类型 | 技能目标 | 技能伤害 | 技能范围 | 眩晕 | 魔法? |
|------|------|------|------|-----|------|------|------|------|------|------|--------|------|---------|---------|---------|---------|------|-------|
| 萧宏吕 | 中洲队 | 武术 | 近战 | 700 | 55 | 0.65 | 35 | 20 | 1 | 100 | 0 | 铁拳 | Damage | NearestEnemy | 140 | 2 | 0 | false |
| 洛丽 | 中洲队 | 精神力 | 远程 | 500 | 35 | 0.60 | 20 | 30 | 3 | 100 | 0 | 精神治愈 | Heal | LowestHpAlly | 150 | 2 | 0 | true |
| 丧尸步兵 | 生化危机 | 血族 | 近战 | 750 | 50 | 0.55 | 30 | 15 | 1 | 120 | 0 | 撕咬 | Damage | NearestEnemy | 120 | 1.5 | 0 | false |
| 半兽人 | 指环王 | 血族 | 近战 | 800 | 45 | 0.55 | 40 | 20 | 1 | 150 | 50 | 战吼 | Stun | NearestEnemy | 40 | 2 | 1.5 | true |
| 抱脸虫 | 异形 | 武术 | 近战 | 550 | 65 | 0.85 | 15 | 15 | 1 | 80 | 0 | 寄生突袭 | Damage | NearestEnemy | 160 | 2 | 0 | false |
| 安布雷拉士兵 | 生化危机 | 科技 | 远程 | 450 | 60 | 0.75 | 20 | 15 | 3 | 90 | 0 | 扫射 | AreaDamage | AllEnemiesInRange | 70 | 2 | 0 | false |
| 哈比人 | 指环王 | 修真 | 远程 | 500 | 40 | 0.60 | 15 | 25 | 3 | 100 | 0 | 星光瓶 | Heal | LowestHpAlly | 120 | 2 | 0 | true |

**2 费（6 个）：**

| 棋子 | 阵营 | 职业 | 类型 | HP | 攻击 | 攻速 | 护甲 | 魔抗 | 射程 | 蓝量 | 初始蓝 | 技能 | 技能类型 | 技能目标 | 技能伤害 | 技能范围 | 眩晕 | 魔法? |
|------|------|------|------|-----|------|------|------|------|------|------|--------|------|---------|---------|---------|---------|------|-------|
| 张恒 | 中洲队 | 科技 | 远程 | 600 | 80 | 0.70 | 20 | 20 | 4 | 80 | 0 | 精准狙击 | Damage | NearestEnemy | 200 | 2 | 0 | false |
| 刘宇 | 中洲队 | 修真 | 远程 | 550 | 60 | 0.65 | 20 | 30 | 3 | 70 | 0 | 御剑术 | AreaDamage | AllEnemiesInRange | 90 | 2.5 | 0 | true |
| 王侠 | 中洲队 | 科技 | 远程 | 650 | 65 | 0.60 | 25 | 20 | 2 | 90 | 0 | C4炸弹 | AreaDamage | AllEnemiesInRange | 130 | 2 | 0 | false |
| 莱戈拉斯 | 指环王 | 科技 | 远程 | 500 | 75 | 0.85 | 15 | 15 | 4 | 90 | 0 | 精灵连射 | AreaDamage | AllEnemiesInRange | 80 | 2 | 0 | false |
| 暴君T-002 | 生化危机 | 血族 | 近战 | 850 | 55 | 0.50 | 40 | 25 | 1 | 120 | 30 | 暴怒冲锋 | Stun | NearestEnemy | 60 | 2 | 1.2 | false |
| 异形潜伏者 | 异形 | 武术 | 近战 | 600 | 80 | 0.90 | 20 | 20 | 1 | 80 | 0 | 尾刺穿刺 | Damage | NearestEnemy | 200 | 2 | 0 | false |

**3 费（5 个）：**

| 棋子 | 阵营 | 职业 | 类型 | HP | 攻击 | 攻速 | 护甲 | 魔抗 | 射程 | 蓝量 | 初始蓝 | 技能 | 技能类型 | 技能目标 | 技能伤害 | 技能范围 | 眩晕 | 魔法? |
|------|------|------|------|-----|------|------|------|------|------|------|--------|------|---------|---------|---------|---------|------|-------|
| 赵樱空 | 中洲队 | 武术 | 近战 | 700 | 90 | 0.85 | 25 | 25 | 1 | 60 | 0 | 暗影突袭 | Damage | NearestEnemy | 280 | 2 | 0 | false |
| 阿拉贡 | 指环王 | 武术 | 近战 | 750 | 75 | 0.70 | 35 | 25 | 1 | 80 | 0 | 安都瑞尔 | AreaDamage | AllEnemiesInRange | 150 | 2.5 | 0 | false |
| 艾丽丝 | 生化危机 | 血族 | 近战 | 700 | 70 | 0.75 | 25 | 20 | 1 | 60 | 0 | T病毒强化 | Damage | NearestEnemy | 180 | 2 | 0 | false |
| 异形战士 | 异形 | 血族 | 近战 | 800 | 65 | 0.65 | 30 | 15 | 1 | 100 | 50 | 酸血喷射 | AreaDamage | AllEnemiesInRange | 120 | 2 | 0 | true |
| 复制体樱空 | 魔鬼队 | 武术 | 近战 | 650 | 95 | 0.90 | 20 | 20 | 1 | 50 | 0 | 嗜血之舞 | Damage | NearestEnemy | 250 | 2 | 0 | false |

**4 费（5 个）：**

| 棋子 | 阵营 | 职业 | 类型 | HP | 攻击 | 攻速 | 护甲 | 魔抗 | 射程 | 蓝量 | 初始蓝 | 技能 | 技能类型 | 技能目标 | 技能伤害 | 技能范围 | 眩晕 | 魔法? |
|------|------|------|------|-----|------|------|------|------|------|------|--------|------|---------|---------|---------|---------|------|-------|
| 零 | 中洲队 | 精神力 | 远程 | 600 | 50 | 0.60 | 20 | 35 | 3 | 80 | 0 | 精神风暴 | Stun | AllEnemiesInRange | 100 | 2.5 | 1.5 | true |
| 甘道夫 | 指环王 | 修真 | 远程 | 700 | 60 | 0.55 | 30 | 40 | 3 | 90 | 20 | 你不可通过 | AreaDamage | AllEnemiesInRange | 200 | 3 | 0 | true |
| 异形女王 | 异形 | 精神力 | 近战 | 900 | 55 | 0.50 | 35 | 30 | 1 | 100 | 30 | 虫巢召唤 | AreaDamage | AllEnemiesInRange | 150 | 2.5 | 0 | true |
| 威斯克 | 生化危机 | 科技 | 近战 | 750 | 85 | 0.80 | 30 | 25 | 1 | 70 | 0 | 闪现突击 | Damage | NearestEnemy | 300 | 2 | 0 | false |
| 复制体张恒 | 魔鬼队 | 科技 | 远程 | 550 | 90 | 0.75 | 20 | 20 | 4 | 80 | 0 | 暗能狙击 | Damage | NearestEnemy | 350 | 2 | 0 | false |

**5 费（4 个）— 各自带独特羁绊：**

| 棋子 | 阵营 | 职业 | 类型 | HP | 攻击 | 攻速 | 护甲 | 魔抗 | 射程 | 蓝量 | 初始蓝 | 技能 | 技能类型 | 技能目标 | 技能伤害 | 技能范围 | 眩晕 | 魔法? |
|------|------|------|------|-----|------|------|------|------|------|------|--------|------|---------|---------|---------|---------|------|-------|
| 郑吒 | 中洲队 | 血族 | 近战 | 1000 | 100 | 0.75 | 35 | 30 | 1 | 80 | 0 | 暗焰斩 | AreaDamage | AllEnemiesInRange | 300 | 2.5 | 0 | false |
| 复制体郑吒 | 魔鬼队 | 血族 | 近战 | 1100 | 95 | 0.70 | 40 | 35 | 1 | 100 | 0 | 黑洞吞噬 | AreaDamage | AllEnemiesInRange | 250 | 3 | 0 | true |
| 楚轩 | 中洲队 | 科技 | 远程 | 700 | 60 | 0.60 | 25 | 35 | 4 | 100 | 30 | 万象推演 | Stun | AllEnemiesInRange | 80 | 3 | 2.0 | true |
| 复制体楚轩 | 魔鬼队 | 精神力 | 远程 | 650 | 40 | 0.50 | 20 | 40 | 5 | 120 | 40 | 因果律攻击 | Damage | NearestEnemy | 400 | 2 | 0 | true |

### 5 费独特羁绊（4 个，1 人即激活）

| 羁绊 | factionName | 颜色 | 1人效果 |
|------|------------|------|---------|
| 洪荒·开天辟地 | 洪荒·开天辟地 | (1.0, 0.4, 0.1) | +50攻击/+300HP/+20护甲 |
| 原暗·宇宙终结 | 原暗·宇宙终结 | (0.2, 0.0, 0.3) | +40攻击/+400HP/+25魔抗 |
| 天机·万象推演 | 天机·万象推演 | (0.9, 0.9, 0.5) | +30攻击/+20%攻速/+200HP |
| 原暗·因果律武器 | 原暗·因果律武器 | (0.3, 0.0, 0.4) | +20攻击/+300HP/+30魔抗 |

### 野怪（恐怖片世界怪物）

| 回合 | 名称 | 数量 | HP | 攻击 | 护甲 | 攻速 | 颜色 | 金币 | 装备数 | 合成? |
|------|------|------|-----|------|------|------|------|------|--------|-------|
| 1 | 丧尸 | 3 | 200 | 20 | 5 | 0.5 | (0.4, 0.5, 0.3) | 1 | 1 | false |
| 2 | 舔食者 | 3 | 350 | 35 | 10 | 0.7 | (0.6, 0.2, 0.2) | 1 | 1 | false |
| 3 | 猎犬 | 1 | 800 | 50 | 25 | 0.6 | (0.3, 0.3, 0.35) | 2 | 1 | false |
| 10 | 抱脸体 | 4 | 500 | 45 | 10 | 0.65 | (0.3, 0.4, 0.3) | 2 | 2 | false |
| 17 | 异形皇后 | 1 | 2000 | 80 | 40 | 0.5 | (0.2, 0.2, 0.3) | 3 | 1 | true |
| 24 | 主神守卫 | 1 | 3500 | 120 | 60 | 0.55 | (0.9, 0.8, 0.3) | 5 | 1 | true |

---

## 文件结构

### 需要修改的文件

**Faction SO 文件（删除旧的 10 个，创建新的 14 个）：**
- Delete: `Assets/ScriptableObjects/Factions/Faction_Warrior.asset` 等全部 10 个
- Create: `Assets/ScriptableObjects/Factions/Faction_Zhongzhou.asset` (中洲队)
- Create: `Assets/ScriptableObjects/Factions/Faction_Devil.asset` (魔鬼队)
- Create: `Assets/ScriptableObjects/Factions/Faction_LOTR.asset` (指环王)
- Create: `Assets/ScriptableObjects/Factions/Faction_Alien.asset` (异形)
- Create: `Assets/ScriptableObjects/Factions/Faction_RE.asset` (生化危机)
- Create: `Assets/ScriptableObjects/Factions/Faction_Blood.asset` (血族)
- Create: `Assets/ScriptableObjects/Factions/Faction_Xiuzhen.asset` (修真)
- Create: `Assets/ScriptableObjects/Factions/Faction_Tech.asset` (科技)
- Create: `Assets/ScriptableObjects/Factions/Faction_Psychic.asset` (精神力)
- Create: `Assets/ScriptableObjects/Factions/Faction_Martial.asset` (武术)
- Create: `Assets/ScriptableObjects/Factions/Faction_Honghuang.asset` (洪荒·开天辟地)
- Create: `Assets/ScriptableObjects/Factions/Faction_DarkEnd.asset` (原暗·宇宙终结)
- Create: `Assets/ScriptableObjects/Factions/Faction_Tianji.asset` (天机·万象推演)
- Create: `Assets/ScriptableObjects/Factions/Faction_Causal.asset` (原暗·因果律武器)

**Hero SO 文件（删除旧的 6 个，创建新的 27 个）：**
- Delete: `Assets/ScriptableObjects/Heroes/Hero_Berserker.asset` 等全部 6 个
- Create: 27 个新 Hero SO（详见棋子池设计）

**CreepRound SO 文件（修改现有 6 个的内容）：**
- Modify: `Assets/ScriptableObjects/CreepRounds/Round1_Beetles.asset` → 丧尸
- Modify: `Assets/ScriptableObjects/CreepRounds/Round2_Wolves.asset` → 舔食者
- Modify: `Assets/ScriptableObjects/CreepRounds/Round3_Gargoyle.asset` → 猎犬
- Modify: `Assets/ScriptableObjects/CreepRounds/Round10_Ghosts.asset` → 抱脸体
- Modify: `Assets/ScriptableObjects/CreepRounds/Round17_Dragon.asset` → 异形皇后
- Modify: `Assets/ScriptableObjects/CreepRounds/Round24_AncientDragon.asset` → 主神守卫

**Editor 脚本：**
- Modify: `Assets/Scripts/Editor/AutoChessSetupWindow.cs` — 更新 CreateHeroes(), CreateFactions(), CreateCreepRounds() 中的所有数据

**不需要修改的文件：**
- `Assets/Scripts/Data/*.cs` — 数据模型完全泛化，无需改动
- `Assets/Scripts/Core/*.cs` — 运行时代码通过 SO 引用，无硬编码名称
- `Assets/Scripts/UI/UIManager.cs` — UI 通过数据驱动显示
- `Assets/ScriptableObjects/Equipment/` — 装备系统保持不变
- `Assets/ScriptableObjects/Hexes/` — 海克斯强化保持不变
- `Assets/ScriptableObjects/GameConfig.asset` — 游戏配置保持不变

---

## Task 1: 删除旧 Faction SO 文件，创建新阵营 SO（5 个 Origin）

**Files:**
- Delete: `Assets/ScriptableObjects/Factions/Faction_Warrior.asset`
- Delete: `Assets/ScriptableObjects/Factions/Faction_Ranger.asset`
- Delete: `Assets/ScriptableObjects/Factions/Faction_Mage.asset`
- Delete: `Assets/ScriptableObjects/Factions/Faction_Guardian.asset`
- Delete: `Assets/ScriptableObjects/Factions/Faction_Assassin.asset`
- Delete: `Assets/ScriptableObjects/Factions/Faction_Oracle.asset`
- Delete: `Assets/ScriptableObjects/Factions/Faction_Noxus.asset`
- Delete: `Assets/ScriptableObjects/Factions/Faction_Demacia.asset`
- Delete: `Assets/ScriptableObjects/Factions/Faction_Ionia.asset`
- Delete: `Assets/ScriptableObjects/Factions/Faction_Void.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_Zhongzhou.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_Devil.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_LOTR.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_Alien.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_RE.asset`

- [ ] **Step 1: 删除旧的 10 个 Faction SO 文件**

```bash
rm "Assets/ScriptableObjects/Factions/Faction_Warrior.asset"
rm "Assets/ScriptableObjects/Factions/Faction_Warrior.asset.meta"
rm "Assets/ScriptableObjects/Factions/Faction_Ranger.asset"
rm "Assets/ScriptableObjects/Factions/Faction_Ranger.asset.meta"
rm "Assets/ScriptableObjects/Factions/Faction_Mage.asset"
rm "Assets/ScriptableObjects/Factions/Faction_Mage.asset.meta"
rm "Assets/ScriptableObjects/Factions/Faction_Guardian.asset"
rm "Assets/ScriptableObjects/Factions/Faction_Guardian.asset.meta"
rm "Assets/ScriptableObjects/Factions/Faction_Assassin.asset"
rm "Assets/ScriptableObjects/Factions/Faction_Assassin.asset.meta"
rm "Assets/ScriptableObjects/Factions/Faction_Oracle.asset"
rm "Assets/ScriptableObjects/Factions/Faction_Oracle.asset.meta"
rm "Assets/ScriptableObjects/Factions/Faction_Noxus.asset"
rm "Assets/ScriptableObjects/Factions/Faction_Noxus.asset.meta"
rm "Assets/ScriptableObjects/Factions/Faction_Demacia.asset"
rm "Assets/ScriptableObjects/Factions/Faction_Demacia.asset.meta"
rm "Assets/ScriptableObjects/Factions/Faction_Ionia.asset"
rm "Assets/ScriptableObjects/Factions/Faction_Ionia.asset.meta"
rm "Assets/ScriptableObjects/Factions/Faction_Void.asset"
rm "Assets/ScriptableObjects/Factions/Faction_Void.asset.meta"
```

- [ ] **Step 2: 创建 Faction_Zhongzhou.asset（中洲队）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Zhongzhou.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Zhongzhou
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "中洲队"
  factionColor: {r: 0.2, g: 0.6, b: 0.9, a: 1}
  thresholds:
  - count: 2
    description: "中洲队+150HP/+10护甲"
    healthBonus: 150
    attackBonus: 0
    attackSpeedBonus: 0
    armorBonus: 10
    magicResistBonus: 0
  - count: 4
    description: "中洲队+350HP/+25护甲/+15魔抗"
    healthBonus: 350
    attackBonus: 0
    attackSpeedBonus: 0
    armorBonus: 25
    magicResistBonus: 15
```

- [ ] **Step 3: 创建 Faction_Devil.asset（魔鬼队）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Devil.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Devil
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "魔鬼队"
  factionColor: {r: 0.6, g: 0.1, b: 0.1, a: 1}
  thresholds:
  - count: 2
    description: "魔鬼队+25攻击/+10%攻速"
    healthBonus: 0
    attackBonus: 25
    attackSpeedBonus: 0.1
    armorBonus: 0
    magicResistBonus: 0
  - count: 4
    description: "魔鬼队+60攻击/+25%攻速"
    healthBonus: 0
    attackBonus: 60
    attackSpeedBonus: 0.25
    armorBonus: 0
    magicResistBonus: 0
```

- [ ] **Step 4: 创建 Faction_LOTR.asset（指环王）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_LOTR.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_LOTR
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "指环王"
  factionColor: {r: 0.9, g: 0.8, b: 0.2, a: 1}
  thresholds:
  - count: 2
    description: "指环王+20攻击/+15护甲"
    healthBonus: 0
    attackBonus: 20
    attackSpeedBonus: 0
    armorBonus: 15
    magicResistBonus: 0
  - count: 4
    description: "指环王+50攻击/+35护甲/+150HP"
    healthBonus: 150
    attackBonus: 50
    attackSpeedBonus: 0
    armorBonus: 35
    magicResistBonus: 0
```

- [ ] **Step 5: 创建 Faction_Alien.asset（异形）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Alien.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Alien
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "异形"
  factionColor: {r: 0.3, g: 0.6, b: 0.3, a: 1}
  thresholds:
  - count: 2
    description: "异形+200HP/+10%攻速"
    healthBonus: 200
    attackBonus: 0
    attackSpeedBonus: 0.1
    armorBonus: 0
    magicResistBonus: 0
  - count: 4
    description: "异形+400HP/+20%攻速/+15护甲"
    healthBonus: 400
    attackBonus: 0
    attackSpeedBonus: 0.2
    armorBonus: 15
    magicResistBonus: 0
```

- [ ] **Step 6: 创建 Faction_RE.asset（生化危机）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_RE.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_RE
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "生化危机"
  factionColor: {r: 0.7, g: 0.3, b: 0.2, a: 1}
  thresholds:
  - count: 2
    description: "生化危机+15攻击/+200HP"
    healthBonus: 200
    attackBonus: 15
    attackSpeedBonus: 0
    armorBonus: 0
    magicResistBonus: 0
  - count: 4
    description: "生化危机+35攻击/+400HP"
    healthBonus: 400
    attackBonus: 35
    attackSpeedBonus: 0
    armorBonus: 0
    magicResistBonus: 0
```

- [ ] **Step 7: Commit**

```bash
git add Assets/ScriptableObjects/Factions/
git commit -m "feat: 替换阵营为无限恐怖主题 - 5个Origin阵营"
```

---

## Task 2: 创建新职业 SO（5 个 Class）

**Files:**
- Create: `Assets/ScriptableObjects/Factions/Faction_Blood.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_Xiuzhen.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_Tech.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_Psychic.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_Martial.asset`

- [ ] **Step 1: 创建 Faction_Blood.asset（血族）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Blood.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Blood
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "血族"
  factionColor: {r: 0.8, g: 0.1, b: 0.15, a: 1}
  thresholds:
  - count: 2
    description: "血族+20攻击/+150HP"
    healthBonus: 150
    attackBonus: 20
    attackSpeedBonus: 0
    armorBonus: 0
    magicResistBonus: 0
  - count: 4
    description: "血族+50攻击/+350HP"
    healthBonus: 350
    attackBonus: 50
    attackSpeedBonus: 0
    armorBonus: 0
    magicResistBonus: 0
```

- [ ] **Step 2: 创建 Faction_Xiuzhen.asset（修真）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Xiuzhen.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Xiuzhen
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "修真"
  factionColor: {r: 0.4, g: 0.3, b: 0.9, a: 1}
  thresholds:
  - count: 2
    description: "修真+30魔抗/+15攻击"
    healthBonus: 0
    attackBonus: 15
    attackSpeedBonus: 0
    armorBonus: 0
    magicResistBonus: 30
  - count: 4
    description: "修真+60魔抗/+40攻击"
    healthBonus: 0
    attackBonus: 40
    attackSpeedBonus: 0
    armorBonus: 0
    magicResistBonus: 60
```

- [ ] **Step 3: 创建 Faction_Tech.asset（科技）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Tech.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Tech
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "科技"
  factionColor: {r: 0.3, g: 0.8, b: 0.9, a: 1}
  thresholds:
  - count: 2
    description: "科技+20%攻速"
    healthBonus: 0
    attackBonus: 0
    attackSpeedBonus: 0.2
    armorBonus: 0
    magicResistBonus: 0
  - count: 4
    description: "科技+40%攻速/+20攻击"
    healthBonus: 0
    attackBonus: 20
    attackSpeedBonus: 0.4
    armorBonus: 0
    magicResistBonus: 0
```

- [ ] **Step 4: 创建 Faction_Psychic.asset（精神力）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Psychic.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Psychic
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "精神力"
  factionColor: {r: 0.6, g: 0.2, b: 0.8, a: 1}
  thresholds:
  - count: 2
    description: "精神力+25魔抗/+100HP"
    healthBonus: 100
    attackBonus: 0
    attackSpeedBonus: 0
    armorBonus: 0
    magicResistBonus: 25
  - count: 4
    description: "精神力+50魔抗/+250HP"
    healthBonus: 250
    attackBonus: 0
    attackSpeedBonus: 0
    armorBonus: 0
    magicResistBonus: 50
```

- [ ] **Step 5: 创建 Faction_Martial.asset（武术）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Martial.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Martial
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "武术"
  factionColor: {r: 0.9, g: 0.5, b: 0.1, a: 1}
  thresholds:
  - count: 2
    description: "武术+15攻击/+10护甲"
    healthBonus: 0
    attackBonus: 15
    attackSpeedBonus: 0
    armorBonus: 10
    magicResistBonus: 0
  - count: 4
    description: "武术+40攻击/+25护甲/+10%攻速"
    healthBonus: 0
    attackBonus: 40
    attackSpeedBonus: 0.1
    armorBonus: 25
    magicResistBonus: 0
```

- [ ] **Step 6: Commit**

```bash
git add Assets/ScriptableObjects/Factions/
git commit -m "feat: 添加5个Class职业羁绊 - 血族/修真/科技/精神力/武术"
```

---

## Task 3: 创建 5 费独特羁绊 SO（4 个，1 人激活）

**Files:**
- Create: `Assets/ScriptableObjects/Factions/Faction_Honghuang.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_DarkEnd.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_Tianji.asset`
- Create: `Assets/ScriptableObjects/Factions/Faction_Causal.asset`

- [ ] **Step 1: 创建 Faction_Honghuang.asset（洪荒·开天辟地 — 郑吒专属）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Honghuang.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Honghuang
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "洪荒·开天辟地"
  factionColor: {r: 1, g: 0.4, b: 0.1, a: 1}
  thresholds:
  - count: 1
    description: "洪荒·开天辟地: +50攻击/+300HP/+20护甲"
    healthBonus: 300
    attackBonus: 50
    attackSpeedBonus: 0
    armorBonus: 20
    magicResistBonus: 0
```

- [ ] **Step 2: 创建 Faction_DarkEnd.asset（原暗·宇宙终结 — 复制体郑吒专属）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_DarkEnd.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_DarkEnd
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "原暗·宇宙终结"
  factionColor: {r: 0.2, g: 0, b: 0.3, a: 1}
  thresholds:
  - count: 1
    description: "原暗·宇宙终结: +40攻击/+400HP/+25魔抗"
    healthBonus: 400
    attackBonus: 40
    attackSpeedBonus: 0
    armorBonus: 0
    magicResistBonus: 25
```

- [ ] **Step 3: 创建 Faction_Tianji.asset（天机·万象推演 — 楚轩专属）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Tianji.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Tianji
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "天机·万象推演"
  factionColor: {r: 0.9, g: 0.9, b: 0.5, a: 1}
  thresholds:
  - count: 1
    description: "天机·万象推演: +30攻击/+20%攻速/+200HP"
    healthBonus: 200
    attackBonus: 30
    attackSpeedBonus: 0.2
    armorBonus: 0
    magicResistBonus: 0
```

- [ ] **Step 4: 创建 Faction_Causal.asset（原暗·因果律武器 — 复制体楚轩专属）**

写入文件 `Assets/ScriptableObjects/Factions/Faction_Causal.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eb0c46330af41cf418dfd7c1dea1111f, type: 3}
  m_Name: Faction_Causal
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.FactionData
  factionName: "原暗·因果律武器"
  factionColor: {r: 0.3, g: 0, b: 0.4, a: 1}
  thresholds:
  - count: 1
    description: "原暗·因果律武器: +20攻击/+300HP/+30魔抗"
    healthBonus: 300
    attackBonus: 20
    attackSpeedBonus: 0
    armorBonus: 0
    magicResistBonus: 30
```

- [ ] **Step 5: Commit**

```bash
git add Assets/ScriptableObjects/Factions/
git commit -m "feat: 添加4个5费独特羁绊 - 洪荒/原暗·宇宙终结/天机/原暗·因果律武器"
```

---

## Task 4: 删除旧 Hero SO，创建 1 费棋子（7 个）

**Files:**
- Delete: `Assets/ScriptableObjects/Heroes/Hero_Berserker.asset` (+ .meta)
- Delete: `Assets/ScriptableObjects/Heroes/Hero_Archer.asset` (+ .meta)
- Delete: `Assets/ScriptableObjects/Heroes/Hero_Mage.asset` (+ .meta)
- Delete: `Assets/ScriptableObjects/Heroes/Hero_Tank.asset` (+ .meta)
- Delete: `Assets/ScriptableObjects/Heroes/Hero_Assassin.asset` (+ .meta)
- Delete: `Assets/ScriptableObjects/Heroes/Hero_Support.asset` (+ .meta)
- Create: 7 个 1 费 Hero SO

- [ ] **Step 1: 删除旧的 6 个 Hero SO 文件**

```bash
rm "Assets/ScriptableObjects/Heroes/Hero_Berserker.asset" "Assets/ScriptableObjects/Heroes/Hero_Berserker.asset.meta"
rm "Assets/ScriptableObjects/Heroes/Hero_Archer.asset" "Assets/ScriptableObjects/Heroes/Hero_Archer.asset.meta"
rm "Assets/ScriptableObjects/Heroes/Hero_Mage.asset" "Assets/ScriptableObjects/Heroes/Hero_Mage.asset.meta"
rm "Assets/ScriptableObjects/Heroes/Hero_Tank.asset" "Assets/ScriptableObjects/Heroes/Hero_Tank.asset.meta"
rm "Assets/ScriptableObjects/Heroes/Hero_Assassin.asset" "Assets/ScriptableObjects/Heroes/Hero_Assassin.asset.meta"
rm "Assets/ScriptableObjects/Heroes/Hero_Support.asset" "Assets/ScriptableObjects/Heroes/Hero_Support.asset.meta"
```

- [ ] **Step 2: 创建 Hero_XiaoHongLv.asset（萧宏吕 — 1费 中洲队/武术 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_XiaoHongLv.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_XiaoHongLv
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "萧宏吕"
  cost: 1
  displayColor: {r: 0.2, g: 0.6, b: 0.9, a: 1}
  maxHealth: 700
  attackDamage: 55
  attackSpeed: 0.65
  armor: 35
  magicResist: 20
  attackRange: 1
  maxMana: 100
  startingMana: 0
  attackType: 0
  factions:
  - "中洲队"
  - "武术"
  skillName: "铁拳"
  skillType: 1
  skillTargetType: 0
  skillDamage: 140
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 3: 创建 Hero_Luoli.asset（洛丽 — 1费 中洲队/精神力 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_Luoli.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_Luoli
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "洛丽"
  cost: 1
  displayColor: {r: 0.6, g: 0.2, b: 0.8, a: 1}
  maxHealth: 500
  attackDamage: 35
  attackSpeed: 0.6
  armor: 20
  magicResist: 30
  attackRange: 3
  maxMana: 100
  startingMana: 0
  attackType: 1
  factions:
  - "中洲队"
  - "精神力"
  skillName: "精神治愈"
  skillType: 3
  skillTargetType: 1
  skillDamage: 150
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 1
```

- [ ] **Step 4: 创建 Hero_ZombieSoldier.asset（丧尸步兵 — 1费 生化危机/血族 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_ZombieSoldier.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_ZombieSoldier
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "丧尸步兵"
  cost: 1
  displayColor: {r: 0.4, g: 0.5, b: 0.3, a: 1}
  maxHealth: 750
  attackDamage: 50
  attackSpeed: 0.55
  armor: 30
  magicResist: 15
  attackRange: 1
  maxMana: 120
  startingMana: 0
  attackType: 0
  factions:
  - "生化危机"
  - "血族"
  skillName: "撕咬"
  skillType: 1
  skillTargetType: 0
  skillDamage: 120
  skillRange: 1.5
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 5: 创建 Hero_Orc.asset（半兽人 — 1费 指环王/血族 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_Orc.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_Orc
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "半兽人"
  cost: 1
  displayColor: {r: 0.9, g: 0.8, b: 0.2, a: 1}
  maxHealth: 800
  attackDamage: 45
  attackSpeed: 0.55
  armor: 40
  magicResist: 20
  attackRange: 1
  maxMana: 150
  startingMana: 50
  attackType: 0
  factions:
  - "指环王"
  - "血族"
  skillName: "战吼"
  skillType: 4
  skillTargetType: 0
  skillDamage: 40
  skillRange: 2
  skillStunDuration: 1.5
  skillIsMagic: 1
```

- [ ] **Step 6: 创建 Hero_Facehugger.asset（抱脸虫 — 1费 异形/武术 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_Facehugger.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_Facehugger
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "抱脸虫"
  cost: 1
  displayColor: {r: 0.3, g: 0.6, b: 0.3, a: 1}
  maxHealth: 550
  attackDamage: 65
  attackSpeed: 0.85
  armor: 15
  magicResist: 15
  attackRange: 1
  maxMana: 80
  startingMana: 0
  attackType: 0
  factions:
  - "异形"
  - "武术"
  skillName: "寄生突袭"
  skillType: 1
  skillTargetType: 0
  skillDamage: 160
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 7: 创建 Hero_UmbrellaSoldier.asset（安布雷拉士兵 — 1费 生化危机/科技 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_UmbrellaSoldier.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_UmbrellaSoldier
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "安布雷拉士兵"
  cost: 1
  displayColor: {r: 0.7, g: 0.3, b: 0.2, a: 1}
  maxHealth: 450
  attackDamage: 60
  attackSpeed: 0.75
  armor: 20
  magicResist: 15
  attackRange: 3
  maxMana: 90
  startingMana: 0
  attackType: 1
  factions:
  - "生化危机"
  - "科技"
  skillName: "扫射"
  skillType: 2
  skillTargetType: 3
  skillDamage: 70
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 8: 创建 Hero_Hobbit.asset（哈比人 — 1费 指环王/修真 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_Hobbit.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_Hobbit
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "哈比人"
  cost: 1
  displayColor: {r: 0.6, g: 0.8, b: 0.4, a: 1}
  maxHealth: 500
  attackDamage: 40
  attackSpeed: 0.6
  armor: 15
  magicResist: 25
  attackRange: 3
  maxMana: 100
  startingMana: 0
  attackType: 1
  factions:
  - "指环王"
  - "修真"
  skillName: "星光瓶"
  skillType: 3
  skillTargetType: 1
  skillDamage: 120
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 1
```

- [ ] **Step 9: Commit**

```bash
git add Assets/ScriptableObjects/Heroes/
git commit -m "feat: 替换棋子为无限恐怖主题 - 7个1费棋子"
```

---

## Task 5: 创建 2 费棋子（6 个）

**Files:**
- Create: `Assets/ScriptableObjects/Heroes/Hero_ZhangHeng.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_LiuYu.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_WangXia.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_Legolas.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_TyrantT002.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_AlienLurker.asset`

- [ ] **Step 1: 创建 Hero_ZhangHeng.asset（张恒 — 2费 中洲队/科技 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_ZhangHeng.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_ZhangHeng
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "张恒"
  cost: 2
  displayColor: {r: 0.3, g: 0.8, b: 0.9, a: 1}
  maxHealth: 600
  attackDamage: 80
  attackSpeed: 0.7
  armor: 20
  magicResist: 20
  attackRange: 4
  maxMana: 80
  startingMana: 0
  attackType: 1
  factions:
  - "中洲队"
  - "科技"
  skillName: "精准狙击"
  skillType: 1
  skillTargetType: 0
  skillDamage: 200
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 2: 创建 Hero_LiuYu.asset（刘宇 — 2费 中洲队/修真 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_LiuYu.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_LiuYu
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "刘宇"
  cost: 2
  displayColor: {r: 0.4, g: 0.3, b: 0.9, a: 1}
  maxHealth: 550
  attackDamage: 60
  attackSpeed: 0.65
  armor: 20
  magicResist: 30
  attackRange: 3
  maxMana: 70
  startingMana: 0
  attackType: 1
  factions:
  - "中洲队"
  - "修真"
  skillName: "御剑术"
  skillType: 2
  skillTargetType: 3
  skillDamage: 90
  skillRange: 2.5
  skillStunDuration: 0
  skillIsMagic: 1
```

- [ ] **Step 3: 创建 Hero_WangXia.asset（王侠 — 2费 中洲队/科技 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_WangXia.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_WangXia
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "王侠"
  cost: 2
  displayColor: {r: 0.5, g: 0.7, b: 0.3, a: 1}
  maxHealth: 650
  attackDamage: 65
  attackSpeed: 0.6
  armor: 25
  magicResist: 20
  attackRange: 2
  maxMana: 90
  startingMana: 0
  attackType: 1
  factions:
  - "中洲队"
  - "科技"
  skillName: "C4炸弹"
  skillType: 2
  skillTargetType: 3
  skillDamage: 130
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 4: 创建 Hero_Legolas.asset（莱戈拉斯 — 2费 指环王/科技 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_Legolas.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_Legolas
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "莱戈拉斯"
  cost: 2
  displayColor: {r: 0.2, g: 0.8, b: 0.2, a: 1}
  maxHealth: 500
  attackDamage: 75
  attackSpeed: 0.85
  armor: 15
  magicResist: 15
  attackRange: 4
  maxMana: 90
  startingMana: 0
  attackType: 1
  factions:
  - "指环王"
  - "科技"
  skillName: "精灵连射"
  skillType: 2
  skillTargetType: 3
  skillDamage: 80
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 5: 创建 Hero_TyrantT002.asset（暴君T-002 — 2费 生化危机/血族 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_TyrantT002.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_TyrantT002
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "暴君T-002"
  cost: 2
  displayColor: {r: 0.6, g: 0.2, b: 0.2, a: 1}
  maxHealth: 850
  attackDamage: 55
  attackSpeed: 0.5
  armor: 40
  magicResist: 25
  attackRange: 1
  maxMana: 120
  startingMana: 30
  attackType: 0
  factions:
  - "生化危机"
  - "血族"
  skillName: "暴怒冲锋"
  skillType: 4
  skillTargetType: 0
  skillDamage: 60
  skillRange: 2
  skillStunDuration: 1.2
  skillIsMagic: 0
```

- [ ] **Step 6: 创建 Hero_AlienLurker.asset（异形潜伏者 — 2费 异形/武术 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_AlienLurker.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_AlienLurker
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "异形潜伏者"
  cost: 2
  displayColor: {r: 0.2, g: 0.4, b: 0.2, a: 1}
  maxHealth: 600
  attackDamage: 80
  attackSpeed: 0.9
  armor: 20
  magicResist: 20
  attackRange: 1
  maxMana: 80
  startingMana: 0
  attackType: 0
  factions:
  - "异形"
  - "武术"
  skillName: "尾刺穿刺"
  skillType: 1
  skillTargetType: 0
  skillDamage: 200
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 7: Commit**

```bash
git add Assets/ScriptableObjects/Heroes/
git commit -m "feat: 添加6个2费棋子 - 张恒/刘宇/王侠/莱戈拉斯/暴君T-002/异形潜伏者"
```

---

## Task 6: 创建 3 费棋子（5 个）

**Files:**
- Create: `Assets/ScriptableObjects/Heroes/Hero_ZhaoYingkong.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_Aragorn.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_Alice.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_AlienWarrior.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_CopyYingkong.asset`

- [ ] **Step 1: 创建 Hero_ZhaoYingkong.asset（赵樱空 — 3费 中洲队/武术 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_ZhaoYingkong.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_ZhaoYingkong
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "赵樱空"
  cost: 3
  displayColor: {r: 0.9, g: 0.5, b: 0.1, a: 1}
  maxHealth: 700
  attackDamage: 90
  attackSpeed: 0.85
  armor: 25
  magicResist: 25
  attackRange: 1
  maxMana: 60
  startingMana: 0
  attackType: 0
  factions:
  - "中洲队"
  - "武术"
  skillName: "暗影突袭"
  skillType: 1
  skillTargetType: 0
  skillDamage: 280
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 2: 创建 Hero_Aragorn.asset（阿拉贡 — 3费 指环王/武术 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_Aragorn.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_Aragorn
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "阿拉贡"
  cost: 3
  displayColor: {r: 0.9, g: 0.8, b: 0.2, a: 1}
  maxHealth: 750
  attackDamage: 75
  attackSpeed: 0.7
  armor: 35
  magicResist: 25
  attackRange: 1
  maxMana: 80
  startingMana: 0
  attackType: 0
  factions:
  - "指环王"
  - "武术"
  skillName: "安都瑞尔"
  skillType: 2
  skillTargetType: 3
  skillDamage: 150
  skillRange: 2.5
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 3: 创建 Hero_Alice.asset（艾丽丝 — 3费 生化危机/血族 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_Alice.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_Alice
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "艾丽丝"
  cost: 3
  displayColor: {r: 0.7, g: 0.3, b: 0.2, a: 1}
  maxHealth: 700
  attackDamage: 70
  attackSpeed: 0.75
  armor: 25
  magicResist: 20
  attackRange: 1
  maxMana: 60
  startingMana: 0
  attackType: 0
  factions:
  - "生化危机"
  - "血族"
  skillName: "T病毒强化"
  skillType: 1
  skillTargetType: 0
  skillDamage: 180
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 4: 创建 Hero_AlienWarrior.asset（异形战士 — 3费 异形/血族 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_AlienWarrior.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_AlienWarrior
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "异形战士"
  cost: 3
  displayColor: {r: 0.2, g: 0.3, b: 0.2, a: 1}
  maxHealth: 800
  attackDamage: 65
  attackSpeed: 0.65
  armor: 30
  magicResist: 15
  attackRange: 1
  maxMana: 100
  startingMana: 50
  attackType: 0
  factions:
  - "异形"
  - "血族"
  skillName: "酸血喷射"
  skillType: 2
  skillTargetType: 3
  skillDamage: 120
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 1
```

- [ ] **Step 5: 创建 Hero_CopyYingkong.asset（复制体樱空 — 3费 魔鬼队/武术 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_CopyYingkong.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_CopyYingkong
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "复制体樱空"
  cost: 3
  displayColor: {r: 0.5, g: 0.1, b: 0.1, a: 1}
  maxHealth: 650
  attackDamage: 95
  attackSpeed: 0.9
  armor: 20
  magicResist: 20
  attackRange: 1
  maxMana: 50
  startingMana: 0
  attackType: 0
  factions:
  - "魔鬼队"
  - "武术"
  skillName: "嗜血之舞"
  skillType: 1
  skillTargetType: 0
  skillDamage: 250
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 6: Commit**

```bash
git add Assets/ScriptableObjects/Heroes/
git commit -m "feat: 添加5个3费棋子 - 赵樱空/阿拉贡/艾丽丝/异形战士/复制体樱空"
```

---

## Task 7: 创建 4 费棋子（5 个）

**Files:**
- Create: `Assets/ScriptableObjects/Heroes/Hero_Ling.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_Gandalf.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_AlienQueen.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_Wesker.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_CopyZhangHeng.asset`

- [ ] **Step 1: 创建 Hero_Ling.asset（零 — 4费 中洲队/精神力 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_Ling.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_Ling
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "零"
  cost: 4
  displayColor: {r: 0.6, g: 0.2, b: 0.8, a: 1}
  maxHealth: 600
  attackDamage: 50
  attackSpeed: 0.6
  armor: 20
  magicResist: 35
  attackRange: 3
  maxMana: 80
  startingMana: 0
  attackType: 1
  factions:
  - "中洲队"
  - "精神力"
  skillName: "精神风暴"
  skillType: 4
  skillTargetType: 3
  skillDamage: 100
  skillRange: 2.5
  skillStunDuration: 1.5
  skillIsMagic: 1
```

- [ ] **Step 2: 创建 Hero_Gandalf.asset（甘道夫 — 4费 指环王/修真 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_Gandalf.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_Gandalf
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "甘道夫"
  cost: 4
  displayColor: {r: 0.9, g: 0.9, b: 0.9, a: 1}
  maxHealth: 700
  attackDamage: 60
  attackSpeed: 0.55
  armor: 30
  magicResist: 40
  attackRange: 3
  maxMana: 90
  startingMana: 20
  attackType: 1
  factions:
  - "指环王"
  - "修真"
  skillName: "你不可通过"
  skillType: 2
  skillTargetType: 3
  skillDamage: 200
  skillRange: 3
  skillStunDuration: 0
  skillIsMagic: 1
```

- [ ] **Step 3: 创建 Hero_AlienQueen.asset（异形女王 — 4费 异形/精神力 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_AlienQueen.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_AlienQueen
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "异形女王"
  cost: 4
  displayColor: {r: 0.2, g: 0.2, b: 0.3, a: 1}
  maxHealth: 900
  attackDamage: 55
  attackSpeed: 0.5
  armor: 35
  magicResist: 30
  attackRange: 1
  maxMana: 100
  startingMana: 30
  attackType: 0
  factions:
  - "异形"
  - "精神力"
  skillName: "虫巢召唤"
  skillType: 2
  skillTargetType: 3
  skillDamage: 150
  skillRange: 2.5
  skillStunDuration: 0
  skillIsMagic: 1
```

- [ ] **Step 4: 创建 Hero_Wesker.asset（威斯克 — 4费 生化危机/科技 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_Wesker.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_Wesker
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "威斯克"
  cost: 4
  displayColor: {r: 0.3, g: 0.3, b: 0.3, a: 1}
  maxHealth: 750
  attackDamage: 85
  attackSpeed: 0.8
  armor: 30
  magicResist: 25
  attackRange: 1
  maxMana: 70
  startingMana: 0
  attackType: 0
  factions:
  - "生化危机"
  - "科技"
  skillName: "闪现突击"
  skillType: 1
  skillTargetType: 0
  skillDamage: 300
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 5: 创建 Hero_CopyZhangHeng.asset（复制体张恒 — 4费 魔鬼队/科技 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_CopyZhangHeng.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_CopyZhangHeng
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "复制体张恒"
  cost: 4
  displayColor: {r: 0.4, g: 0.1, b: 0.1, a: 1}
  maxHealth: 550
  attackDamage: 90
  attackSpeed: 0.75
  armor: 20
  magicResist: 20
  attackRange: 4
  maxMana: 80
  startingMana: 0
  attackType: 1
  factions:
  - "魔鬼队"
  - "科技"
  skillName: "暗能狙击"
  skillType: 1
  skillTargetType: 0
  skillDamage: 350
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 6: Commit**

```bash
git add Assets/ScriptableObjects/Heroes/
git commit -m "feat: 添加5个4费棋子 - 零/甘道夫/异形女王/威斯克/复制体张恒"
```

---

## Task 8: 创建 5 费棋子（4 个传说级）

**Files:**
- Create: `Assets/ScriptableObjects/Heroes/Hero_ZhengZha.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_CopyZhengZha.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_ChuXuan.asset`
- Create: `Assets/ScriptableObjects/Heroes/Hero_CopyChuXuan.asset`

每个 5 费棋子有 3 个 factions：阵营 + 职业 + 独特羁绊。

- [ ] **Step 1: 创建 Hero_ZhengZha.asset（郑吒 — 5费 中洲队/血族/洪荒·开天辟地 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_ZhengZha.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_ZhengZha
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "郑吒"
  cost: 5
  displayColor: {r: 1, g: 0.4, b: 0.1, a: 1}
  maxHealth: 1000
  attackDamage: 100
  attackSpeed: 0.75
  armor: 35
  magicResist: 30
  attackRange: 1
  maxMana: 80
  startingMana: 0
  attackType: 0
  factions:
  - "中洲队"
  - "血族"
  - "洪荒·开天辟地"
  skillName: "暗焰斩"
  skillType: 2
  skillTargetType: 3
  skillDamage: 300
  skillRange: 2.5
  skillStunDuration: 0
  skillIsMagic: 0
```

- [ ] **Step 2: 创建 Hero_CopyZhengZha.asset（复制体郑吒 — 5费 魔鬼队/血族/原暗·宇宙终结 近战）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_CopyZhengZha.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_CopyZhengZha
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "复制体郑吒"
  cost: 5
  displayColor: {r: 0.2, g: 0, b: 0.3, a: 1}
  maxHealth: 1100
  attackDamage: 95
  attackSpeed: 0.7
  armor: 40
  magicResist: 35
  attackRange: 1
  maxMana: 100
  startingMana: 0
  attackType: 0
  factions:
  - "魔鬼队"
  - "血族"
  - "原暗·宇宙终结"
  skillName: "黑洞吞噬"
  skillType: 2
  skillTargetType: 3
  skillDamage: 250
  skillRange: 3
  skillStunDuration: 0
  skillIsMagic: 1
```

- [ ] **Step 3: 创建 Hero_ChuXuan.asset（楚轩 — 5费 中洲队/科技/天机·万象推演 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_ChuXuan.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_ChuXuan
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "楚轩"
  cost: 5
  displayColor: {r: 0.9, g: 0.9, b: 0.5, a: 1}
  maxHealth: 700
  attackDamage: 60
  attackSpeed: 0.6
  armor: 25
  magicResist: 35
  attackRange: 4
  maxMana: 100
  startingMana: 30
  attackType: 1
  factions:
  - "中洲队"
  - "科技"
  - "天机·万象推演"
  skillName: "万象推演"
  skillType: 4
  skillTargetType: 3
  skillDamage: 80
  skillRange: 3
  skillStunDuration: 2
  skillIsMagic: 1
```

- [ ] **Step 4: 创建 Hero_CopyChuXuan.asset（复制体楚轩 — 5费 魔鬼队/精神力/原暗·因果律武器 远程）**

写入文件 `Assets/ScriptableObjects/Heroes/Hero_CopyChuXuan.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d22adb44297f544595b04ff2582bc83, type: 3}
  m_Name: Hero_CopyChuXuan
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.HeroData
  heroName: "复制体楚轩"
  cost: 5
  displayColor: {r: 0.3, g: 0, b: 0.4, a: 1}
  maxHealth: 650
  attackDamage: 40
  attackSpeed: 0.5
  armor: 20
  magicResist: 40
  attackRange: 5
  maxMana: 120
  startingMana: 40
  attackType: 1
  factions:
  - "魔鬼队"
  - "精神力"
  - "原暗·因果律武器"
  skillName: "因果律攻击"
  skillType: 1
  skillTargetType: 0
  skillDamage: 400
  skillRange: 2
  skillStunDuration: 0
  skillIsMagic: 1
```

- [ ] **Step 5: Commit**

```bash
git add Assets/ScriptableObjects/Heroes/
git commit -m "feat: 添加4个5费传说棋子 - 郑吒/复制体郑吒/楚轩/复制体楚轩"
```

---

## Task 9: 替换野怪为恐怖片世界怪物

**Files:**
- Modify: `Assets/ScriptableObjects/CreepRounds/Round1_Beetles.asset`
- Modify: `Assets/ScriptableObjects/CreepRounds/Round2_Wolves.asset`
- Modify: `Assets/ScriptableObjects/CreepRounds/Round3_Gargoyle.asset`
- Modify: `Assets/ScriptableObjects/CreepRounds/Round10_Ghosts.asset`
- Modify: `Assets/ScriptableObjects/CreepRounds/Round17_Dragon.asset`
- Modify: `Assets/ScriptableObjects/CreepRounds/Round24_AncientDragon.asset`

CreepRound SO 的 GUID 是 `c736d37305d710b4f91f5ba7ef28d4c7`。保留文件名不变（因为 Scene 中通过 GUID 引用），只改内容。

- [ ] **Step 1: 修改 Round1_Beetles.asset → 丧尸**

覆写文件 `Assets/ScriptableObjects/CreepRounds/Round1_Beetles.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: c736d37305d710b4f91f5ba7ef28d4c7, type: 3}
  m_Name: Round1_Beetles
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.CreepRoundData
  roundName: "丧尸"
  creeps:
  - creepName: "丧尸"
    health: 200
    attackDamage: 20
    armor: 5
    attackSpeed: 0.5
    color: {r: 0.4, g: 0.5, b: 0.3, a: 1}
  - creepName: "丧尸"
    health: 200
    attackDamage: 20
    armor: 5
    attackSpeed: 0.5
    color: {r: 0.4, g: 0.5, b: 0.3, a: 1}
  - creepName: "丧尸"
    health: 200
    attackDamage: 20
    armor: 5
    attackSpeed: 0.5
    color: {r: 0.4, g: 0.5, b: 0.3, a: 1}
  goldReward: 1
  equipmentDropCount: 1
  dropCombinedEquipment: 0
```

- [ ] **Step 2: 修改 Round2_Wolves.asset → 舔食者**

覆写文件 `Assets/ScriptableObjects/CreepRounds/Round2_Wolves.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: c736d37305d710b4f91f5ba7ef28d4c7, type: 3}
  m_Name: Round2_Wolves
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.CreepRoundData
  roundName: "舔食者"
  creeps:
  - creepName: "舔食者"
    health: 350
    attackDamage: 35
    armor: 10
    attackSpeed: 0.7
    color: {r: 0.6, g: 0.2, b: 0.2, a: 1}
  - creepName: "舔食者"
    health: 350
    attackDamage: 35
    armor: 10
    attackSpeed: 0.7
    color: {r: 0.6, g: 0.2, b: 0.2, a: 1}
  - creepName: "舔食者"
    health: 350
    attackDamage: 35
    armor: 10
    attackSpeed: 0.7
    color: {r: 0.6, g: 0.2, b: 0.2, a: 1}
  goldReward: 1
  equipmentDropCount: 1
  dropCombinedEquipment: 0
```

- [ ] **Step 3: 修改 Round3_Gargoyle.asset → 猎犬**

覆写文件 `Assets/ScriptableObjects/CreepRounds/Round3_Gargoyle.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: c736d37305d710b4f91f5ba7ef28d4c7, type: 3}
  m_Name: Round3_Gargoyle
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.CreepRoundData
  roundName: "猎犬"
  creeps:
  - creepName: "猎犬"
    health: 800
    attackDamage: 50
    armor: 25
    attackSpeed: 0.6
    color: {r: 0.3, g: 0.3, b: 0.35, a: 1}
  goldReward: 2
  equipmentDropCount: 1
  dropCombinedEquipment: 0
```

- [ ] **Step 4: 修改 Round10_Ghosts.asset → 抱脸体**

覆写文件 `Assets/ScriptableObjects/CreepRounds/Round10_Ghosts.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: c736d37305d710b4f91f5ba7ef28d4c7, type: 3}
  m_Name: Round10_Ghosts
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.CreepRoundData
  roundName: "抱脸体"
  creeps:
  - creepName: "抱脸体"
    health: 500
    attackDamage: 45
    armor: 10
    attackSpeed: 0.65
    color: {r: 0.3, g: 0.4, b: 0.3, a: 1}
  - creepName: "抱脸体"
    health: 500
    attackDamage: 45
    armor: 10
    attackSpeed: 0.65
    color: {r: 0.3, g: 0.4, b: 0.3, a: 1}
  - creepName: "抱脸体"
    health: 500
    attackDamage: 45
    armor: 10
    attackSpeed: 0.65
    color: {r: 0.3, g: 0.4, b: 0.3, a: 1}
  - creepName: "抱脸体"
    health: 500
    attackDamage: 45
    armor: 10
    attackSpeed: 0.65
    color: {r: 0.3, g: 0.4, b: 0.3, a: 1}
  goldReward: 2
  equipmentDropCount: 2
  dropCombinedEquipment: 0
```

- [ ] **Step 5: 修改 Round17_Dragon.asset → 异形皇后**

覆写文件 `Assets/ScriptableObjects/CreepRounds/Round17_Dragon.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: c736d37305d710b4f91f5ba7ef28d4c7, type: 3}
  m_Name: Round17_Dragon
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.CreepRoundData
  roundName: "异形皇后"
  creeps:
  - creepName: "异形皇后"
    health: 2000
    attackDamage: 80
    armor: 40
    attackSpeed: 0.5
    color: {r: 0.2, g: 0.2, b: 0.3, a: 1}
  goldReward: 3
  equipmentDropCount: 1
  dropCombinedEquipment: 1
```

- [ ] **Step 6: 修改 Round24_AncientDragon.asset → 主神守卫**

覆写文件 `Assets/ScriptableObjects/CreepRounds/Round24_AncientDragon.asset`：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: c736d37305d710b4f91f5ba7ef28d4c7, type: 3}
  m_Name: Round24_AncientDragon
  m_EditorClassIdentifier: Assembly-CSharp::AutoChess.Data.CreepRoundData
  roundName: "主神守卫"
  creeps:
  - creepName: "主神守卫"
    health: 3500
    attackDamage: 120
    armor: 60
    attackSpeed: 0.55
    color: {r: 0.9, g: 0.8, b: 0.3, a: 1}
  goldReward: 5
  equipmentDropCount: 1
  dropCombinedEquipment: 1
```

- [ ] **Step 7: Commit**

```bash
git add Assets/ScriptableObjects/CreepRounds/
git commit -m "feat: 替换野怪为恐怖片世界怪物 - 丧尸/舔食者/猎犬/抱脸体/异形皇后/主神守卫"
```

---

## Task 10: 更新 AutoChessSetupWindow.cs Editor 脚本

**Files:**
- Modify: `Assets/Scripts/Editor/AutoChessSetupWindow.cs`

Editor 脚本中的 `CreateHeroes()`, `CreateFactions()`, `CreateCreepRounds()` 方法包含硬编码的旧数据。需要全部替换为新的《无限恐怖》主题数据，以确保重新运行 "AutoChess/Setup Complete Scene" 时生成正确的 SO。

注意：`CreateFactionAsset` 方法签名只接受 `(hp, atk, spd)` 三个字段，但新的 FactionThreshold 还需要 `armorBonus` 和 `magicResistBonus`。需要更新 `CreateFactionAsset` 方法签名。

- [ ] **Step 1: 更新 CreateFactionAsset 方法签名**

在 `Assets/Scripts/Editor/AutoChessSetupWindow.cs` 中，修改 `CreateFactionAsset` 方法，将 tuple 参数从 `(int count, string desc, int hp, int atk, float spd)` 改为 `(int count, string desc, int hp, int atk, float spd, int armor, int mr)`：

```csharp
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
```

- [ ] **Step 2: 替换 CreateFactions() 方法体**

替换整个 `CreateFactions()` 方法中的 faction 列表为新的 14 个羁绊（5 Origin + 5 Class + 4 独特）：

```csharp
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
```

- [ ] **Step 3: 替换 CreateHeroes() 方法体**

替换整个 `CreateHeroes()` 方法为 27 个新棋子。由于代码量大，按费用分段写入。每个棋子遵循现有模式：`ScriptableObject.CreateInstance<HeroData>()` → 设置字段 → `AssetDatabase.CreateAsset()` → `heroes.Add()`。

完整的 `CreateHeroes()` 方法需要包含所有 27 个棋子的定义。棋子数据完全来自 Task 4-8 中的 SO 文件定义，字段映射如下：
- `heroName` = SO 中的 heroName
- `cost` = SO 中的 cost
- `displayColor` = SO 中的 displayColor
- `maxHealth/attackDamage/attackSpeed/armor/magicResist/attackRange/maxMana/startingMana` = SO 中对应字段
- `attackType` = 0 (Melee) 或 1 (Ranged)
- `factions` = SO 中的 factions 数组
- `skillName/skillType/skillTargetType/skillDamage/skillRange/skillStunDuration/skillIsMagic` = SO 中对应字段
- `skillType` 枚举: 0=None, 1=Damage, 2=AreaDamage, 3=Heal, 4=Stun
- `skillTargetType` 枚举: 0=NearestEnemy, 1=LowestHpAlly, 2=Self, 3=AllEnemiesInRange

Asset 文件名使用英文拼音/英文名，格式为 `Hero_{Name}.asset`。

完整棋子列表（27个）及其 asset 文件名：

| # | 费用 | 文件名 | 中文名 |
|---|------|--------|--------|
| 1 | 1 | Hero_XiaoHongLv | 萧宏吕 |
| 2 | 1 | Hero_Luoli | 洛丽 |
| 3 | 1 | Hero_ZombieSoldier | 丧尸步兵 |
| 4 | 1 | Hero_Orc | 半兽人 |
| 5 | 1 | Hero_Facehugger | 抱脸虫 |
| 6 | 1 | Hero_UmbrellaSoldier | 安布雷拉士兵 |
| 7 | 1 | Hero_Hobbit | 哈比人 |
| 8 | 2 | Hero_ZhangHeng | 张恒 |
| 9 | 2 | Hero_LiuYu | 刘宇 |
| 10 | 2 | Hero_WangXia | 王侠 |
| 11 | 2 | Hero_Legolas | 莱戈拉斯 |
| 12 | 2 | Hero_TyrantT002 | 暴君T-002 |
| 13 | 2 | Hero_AlienLurker | 异形潜伏者 |
| 14 | 3 | Hero_ZhaoYingkong | 赵樱空 |
| 15 | 3 | Hero_Aragorn | 阿拉贡 |
| 16 | 3 | Hero_Alice | 艾丽丝 |
| 17 | 3 | Hero_AlienWarrior | 异形战士 |
| 18 | 3 | Hero_CopyYingkong | 复制体樱空 |
| 19 | 4 | Hero_Ling | 零 |
| 20 | 4 | Hero_Gandalf | 甘道夫 |
| 21 | 4 | Hero_AlienQueen | 异形女王 |
| 22 | 4 | Hero_Wesker | 威斯克 |
| 23 | 4 | Hero_CopyZhangHeng | 复制体张恒 |
| 24 | 5 | Hero_ZhengZha | 郑吒 |
| 25 | 5 | Hero_CopyZhengZha | 复制体郑吒 |
| 26 | 5 | Hero_ChuXuan | 楚轩 |
| 27 | 5 | Hero_CopyChuXuan | 复制体楚轩 |

每个棋子的完整属性数据参见 Task 4-8 中对应的 .asset 文件内容。C# 代码中的字段值必须与 .asset 文件完全一致。

- [ ] **Step 4: 替换 CreateCreepRounds() 方法体**

```csharp
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
```

- [ ] **Step 5: 更新标题文字**

在 `SetupCompleteScene()` 方法的 `EditorUtility.DisplayDialog` 中，将 "AutoChess Milestone 4" 改为 "无限恐怖自走棋"：

```csharp
EditorUtility.DisplayDialog("Setup Complete",
    "无限恐怖自走棋 scene has been set up!\n\n" +
    "Press Play to start the game.", "OK");
```

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Editor/AutoChessSetupWindow.cs
git commit -m "feat: 更新Editor脚本为无限恐怖主题数据"
```

---

## Task 11: 更新 UIManager 标题文字

**Files:**
- Modify: `Assets/Scripts/UI/UIManager.cs`

- [ ] **Step 1: 搜索并替换标题文字**

在 `UIManager.cs` 中搜索 "3D自走棋" 或 "3D Auto Chess"，替换为 "无限恐怖自走棋"。

```bash
grep -rn "3D自走棋\|3D Auto Chess\|AutoChess" Assets/Scripts/UI/UIManager.cs
```

根据搜索结果，修改 `ShowTitleScreen()` 或相关方法中的标题文字。

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/UI/UIManager.cs
git commit -m "feat: 更新UI标题为无限恐怖自走棋"
```

---

## Task 12: 验证与最终提交

- [ ] **Step 1: 验证羁绊覆盖完整性**

检查每个阵营/职业至少有 2 个棋子可以激活 2 人羁绊：

| 羁绊 | 棋子数 | 棋子列表 |
|------|--------|---------|
| 中洲队 | 9 | 萧宏吕(1), 洛丽(1), 张恒(2), 刘宇(2), 王侠(2), 赵樱空(3), 零(4), 郑吒(5), 楚轩(5) |
| 魔鬼队 | 4 | 复制体樱空(3), 复制体张恒(4), 复制体郑吒(5), 复制体楚轩(5) |
| 指环王 | 5 | 半兽人(1), 哈比人(1), 莱戈拉斯(2), 阿拉贡(3), 甘道夫(4) |
| 异形 | 4 | 抱脸虫(1), 异形潜伏者(2), 异形战士(3), 异形女王(4) |
| 生化危机 | 5 | 丧尸步兵(1), 安布雷拉士兵(1), 暴君T-002(2), 艾丽丝(3), 威斯克(4) |
| 血族 | 7 | 丧尸步兵(1), 半兽人(1), 暴君T-002(2), 艾丽丝(3), 异形战士(3), 郑吒(5), 复制体郑吒(5) |
| 修真 | 3 | 哈比人(1), 刘宇(2), 甘道夫(4) |
| 科技 | 7 | 安布雷拉士兵(1), 张恒(2), 王侠(2), 莱戈拉斯(2), 威斯克(4), 复制体张恒(4), 楚轩(5) |
| 精神力 | 4 | 洛丽(1), 零(4), 异形女王(4), 复制体楚轩(5) |
| 武术 | 6 | 萧宏吕(1), 抱脸虫(1), 异形潜伏者(2), 赵樱空(3), 阿拉贡(3), 复制体樱空(3) |

所有羁绊都有足够的棋子激活 2 人阈值。4 人阈值需要更高等级才能凑齐，符合游戏设计。

- [ ] **Step 2: 验证费用分布**

| 费用 | 数量 | 卡池大小 (poolCountByCost) |
|------|------|--------------------------|
| 1费 | 7 | 39 (每个约 5.6 张) |
| 2费 | 6 | 26 (每个约 4.3 张) |
| 3费 | 5 | 18 (每个约 3.6 张) |
| 4费 | 5 | 13 (每个约 2.6 张) |
| 5费 | 4 | 10 (每个约 2.5 张) |

总计 27 个棋子，分布合理。

- [ ] **Step 3: 在 Unity 中运行验证**

1. 打开 Unity Editor
2. 运行 "AutoChess/Setup Complete Scene" 重新生成场景
3. 点击 Play 进入游戏
4. 验证：
   - 商店显示新棋子名称
   - 羁绊面板显示新阵营/职业名称
   - 野怪回合显示新怪物名称
   - 棋子详情面板显示正确的属性和羁绊
   - 5 费棋子显示 3 个羁绊（阵营+职业+独特）
   - 所有羁绊效果正确激活

- [ ] **Step 4: 最终 Commit**

```bash
git add -A
git commit -m "feat: 无限恐怖主题替换完成 - 27棋子/14羁绊/6野怪回合"
```
