# 环境数值设置面板 - Unity设置指南

## 概述
本指南将帮助您在Unity中创建环境数值设置面板，该面板将显示为游戏界面右上角的一个小矩形半透明面板，与现有游戏UI风格保持一致。

## 步骤1：创建UI Canvas

1. 在Unity编辑器中打开您的场景（`Assets/Scenes/SampleScene.unity`）
2. 在Hierarchy面板中右键点击 → **UI** → **Canvas**
3. 将新创建的Canvas重命名为 `EnvironmentValueSetterCanvas`

### Canvas设置
在Inspector面板中设置Canvas组件：
- **Render Mode**: `Screen Space - Overlay` （确保UI始终在屏幕上）
- **Pixel Perfect**: 勾选（可选，提升清晰度）
- **Sort Order**: 设置为较高的值，如100，确保在其他UI之上

## 步骤2：创建面板背景

1. 右键点击Canvas → **UI** → **Panel**
2. 将Panel重命名为 `ValueSetterPanel`
3. 在Inspector中设置Panel的Image组件：
   - **Color**: 设置为半透明深色，如 RGBA(0.1, 0.1, 0.1, 0.85)

### RectTransform设置（关键步骤 - 小矩形右上角）
在Inspector中设置RectTransform：
- **Anchors**:
  - Min: X=1, Y=1 （右上角）
  - Max: X=1, Y=1 （右上角）
- **Pivot**: X=1, Y=1 （以右上角为基准点）
- **Position**: X=0, Y=0 （相对于右上角）
- **Width**: 280 （小矩形宽度）
- **Height**: 240 （调整高度以容纳所有元素）

## 步骤3：添加UI元素

### 标题文本
1. 右键点击Panel → **UI** → **TextMeshPro - Text**
2. 重命名为 `TitleText`
3. 设置TextMeshPro组件：
   - **Text**: "环境设置"
   - **Font Size**: 16
   - **Color**: 白色
   - **Alignment**: 居中

RectTransform设置：
- **Anchors**: Min(0,1), Max(1,1)
- **Pivot**: (0.5, 1) （水平居中，顶部对齐）
- **Position**: X=0, Y=0
- **Width**: 260
- **Height**: 30

### 房间选择下拉菜单
1. 右键点击Panel → **UI** → **TextMeshPro - Dropdown**
2. 重命名为 `RoomDropdown`
3. 设置TMP_Dropdown组件的Label为合适的字体大小（12-14）

RectTransform设置：
- **Anchors**: Min(0,1), Max(1,1)
- **Pivot**: (0.5, 1) （水平居中，顶部对齐）
- **Position**: X=0, Y=-35
- **Width**: 260
- **Height**: 35

### 输入字段（紧凑布局）
为每个数值创建紧凑的输入字段：

#### 温度输入
1. 右键点击Panel → **UI** → **TextMeshPro - Input Field**
2. 重命名为 `TemperatureInput`
3. 设置Placeholder Text为 "温度°C"
4. 设置Content Type为 Decimal Number

RectTransform设置：
- **Anchors**: Min(0,1), Max(1,1)
- **Pivot**: (0.5, 1) （水平居中，顶部对齐）
- **Position**: X=0, Y=-80
- **Width**: 120
- **Height**: 30

#### PM2.5输入
类似创建，重命名为 `PM25Input`，Placeholder为 "PM2.5"

RectTransform：
- **Anchors**: Min(0,1), Max(1,1)
- **Pivot**: (0.5, 1) （水平居中，顶部对齐）
- **Position**: X=0, Y=-115
- **Width**: 120
- **Height**: 30

#### 烟雾浓度输入
重命名为 `SmokeInput`，Placeholder为 "烟雾"

RectTransform：
- **Anchors**: Min(0,1), Max(1,1)
- **Pivot**: (0.5, 1) （水平居中，顶部对齐）
- **Position**: X=0, Y=-150
- **Width**: 120
- **Height**: 30

