# TopView相机显示面板设置指南

## 功能描述

在FPS玩家视角中，通过房间内的智能面板显示TopView相机的俯视图画面。玩家可以在第一人称视角中查看房子的平面图和热力图。

---

## 实现原理

使用Unity的**Render Texture（渲染纹理）**技术：
1. 创建一个Render Texture作为TopView相机的渲染目标
2. 将TopView相机的画面渲染到这个纹理上
3. 在3D面板上使用这个纹理作为材质贴图
4. 玩家在第一人称视角中可以看到这个面板，就像看一个屏幕一样

---

## 使用步骤

### 步骤1：创建显示面板

**方法A：使用Quad（推荐，简单快速）**
1. 在场景中创建一个Quad：`GameObject > 3D Object > Quad`
2. 命名为 `TopViewDisplayPanel`
3. 调整位置和大小：
   - 放在墙上或合适的位置（例如：客厅墙上）
   - 调整Scale，例如：`(2, 1.5, 1)` 表示宽2米、高1.5米
   - 旋转面板使其面向玩家（Quad默认朝Z轴正方向，可能需要绕X轴旋转90度）

**方法B：使用自定义模型**
1. 导入或创建平板/屏幕模型
2. 确保模型有MeshRenderer组件
3. 放置在合适的位置

### 步骤2：添加TopViewDisplayPanel脚本

1. 选择刚才创建的面板GameObject
2. 在Inspector中点击 `Add Component`
3. 搜索并添加 `TopView Display Panel` 脚本

### 步骤3：配置脚本

在Inspector中配置以下参数：

**相机设置：**
- `Top View Camera`：拖拽场景中的TopViewCamera到此处
- 或者点击"自动查找TopView相机"按钮

**渲染纹理设置：**
- `Render Texture Width`：宽度（默认512，可以调高获得更好画质，如1024或2048）
- `Render Texture Height`：高度（默认512）
- `Enable MSAA`：是否启用抗锯齿（默认开启）

**画面裁剪设置：**
- `Enable Viewport Crop`：是否启用画面裁剪（默认开启）
- `Crop X`：裁剪区域X坐标（0-1，0=左边缘，1=右边缘）
- `Crop Y`：裁剪区域Y坐标（0-1，0=下边缘，1=上边缘）
- `Crop Width`：裁剪区域宽度（0-1）
- `Crop Height`：裁剪区域高度（0-1）

**提示：** 如果相机画面是正方形但房子是矩形，可以：
1. 点击"自动计算裁剪区域"按钮（基于房间布局自动计算）
2. 或手动调整Crop参数来只显示房子部分

**面板设置：**
- `Panel Material`：可选，如果为空会自动创建默认材质
- `Auto Find Camera`：是否自动查找TopView相机（默认开启）

**交互设置（可选）：**
- `Enable Interaction`：是否启用交互检测
- `Interaction Distance`：交互距离（米）
- `Player Tag`：玩家标签

### 步骤4：运行测试

1. 运行游戏
2. 切换到FPS视角
3. 走到面板附近
4. 应该能看到TopView相机的画面显示在面板上

---

## 高级配置

### 调整画面质量

在Inspector中修改：
- `Render Texture Width` 和 `Render Texture Height`
- 推荐值：
  - 低质量：512x512（性能好）
  - 中等质量：1024x1024（平衡）
  - 高质量：2048x2048（画质好，但性能消耗大）

### 调整面板大小和位置

1. 选择面板GameObject
2. 在Transform中调整：
   - `Position`：面板在场景中的位置
   - `Rotation`：面板的朝向（确保面向玩家）
   - `Scale`：面板的大小

**提示：**
- Quad默认朝Z轴正方向，如果面板在墙上，可能需要旋转：
  - 如果面板在X轴方向的墙上：绕Y轴旋转90度或-90度
  - 如果面板在Z轴方向的墙上：不需要旋转
  - 如果面板在天花板：绕X轴旋转180度

