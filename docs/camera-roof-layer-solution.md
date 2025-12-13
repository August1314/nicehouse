# 相机屋顶遮挡问题解决方案

> **适用场景文件**：`nicehouse/Assets/scene.unity`

## 问题描述

- **TopView相机**（俯视图）：用于展示房子的平面图+热力图，但屋顶会遮挡视野
- **FPS相机**（第一人称）：如果去掉屋顶，游玩时没有屋顶会很奇怪

## 解决方案

使用Unity的**Layer系统**和**Camera的Culling Mask**功能：
- 将屋顶放到独立的Layer（如"Roof"）
- FPS相机渲染所有Layer（包括屋顶）
- TopView相机排除屋顶Layer（不渲染屋顶）

这样，两个相机可以同时存在，但看到的内容不同。

---

## 使用步骤

### 步骤1：创建Roof Layer

1. 打开Unity编辑器
2. 菜单：`Edit > Project Settings > Tags and Layers`
3. 在`Layers`列表中找到第一个空位（通常是User Layer 6或更高）
4. 输入Layer名称：`Roof`
5. 关闭设置窗口

### 步骤2：添加CameraRoofManager脚本

1. 打开场景文件：`nicehouse/Assets/scene.unity`
2. 在场景中创建一个空的GameObject，命名为`CameraRoofManager`
3. 将`CameraRoofManager.cs`脚本添加到这个GameObject上
4. 在Inspector中：
   - 点击"自动查找所有相机"按钮（脚本会自动找到`TopViewCamera`和FPS相机）
   - 或者手动拖拽相机到对应字段
   - 确认`FPS Camera`和`TopView Camera`都已正确设置

### 步骤3：将屋顶移动到Roof Layer

**方法A：使用脚本（推荐）**
1. 在Hierarchy中选择屋顶GameObject（或包含所有屋顶的父对象）
2. 在`CameraRoofManager`的Inspector中，点击"将选中对象移动到屋顶Layer"按钮
3. 脚本会自动将该对象及其所有子对象移动到Roof Layer

**方法B：手动设置**
1. 在Hierarchy中选择屋顶GameObject
2. 在Inspector顶部，找到`Layer`下拉菜单
3. 选择`Roof` Layer
4. 如果屋顶有子对象，需要递归设置每个子对象的Layer

### 步骤4：配置相机Culling Mask

1. 在`CameraRoofManager`的Inspector中，点击"配置相机Culling Mask"按钮
2. 脚本会自动：
   - 设置FPS相机渲染所有Layer（包括Roof）
   - 设置TopView相机排除Roof Layer

**或者手动配置：**
1. 选择FPS相机，在Camera组件的`Culling Mask`中，确保勾选了`Roof` Layer
2. 选择TopView相机，在Camera组件的`Culling Mask`中，取消勾选`Roof` Layer

---

## 验证

1. **FPS相机视角**：
   - 切换到FPS相机（或进入游戏模式）
   - 应该能看到屋顶

2. **TopView相机视角**：
   - 切换到TopView相机
   - 应该看不到屋顶，可以直接看到房子内部和热力图

---

## 技术说明

### Layer系统
- Unity的Layer系统允许将GameObject分类到不同的层
- 每个Layer用32位整数表示（最多32个Layer）
- Layer 0-5是Unity内置Layer，6-31是用户自定义Layer

### Culling Mask
- Camera组件的`Culling Mask`决定该相机渲染哪些Layer
- 使用位掩码（bitmask）实现，每个Layer对应一个位
- 例如：`cullingMask = ~(1 << roofLayer)` 表示排除roofLayer

### 性能考虑
- 使用Layer系统不会影响性能
- 只是控制相机渲染哪些对象，不会实际隐藏对象
- 如果对象不在相机的Culling Mask中，Unity会自动跳过渲染

---

## 常见问题

### Q: 找不到"Roof" Layer选项？
**A:** 确保已经在`Edit > Project Settings > Tags and Layers`中创建了Roof Layer。

### Q: 屋顶在TopView相机中仍然可见？
**A:** 检查：
1. 屋顶GameObject是否真的在Roof Layer上
2. TopView相机的Culling Mask是否排除了Roof Layer
3. 是否有多个屋顶对象，某些没有移动到Roof Layer

### Q: FPS相机看不到屋顶？
**A:** 检查：
1. FPS相机的Culling Mask是否包含Roof Layer
2. 屋顶GameObject是否在Roof Layer上
3. 屋顶GameObject是否被禁用（Inactive）

### Q: 如何临时显示/隐藏屋顶？
**A:** 可以使用以下方法：
- **方法1**：修改相机的Culling Mask（运行时可用）
- **方法2**：启用/禁用屋顶GameObject（会影响所有相机）
- **方法3**：使用`CameraRoofManager`脚本动态切换

---

## 扩展：动态切换屋顶显示

如果需要运行时动态切换，可以在`CameraRoofManager`中添加方法：

```csharp
public void ShowRoofForCamera(Camera cam, bool show)
{
    if (cam == null) return;
    
    int roofLayer = LayerMask.NameToLayer(roofLayerName);
    if (roofLayer == -1) return;
    
    if (show)
    {
        cam.cullingMask |= (1 << roofLayer);
    }
    else
    {
        cam.cullingMask &= ~(1 << roofLayer);
    }
}
```

---

## 总结

这个解决方案的优点：
- ✅ 不需要删除或禁用屋顶
- ✅ 两个相机可以同时存在
- ✅ 性能友好（只是控制渲染，不实际隐藏对象）
- ✅ 易于管理和维护
- ✅ 可以轻松扩展到更多相机

这是Unity中处理"不同相机看到不同内容"的标准做法。

