# 工作流约定 —— 你和代码之间的接口

这份文档是**你 K 动画、搭关卡时的检查清单**，也是我写代码时遵守的契约。
分工：**代码与架构我做；3D（动画/手感参数/关卡搭建）你做。** 你卡住时问我"该怎么做"，你来做；我保证"手感旋钮"全部是编辑器里可调的参数，不是硬编码。

---

## 1. 第一人称到底怎么做（含"你 K 的动画为什么仍然有用"）

### 1.1 现状：结构正确

现在是 `Player` → `MainCamera`（局部 `(0, 1.5, -0.3)`），yaw 转身体、pitch 转相机。这是正确做法，而且**第一人称比第三人称省事**：不需要 `SpringArm3D` 处理贴墙，因为相机本来就在胶囊体内部，走到墙边天然不会穿墙。

相机固定在"身高 1.5m"这个数值上、**不跟动画**——这是刻意的，不是缺陷。理由：
- **绝不能**把相机挂到头部骨骼上：那等于把躯干与头部的动画运动全部叠加到视角上，既破坏鼠标俯仰控制，也必然晕。"动作完全保真"和"玩家不晕"不可兼得。
- 视角动效必须**刻意设计、幅度小**：要走路/落地的晃动，就单独做一层 ViewBob（幅度、频率、可按速度驱动），默认很轻微、可一键关闭。
- 蹲下时的视角高度由**代码**压低（读蹲行状态给一个相机高度参数），不去跟头骨骼的动画。

### 1.2 目标结构

```
Player (CharacterBody3D)
└── CameraPivot (Node3D)              ← 负责俯仰 (pitch)
    └── MainCamera (Camera3D)         ← 负责抖动、cull_mask、ViewBob
```

约定：
- 相机**永远在稳定枢轴上**，不继承任何骨骼运动。
- ViewBob 是独立一层，我写，你调；**不要**靠骨骼动画去制造视角晃动。
- 你 K 的动画在 FPP 下的价值点：**手臂与武器**（你自己看得见）、**队友和敌人看到的全身动作**、以及任何第三人称观察时的整体质量。

**不需要**为了相机去约束骨骼动画——相机不跟骨骼，头骨骼你爱怎么动就怎么动（点头、转头都影响不到视角）。

### 1.3 自视：自己的头不能给自己看，但别人要看得见

- 现在靠 `visible = false`（`character/骨架/Skeleton3D` 下有个 `Hat Layer` 被关掉）。**这在多人下是错的**——`visible = false` 对所有相机生效，等于队友看你也会缺一块。
- 正确做法：用**视觉层**。
  - 头部/帽子的 `MeshInstance3D` → `layers = 2`；
  - 自己的 `MainCamera.cull_mask` 去掉第 2 层；
  - 其他人的相机 `cull_mask` **保留**第 2 层。
- 约定：**视觉层 1 = 默认，视觉层 2 = "只对自己隐藏"**（以后要隐藏的东西都往这层丢）。

### 1.4 第一人称下的武器和手

武器现在是挂在 `BoneAttachment3D → WeaponPlaceholder` 上的。第一人称下武器离相机很近，可能出现裁切。项目里其实已经留了正确做法的痕迹（`EquipmentComponent` 里被注释掉的 `IsAlwaysInFront`，给武器材质加"永远渲染在最前"），这是第一人称顺手武器（viewmodel）的标准技巧——**M0 里恢复启用它**。

真正的"独立第一人称手臂模型"是后期优化，现在不做。

---

## 2. 动画管线（AnimationTree）

### 2.1 结构

```
AnimationTree (root = AnimationNodeStateMachine)
├── Locomotion      : AnimationNodeBlendSpace1D  (由水平速度驱动)
│                      点：蹲行 1.6 / 走 3.0 / 跑 5.0
├── Jump            : 起跳 → 滞空(loop) → 落地
├── Attack          : attack_startup → attack_active → attack_recovery
├── Defend          : 格挡(loop) → 格挡受击(one-shot)
├── Slide / Climb / Vault
├── PickUp / Cast
├── Assassinate / Execute / Staggered
└── Hurt / Death
```

