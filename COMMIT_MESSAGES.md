# Git 提交信息

## 提交 1: 优化 README 文档

**提交信息:**
```
docs: 全面优化 README.md 文档

- 采用标准开源项目文档结构，添加特性展示、安装指南、使用说明
- 提供详细的文件格式示例（JSON、TXT、M3U）
- 添加开发者指南，包含项目结构、构建命令和技术栈信息
- 完善许可证、贡献指南、致谢和联系信息
- 使用 emoji 图标增强可读性和视觉效果
```

**更改文件:**
- README.md

## 提交 2: 统一版本管理系统

**提交信息:**
```
feat: 实现软件版本、.csproj 和 GitHub Releases 三统一

- 修改 GetCompileVersion() 方法，从程序集信息版本读取版本号
- 移除版本号中可能附加的提交哈希，确保返回纯净版本
- 优化 GitHub Actions 工作流，自动从 .csproj 提取版本号
- 确保软件界面显示版本与发布包命名完全一致
- 修复变量命名冲突问题，确保代码编译无错误
```

**更改文件:**
- src/Common/Utils/M3u.cs
- .github/workflows/build.yml

## 提交命令示例

```bash
# 第一个提交：文档优化
git add README.md
git commit -m "docs: 全面优化 README.md 文档

- 采用标准开源项目文档结构，添加特性展示、安装指南、使用说明
- 提供详细的文件格式示例（JSON、TXT、M3U）
- 添加开发者指南，包含项目结构、构建命令和技术栈信息
- 完善许可证、贡献指南、致谢和联系信息
- 使用 emoji 图标增强可读性和视觉效果"

# 第二个提交：版本统一
git add src/Common/Utils/M3u.cs .github/workflows/build.yml
git commit -m "feat: 实现软件版本、.csproj 和 GitHub Releases 三统一

- 修改 GetCompileVersion() 方法，从程序集信息版本读取版本号
- 移除版本号中可能附加的提交哈希，确保返回纯净版本
- 优化 GitHub Actions 工作流，自动从 .csproj 提取版本号
- 确保软件界面显示版本与发布包命名完全一致
- 修复变量命名冲突问题，确保代码编译无错误"

# 推送到远程仓库
git push origin main
```

## 提交信息规范说明

- **docs**: 文档相关的更改
- **feat**: 新功能或特性
- 主题行简洁明了，描述更改的性质
- 正文详细说明具体更改内容和原因
- 使用项目符号列表提高可读性
- 符合常规提交 (Conventional Commits) 规范
