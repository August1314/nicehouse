## 墙体嵌入式控制中枢面板开发文档

> 本文档规划“墙体嵌入式控制中枢面板（Control Hub Panel）”及其与第一人称交互的升级工作，作为所有现有 UI 模块的统一入口。

---

### 1. 功能模块基本信息

**模块名称：** 墙体嵌入式控制中枢面板  
**模块编号：** UI-005  
**负责人：** [梁力航]  
**关联需求：** 场景 UI 统一入口、功能演示便捷化、第一人称交互增强  
**核心价值：** 在房间墙体嵌入一块具备科技感的控制面板，集中展示并跳转所有功能模块 UI，支持第一人称玩家以鼠标左键/准星点击进行交互。

---

### 2. 功能模块描述

#### 2.1 功能概述

- **中枢入口**：嵌入墙体的 3D 面板展示所有现有功能 UI（环境智控、能耗智控、智能监护、健康监测、一键呼叫、安全防护、系统分层等）的入口卡片。
- **即时状态**：每个入口卡片以极简图标 + 数字/状态标识当前模块的关键指标（如 PM2.5 数值、当前能耗模式、报警状态等）。
- **一键唤起**：点击卡片，在屏幕上打开对应的 UI 面板（复用现有 UI 或调用其 Canvas）。
- **第一人称操作**：引入中心准星（Crosshair）与鼠标左键点击逻辑，支持角色对 3D 面板进行射线点击。
- **科技感体验**：面板具备动态灯带、背光、霓虹边框与轻量粒子/扫描线等视觉效果。

#### 2.2 核心功能点

- [ ] 世界空间面板模型/Prefab，嵌入墙体并挂载 Sci-Fi Shader、动态灯效。
- [ ] 面板 UI Grid，动态生成入口卡片，并展示实时状态摘要。
- [ ] 入口卡片点击后触发对应 UI Canvas 的 `Show()` / `Activate()`。
- [ ] 与 `FirstPersonController` 联动的射线检测、中心准星与左键点击逻辑。
- [ ] “网页感”二级页面：在面板自身区域内嵌入子页面容器，可内嵌现有 UI（可先展示内嵌网页式占位）。
- [ ] 安全退出/关闭按钮，返回入口卡片列表。

#### 2.3 用户场景

**场景1：快速查看环境状态**  
- 触发：玩家走近客厅墙面控制面板。  
- 操作：准星指向“环境智控”卡片，左键点击。  
- 响应：面板中央区域切换为环境智控 UI，显示实时温湿度/PM 数据，同时在 HUD 打开全屏环境面板（可选）。  

**场景2：切换至能耗模式**  
- 触发：面板已展开环境 UI，玩家想查看能耗。  
- 操作：点击面板左侧返回按钮 → 选择“能耗智控”。  
- 响应：面板显示能耗曲线缩略图与模式切换控件，允许直接在墙面操作。  

**场景3：第一人称交互教程**  
- 触发：玩家首次靠近面板。  
- 操作：看到屏幕中心准星与提示“左键点击交互”。  
- 响应：系统锁定鼠标、启用射线点击；点击空白区域无效，点击按钮触发音效 + UI 切换。  

**场景4：网页模式扩展（可选）**  
- 触发：玩家打开“系统分层”入口。  
- 操作：面板内部加载一块类似网页的 Tab 控件。  
- 响应：可在面板内直接切换剖面/温度/湿度等小组件，无需回到大屏 UI。  

---

### 3. 设计说明

#### 3.1 架构设计

```
ControlHubPanel/
├── Prefabs/
│   └── ControlHubWallPanel.prefab
├── Scripts/
│   ├── ControlHubPanelController.cs      # 主面板状态机
│   ├── ControlHubEntryWidget.cs          # 单个入口卡片
│   ├── ControlHubStatusAggregator.cs     # 汇总各模块状态
│   ├── ControlHubAnimator.cs             # 灯效/展开动画
│   ├── FPRaycastInteractor.cs            # 第一人称射线 & 点击逻辑
│   └── CrosshairUI.cs                    # 准星与提示
└── Materials/
    └── SciFiPanel.mat                    # 发光材质/Shader
```

**关键类设计：**

