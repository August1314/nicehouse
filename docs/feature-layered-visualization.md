## 分层数据可视化模块开发文档

> 本文档用于规划“系统分层查看 / Layered Data Visualization”模块的开发工作，覆盖场景剖切、环境图层、健康安全图层与活动热力图等可视化能力。

---

### 1. 功能模块基本信息

**模块名称：** 分层数据可视化模块（Layered Visualization）

**模块编号：** FM-004

**负责人：** [梁力航]（可根据实际分工调整）

**关联需求：** 基础要求 - 系统分层查看（剖面图、温度、湿度、PM2.5、活动热力图）；进阶要求 - 全量图层动态展示

**核心价值：** 将环境、健康、安全与活动数据以图层方式叠加到 3D 好房子模型上，实现“所见即所得”的数据洞察体验，支撑课堂演示与跨模块联动。

---

### 2. 功能模块描述

#### 2.1 功能概述

- **场景剖切 / 分层浏览**：支持垂直剖切、楼层隐藏、高亮单房间，帮助用户快速定位关注区域。
- **环境数值图层**：将温度、湿度、PM2.5 等实时数据映射为色彩覆盖、图例和阈值提示。
- **安全 & 健康提示层**：基于 `AlarmManager` 数据，将异常房间进行脉冲高亮或贴花提示。
- **活动热力图**：对接 `ActivityTracker`，显示各房间停留时长/访问频次，并提供时间轴回放。
- **统一交互**：Layer 选择器 + 图例面板 + 时间轴控制，保证学习演示流畅。

#### 2.2 核心功能点

- [ ] 场景剖切控制（剖面滑块、楼层显示/隐藏）
- [ ] 温度图层（渐变色覆盖 + 阈值提示）
- [ ] 湿度图层（双色区间显示）
- [ ] PM2.5 图层（分级填充 + 告警闪烁）
- [ ] 活动热力图（累计/回放两种模式）
- [ ] 告警叠加提示（烟雾/跌倒等房间高亮）
- [ ] 图层切换 UI + 图例/阈值说明
- [ ] 时间轴/回放控件（收敛到 24h 或 2h Demo 数据）
- [ ] 数据驱动材质/Shader 统一管理

#### 2.3 用户场景

**场景1：课堂演示剖面图**
- 触发条件：讲解者勾选“剖面浏览”图层。
- 用户操作：拖动剖切滑块定位到客厅高度。
- 系统响应：隐藏指定高度以上模型，只保留目标楼层，高亮剖切面。

**场景2：温度异常排查**
- 触发条件：温控 UI 提示“卧室温度偏高”。
- 用户操作：切换到“温度图层”，并将图例范围锁定 16–32℃。
- 系统响应：`EnvironmentDataStore` 温度数据映射到颜色，卧室显示为橙红色，并弹出阈值提示。

**场景3：PM2.5 扩散监控**
- 触发条件：PM2.5 传感器触发告警。
- 用户操作：切换到 PM2.5 图层并启动时间轴回放。
- 系统响应：回放模式按时间显示污染扩散轨迹，告警房间带有脉冲贴花和 `AlarmManager` 信息。

**场景4：活动热力图分析**
- 触发条件：老师讲解“老人夜间活动轨迹”。
- 用户操作：切换到热力图，选择“停留时长”指标，并点击“播放”。
- 系统响应：房间随时间播放渐变颜色，图例同步更新，底部显示当前时间戳与数值。

**场景5：一键对比模式**
- 触发条件：用户勾选“对比视图”，同时查看温度与活动热力。
- 用户操作：在副图层槽中选择“活动热力”，主图层选择“温度”。
- 系统响应：主渲染材质展示温度色板，活动热力以内发光轮廓叠加，图例分区显示。

---

### 3. 设计说明

#### 3.1 架构设计