### 创建多个显示面板

可以在不同房间创建多个面板，都显示同一个TopView相机的画面：
1. 复制面板GameObject
2. 移动到其他房间
3. 所有面板会自动使用同一个TopView相机

### 自定义材质

如果需要自定义面板外观（例如：添加边框、发光效果等）：
1. 创建一个新材质
2. 在 `Panel Material` 字段中指定这个材质
3. 脚本会自动将Render Texture应用到材质的 `_MainTex` 属性

---

## 常见问题

### Q: 面板显示为黑色或空白？
**A:** 检查：
1. TopView相机是否已正确指定
2. TopView相机是否启用（Enabled）
3. TopView相机的Culling Mask是否正确设置
4. 查看Console是否有错误信息

### Q: 画面质量很差？
**A:** 提高Render Texture分辨率：
- 在Inspector中增加 `Render Texture Width` 和 `Render Texture Height`
- 注意：分辨率越高，性能消耗越大

### Q: 面板方向不对？
**A:** 调整面板的Rotation：
- Quad默认朝Z轴正方向
- 如果面板在墙上，需要旋转使其面向玩家
- 可以在Scene视图中实时查看效果

### Q: 如何让面板只在玩家靠近时显示？
**A:** 可以：
1. 启用 `Enable Interaction`
2. 在 `OnPlayerDistanceChanged` 回调中添加显示/隐藏逻辑
3. 或者使用其他交互系统（如ControlHub系统）

### Q: 可以显示多个不同的相机画面吗？
**A:** 可以！创建多个面板，每个面板使用不同的相机：
- 创建多个 `TopViewDisplayPanel` 组件
- 每个组件指定不同的相机
- 每个组件会创建独立的Render Texture

### Q: 相机画面是正方形，但我只想显示矩形的房子部分？
**A:** 使用画面裁剪功能：
1. 启用 `Enable Viewport Crop`
2. 点击"自动计算裁剪区域"按钮（推荐）
3. 或手动调整 `Crop X`、`Crop Y`、`Crop Width`、`Crop Height` 参数
4. 这些参数使用0-1范围，表示在相机视野中的相对位置和大小

---

## 性能考虑

- **Render Texture分辨率**：分辨率越高，GPU内存占用越大
- **多个面板**：如果创建多个面板显示同一个相机，只消耗一个Render Texture的内存
- **MSAA**：启用抗锯齿会略微增加性能消耗，但画面更平滑

**推荐配置：**
- 单个面板：1024x1024，启用MSAA
- 多个面板：512x512，启用MSAA（如果性能允许可以提高到1024x1024）

---

## 与ControlHub系统集成

如果项目中有ControlHub系统，可以将TopView显示面板集成到控制面板中：

1. 在ControlHub面板上创建一个子对象作为显示区域
2. 添加 `TopViewDisplayPanel` 脚本
3. 配置TopView相机
4. 玩家可以通过ControlHub系统访问这个视图

---

## 技术细节

### Render Texture生命周期
- Render Texture在 `Start()` 时创建
- 在 `OnDestroy()` 时自动清理
- 如果脚本被禁用，Render Texture仍然存在但不会被更新

### 相机设置
- TopView相机的 `Target Texture` 会被自动设置为创建的Render Texture
- 如果TopView相机之前有Target Texture，会被替换
- 脚本销毁时会尝试恢复相机的原始设置

### 材质管理
- 如果指定了 `Panel Material`，会创建材质实例（避免修改原始材质）
- 如果没有指定，会使用默认的 `Unlit/Texture` 材质
- 材质实例在脚本销毁时会被清理

---

## 总结

这个功能让玩家可以在第一人称视角中查看俯视图，非常适合：
- 查看整个房子的布局
- 查看热力图分布
- 了解房间温度分布
- 作为智能家居系统的监控屏幕

通过简单的设置，就可以在房间中创建一个"智能监控屏"，增强游戏的沉浸感和功能性。

