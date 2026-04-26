# 美术资源替换清单

本文档列出项目中所有使用占位视觉（纯色方块、胶囊体、硬编码颜色）的位置，以及需要的正式美术资源。

---

## 一、3D 模型

### 1.1 英雄棋子模型
- **当前**: 所有英雄共用 `Capsule` 胶囊体，通过 `displayColor` 着色
- **位置**: `AutoChessSetupWindow.cs` → `CreatePiecePrefab()`、`ChessPiece.cs` → `Initialize()`
- **需要**: 每个英雄独立的 3D 模型 (.fbx)，含 idle/攻击/施法/死亡 动画
- **放置**: `Assets/Art/Heroes/{英雄名}/`
- **数量**: 50+ (每个英雄一个)

### 1.2 野怪模型
- **当前**: 野怪也是 `Capsule` 胶囊体，通过 `CreepInfo.color` 着色
- **位置**: `CreepManager.cs` → 生成野怪处
- **需要**: 每种野怪的 3D 模型 (.fbx)
- **放置**: `Assets/Art/Heroes/Creeps/`
- **数量**: 6 种 (甲虫、狼、石像鬼、幽灵、龙、远古巨龙)

### 1.3 装备悬浮图标
- **当前**: 棋子头顶的装备用 `Cube` 小方块表示
- **位置**: `ChessPiece.cs` → `SpawnEquipmentVisual()`
- **需要**: 小型 3D 图标或 Billboard Sprite
- **放置**: `Assets/Art/Equipment/`

---

## 二、2D 图标 (Sprite)

### 2.1 英雄头像/立绘
- **当前**: 商店卡片和选秀轮用 `displayColor` 纯色方块
- **位置**: `UIManager.cs` → `UpdateShopUI()`、`ShowCarouselSelection()`
- **需要**: 每个英雄的头像 Sprite (.png)
- **放置**: `Assets/Art/Heroes/{英雄名}/`
- **数量**: 50+
- **建议尺寸**: 128×128 或 256×256

### 2.2 装备图标
- **当前**: 装备用 `displayColor` 纯色方块 + 首字显示
- **位置**: `UIManager.cs` → 装备栏、棋子详情面板装备图标、商店装备显示
- **需要**: 每件装备的图标 Sprite (.png)
- **放置**: `Assets/Art/Equipment/`
- **数量**: 44 (8 基础 + 36 合成)
- **建议尺寸**: 64×64

### 2.3 羁绊/阵营图标
- **当前**: 羁绊面板只有文字，用 `factionColor` 区分
- **位置**: `UIManager.cs` → `UpdateFactionUI()`
- **需要**: 每个阵营的徽章图标 (.png)
- **放置**: `Assets/Art/Factions/`
- **数量**: 10 (战士、射手、法师、护卫、刺客、神谕、诺克萨斯、德玛西亚、艾欧尼亚、虚空)
- **建议尺寸**: 48×48

### 2.4 技能图标
- **当前**: 详情面板技能只有文字描述，无图标
- **位置**: `UIManager.cs` → `ShowPieceDetail()` 技能区域
- **需要**: 每个技能的图标 (.png)
- **放置**: `Assets/Art/Heroes/{英雄名}/`
- **数量**: 50+ (每英雄一个技能)
- **建议尺寸**: 48×48

### 2.5 海克斯天赋图标
- **当前**: 天赋卡片用 `iconColor` 色条代替图标
- **位置**: `UIManager.cs` → `ShowAugmentSelection()`
- **需要**: 每个天赋的图标 (.png)
- **放置**: `Assets/Art/UI/Augments/`
- **数量**: 18 (每 tier 6 个)
- **建议尺寸**: 64×64

---

## 三、UI 资源

### 3.1 面板背景
- **当前**: 所有面板用纯色 `Image.color` 半透明背景
- **涉及面板**:
  - 商店面板 (`CreateShopPanel`)
  - 玩家信息面板 (`CreatePlayerInfoPanel`)
  - 羁绊面板 (`CreateFactionPanel`)
  - 装备栏面板 (`CreateEquipmentPanel`)
  - 棋子详情面板 (`CreatePieceDetailPanel`)
  - 装备详情面板 (equipDetailPanel)
  - 战斗日志面板 (`CreateBattleLogPanel`)
  - 游戏结束面板 (`CreateGameOverPanel`)
  - 标题画面 (`CreateTitleScreen`)
