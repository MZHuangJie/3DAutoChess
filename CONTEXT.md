# 3D AutoChess 项目上下文

## 项目概况
Unity 6 (C#) 3D 自走棋游戏，URP 渲染管线，所有 UI 文本为中文。

## 命名空间
- `AutoChess.Core` — 核心逻辑（BoardManager, ChessPiece, CombatManager, GameLoopManager 等）
- `AutoChess.Data` — 数据定义（HeroData, GameConfig, EquipmentData, FactionData, PlayerData 等）
- `AutoChess.AI` — AI 控制器
- `AutoChess.UI` — UIManager
- `AutoChess.Editor` — 编辑器工具（AutoChessSetupWindow, ChineseFontSetup）

## 里程碑完成状态

### 里程碑 1 ✅ — 核心玩法
棋盘（六边形网格）、商店、拖拽、基础战斗、备战席

### 里程碑 2 ✅ — 多人与经济
经济系统（利息/连胜连败）、羁绊系统（10 个阵营）、升星合成（3 合 1）、8 人局（1 人类 + 7 AI）、镜像对战（PlayerBoard 快照）

### 里程碑 3 ✅ — 装备/技能/PvE/AI（刚完成）
- **英雄技能系统**: 6 个英雄各有技能（Damage/AreaDamage/Heal/Stun），蓝条满释放，魔法伤害路径，眩晕机制
- **装备系统**: 8 基础 + 4 合成装备，穿戴/自动合成/升星转移/卖出退还
- **PvE 野怪轮**: 第 1/2/3/10/17/24 回合打野怪，掉落装备
- **AI 强化**: 主羁绊追踪、升星优先购买、坦克/输出站位、装备自动分配

## 关键架构

### 游戏循环
`GameLoopManager` 控制：Preparation → Combat → Result → 下一回合
- PvE 回合：CreepManager 生成野怪，胜利后 GrantRewards 发装备
- PvP 回合：SelectOpponent → LoadOpponentMirror → CombatManager.StartCombat

### 棋子生命周期
SpawnPiece → PlacePiece/PlacePieceOnBench → RemovePieceFromAnywhere → RemoveFromTracking → Destroy
- AI 棋子隐藏在 y=-100，不占物理棋盘格子
- 镜像对战时 PlayerBoard.LoadTo 复制到棋盘

### 编辑器一键设置
`AutoChess > Setup Scene` 运行 AutoChessSetupWindow，自动创建所有 ScriptableObject 和场景对象

## 已修复的 Bug
1. EventSystem 缺失导致 UI 无响应
2. 中文字体不显示（TMP 缺 CJK 字符）→ ChineseFontSetup 工具
3. 镜像棋子内存泄漏（未从 owner.boardPieces 移除）
4. 战斗超时无判定（添加 combatTimer）
5. 最后存活者排名为 0
6. 羁绊攻速加成改为乘法
7. RestartGame 清理延迟（ClearAllTracking）
8. 第一回合商店只有 2 个英雄（去掉自动购买，改为免费赠送）
9. 备战席棋子被计入羁绊（过滤 isOnBench）
10. 结算文本不消失（ShowPhase 时隐藏 resultText/matchupText）
11. 第二回合商店不显示棋子（UpdateUI 移到 RefreshShop 之后）
12. 装备背包 UI 偏移（SetParent 加 false 参数）
13. 羁绊只显示已激活的（改为显示所有，格式 "战士 1/2"，未激活灰色）

## 当前已知状态
- Hierarchy 中大量棋子 GameObject 是正常的（7 个 AI 各自买棋子，隐藏在 y=-100）
- 左上角玩家列表显示 8 个玩家血量排行是正常设计
- 装备背包面板在没有装备时自动隐藏

## 文件结构
```
Assets/Scripts/
├── AI/
│   ├── AIController.cs      — AI 决策（购买/站位/装备）
│   └── AIManager.cs         — AI 回合管理
├── Core/
│   ├── BoardManager.cs      — 棋盘/格子/备战席管理
│   ├── CameraController.cs  — 相机控制
│   ├── ChessPiece.cs        — 棋子（战斗/技能/装备/状态机）
│   ├── CombatManager.cs     — 战斗管理（超时/范围查找）
│   ├── CreepManager.cs      — PvE 野怪生成/奖励
│   ├── DragController.cs    — 拖拽（棋子+装备）
│   ├── EconomyManager.cs    — 经济（利息/连胜连败）
│   ├── EquipmentManager.cs  — 装备穿戴/合成/转移
│   ├── FactionManager.cs    — 羁绊计算/应用
│   ├── GameLoopManager.cs   — 游戏主循环
│   ├── HeroPool.cs          — 英雄池（抽卡概率）
│   ├── HexGridRenderer.cs   — 六边形网格渲染（GL API）
│   ├── PlayerBoard.cs       — 玩家棋盘快照（镜像用）
│   ├── ShopManager.cs       — 商店（刷新/购买/卖出）
│   └── StarMergeManager.cs  — 升星合成（含装备转移）
├── Data/
│   ├── CreepRoundData.cs    — 野怪回合 SO
│   ├── EquipmentData.cs     — 装备 SO
│   ├── FactionData.cs       — 羁绊 SO
│   ├── GameConfig.cs        — 全局配置 SO
│   ├── HeroData.cs          — 英雄 SO
│   └── PlayerData.cs        — 玩家运行时数据
├── Editor/
│   ├── AutoChessSetupWindow.cs — 一键场景设置
│   └── ChineseFontSetup.cs    — 中文字体设置
└── UI/
    └── UIManager.cs         — 全部 UI（商店/血量/羁绊/装备/结算）
```

## 下一步（里程碑 4 待规划）
尚未规划。可能方向：更多英雄/装备、视觉特效、音效、排行榜、存档系统等。

## 注意事项
- 运行前需执行 `AutoChess > Setup Scene` 生成场景
- 中文字体需执行 `AutoChess > Setup Chinese Font` 生成 TMP SDF 字体
- 所有 UI 文本使用中文
- 用户偏好中文交流