**目录结构建议：**
```
LayeredVisualization/
├── Controllers/
│   ├── LayeredVisualizationController.cs   # 核心调度器（单例）
│   ├── LayerToggleController.cs            # 图层切换逻辑
│   ├── SectionCutController.cs             # 剖切/楼层可见性
│   ├── TimelinePlaybackController.cs       # 时间轴 + 数据回放
│   └── TooltipController.cs                # Hover/点击提示
├── Visuals/
│   ├── LayerMaterialBinder.cs              # 材质参数写入
│   ├── HeatmapMeshPainter.cs               # 顶点颜色/UV 写入
│   ├── AlarmOverlayView.cs                 # 告警贴花/脉冲
│   └── LegendGenerator.cs                  # 图例 UI 动态生成
├── UI/
│   ├── LayerPanel.prefab                   # 图层选择面板
│   ├── LegendPanel.prefab                  # 图例/阈值展示
│   └── TimelinePanel.prefab                # 时间轴控件
└── Shaders/
    ├── LayeredHeatmap.shader               # 通用渐变/透明
    └── AlarmPulse.shader                   # 告警脉冲
```

**关键类职责表：**
| 类名 | 职责 | 依赖 |
|------|------|------|
| `LayeredVisualizationController` | 统一管理当前图层状态、数据采样、刷新节奏 | `EnvironmentDataStore`, `ActivityTracker`, `RoomManager`, `AlarmManager` |
| `SectionCutController` | 控制剖面材质、Mesh Renderer 显隐、相机裁剪参数 | 场景 Mesh、`RoomDefinition` |
| `LayerMaterialBinder` | 将数值映射到材质属性（Color Gradient、Mask Texture） | `EnvironmentDataStore`, `Gradient` |
| `HeatmapMeshPainter` | 依据房间数据写入 Mesh 顶点颜色或顶点流 | `RoomManager`, `ActivityTracker` |
| `TimelinePlaybackController` | 维护时间轴、播放/暂停、采样历史数据 | 历史缓存（内存或 ScriptableObject） |
| `LayerPanel` (MonoBehaviour) | UI 交互（单选/多选图层、阈值调节） | `LayeredVisualizationController` |

#### 3.2 数据接口依赖

- `RoomManager.Instance.GetAllRooms()`：获取房间 Mesh / Bounds。
- `EnvironmentDataStore.Instance.TryGetRoomData(roomId, out data)`：温湿度、PM2.5、PM10。
- `ActivityTracker.Instance.GetRoomActivity(roomId)`：访问次数、停留时间、历史轨迹。
- `AlarmManager.Instance.GetRecentAlarms(count)` / `OnAlarmAdded`：异常房间提示。
- `EnvironmentDataSimulator` / `ActivityTracker` 历史缓存：供时间轴回放。

**参考文档：**
- `docs/data-design.md`（第 3、6 章）
- `docs/data-api-examples.md`（环境/活动/告警示例）
- `docs/data-quick-reference.md`
- `docs/interactions.md`（模型拆分与点击规则）

#### 3.3 UI 设计

- **图层选择面板（左上）**：Toggle 列表 + 图标，支持单选/多选，提供“对比模式”副槽。
- **图例 & 阈值面板（右下）**：根据当前图层动态生成色带、数值区间、单位，支持手动锁定范围。
- **剖面滑块（右侧垂直）**：支持拖动调整剖切高度，附带“一键隐藏屋顶”按钮。
- **时间轴（底部）**：包含播放/暂停、快进、跳转至实时按钮，可显示当前时间戳与数值。
- **提示气泡**：鼠标悬停房间时显示详细数据；告警房间点击后弹出`AlarmRecord`详情。
- **演示模式按钮**：快捷切换预设场景（例如“雾霾日”、“夜间巡查”）。

#### 3.4 交互设计

1. **图层切换流程**
   1. 用户在 LayerPanel 勾选图层。
   2. `LayerToggleController` 更新当前渲染策略。
   3. `LayerMaterialBinder` 按需刷新材质参数；`LegendGenerator` 同步图例。

2. **剖切 & 聚焦**
   - 拖动剖面滑块 → `SectionCutController` 通过 Shader（Clip Plane）和 Mesh 显隐实现剖切。
   - 点击房间 → `RoomManager` 返回 `RoomDefinition`，UI 高亮并显示详情。

3. **时间轴回放**
   - 时间轴控制器带动采样游标，向数据缓存请求指定时间点数值。
   - 环境层/热力图层按历史值更新；若回放中有告警，则显示过往告警状态。

