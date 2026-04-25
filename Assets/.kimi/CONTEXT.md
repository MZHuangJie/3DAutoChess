# AutoChess 项目上下文 (供新对话快速接入)

## 项目概况
- 这是一个 3D 自走棋游戏（金铲铲/TFT-like），Unity 6 URP 项目
- 当前阶段：**里程碑 2 核心系统已完成，商店/人口/升星/羁绊/经济/8人局全部实现**
- 美术：纯色几何体占位（Cube/Capsule）
- 规模：目前是 1人+1AI（MVP阶段），目标是扩展到 1人+7AI

## 已完成内容 (里程碑 1 ✅)
1. 棋盘系统：8行×7列棋盘 + 9格候场区(Bench)
2. 6个英雄数据（狂战士、神射手、元素法师、铁壁守卫、暗影刺客、圣光祭司）
3. 英雄实体：自动寻敌、移动、普攻、血量、蓝条、死亡判定
4. 准备阶段 ↔ 战斗阶段循环（30秒准备倒计时）
5. 鼠标拖拽排兵布阵（棋盘/Bench之间）
6. 回合制血量结算（基础2伤害 + 存活星级）
7. 1个随机AI对手（每回合随机生成阵容）
8. 游戏结束判定 + 重新开始
9. 基础UI（阶段、倒计时、血量、回合、结果）
10. 相机控制（右键旋转、滚轮缩放）

## 已知待处理问题
- [ ] 输入系统当前设置为 **Both**（旧+新兼容），如需纯 Input System 需改代码
- [ ] TextMeshPro Essential Resources 已导入，无需重复操作

### 🔴 高优先级 Bug（运行时）— ✅ 已全部修复

1. ~~**镜像棋子残留污染 opponent.boardPieces（内存泄漏）**~~ ✅ 已修复：ClearEnemySide 和 LoadOpponentMirror 清理时从 owner.boardPieces 移除
2. ~~**战斗超时未生效（可能导致无限战斗）**~~ ✅ 已修复：CombatManager 加入 combatTimer，超过 combatMaxDuration 按剩余血量判定胜负

### 🟡 低优先级 问题 / 注意事项 — ✅ 已全部修复

3. ~~**最终存活者排名可能为 0**~~ ✅ 已修复：game over 时给 lastAlive 设置 placement = 1
4. ~~**羁绊百分比数值与描述不符**~~ ✅ 已修复：攻速加成改为乘法 attackSpeed *= (1 + bonus)
5. ~~**RestartGame 中 CleanupNullPieces 清理有延迟**~~ ✅ 已修复：新增 ClearAllTracking() 直接清空列表和 slot

## 下一步：里程碑 3 — 进阶系统
需要实现的功能清单：
1. **装备系统**：PvE掉落、两件合成、穿戴加成
2. **英雄技能**：蓝条满后自动释放
3. **PvE野怪轮**：特定回合与野怪战斗，掉落装备
4. **AI强化**：AI会穿戴装备、调整站位

## 关键文件位置
- 游戏配置：`Assets/ScriptableObjects/GameConfig.asset`
- 英雄数据：`Assets/ScriptableObjects/Heroes/`
- 羁绊数据：`Assets/ScriptableObjects/Factions/`（10个羁绊已配置）
- 核心脚本：`Assets/Scripts/Core/`
- AI脚本：`Assets/Scripts/AI/`
- UI脚本：`Assets/Scripts/UI/`
- 场景配置脚本：`Assets/Scripts/Editor/AutoChessSetupWindow.cs`
- 计划文档：`PLAN.md`

## Unity 操作说明
- 运行场景配置：`菜单栏 → AutoChess → Setup Complete Scene`
- 如果丢失引用，重新运行上述菜单即可
- 当前场景文件：`Assets/Scenes/MainGame.unity`（Setup后自动生成）
- 启动场景按 Play

## 操作方式
- **左键拖拽**：移动英雄（棋盘 / Bench）
- **右键拖动**：旋转视角
- **滚轮**：缩放视角

---
*Context Version: 1.0*
*Last Updated: 2026-04-25*
*Milestone 2 completed*