| 类名 | 职责 | 依赖 | 挂载位置 |
|------|------|------|----------|
| `ControlHubPanelController` | 面板状态机、入口列表、二级内容容器 | `ControlHubStatusAggregator`, UI Prefabs | 面板 GameObject |
| `ControlHubEntryWidget` | 渲染入口卡片、响应点击 | `ControlHubPanelController` | UI 子节点 |
| `ControlHubStatusAggregator` | 收集各模块的摘要状态（环境、能耗等） | 现有模块 API（`EnvironmentController`, `EnergyManager` …） | 面板 GameObject |
| `ControlHubAnimator` | 控制灯带、屏幕特效、开合动画 | Timeline/Shader 参数 | 面板 GameObject |
| `FPRaycastInteractor` | 从玩家视角发射射线并分发点击事件 | `Camera`, `Physics.Raycast`, UnityEvents | Player（第一人称相机） |
| `CrosshairUI` | 渲染中心准星及交互提示 | Canvas (Screen Space Overlay) | Player HUD |

#### 3.2 数据与模块依赖

- **状态来源**：  
  - 环境：`EnvironmentController`、`EnvironmentDataStore`  
  - 能耗：`EnergyManager`  
  - 健康/监护：对应数据模拟器  
  - 分层：`LayeredVisualizationController`（若已有）  
- **UI 控制**：各模块需对外暴露统一接口（建议接口：`IControlPanelModule { string ModuleId; Sprite Icon; string GetSummary(); void Show(); void Hide(); }`）。  
- **输入**：基于 `FirstPersonController` 主摄像机的屏幕中心射线；左键点击触发 `OnClick`。  
- **资源**：Sci-Fi 面板材质、动态贴花、发光字体（可使用 TextMeshPro）。  

#### 3.3 UI 设计

- **布局**：  
  - 左侧窄栏：模块类别 Tabs（功能/安全/分层）。  
  - 中央主屏：默认显示入口卡片矩阵（2x3），点击后以“网页”形式填充整个主屏。  
  - 右侧状态条：滚动显示最近告警/提示。  
  - 底部光条：显示当前房间名称 + 时间。  
- **科技风格细节**：  
  - 使用深色玻璃材质 + 蓝紫渐变描边。  
  - 背面发光粒子沿面板边缘流动（使用 Shader Graph 或脚本控制 `EmissionColor` 的 ping-pong）。  
  - 入口卡片 hover 时增加轻微缩放与描边。  
- **准星 UI**：  
  - 屏幕中心一个白色细线十字，悬停可交互对象时切换为亮蓝色。  
  - 提示文本：“Left Click to interact”。  

#### 3.4 交互设计

1. 玩家进入面板交互区域（可使用 `SphereCollider + OnTriggerEnter`） → 面板唤醒灯效。  
2. `FPRaycastInteractor` 持续从摄像机中心发射射线，如果命中带 `IInteractable` 的物体则高亮并在准星旁显示提示。  
3. 玩家按下鼠标左键：  
   - 若命中入口卡片 → 调用 `ControlHubPanelController.OpenModule(moduleId)`。  
   - 若命中返回/关闭按钮 → 回到入口列表或退出。  
4. 面板内嵌内容使用 UI 子 Canvas（World Space）或直接调用全屏 Canvas 的 Show/Hide。  
5. 可选：长按 2 秒触发“锁定面板”模式，禁止自动收起。  

---

### 4. 开发任务

#### 4.1 核心功能

**任务 UI-4.1.1：建立控制面板 Prefab**  
- 描述：基于现有墙体尺寸建模或组合 3D 面板，配置材质、灯效、动画。  
- 输入：室内墙体模型、材质库。  
- 输出：`ControlHubWallPanel.prefab`。  
- 验收：Prefab 放入场景即可展示灯效，具备 Collider。  
- 预计工时：4h。

**任务 UI-4.1.2：实现 ControlHubPanelController**  
- 描述：管理入口数据、动态生成卡片、切换模块内容。  
- 输入：模块注册列表/ScriptableObject。  
- 输出：可点击入口并切换面板内容。  
- 验收：运行场景时可看到所有入口卡片并可切换。  
- 预计工时：6h。

**任务 UI-4.1.3：状态汇总与模块接口**  
- 描述：定义 `IControlPanelModule`，为现有 UI 模块提供适配器，汇总关键信息。  
- 输入：各模块数据接口。  
- 输出：面板入口能显示实时数值/状态。  
- 验收：UI 卡片状态 1 秒刷新一次，数据正确。  
- 预计工时：5h。