4. **告警叠加**
   - 当 `AlarmManager` 推送新告警时，`AlarmOverlayView` 给对应房间加脉冲材质。
   - 告警层可单独开关，避免干扰。

#### 3.5 演示动线（房间顺序）

- **入口 → 客厅**：从主门进入即看到客厅，主要展示环境智控与温度图层。
- **客厅 → 厨房/餐厅**：顺着客厅左侧走到开放式厨房与餐桌，展示 PM2.5、湿度等图层联动。
- **厨房 → 书房角**：沿着中部转到书房区（工作台），展示健康监测/活动热力。
- **书房 → 走廊**：离开书房进入中部走廊，作为过渡区域，用于播放时间轴或剖切演示。
- **走廊 → 卫生间**：向左进入卫生间，强调安全/告警叠加。
- **卫生间 → 走廊 → 卧室**：返回走廊后进入最左侧卧室，展示睡眠相关的健康/环境图层。

> 上述动线将实际讲解顺序固定为 **门口 → 客厅 → 厨房 → 书房 → 走廊 → 卫生间 → 走廊 → 卧室**，UI 面板、自动巡航以及 Layer 切换默认按此排列，方便演示和脚本联动。

#### 3.6 房间空间数据基线

基于 `RoomVolume` BoxCollider 导出的房间中心点与包围盒（单位：米）如下：

| roomId        | displayName | roomType    | centerX | centerY | centerZ | sizeX | sizeY | sizeZ |
|--------------|-------------|------------|--------:|--------:|--------:|------:|------:|------:|
| Corridor01   | 走廊        | Corridor   |  -9.01  |  1.72   |  6.72   | 14.45 |  3.30 |  2.00 |
| Study01      | 书房        | Study      |  -4.17  |  1.72   |  3.06   |  4.78 |  3.30 |  4.86 |
| BedRoom01    | 卧室        | Bedroom    | -13.39  |  1.72   |  3.09   |  5.78 |  3.30 |  4.86 |
| Bathroom01   | 浴室        | Bathroom   |  -8.65  |  1.72   |  3.06   |  3.95 |  3.30 |  4.86 |
| LivingRoom01 | 客厅        | LivingRoom |   3.27  |  1.72   |  5.36   |  9.96 |  3.30 |  4.67 |
| Kitchen01    | 厨房        | Kitchen    |   3.28  |  1.72   |  1.80   | 10.12 |  3.30 |  2.30 |

- **默认剖切高度**：天花约在 \\(y \\approx 1.72 + 3.30 / 2 \\approx 3.37\\)，分层浏览默认采用两档剖切：
  - “去屋顶视图”：剖切平面 \\(y = 3.4\\)，完全移除屋顶，仅展示室内。
  - “半剖面视图”：剖切平面 \\(y = 2.0\\)，略高于人物身高，用于展示墙体高度变化。

#### 3.7 图层 UI 与热力图实现

- **FloorHeatmapGrid（Visuals）**
  - `MetricType` 支持温度、湿度、PM2.5，可配置 `min/max` 与 `Gradient`，形成连续热力地面。
  - 自动基于所有 `RoomVolume` 生成统一网格，通过房间中心距离加权保证颜色平滑过渡。
  - `updateInterval` 控制刷新频率，实时读取 `EnvironmentDataStore`，与模拟器/实测数据同步。
- **LegendPanelUI（UI）**
  - 统一更新标题、单位（℃ / % / μg/m³）、数值范围，并生成渐变纹理，方便课堂讲解。
- **LayerPanelUI**
  - 用 Toggle 绑定多个 `FloorHeatmapGrid`，确保同一时间只激活一个图层，自动同步 Legend 与“当前图层”标签。
  - 默认配置可一键切换“温度 / 湿度 / PM2.5”三层，后续可扩展活动热力、告警叠加等层级。

> 场景建议：创建 `FloorHeatmapGridRoot_Temp` / `_Humidity` / `_PM25` 三个对象，分别设置 `MetricType`、色表与阈值，再由 `LayerPanelUI` 控制显隐，即可完成正式的图层 UI 演示流程。