- **用参数驱动，不用代码 `Play("xxx")`**：`speed`(float)、`crouch`(bool)、`airborne`(bool)、`staggered`(bool) 等。转换条件写在 AnimationTree 里，你在编辑器里点着调。
- Locomotion 的 `BlendSpace1D` 上每帧重设 `speed` 参数；跑速变了，步子与地面自动咬合（这是解决"打滑"的标准做法）。
- 现有的 `PlayerAttackState` 用 `AnimationFinished` 串 startup→active→recovery，改造后交由 AnimationTree 的转换处理，但**判定窗仍然由动画事件驱动**（见 2.3）。

### 2.2 动画命名规范

全小写 + 下划线，`模块_动作[_变体]`。**循环动作加 `_loop` 后缀**（Godot 导入时会去掉后缀并自动设为循环），一次性动作用裸名：

```
循环动作（Godot 会自动去掉 `_loop` 后缀并设为循环）：
    idle_loop / run_loop / walk_loop
    crouch_idle_loop / crouch_walk_loop
    defend_loop / stagger_loop / jump_loop

一次性动作（裸名，播完即止）：
    jump_start / jump_land
    attack_startup / attack_active / attack_recovery    ← 保留现名
    defend_hit / parry
    slide / climb_up / vault
    pickup / cast_blink / cast_bendtime
    assassinate_back / assassinate_air / executed
    hurt / death
```

现有的 `idle_loop` / `run_loop` 已经是正确写法——代码里 `Anim.Play("idle")` 没问题，原因见 §7。

### 2.3 判定窗：用 CallMethod 轨道，不用代码常量

在动画里加 **CallMethod 轨道**，调用角色脚本上的方法：

| 轨道事件 | 含义 |
|---|---|
| `AnimActiveEnter()` | 判定窗开始（这之后挥出去能砍到人） |
| `AnimActiveExit()` | 判定窗结束 |
| `AnimRootLock(true/false)` | 这期间锁住位移（我在代码里实现） |

**这样你调整动作节奏时不用改一行代码**，判定窗跟着动画走。这是"你负责感觉、我负责逻辑"的分界点。

---

## 3. 定点动作（暗杀/处决/攀爬/翻越/划铲）

**不用 root motion**。这类动作的位移由代码在固定时长内插值，参数放在一个 `ActionMoveData` 资源里：

| 字段 | 说明 |
|---|---|
| `Duration` | 动作时长 |
| `MotionCurve` | 位移曲线（缓入缓出、先快后慢……你在编辑器里画） |
| `StartOffset` / `EndOffset` | 相对目标点的起止偏移（比如暗杀时站到敌人背后哪个位置） |
| `LockRotation` | 是否锁住朝向 |
| `Invulnerable` | 是否免疫伤害（处决 = true） |
| `CancelableBy` | 允许哪些动作打断它（见设计文档 1.3 的动作优先级表） |

**你只需关心**：动作看起来对不对 + 该多久（`Duration`）+ 该滑多远（曲线）。其余（对齐目标点、打断规则、联机同步）我处理。

---

## 4. 编辑器里的手感旋钮（全部由你调，不需要改代码）

| 在哪 | 旋钮 |
|---|---|
| `Player` 节点 | 加速度、重力、走/跑/蹲速度、跳跃（起跳初速 / 空中按住上限 MaxJumpHold / 松手截断系数）、地面与空中加速度、下落重力倍率、ViewBob、法力上限/回充 |
| `EnemyData`（资源） | 生命、伤害、移速、攻击间隔、**反应时间**、**防御倾向**、失衡时长 —— 精英就是这份资源换几个数 |
| `PerceptionData`（资源） | 视锥角度/视距/近距全向距离、搜索时长、喊叫广播半径 |
| `NoiseData`（资源） | 各噪声源（脚步/跑/落地/划铲/开门/撞击）的半径 |
| `ActionMoveData`（资源） | 见上一节 |
| `AnimationTree` | 状态机转换条件、BlendSpace 点位、播放速度绑定 |