#### 4.2 第一人称交互增强

**任务 FP-4.2.1：中心准星 HUD**  
- 描述：在现有第一人称 HUD 上新增准星与提示。  
- 验收：进入游戏即可看到准星，射线命中可交互对象时颜色变化。  
- 预计工时：2h。

**任务 FP-4.2.2：FPRaycastInteractor**  
- 描述：封装射线检测、左键点击事件，并与面板可交互元素对接。  
- 验收：指向面板按钮后左键可触发 UnityEvent。  
- 预计工时：4h。

#### 4.3 UI 与视觉

**任务 UI-4.3.1：入口卡片视觉**  
- 描述：设计 6+ 卡片的图标、颜色、hover 状态。  
- 验收：卡片契合科幻风格，状态切换流畅。  
- 预计工时：3h。

**任务 UI-4.3.2：嵌入式“网页”容器**  
- 描述：在面板内加入 Content Frame，用于嵌入/复用现有 UI。  
- 验收：成功加载至少一个现有面板（如环境智控）并可返回入口。  
- 预计工时：4h。

#### 4.4 测试与验收

- 用例：  
  1. 第一人称准星命中卡片 → 左键打开对应 UI。  
  2. 面板离开交互范围后自动暗淡/关闭。  
  3. 多个模块状态同时刷新，性能稳定（<1ms/帧）。  
  4. UI 内嵌内容不影响全屏版面板（互斥或并存策略明确）。  
- 验收标准：全部用例通过，Raycast 点击与 UI 响应无遗漏。

---

### 5. 开发计划（建议）

| 阶段 | 任务 | 时间 | 负责人 |
|------|------|------|--------|
| Day1 | 面板建模 + 材质灯效 | 上午 | [姓名] |
| Day1 | ControlHubPanelController 脚本 | 下午 | [姓名] |
| Day2 | 状态汇总接口 + 卡片 UI | 上午 | [姓名] |
| Day2 | 第一人称准星 & 射线交互 | 下午 | [姓名] |
| Day3 | 嵌入式网页容器 + 模块整合 | 上午 | [姓名] |
| Day3 | 测试与调优 | 下午 | [姓名] |

---

### 6. 风险与依赖

**技术风险：**
- [ ] 射线命中 UI 不准确：需调整面板 Collider、使用 `GraphicRaycaster` + `PhysicsRaycaster` 联动。
- [ ] 面板内嵌内容尺寸不匹配：需要为各模块提供自适应布局或单独的缩略版 UI。
- [ ] 状态数据频繁刷新导致 GC：建议使用对象池或缓存字符串。

**依赖项：**
- [ ] 各模块提供 `Show/Hide` 接口与状态摘要 API。  
- [ ] 美术资源：面板贴图、图标、灯效 Shader。  
- [ ] 第一人称控制器升级（准星 + 射线），参考 `docs/feature-first-person-controller.md`。

---

### 7. 验收标准

#### 7.1 功能完整性
- [ ] 面板可嵌入墙体且材质效果符合设计。  
- [ ] 至少 6 个模块入口可显示实时状态并可点击。  
- [ ] 第一人称射线点击逻辑稳定，准星反馈清晰。  
- [ ] 面板内嵌内容与全屏 UI 能够互相调用或独立显示。  

#### 7.2 体验与视觉
- [ ] 面板整体具备科技感，灯效与动画无明显卡顿。  
- [ ] 卡片 hover/点击有视觉反馈与音效。  
- [ ] UI 文案、图标统一风格。  

#### 7.3 代码质量
- [ ] 控制器与交互脚本通过代码审查，无编译警告。  
- [ ] 支持 Inspector 配置（入口列表、图标、颜色等）。  
- [ ] 关键脚本含必要注释与 README 引用。  

---

### 8. 参考资料

- `docs/feature-environment-control.md`
- `docs/feature-first-person-controller.md`
- `docs/interactions.md`
- `docs/feature-module-template.md`

---

### 9. 更新记录

| 日期 | 版本 | 更新内容 | 更新人 |
|------|------|----------|--------|
| 2025-12-04 | 1.0 | 初稿：定义墙体控制中枢与交互升级 | [待定] |


