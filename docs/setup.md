## 环境与资源配置说明

### 1. Unity 环境

- 推荐统一版本：**Unity 2022.3.62f3c1**
  - 所有组员尽量安装同一版本，避免因为版本差异导致工程文件频繁变动或无法打开。

### 2. 项目获取方式

1. 组员在本地创建工作目录，例如 `nicehouse/`。
2. 从 GitHub 克隆项目（由组长提供仓库地址）：

```bash
git clone <your-repo-url>
```

3. 使用 Unity Hub 选择 **“Open”**，打开仓库中的 `My project/` 目录。

> 注意：首次打开 Unity 时会自动生成 `Library/` 等本地缓存目录，这些已经通过仓库根目录的 `.gitignore` 忽略，不需要也不能提交到 Git。

### 3. 房屋三维模型资源

本项目使用的公寓场景模型来源于 Sketchfab，授权为 **CC Attribution（署名）**：

- 模型页面（浏览 / 下载）：  
  [`Modern Apartment` on Sketchfab](https://sketchfab.com/3d-models/modern-apartment-1fbb649cd6624f2bb7b7d6e30c6533a5)

> 使用时请注意在 PPT 与报告中对作者进行署名（页面右侧 “Credit the Creator” 中可复制推荐说明）。

#### 3.1 推荐下载格式

- 在模型页面的 Download 面板中，优先选择：
  - **`fbx` 原始格式（约 250MB）**
- Unity 对 `FBX` 支持最好，导入最方便、最稳定。

#### 3.2 导入到 Unity 的步骤（示例）

1. 从 Sketchfab 下载 `.fbx` 文件以及附带的贴图（如有单独贴图包也一起下载）。
2. 在 Unity 工程中创建资源目录，例如：
   - `Assets/Models/ModernApartment/`
3. 将下载的 `.fbx` 和贴图文件一起拷贝到上述目录中。
4. 回到 Unity，等待导入完成，在 `Project` 面板中找到该模型：
   - 可以把模型拖拽到场景中作为房屋主场景；
   - 如有需要可在 `Inspector` 中调整缩放比例、碰撞器、材质等。

### 4. 资源与场景命名建议

- 模型相关：
  - 目录：`Assets/Models/ModernApartment/`
  - 预制体：`ModernApartment.prefab`
- 场景：
  - 主场景：`HouseScene.unity`
  - 主菜单：`MainMenu.unity`

> 建议团队统一命名，避免不同成员各自创建多个类似资源，导致合并冲突和混乱。