#### 电力能耗显示
1. 右键点击Panel → **UI** → **TextMeshPro - Input Field**
2. 重命名为 `EnergyInput`
3. 设置Placeholder Text为 "能耗 kWh"
4. **重要**: 设置为只读显示
   - 取消勾选 **Interactable** (使其不可编辑)
   - 或者在脚本中设置为只读

RectTransform：
- **Anchors**: Min(0,1), Max(1,1)
- **Pivot**: (0.5, 1) （水平居中，顶部对齐）
- **Position**: X=0, Y=-185
- **Width**: 120
- **Height**: 30

### 按钮（紧凑布局）
#### 应用按钮
1. 右键点击Panel → **UI** → **Button - TextMeshPro**
2. 重命名为 `ApplyButton`
3. 设置Button Text为 "应用"

RectTransform：
- **Anchors**: Min(0,0), Max(0.5,0)
- **Pivot**: (0.5, 0.5) （居中对齐）
- **Position**: X=35, Y=20
- **Width**: 70
- **Height**: 30

#### 重置按钮
类似创建，重命名为 `ResetButton`，Text为 "重置"

RectTransform：
- **Anchors**: Min(0.5,0), Max(1,0)
- **Pivot**: (0.5, 0.5) （居中对齐）
- **Position**: X=-35, Y=20
- **Width**: 70
- **Height**: 30

## 步骤4：添加脚本组件

1. 选中 `ValueSetterPanel` GameObject
2. 在Inspector中点击 **Add Component**
3. 添加 `EnvironmentValueSetter` 脚本
4. 添加 `CanvasGroup` 组件（用于半透明控制）

### 脚本引用设置
在EnvironmentValueSetter脚本中设置引用：
- **Room Dropdown**: 拖入 `RoomDropdown`
- **Temperature Input**: 拖入 `TemperatureInput`
- **Pm25 Input**: 拖入 `PM25Input`
- **Smoke Input**: 拖入 `SmokeInput`
- **Energy Input**: 拖入 `EnergyInput` （只读显示能耗）
- **Apply Button**: 拖入 `ApplyButton`
- **Reset Button**: 拖入 `ResetButton`
- **Canvas Group**: 拖入Panel自身的CanvasGroup

## 步骤5：样式调整

为了与游戏界面保持一致：

1. **颜色方案**：
   - 背景：深灰色半透明 RGBA(0.1, 0.1, 0.1, 0.85)
   - 文字：白色
   - 输入框：深色背景

2. **字体大小**：
   - 标题：16pt
   - 标签：12-14pt
   - 输入文字：12pt

3. **间距**：
   - 元素间距：5-10像素
   - 内边距：5像素

## 步骤7：验证设置

### 使用UI验证器
1. 将 `EnvironmentValueSetterValidator.cs` 脚本添加到 `ValueSetterPanel` GameObject
2. 在Inspector中设置所有UI组件的引用
3. 点击 **Validate UI** 按钮查看验证结果
4. 根据验证结果修复任何配置问题

### 手动验证
1. 保存场景
2. 运行游戏
3. 验证面板是否出现在右上角小矩形区域
4. 尝试改变相机视角，确认面板位置不变
5. 测试数值输入和应用功能

## 步骤8：故障排除

如果遇到问题，请参考 `EnvironmentValueSetter_Troubleshooting.md` 获取详细的故障排除指南。

- **尺寸控制**: 面板尺寸设置为280x200像素，是一个小矩形
- **位置固定**: 锚点和基准点都设置为右上角，确保始终固定在该位置
- **半透明效果**: 通过CanvasGroup的alpha值控制整体透明度
- **紧凑布局**: 为适应小尺寸，元素排列更加紧凑

## 与现有UI的协调

参考现有 `EnvironmentControlPanel` 的布局风格：
- 类似的颜色方案
- 一致的字体大小
- 相似的交互方式

这样新的面板会自然融入现有的游戏界面。