---

### 4. 开发任务

#### 4.1 核心功能开发

**任务 4.1.1：LayeredVisualizationController 实现**
- 描述：搭建图层状态机、刷新周期（如 0.5s）、数据采样与事件广播。
- 输入：`EnvironmentDataStore`、`ActivityTracker`、`AlarmManager` 数据。
- 输出：供材质/UI 使用的标准化 `LayerSample`。
- 验收：切换图层时延迟 < 100ms，数据映射正确，支持实时/回放双模式。
- 预计工时：6h.

```csharp
public class LayeredVisualizationController : MonoBehaviour
{
    public static LayeredVisualizationController Instance { get; private set; }
    public LayerType activePrimaryLayer = LayerType.Temperature;
    public LayerType? secondaryLayer;
    public float refreshInterval = 0.5f;

    private readonly Dictionary<string, LayerSample> _roomSamples = new();

    private void Start()
    {
        StartCoroutine(RefreshLoop());
    }

    private IEnumerator RefreshLoop()
    {
        while (true)
        {
            SampleRooms();
            OnLayerDataUpdated?.Invoke(_roomSamples);
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    private void SampleRooms()
    {
        foreach (var kv in RoomManager.Instance.GetAllRooms())
        {
            var roomId = kv.Key;
            var sample = _roomSamples.GetValueOrDefault(roomId) ?? new LayerSample(roomId);

            if (EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var env))
            {
                sample.temperature = env.temperature;
                sample.humidity = env.humidity;
                sample.pm25 = env.pm25;
            }

            var activity = ActivityTracker.Instance.GetRoomActivity(roomId);
            sample.activityScore = activity?.totalStayTime ?? 0f;

            sample.hasAlarm = AlarmManager.Instance.HasActiveAlarm(roomId);
            _roomSamples[roomId] = sample;
        }
    }

    public event Action<IReadOnlyDictionary<string, LayerSample>> OnLayerDataUpdated;
}
```

**任务 4.1.2：SectionCutController**
- 描述：支持垂直剖切、楼层显隐、房间聚焦。
- 输入：剖切高度、房间列表。
- 输出：Shader Clip Plane 参数或 Renderer.enabled 控制。
- 验收：剖切拖动平滑，性能损耗 < 1ms/frame。
- 预计工时：4h.

**任务 4.1.3：TimelinePlaybackController**
- 描述：维护历史数据缓冲（滚动数组/ScriptableObject），支持播放/暂停/跳转。
- 输入：模拟器推送的历史帧。
- 输出：某时间点的层数据，广播给 `LayerMaterialBinder`。
- 验收：1x/2x/4x 播放稳定；回放切换实时模式 < 200ms。
- 预计工时：5h.

#### 4.2 UI 开发

**任务 4.2.1：LayerPanel & LegendPanel**
- 描述：实现图层选择、对比模式、图例动态生成。
- 组件：ToggleGroup、ScrollRect、Gradient Image、TMP 文本。
- 验收：可扩展图层列表、图例随数据范围调整。
- 工时：4h.

**任务 4.2.2：TimelinePanel**
- 描述：播放/暂停、拖拽滑块、实时跳转按钮、当前时间/数值显示。
- 验收：滑块拖动+实时预览、按钮状态同步。
- 工时：3h.

**任务 4.2.3：Tooltip/房间详情**
- 描述：Raycast 检测房间，显示悬浮卡片；支持固定在屏幕侧栏。
- 验收：延迟 < 50ms，无频闪。
- 工时：2h.

#### 4.3 视觉与材质开发

**任务 4.3.1：LayeredHeatmap Shader**
- 描述：单 Shader 支持颜色渐变、透明度控制、二级叠加（主层+次层描边）。
- 验收：支持 GPU Instancing、颜色范围可配置。
- 工时：5h.

**任务 4.3.2：HeatmapMeshPainter**
- 描述：对每个房间 Mesh 应用颜色/Emission，或生成 Overlay Mesh。
- 验收：更新频率 2Hz 时无明显 GC；可热切换材质。
- 工时：4h.