---

## 5. 物理层与视觉层约定

**物理层**（现有 1–6：`World / Player / Enemy / PickableItems / Projectiles / Door`）
- 新增 **7 = `Climbable`**：只有这一层上的碰撞体能被攀爬。你想让哪面墙可爬，给它这一层。（这是"墙仍然是关卡设计元素"的保证。）

**视觉层**
- 1 = 默认；2 = **只对自己隐藏**（头部/帽子）。

**网格库（GridMap）约定**：可攀爬的墙砖要能打出 `Climbable` 层——如果 GridMap 做不到按 tile 分物理层，就用 `MeshLibrary` 的 item 分两种（普通墙 / 可攀爬墙）。

---

## 6. 代码约定（我遵守）

- 输入只写进 `Intent`，角色只读 `Intent`；控制器不碰物理、不碰状态机。这条是多人能落地的根。
- 状态机只管"现在在干什么"，不做优先级分发——优先级由行为调度器负责（敌人）和动作优先级表负责（玩家）。
- 组件化：`HealthComponent` / `EquipmentComponent` / `ImpulseComponent` / `PerceptionComponent` / `InventoryComponent` / `NoiseEmitter`，挂在角色下，用信号通信。
- **不实现超出需求的代码**；需要但暂不做的，留接口并写 `// TODO(接口预留)` 注释。
- 手感参数一律 `[Export]` 或 `Resource`，不写死在逻辑里。
- 提交信息沿用现有风格（`feat: / fix: / refactor:`），每个里程碑一个分支。

---

## 7. Godot 特性备忘（持续追加）

这些是引擎自己会做的事，不知道就会误判成 bug——记在这里，别再踩。

- **动画名的 `_loop` 后缀**：glTF/模型导入时，名字以 `_loop`（或 `-loop`）结尾的动画**会被去掉后缀并自动设为循环动画**。所以 glb 里的 `idle_loop` / `run_loop` 在 Godot 里就叫 `idle` / `run`——代码里 `Anim.Play("idle")` 是正确的，不需要手动设循环。
  → 约定：**循环动作用 `_loop` 后缀命名，一次性动作不加后缀。** 循环与否由名字表达，代码里不写。
- **模型名的 `.col` 后缀（导入提示 / importer hints）**：网格或模型名带 `-col`、`-colonly`、`-convcol`、`-convcolonly`、`-noimp` 这类后缀时，导入器会按约定处理（生成碰撞体、只生成碰撞体、忽略不导入等）。搭关卡和做道具时可以用这套后缀**直接产出碰撞体**，省掉手工摆形状。
- **`Velocity` 是速度不是位移**：`CharacterBody3D.Velocity` 的单位是 m/s，`MoveAndSlide()` 自己会乘 `delta`。把 `Velocity` 赋成"方向 × delta"是 Godot 文档明确点名的常见错误（那算出来的是位移向量）。
- **物理帧率决定一切逐帧累加量**：重力要写成 `Velocity.Y -= Gravity * delta`（`Gravity` 单位 m/s²），这样改 `physics_ticks_per_second` 不会改变手感。现在 `Player.Gravity = 0.98` 是"每物理帧给速度加 0.98 m/s"（60Hz 等效 58.8 m/s²），能跑但和帧率绑定。**具体取什么值见设计文档 §4 的"跳跃手感配方"——别用 9.8。**
- **`Velocity` 是速度不是位移**：`CharacterBody3D.Velocity` 单位是 m/s，`MoveAndSlide()` 自己会乘 `delta`。把 `Velocity` 赋成"方向 × delta"是 Godot 文档明确点名的常见错误。
