## 数据内容开发计划（基于 `data-design.md`）

> 目标：把数据相关工作拆成**清晰的开发任务与阶段**，方便多人协作推进。

---

### 阶段 0：准备与统一约定

- **任务 0.1：统一命名与目录**
  - 在 Unity 工程中创建 `Assets/Scripts/Data` 目录，用于存放所有数据类和 Manager。
  - 约定所有数据 Store/Manager 挂在一个 `GameManager` 或 `DataRoot` 对象上，场景中唯一。

- **任务 0.2：阅读并确认数据设计**
  - 全组通读 `docs/data-design.md`，确认本组是否要简化/删除某些数据（例如是否做时间轴回放）。
  - 在该文档中用批注/勾选标出“本次大作业实际采用”的部分。

---

### 阶段 1：基础结构数据（Room / Device）

> 负责人建议：1 人，擅长场景结构和脚本基础。

- **任务 1.1：实现房间数据结构**
  - 在 `Assets/Scripts/Data` 中实现：
    - `RoomType` 枚举；
    - `RoomDefinition` 组件；
    - `RoomManager`（在 `Awake/Start` 中收集场景中所有 `RoomDefinition`，建立字典）。
  - 在场景中为每个房间 GameObject 挂上 `RoomDefinition`，配置 `roomId`、`roomType`、`displayName`。

- **任务 1.2：实现设备与传感器数据结构**
  - 实现：
    - `DeviceType` 枚举；
    - `DeviceDefinition` 组件；
    - `DeviceManager`（按 `deviceId` 和 `roomId` 建立索引）。
  - 在模型中为空调、新风、净化器、风扇、窗户、灯具、传感器、互助按钮等挂上 `DeviceDefinition`。

- **产出检查**
  - 通过简单的调试脚本或 `Debug.Log`，在运行时打印出所有房间与设备列表，确认数据绑定正确。

---

### 阶段 2：环境与能耗数据

> 负责人建议：1 人，可以与环境智控/能耗功能开发者协作。

- **任务 2.1：环境数据存储与访问**
  - 实现：
    - `RoomEnvironmentData` 类；
    - `EnvironmentDataStore`：
      - 内部维护 `Dictionary<string, RoomEnvironmentData>`；
      - 提供 `GetRoomData(roomId)`、`SetRoomData(roomId, data)` 与便捷方法（如 `UpdateTemperature(roomId, value)`）。

- **任务 2.2：环境数据模拟器**
  - 实现 `EnvironmentDataSimulator`：
    - 在 `Update` 或定时协程中，根据不同“模式”（正常/雾霾）更新各房间数据；
    - 支持配置：更新间隔、温度/PM2.5 范围、噪声幅度等。
  - 与后续模块的连接：
    - 通知 UI 及 Heatmap 组件数据变更（可使用事件或轮询）。

- **任务 2.3：能耗数据管理**
  - 实现：
    - `EnergyData` 类；
    - `EnergyManager`：
      - 维护 `Dictionary<string, EnergyData>`；
      - 提供 `UpdateEnergyUsage(deviceId, deltaTime)`、`GetRoomDailyConsumption(roomId)` 等接口。
  - 与设备控制脚本对接：设备开启/关闭时调用对应接口。

- **产出检查**
  - 在 Editor 或 UI 上简单显示某个房间的温度和用电量是否随时间变化。

---

### 阶段 3：数字人、健康与活动数据

> 负责人建议：1–2 人，最好是负责智能监护/健康监测/热力图的同学。

- **任务 3.1：数字人行为状态控制**
  - 实现：
    - `PersonState` 枚举；
    - `PersonStatus` 类；
    - `PersonStateController` 组件：
      - 维护 `Status`；
      - 提供 `ChangeState(PersonState newState, string roomId)` 接口；
      - 自动累计 `stateDuration`。

- **任务 3.2：活动追踪与热力图数据**
  - 实现：
    - `ActivityData`；
    - `ActivityTracker`：
      - 监听数字人进入/离开房间（由 `PersonStateController` 或触发器调用）；
      - 更新每个房间的 `visitCount` 与 `totalStayTime`；
      - 提供 `GetRoomActivity(roomId)` 接口给热力图模块使用。

- **任务 3.3：健康数据存储与模拟**
  - 实现：
    - `VitalSignsData`；
    - `HealthDataStore`：
      - 维护 `Current` 生命体征数据；
      - `SimulateNextStep()` 随时间生成心率、呼吸、体动、睡眠阶段等。

- **产出检查**
  - 使用简单 UI 把当前数字人状态、所在房间、心率等信息打印出来，观察随时间是否合理变化。

---

### 阶段 4：安全与告警数据

> 负责人建议：1 人，可与安全防护/一键呼叫功能开发者配合。

- **任务 4.1：安全数据管理**
  - 实现：
    - `SafetyData`；
    - `SafetyDataStore`：
      - 按房间记录 `smokeLevel`、`gasLevel`；
      - 提供 `GetRoomSafety(roomId)`，供烟感/报警脚本使用。

- **任务 4.2：统一告警管理器**
  - 实现：
    - `AlarmType` 枚举；
    - `AlarmRecord`；
    - `AlarmManager`：
      - `AddAlarm(type, roomId)`：对外统一接口；
      - `GetRecentAlarms(int count)`：供 UI 列表使用；
      - 后续可扩展 `MarkHandled(record)` 等接口。

- **任务 4.3：与各模块集成**
  - 环境智控、智能监护、健康监测、安全防护、一键呼叫等脚本在检测到异常时，统一调用 `AlarmManager.AddAlarm`。
  - 告警 UI 模块从 `AlarmManager` 读取最近记录，展示给用户。

- **产出检查**
  - 手动触发几种告警，检查列表中是否按时间、类型、房间正确记录。

---

### 阶段 5：对外接口与文档完善

> 负责人建议：由一位“总集成人”完成，保证接口统一、文档清晰。

- **任务 5.1：统一接口命名与访问方式**
  - 确认各 Store/Manager 的常用 API（例如 `GetRoomData` / `GetRoomActivity` / `AddAlarm`）命名一致、参数清晰。
  - 在代码中为关键 API 添加 XML 注释，便于 IDE 提示。

- **任务 5.2：为功能模块写“小抄”示例**
  - 在 `docs/data-design.md` 或新建简短示例部分，补充典型调用代码，例如：
    - “如何从 `EnvironmentDataStore` 获取客厅温度”；
    - “如何为某个异常写入告警记录”；
    - “如何从 `ActivityTracker` 获取房间热力值”。

- **任务 5.3：联调与重构**
  - 在功能模块开发过程中，若发现数据接口不够用或设计不合理，及时回到本计划和 `data-design.md` 做小幅调整，并同步给全组。

---

### 阶段整体时间建议（可按周划分）

- **第 1 周**：阶段 0 + 阶段 1（基础结构数据搭建完毕）。
- **第 2 周**：阶段 2（环境 & 能耗数据） + 初步与环境智控/热力图模块对接。
- **第 3 周**：阶段 3（数字人、健康、活动）+ 阶段 4（安全与告警）。
- **第 4 周**：阶段 5（接口统一、文档完善）+ 与所有交互功能联调、修正。

> 建议在 GitHub 上为数据相关工作创建分支（如 `feature/data-core-<name>`），每个阶段完成后提交 PR，并在 PR 描述中引用本文件中已完成的任务编号。