**任务 4.3.3：AlarmOverlayView**
- 描述：告警房间加脉冲描边/贴花，支持不同 AlarmType 不同颜色。
- 验收：脉冲周期与 Alarm 状态同步。
- 工时：3h.

#### 4.4 测试与优化

**任务 4.4.1：功能测试**
- 测试用例：剖切交互、各环境层、热力图累计/回放、告警叠加、对比模式、UI 切换。
- 验收：全部通过，发现的 Bug 归档。

**任务 4.4.2：性能测试**
- 目标：在 1080p、场景 100k+ 多边形情况下，Layer 更新不低于 60 FPS。
- 方法：Profile 剖切 Shader、材质写入、UI Rebuild，必要时引入对象池。

**任务 4.4.3：可视化校准**
- 检查颜色映射是否符合国标/老师要求（例如 PM2.5 0-35 绿色、35-75 黄色、>115 红色）。
- 验收：与阈值文档一致。

---

### 5. 开发计划

#### 5.1 时间安排（建议，以一周五天为例）

| 阶段 | 任务 | 开始 | 结束 | 负责人 |
|------|------|------|------|--------|
| 准备 | 需求确认、视觉草图、接口复核 | Day1 | Day1 | [姓名] |
| 开发 | 控制器 + 剖切实现 | Day2 | Day3 | [姓名] |
| 开发 | 环境/热力图层材质 & 数据绑定 | Day3 | Day4 | [姓名] |
| 开发 | UI（LayerPanel、Timeline、Legend） | Day4 | Day5 | [姓名] |
| 联调 | 接入模拟器 + 告警叠加 + 对比模式 | Day6 | Day6 | [姓名] |
| 测试 | 功能/性能测试 + 调色 | Day7 | Day7 | [姓名] |

#### 5.2 里程碑

- **M1：剖切 & 控制器可用**（Day3）
- **M2：环境层 & 热力图 Demo**（Day4）
- **M3：UI + 时间轴联通**（Day5）
- **M4：告警叠加 & 对比模式**（Day6）
- **M5：全链演示通过**（Day7）

#### 5.3 风险与依赖

**技术风险**
- [ ] Shader 性能不足 → 预案：降级为顶点颜色或 URP Decal。
- [ ] 历史数据不足 → 预案：使用 `EnvironmentDataSimulator` 提供缓存。
- [ ] 活动热力数值跨度大 → 预案：对数映射/动态归一化。

**依赖项**
- [x] 数据底座接口（Room/Environment/Activity/Alarm）
- [ ] 模型剖切标记与材质分组（依赖建模同学，预计 Day2 完成）
- [ ] UI 设计稿/色板（依赖视觉同学，预计 Day1）

---

### 6. 验收标准

**功能完整性**
- [ ] 至少 4 个图层（剖切、温度、PM2.5、活动热力）可独立显示。
- [ ] 支持同屏对比（主+副图层）。
- [ ] 告警叠加与数据联动正常。
- [ ] 时间轴回放能驱动环境 + 活动层。
- [ ] UI 操作（剖切、图例、图层切换）流畅无卡顿。

**可视化效果**
- [ ] 色彩映射与阈值文档一致，图例清晰。
- [ ] Hotspot 提示准确定位房间。
- [ ] 剖切面干净，无遮挡残留。

**性能与稳定性**
- [ ] PC 端 60 FPS（Profile 证明 Layer 刷新耗时 < 3ms）。
- [ ] 无 GC 峰值（单次更新 GC < 0.5KB）。
- [ ] 切换图层/模式无闪屏或材质错乱。

**演示准备**
- [ ] 预置“雾霾日 / 夜间巡查 / 健康安全”三个演示场景。
- [ ] 演示脚本与录屏准备完毕。

---

### 7. 参考资料

- `docs/data-design.md`
- `docs/data-dev-plan.md`
- `docs/data-api-examples.md`
- `docs/interactions.md`
- `docs/requirements.md`
- Unity Shader Graph / URP Decal 官方示例

---

### 8. 更新记录

| 日期 | 版本 | 更新内容 | 更新人 |
|------|------|----------|--------|
| 2025-12-03 | 1.0 | 创建分层数据可视化开发文档 | 梁力航 |


