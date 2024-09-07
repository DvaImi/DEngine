# DEngine
#### 基于GameFramework框架和Fantasy框架，整合（缝合）为完整的双端开发套件 。

## Unity version

UNITY_2020_1_OR_NEWER

## Feature

### Todo && Done
- 扩充资源收集器
  - [x] 支持自定义资源收集器
  - [x] 收集器支持多个package
  - [ ] 支持导入配置
- 扩充数据表
  - [x] 数据表支持多sheet导出  
  - [x] 本地化修改为数据表方式导出
  - [ ] 后续考虑接入Luban 
- 接入hybridclr
  - [x] 拆分热更程序集和AOT程序集，流程已跑通 
- 接入Unitask
  - [x] 扩充资源加载为await模式
  - [ ] 考虑扩充异步事件系统
- 接入Fantasy
  - [x] 接入 Fantasy，测试流程已跑通
- 接入Luban
  - [ ] TODO 整合客户端和服务端配表统一为Luban


<strong>致谢

<a href="https://github.com/EllanJiang/GameFramework.git"><strong>GameFramework</strong></a> - Game Framework 是一个基于 Unity 引擎的游戏框架，主要对游戏开发过程中常用模块进行了封装，很大程度地规范开发过程、加快开发速度并保证产品质量。

<a href="https://github.com/focus-creative-games/hybridclr"><strong>HybridCLR</strong></a> - 特性完整、零成本、高性能、低内存的近乎完美的Unity全平台原生c#热更方案

<a href="https://github.com/qq362946/Fantasy.git"><strong>Fantasy</strong></a> - Fantasy是基于.NET的高性能网络开发框架，支持主流协议，前后端分离，适合需要快速上手、可扩展、分布式全平台商业级解决方案的开发团队或个人。它旨在提供易用工具，同时保证系统的高性能和扩展性。

