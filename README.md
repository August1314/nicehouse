## 好房子三维可视化系统（Unity 项目）

本仓库用于计算机图形学课程大作业 —— **“好房子”三维可视化系统**，采用 **Unity + GitHub + 敏捷开发** 方式，由 5 人小组协作完成。

### 仓库结构

- **My project/**：Unity 工程目录（已初始化）
- **docs/**：需求、设计文档和任务拆分
  - `requirements.md`：作业要求与功能梳理
  - `architecture.md`：整体架构与技术方案
  - `tasks.md`：小组成员任务分配 & 敏捷迭代计划

### 开发环境

- Unity 版本：建议所有成员统一使用 **同一小版本号**（例如 2022.x.y）
- IDE：Visual Studio / Rider / VS Code 均可

### Git & 分支建议

- **main**：稳定可演示版本（老师演示用）
- **dev**：日常集成分支（所有功能分支先合到 dev，测试 OK 再合到 main）
- 功能分支：`feature/<模块名>-<开发者>`，例如：
  - `feature/environment-control-zhangsan`
  - `feature/health-monitoring-lisi`

### 提交与协作规范（简化版）

- 每次开始开发前：先从远程 `dev` 拉取最新代码（`git pull`）
- 每个功能点独立分支，完成后发起 Pull Request，由至少 1 名同组同学 Code Review
- Commit 信息尽量清晰，例如：
  - `feat: add environment monitoring panel`
  - `fix: correct PM2.5 threshold logic`

### Unity 目录说明（默认）

Unity 工程位于 `My project/` 下，主要目录：

- `Assets/`：所有场景、脚本、预制体、材质等
- `Packages/`：Unity 包管理配置
- `ProjectSettings/`：项目设置（版本控制下）

> 注意：`Library/`、`Temp/`、`Logs/` 等目录体积大且为自动生成，已在 `.gitignore` 中忽略，不会推送到 GitHub。