- **需要**: 通用面板背景 9-slice Sprite (.png)
- **放置**: `Assets/Art/UI/`
- **数量**: 2-3 种风格即可复用

### 3.2 按钮背景
- **当前**: 按钮用纯色矩形
- **涉及**: 刷新按钮、购买经验按钮、商店折叠按钮、开始游戏按钮、重新开始按钮
- **需要**: 按钮 9-slice Sprite (.png)，含 normal/hover/pressed 状态
- **放置**: `Assets/Art/UI/`
- **数量**: 2-3 种

### 3.3 商店卡片框
- **当前**: 商店卡槽是 120×100 纯色矩形
- **位置**: `UIManager.cs` → `CreateShopPanel()`
- **需要**: 卡片边框 (.png)，按费用分 5 种颜色/品质
- **放置**: `Assets/Art/UI/`
- **建议尺寸**: 120×100

### 3.4 血条贴图
- **当前**: 血条由代码拼接的纯色矩形组成 (黑色边框 + 深灰背景 + 红色填充)
- **位置**: `ChessPiece.cs` → `CreateHealthBar()`
- **需要**: 血条边框、背景、填充各一张 Sprite
- **放置**: `Assets/Art/UI/`

### 3.5 标题画面
- **当前**: 纯色深蓝背景 + 文字标题
- **位置**: `UIManager.cs` → `CreateTitleScreen()`
- **需要**: 标题背景图 (.png/.jpg) + 游戏 Logo (.png)
- **放置**: `Assets/Art/UI/`

---

## 四、材质与贴图

### 4.1 棋盘格子
- **当前**: GL 直接绘制纯色六边形 (绿/红/黄)
- **位置**: `HexGridRenderer.cs`
- **需要**: 六边形格子贴图 (.png) × 3 种 (己方/敌方/备战席) + 高亮贴图
- **放置**: `Assets/Art/UI/`

### 4.2 地面
- **当前**: `Plane` 原始体 + 纯深色材质
- **位置**: `AutoChessSetupWindow.cs` → `SetupScene()`
- **需要**: 地面贴图 (Texture2D) + PBR 材质
- **放置**: `Assets/Art/UI/`

### 4.3 棋子材质
- **当前**: 所有棋子共用一个白色基础材质 `PieceBase.mat`
- **位置**: `AutoChessSetupWindow.cs` → `CreatePieceMaterial()`
- **需要**: 替换为模型自带材质，或通用角色 Shader

---

## 五、特效

### 5.1 技能 VFX
- **当前**: 技能释放无任何视觉效果
- **位置**: `ChessPiece.cs` → `CastSkill()`
- **需要**: 每个技能的粒子特效预制体 (.prefab)
- **放置**: `Assets/Art/VFX/`

### 5.2 选中/高亮特效
- **当前**: 高亮通过修改 `_Metallic` 参数，出售高亮改为红色
- **位置**: `ChessPiece.cs` → `SetHighlight()`、`SetSellHighlight()`
- **需要**: 描边 Shader 或选中光圈特效
- **放置**: `Assets/Art/VFX/`

---

## 六、需要添加的 ScriptableObject 字段

替换素材时需要给数据类添加 Sprite/Prefab 引用字段：

| 文件 | 需添加字段 |
|------|-----------|
| `HeroData.cs` | `Sprite portrait` (头像)、`GameObject modelPrefab` (3D模型)、`Sprite skillIcon` (技能图标) |
| `EquipmentData.cs` | `Sprite icon` (装备图标) |
| `FactionData.cs` | `Sprite factionIcon` (阵营图标) |
| `AugmentData.cs` | `Sprite icon` (天赋图标) |
| `CreepRoundData.cs` | `GameObject creepPrefab` (野怪模型预制体) |
