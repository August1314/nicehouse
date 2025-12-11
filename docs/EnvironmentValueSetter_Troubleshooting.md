# EnvironmentValueSetter 故障排除指南

## 已修复的Bug

### Bug 1: EnergyDisplayTester.cs 中的命名空间错误
**问题描述**: `EnergyDisplayTester.cs` 中使用了错误的命名空间 `NiceHouse.EnvironmentControl.DeviceDefinition`，实际应该是 `NiceHouse.Data.DeviceDefinition`。

**修复方法**:
- 将第61行的 `FindObjectsOfType<NiceHouse.EnvironmentControl.DeviceDefinition>()` 改为 `FindObjectsOfType<NiceHouse.Data.DeviceDefinition>()`
- 将第66行的 `device.deviceType` 改为 `device.type`

**验证方法**: 运行Unity编译检查，确保没有命名空间错误。

### Bug 2: 能耗输入框未设置为只读
**问题描述**: `EnvironmentValueSetter.cs` 中的能耗输入框应该只显示计算出的能耗值，不允许用户手动编辑。

**修复方法**:
- 在 `Start()` 方法中添加 `energyInput.readOnly = true;` 设置

**验证方法**: 在Unity编辑器中检查能耗输入框是否为只读状态。

## 常见问题排查

### 问题1: 能耗显示为0或不更新
**可能原因**:
- EnergyManager.Instance 为 null
- 场景中没有设备在运行
- 设备没有正确配置能耗数据

**解决步骤**:
1. 确保场景中有 EnergyManager 组件
2. 检查设备是否在运行状态（空调、灯光等）
3. 使用 EnergyDisplayTester 脚本进行状态检查

### 问题2: UI组件引用丢失
**可能原因**:
- UI预制件未正确设置
- 组件名称不匹配

**解决步骤**:
1. 使用 EnvironmentValueSetterValidator 脚本验证UI设置
2. 参考 EnvironmentValueSetter_UnitySetup.md 重新创建UI

### 问题3: 房间数据不显示
**可能原因**:
- EnvironmentDataStore.Instance 为 null
- 房间ID不存在

**解决步骤**:
1. 确保 EnvironmentDataStore 单例已创建
2. 检查房间ID是否正确
3. 使用默认房间ID进行测试

## 调试工具

- **EnvironmentValueSetterValidator.cs**: 验证UI组件设置
- **EnergyDisplayTester.cs**: 测试能耗显示功能
- **DataDebug.cs**: 调试数据存储状态

## 联系支持

如果问题仍然存在，请提供：
- Unity控制台错误信息
- 场景截图
- 相关脚本的Inspector设置截图</content>
<parameter name="filePath">d:\graph\nicehouse\docs\EnvironmentValueSetter_Troubleshooting.md