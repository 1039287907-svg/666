# ISO 11820 建筑材料不燃性试验仿真系统

> 🏗️ 基于 .NET 8 + WinForms 的建筑材料不燃性试验仿真软件
> 🎓 无需真实硬件，完整跑通 ISO 11820 标准试验流程
> 📊 5通道温度仿真 | 实时曲线 | Excel/PDF 报告 | 历史查询

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![WinForms](https://img.shields.io/badge/UI-WinForms-5C2D91)](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
[![SQLite](https://img.shields.io/badge/DB-SQLite-003B57?logo=sqlite)](https://www.sqlite.org/)
[![License](https://img.shields.io/badge/License-Educational%20Use%20Only-blue)](LICENSE)

---

## 📑 目录

- [项目简介](#项目简介)
- [快速开始](#快速开始)
- [完整演示流程](#完整演示流程)
- [系统架构](#系统架构)
- [技术栈详解](#技术栈详解)
- [项目文件结构](#项目文件结构)
- [数据模型](#数据模型)
- [核心业务层](#核心业务层)
- [UI 层详解](#ui-层详解)
- [配置说明](#配置说明)
- [数据库设计](#数据库设计)
- [开发指南](#开发指南)
- [团队分工](#团队分工)
- [API 参考](#api-参考)
- [常见问题](#常见问题)

---

## 项目简介

### 背景

在建材防火实验室中，需要将建筑材料样品放入加热炉加热到 **750°C**，持续记录 **60 分钟**的温度变化数据，根据温升和失重率等指标判断材料是否符合 ISO 11820 标准的"不燃"要求。

**本软件的核心价值在于：无需真实硬件设备，用程序仿真出完整的试验流程。** 温度数据由软件内置的仿真引擎自动生成，用户按照标准操作流程操作，最终自动生成 CSV / Excel / PDF 格式的标准试验报告。

### 软件定位

| 项目 | 说明 |
|------|------|
| **类型** | Windows 桌面应用（WinForms） |
| **开发语言** | C# 12 / .NET 8 |
| **数据库** | SQLite（本地文件，零配置） |
| **运行要求** | Windows 10/11，无需联网，无需硬件 |
| **适用场景** | 高校实验教学、实验室培训、标准演示 |

### 核心特性

- 🔐 **双角色登录系统**：管理员/试验员，密码验证
- 🔬 **5通道温度仿真**：炉温1、炉温2、表面温、中心温、校准温
- 🤖 **6状态自动机**：Idle → Preparing → Ready → Recording → Complete，自动流转
- 📈 **实时温度曲线**：OxyPlot 4 条折线，10 分钟滚动窗口，支持缩放拖拽
- 💾 **数据持久化**：SQLite 存储试验记录，CSV 存储秒级温度时序
- 📄 **多格式报告**：CSV 原始数据 + Excel 含图表 + PDF 含中文字体
- 🔍 **历史查询**：按日期、样品、操作员多维度检索
- 🔧 **设备校准**：炉壁 9 测温点校准，历史记录管理

---

## 快速开始

### 环境要求

| 项目 | 最低要求 | 推荐 |
|------|---------|------|
| **操作系统** | Windows 10 | Windows 11 |
| **.NET 运行时** | .NET 8.0 Desktop Runtime | .NET 8.0 SDK |
| **IDE** | 任意文本编辑器 | Visual Studio 2022 |
| **磁盘空间** | 200 MB | 500 MB |

### 安装运行

```bash
# 1. 进入项目目录
cd ISO11820\ISO11820.App

# 2. 还原 NuGet 依赖包
dotnet restore

# 3. 编译并运行
dotnet run

# 或者，发布为单文件可执行程序
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```

### 默认登录账号

| 角色 | 用户名 | 密码 | 说明 |
|------|--------|------|------|
| 🔑 管理员 | `admin` | `123456` | 全部功能权限 |
| 👷 试验员 | `experimenter` | `123456` | 标准试验操作 |

> ⚠️ **注意**：登录界面没有用户名输入框，用户名由角色选择自动确定。选择"管理员"即使用 `admin`，选择"试验员"即使用 `experimenter`。

---

## 完整演示流程

以下是软件跑通一个完整试验的步骤：

```
┌─────────────────────────────────────────────────────────────────┐
│  1. 启动程序 → 显示登录界面                                      │
│                                                                  │
│  2. 选择"管理员" → 输入密码 123456 → 登录成功 → 进入主界面        │
│                                                                  │
│  3. 点击「新建试验」→ 填写：                                      │
│       - 样品编号（如 20240613-001）                               │
│       - 样品名称（如 岩棉隔热板）                                 │
│       - 规格型号（如 100×50×25mm）                               │
│       - 高度 50mm、直径 45mm                                     │
│       - 试验前质量 50g                                            │
│       - 环境温度 25°C、环境湿度 50%                              │
│       - 选择"标准 60 分钟"                                       │
│     → 点击「创建试验」保存                                       │
│                                                                  │
│  4. 点击「开始升温」→ 炉温开始从 720°C 上升到 750°C              │
│     → 状态变为：升温中（Preparing）                               │
│     → 曲线图实时更新（每 800ms 一帧，每秒一个数据点）             │
│                                                                  │
│  5. 等待温度升到 747°C 以上且连续稳定（约 3 秒）                  │
│     → 状态自动变为：就绪（Ready）                                 │
│     → LED 面板炉温显示 750.0°C 附近                              │
│                                                                  │
│  6. 点击「开始记录」→ 状态变为：记录中（Recording）               │
│     → 计时器开始计数，每秒累加                                    │
│     → 表面温和中心温开始向炉温指数逼近                            │
│     → sensor_data.csv 每秒追加一行温度数据                       │
│                                                                  │
│  7. 等待 60 分钟（或手动点击「停止记录」）                       │
│     → 状态变为：完成（Complete）                                  │
│     → 点击「保存记录」→ 填写火焰现象和试验后质量                  │
│     → 自动计算失重率、温升、判定结论                              │
│     → 自动生成 Excel + PDF 报告                                  │
│                                                                  │
│  8. 切换到「记录查询」Tab → 能看到刚完成的试验记录               │
│     → 双击行查看详情 | 点击导出 Excel                            │
│                                                                  │
│  9. 切换到「设备校准」Tab → 可记录校准数据                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## 系统架构

### 分层架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                        UI 层 (Forms)                             │
│  ┌──────────┐ ┌──────────┐ ┌────────────────┐ ┌─────────────┐  │
│  │LoginForm │ │MainForm  │ │NewExperiment   │ │Calibration  │  │
│  │          │ │  ├ 试验操作│ │Dialog          │ │Dialog       │  │
│  │          │ │  ├ 记录查询│ └────────────────┘ └─────────────┘  │
│  │          │ │  └ 设备校准│ ┌────────────────────┐              │
│  └──────────┘ └──────────┘ │ExperimentRecord    │              │
│                            │Dialog               │              │
│  ┌──────────────────────┐  └────────────────────┘              │
│  │ ChartPanel (OxyPlot) │                                        │
│  └──────────────────────┘                                        │
├─────────────────────────────────────────────────────────────────┤
│                      核心业务层 (Core)                           │
│  ┌─────────────────┐  ┌──────────────────┐                      │
│  │ TestController   │  │ SensorSimulator  │                      │
│  │  - 状态机        │  │  - 5通道仿真     │                      │
│  │  - 终止判定      │  │  - 温漂计算      │                      │
│  │  - 恒功率计算    │  │  - 稳定判定      │                      │
│  └────────┬────────┘  └────────┬─────────┘                      │
│           │                    │                                  │
│  ┌────────┴────────────────────┴─────────┐                      │
│  │ DaqWorker (数据采集调度)               │                      │
│  │  - 800ms 定时器                        │                      │
│  │  - CSV 写入                            │                      │
│  │  - 数据广播到 UI                       │                      │
│  └────────────────────────────────────────┘                      │
│  ┌────────────────────────────────────────┐                      │
│  │ AppContext (全局单例)                   │                      │
│  │  - Config / Db / Controller / DaqWorker│                      │
│  └────────────────────────────────────────┘                      │
├─────────────────────────────────────────────────────────────────┤
│                       服务层 (Services)                          │
│  ┌────────────────────────────────────────┐                      │
│  │ ExportService (报告导出)                │                      │
│  │  - ExportExcel (EPPlus)                 │                      │
│  │  - ExportPdf (PDFsharp)                 │                      │
│  └────────────────────────────────────────┘                      │
├─────────────────────────────────────────────────────────────────┤
│                       数据层 (Data)                              │
│  ┌────────────────────────────────────────┐                      │
│  │ DbHelper (SQLite 操作封装)              │                      │
│  │  - 建表 & 初始数据                      │                      │
│  │  - 全部 CRUD 操作                       │                      │
│  │  - 参数化 SQL（$param）                 │                      │
│  └──────────────┬─────────────────────────┘                      │
│                 │ SQLite 文件                                    │
│             ┌───┴────┐                                           │
│             │ ISO11820│.db                                       │
│             └────────┘                                           │
├─────────────────────────────────────────────────────────────────┤
│                       配置层 (Config)                            │
│  ┌────────────────────────────────────────┐                      │
│  │ AppConfig (读取 appsettings.json)       │                      │
│  │  - Database / Simulation / Hardware     │                      │
│  │  - FileStorage / Report                 │                      │
│  └────────────────────────────────────────┘                      │
└─────────────────────────────────────────────────────────────────┘
```

### 事件驱动通信

```
DaqWorker (后台线程, 每 800ms)
  │
  ├─→ SensorSimulator.Update(state)
  │     └─→ 返回新温度值
  │
  ├─→ TestController.Tick()
  │     └─→ 检查状态转换
  │
  ├─→ (每 1000ms) OnSecondTick()
  │     ├─→ TestController.SecondTick()
  │     │     ├─→ ElapsedSeconds++
  │     │     └─→ 检查终止条件
  │     ├─→ WriteCsvRow()   (Recording 状态)
  │     ├─→ UpdateMaxTracking()
  │     └─→ 触发 DataBroadcast 事件
  │           │
  │           └─→ MainForm.OnDataBroadcast() (UI线程 via Invoke)
  │                 ├─→ 更新温度 Label
  │                 ├─→ 更新状态/计时器/温漂
  │                 ├─→ 追加系统消息
  │                 ├─→ 更新 OxyPlot 曲线
  │                 └─→ 更新按钮状态

关键原则：
  ✅ 上层依赖下层，下层不感知上层
  ✅ 数据从下到上通过事件传递，不直接调用 UI 方法
  ✅ 所有 UI 更新必须在 UI 线程执行（BeginInvoke）
```

---

## 技术栈详解

### 开发框架

| 技术 | 版本 | 用途 | 选型理由 |
|------|------|------|---------|
| **.NET** | 8.0 | 应用运行时 | 跨平台、高性能、长期支持 |
| **C#** | 12.0 | 编程语言 | 现代语法、类型安全、LINQ |
| **Windows Forms** | .NET 8 内置 | UI 框架 | 快速开发桌面应用、丰富的控件 |

### 第三方 NuGet 包

| 包名 | 版本 | 用途 | 关键 API |
|------|------|------|---------|
| **Microsoft.Data.Sqlite** | 8.0.10 | SQLite 数据库操作 | `SqliteConnection`, `SqliteCommand` |
| **OxyPlot.WindowsForms** | 2.2.0 | 实时温度折线图 | `PlotView`, `LineSeries`, `LinearAxis` |
| **EPPlus** | 7.4.2 | Excel 报告（含图表） | `ExcelPackage`, `ExcelChart` |
| **PDFsharp-MigraDoc** | 6.1.1 | PDF 报告生成 | `PdfDocument`, `XGraphics` |
| **Serilog** | 4.1.0 | 结构化文件日志 | `LoggerConfiguration`, 滚动日志 |
| **Serilog.Sinks.File** | 6.0.0 | Serilog 文件输出 | 按天滚动，保留 30 天 |
| **MathNet.Numerics** | 5.0.0 | 线性回归计算 | `SimpleRegression` |
| **Microsoft.Extensions.Configuration** | 8.0.0 | 配置文件读取 | `ConfigurationBuilder` |
| **Microsoft.Extensions.Configuration.Json** | 8.0.1 | JSON 配置支持 | `AddJsonFile` |

### 不需要的库

| 不需要的库 | 原因 |
|------------|------|
| FluentModbus | 无真实硬件，仿真模式不需要串口通信 |
| Emgu.CV | 无摄像头，跳过火焰图像检测 |
| Entity Framework Core | 手动 SQL 更灵活透明，学习成本更低 |
| System.IO.Ports | 仿真模式下不连接真实串口设备 |

---

## 项目文件结构

```
ISO11820/
│
├── README.md                              # ← 你正在看的文件
├── ISO11820-开发文档.md                   # 详细功能需求与技术设计
├── DB-数据库设计.md                       # SQLite 完整建表 SQL 和初始数据
├── ISO11820-三人分工方案.md               # 团队三层分工与接口约定
│
├── ISO11820.App/                          # WinForms 应用主项目
│   ├── ISO11820.App.csproj                # 项目文件（含 NuGet 引用）
│   ├── appsettings.json                   # 应用配置（仿真参数等）
│   ├── Program.cs                         # 程序入口：初始化 → 启动
│   │
│   ├── Models/                            # 数据模型（6 个实体类）
│   │   ├── Operator.cs                    # 操作员（userid, username, pwd, usertype）
│   │   ├── Apparatus.cs                   # 设备信息（innernumber, constpower 等）
│   │   ├── ProductMaster.cs               # 样品信息（productid, diameter, height）
│   │   ├── TestMaster.cs                  # 试验记录⭐（约 40 个字段，联合主键）
│   │   ├── Sensor.cs                      # 传感器配置（17 通道，量程/信号类型）
│   │   └── CalibrationRecord.cs           # 校准记录（9 测温点 + 统计量）
│   │
│   ├── Data/                              # 数据访问层
│   │   └── DbHelper.cs                    # SQLite 操作封装（~920 行）
│   │       ├── InitializeDatabase()       #   建 6 张表 + 初始化数据
│   │       ├── Login()                    #   登录验证
│   │       ├── GetApparatus()             #   设备信息
│   │       ├── UpsertProduct()            #   样品增改
│   │       ├── GetSensors()               #   传感器列表
│   │       ├── UpdateSensorValue()        #   更新传感器当前值
│   │       ├── InsertTest()               #   新建试验（40 个 INSERT 参数）
│   │       ├── UpdateTestResult()         #   试验完成更新（32 个 UPDATE 参数）
│   │       ├── GetCurrentTest()           #   获取未保存试验
│   │       ├── GetTest()                  #   按联合主键查询
│   │       ├── QueryTests()               #   多条件查询
│   │       ├── InsertCalibrationRecord()  #   保存校准记录（38 个参数）
│   │       └── QueryCalibrationRecords()  #   查询校准历史
│   │
│   ├── Core/                              # 核心业务层
│   │   ├── AppContext.cs                  # 全局单例上下文
│   │   ├── TestState.cs                   # 状态枚举 + 中文扩展
│   │   ├── TestController.cs              # 试验状态机（~435 行）
│   │   │   ├── StartHeating()             #   Idle → Preparing
│   │   │   ├── StopHeating()              #   Preparing/Ready → Idle
│   │   │   ├── StartRecording()           #   Ready → Recording
│   │   │   ├── StopRecording()            #   Recording → Complete
│   │   │   ├── SetTestContext()           #   设置试验上下文
│   │   │   ├── Tick()                     #   每帧更新（800ms）
│   │   │   ├── SecondTick()               #   每秒脉冲
│   │   │   ├── BuildTestMasterForSave()   #   构建保存对象
│   │   │   └── CalculateDrift()           #   温漂计算（最小二乘法）
│   │   ├── SensorSimulator.cs             # 温度仿真引擎（~262 行）
│   │   │   ├── Update(state)              #   每帧更新 5 通道
│   │   │   ├── UpdateHeating()            #   升温阶段算法
│   │   │   ├── UpdateStable()             #   稳定阶段算法
│   │   │   ├── UpdateRecording()          #   记录阶段算法
│   │   │   ├── UpdateCooling()            #   降温阶段算法
│   │   │   └── CheckStableCondition()     #   稳定判定（745~755°C + 3 ticks）
│   │   ├── DaqWorker.cs                   # 数据采集调度器（~235 行）
│   │   │   ├── Start() / Stop()           #   启停 800ms 定时器
│   │   │   ├── PrepareCsv()               #   创建 CSV 目录和文件
│   │   │   ├── OnTick()                   #   800ms 回调
│   │   │   ├── OnSecondTick()             #   秒脉冲（累积 800ms → 1000ms）
│   │   │   ├── WriteCsvRow()              #   追加 CSV 行
│   │   │   └── UpdateMaxTracking()        #   追踪各通道最大值
│   │   ├── DataBroadcastEventArgs.cs      # 广播事件参数（12 个属性）
│   │   └── MasterMessage.cs               # 系统消息模型 + 消息类型枚举
│   │
│   ├── Config/                            # 配置层
│   │   └── AppConfig.cs                   # 强类型配置（~55 行）
│   │       ├── 数据库配置（Provider, SqlitePath）
│   │       ├── 硬件配置（ConstPower, PidTemperature）
│   │       ├── 仿真参数（10 个参数）⭐
│   │       ├── 文件存储（BaseDirectory, TestDataDirectory）
│   │       └── 报告设置（OutputDirectory, EnablePdfExport）
│   │
│   ├── Services/                          # 服务层
│   │   └── ExportService.cs               # 报告导出服务（~407 行）
│   │       ├── ExportExcel(test)           #   生成 .xlsx（3 Sheet）
│   │       ├── ExportQueryResult(tests)    #   导出查询结果为 Excel
│   │       └── ExportPdf(test)             #   生成 PDF（中文字体嵌入）
│   │
│   ├── Forms/                             # UI 窗体
│   │   ├── LoginForm.cs                   # 登录界面（~150 行）
│   │   ├── MainForm.cs                    # 主窗体（核心字段 + 构造）
│   │   ├── MainForm.Layout.cs             # 主窗体 UI 布局（温度面板、按钮、消息框）
│   │   ├── MainForm.Events.cs             # 主窗体事件处理（按钮、广播、状态更新）
│   │   ├── MainForm.Tabs.cs               # 查询 Tab + 校准 Tab
│   │   ├── ChartPanel.cs                  # OxyPlot 图形控件（4 条曲线）
│   │   ├── NewExperimentDialog.cs         # 新建试验对话框
│   │   ├── ExperimentRecordDialog.cs      # 试验记录对话框
│   │   └── CalibrationDialog.cs           # 校准数据录入对话框
│   │
│   └── bin/Debug/net8.0-windows/          # 编译输出
│       ├── Logs/                          #   滚动日志文件
│       └── Data/                          #   SQLite 数据库文件
│
├── Reports/                               # 生成的报告文件（Excel + PDF）
└── TestData/                              # 温度时序 CSV 数据
    └── {ProductId}/
        └── {TestId}/
            └── sensor_data.csv            # 每秒一行 6 列温度数据
```

---

## 数据模型

### Operator（操作员）

```csharp
// 对应表: operators
// 注意：此表无主键约束，密码明文存储
public class Operator
{
    public string UserId { get; set; }   // "1" = admin, "2" = experimenter
    public string Username { get; set; }  // 登录用，如 "admin"
    public string Pwd { get; set; }       // 明文密码
    public string UserType { get; set; }  // "admin" 或 "operator"
}
```

### Apparatus（设备信息）

```csharp
// 对应表: apparatus
// 全局只有一条记录 (apparatusid = 0)
public class Apparatus
{
    public int ApparatusId { get; set; }          // 0
    public string InnerNumber { get; set; }        // "FURNACE-01"
    public string ApparatusName { get; set; }      // "一号试验炉"
    public DateTime CheckDateF { get; set; }       // 检定有效期开始
    public DateTime CheckDateT { get; set; }       // 检定有效期结束
    public string PidPort { get; set; }            // "COM9"（仿真模式预留）
    public string PowerPort { get; set; }          // "COM9"（仿真模式预留）
    public int? ConstPower { get; set; }           // 恒功率值，初始 2048
}
```

### ProductMaster（样品信息）

```csharp
// 对应表: productmaster
// 主键: ProductId
public class ProductMaster
{
    public string ProductId { get; set; }    // 如 "20240613-001"
    public string ProductName { get; set; }  // 如 "岩棉隔热板"
    public string Specific { get; set; }     // 规格型号，如 "100×50×25mm"
    public double Diameter { get; set; }      // 直径（mm）
    public double Height { get; set; }        // 高度（mm）
    public string? Flag { get; set; }         // 备用字段
}
```

### TestMaster（试验记录）⭐ 核心模型

```csharp
// 对应表: testmaster
// 联合主键: (ProductId, TestId)
// 外键: ProductId → productmaster.ProductId
// 约 40 个字段，是系统最核心的数据模型
public class TestMaster
{
    // ===== 基本信息（新建试验时填充）=====
    public string ProductId { get; set; }           // 样品编号
    public string TestId { get; set; }              // 试验ID，格式 yyyyMMdd-HHmmss
    public DateTime TestDate { get; set; }          // 试验日期
    public double AmbTemp { get; set; }             // 环境温度（°C）
    public double AmbHumi { get; set; }             // 环境湿度（%）
    public string According { get; set; }           // 试验依据，默认 "ISO 11820:2022"
    public string Operator { get; set; }            // 操作员用户名
    public string ApparatusId { get; set; }         // 设备编号
    public string ApparatusName { get; set; }       // 设备名称（冗余）
    public DateTime ApparatusChkDate { get; set; } // 设备检定日期
    public string RptNo { get; set; }               // 报告编号

    // ===== 质量数据（试验记录时填充）=====
    public double PreWeight { get; set; }           // 试验前质量（g）
    public double PostWeight { get; set; }          // 试验后质量（g）
    public double LostWeight { get; set; }          // 失重量（自动计算）
    public double LostWeightPer { get; set; }       // 【判定项】失重率（%）

    // ===== 试验过程 =====
    public int TotalTestTime { get; set; }          // 总试验时长（秒）
    public int ConstPower { get; set; }             // 恒功率值（0~25600）
    public string PhenoCode { get; set; }           // 现象编码（如 "flame:120s,5s"）
    public int FlameTime { get; set; }              // 火焰开始时刻（秒）
    public int FlameDuration { get; set; }          // 火焰持续时间（秒）

    // ===== 各通道温度最大值 =====
    public double MaxTf1 { get; set; }              // 炉温1 最大值
    public double MaxTf2 { get; set; }              // 炉温2 最大值
    public double MaxTs { get; set; }               // 表面温 最大值
    public double MaxTc { get; set; }               // 中心温 最大值
    public int MaxTf1Time { get; set; }             // 对应时刻（秒）
    public int MaxTf2Time { get; set; }
    public int MaxTsTime { get; set; }
    public int MaxTcTime { get; set; }

    // ===== 各通道最终值（试验结束时刻）=====
    public double FinalTf1 { get; set; }
    public double FinalTf2 { get; set; }
    public double FinalTs { get; set; }
    public double FinalTc { get; set; }
    public int FinalTf1Time { get; set; }
    public int FinalTf2Time { get; set; }
    public int FinalTsTime { get; set; }
    public int FinalTcTime { get; set; }

    // ===== 温升（自动计算）=====
    public double DeltaTf1 { get; set; }            // 炉温1温升
    public double DeltaTf2 { get; set; }            // 炉温2温升
    public double DeltaTf { get; set; }             // 【判定项】综合温升（°C）
    public double DeltaTs { get; set; }             // 表面温升
    public double DeltaTc { get; set; }             // 中心温升

    // ===== 辅助字段 =====
    public string? Memo { get; set; }               // 备注
    public string? Flag { get; set; }               // "10000000" = 已保存

    // ===== 派生属性 =====
    public bool IsSaved => Flag == "10000000";       // 是否已保存
    public bool IsFinished => TotalTestTime > 0;     // 是否已完成记录
}
```

### Sensor（传感器配置）

```csharp
// 对应表: sensors
// 主键: SensorId (0~16，共 17 个通道)
public class Sensor
{
    public int SensorId { get; set; }        // 0~3=主要通道, 16=校准, 4~15=备用
    public string SensorName { get; set; }   // 如 "Sensor0"
    public string DispName { get; set; }     // 如 "炉温1"
    public string SensorGroup { get; set; }  // "采集" 或 "校准"
    public string Unit { get; set; }         // "℃"
    public string Discription { get; set; }  // 描述
    public string Flag { get; set; }         // "启用"
    public double SignalZero { get; set; }   // 信号零点
    public double SignalSpan { get; set; }   // 信号量程
    public double OutputZero { get; set; }   // 输出温度下限（0）
    public double OutputSpan { get; set; }   // 输出温度上限（1000）
    public double OutputValue { get; set; }  // 当前温度值（运行时更新）
    public double InputValue { get; set; }   // 当前输入值（运行时更新）
    public int SignalType { get; set; }      // 4 = 数字量（仿真用）
}
```

### CalibrationRecord（校准记录）

```csharp
// 对应表: CalibrationRecords（注意大写开头，与其他表不同）
// 主键: Id (GUID)
public class CalibrationRecord
{
    public string Id { get; set; }               // GUID 主键
    public string CalibrationDate { get; set; }   // ISO 8601 日期
    public string CalibrationType { get; set; }   // "Surface" 或 "Center"
    public int ApparatusId { get; set; }
    public string Operator { get; set; }
    public string TemperatureData { get; set; }   // JSON 字符串

    // 炉壁 9 测温点（A/B/C 层 × 1/2/3 轴）
    public double? TempA1 { get; set; }  public double? TempA2 { get; set; }  public double? TempA3 { get; set; }
    public double? TempB1 { get; set; }  public double? TempB2 { get; set; }  public double? TempB3 { get; set; }
    public double? TempC1 { get; set; }  public double? TempC2 { get; set; }  public double? TempC3 { get; set; }

    // 计算结果
    public double? TAvg { get; set; }              // 总均温
    public double? TAvgAxis1 { get; set; }          // 轴1平均
    public double? TAvgAxis2 { get; set; }          // 轴2平均
    public double? TAvgAxis3 { get; set; }          // 轴3平均
    public double? TAvgLevela { get; set; }         // A层平均
    public double? TAvgLevelb { get; set; }         // B层平均
    public double? TAvgLevelc { get; set; }         // C层平均
    public double? TDevAxis1 { get; set; }          // 轴1偏差
    public double? TDevAxis2 { get; set; }          // 轴2偏差
    public double? TDevAxis3 { get; set; }          // 轴3偏差
    public double? TDevLevela { get; set; }         // A层偏差
    public double? TDevLevelb { get; set; }         // B层偏差
    public double? TDevLevelc { get; set; }         // C层偏差
    public double? TAvgDevAxis { get; set; }         // 轴平均偏差
    public double? TAvgDevLevel { get; set; }        // 层平均偏差
    public string? CenterTempData { get; set; }     // Center 类型额外 JSON

    public double? UniformityResult { get; set; }
    public double? MaxDeviation { get; set; }        // 最大偏差
    public double? AverageTemperature { get; set; }   // 平均温度
    public int PassedCriteria { get; set; }          // 0=未通过, 1=通过
    public string Remarks { get; set; }
    public string CreatedAt { get; set; }
    public string? Memo { get; set; }
}
```

---

## 核心业务层

### 1. 试验状态机（TestController）

#### 状态枚举

```csharp
public enum TestState
{
    Idle      = 0,  // 空闲（炉子未加热）
    Preparing = 1,  // 升温中（炉温上升至 750°C）
    Ready     = 2,  // 就绪（温度稳定在 745~755°C）
    Recording = 3,  // 记录中（每秒采集温度数据）
    Complete  = 4   // 完成（等待保存试验记录）
}
```

#### 状态流转图

```
                    ┌─────────────────────────────┐
                    │                             │
                    ▼                             │
    ┌──────┐  StartHeating()  ┌───────────┐     │
    │ Idle │ ───────────────→ │ Preparing │     │
    └──────┘                  └─────┬─────┘     │
       ▲                           │            │
       │                   温度达标+稳定          │
       │                    (自动判定)           │
       │                           │            │
       │                           ▼            │
       │                      ┌───────┐         │
       │  StopHeating()       │ Ready │─────────┘
       │ ←─────────────────── └───┬───┘  温度跌落
       │                          │     (自动回退)
       │                   StartRecording()
       │                          │
       │                          ▼
       │                    ┌───────────┐
       │                    │ Recording │
       │                    └─────┬─────┘
       │                          │
       │              时间到 / 手动 / 终止条件
       │                          │
       │                          ▼
       │                    ┌──────────┐          ┌──────────┐
       │                    │ Complete │ ───────→ │ Idle     │
       │                    └──────────┘ SaveRecord └──────────┘
       │                                    (炉子冷却)
       │
       └──── StopHeating()（从 Preparing 或 Ready 状态）
```

#### 状态转换规则详解

| 转换 | 触发方式 | 条件 |
|------|---------|------|
| **Idle → Preparing** | 用户点击「开始升温」 | 无未保存的已完成试验 |
| **Preparing → Ready** | 自动 | TF1 ≥ 747°C 且连续稳定 > 3 tick（约 3.2 秒） |
| **Ready → Preparing** | 自动回退 | 温度跌出 745~755°C 范围 |
| **Ready → Recording** | 用户点击「开始记录」 | 计算恒功率 = PID队列平均值 |
| **Recording → Complete** | 自动/手动 | 时间到 / 终止条件满足 / 用户停止 |
| **Complete → Idle** | 保存记录后 | 清空试验上下文，炉子开始冷却 |
| **Preparing/Ready → Idle** | 用户点击「停止升温」 | 炉子开始冷却 |

#### 特殊保护规则

1. **未保存试验阻止新建**：如果存在 `totaltesttime > 0` 且 `flag != "10000000"` 的试验，禁止新建试验和开始记录
2. **Ready 回退**：Ready 状态下温度跌出稳定范围，自动回退到 Preparing
3. **Complete 后保持炉温**：Complete → Preparing 而不是 Idle，避免重新升温等待
4. **无有效样本保护**：Recording → Complete 前检查是否有有效记录（ElapsedSeconds > 0）

### 2. 传感器仿真引擎（SensorSimulator）

#### 5 通道定义

| 通道 | 代码 | 名称 | 行为描述 |
|------|------|------|---------|
| **TF1** | `TF1` | 炉温1（加热炉内主温度） | 升温→稳定在 750°C |
| **TF2** | `TF2` | 炉温2（加热炉内副温度） | 与 TF1 同步但独立随机噪声 |
| **TS** | `TS` | 表面温度（样品外表面） | 记录阶段向 TF1×0.95 指数接近 |
| **TC** | `TC` | 中心温度（样品中心） | 记录阶段向 TF1×0.85 指数接近（更慢） |
| **TCal** | `TCal` | 校准温度（标定用） | = TF1 + 随机波动×2（不画曲线） |

#### 仿真算法（每 800ms 执行一次）

```
═══════════════════════════════════════════════
升温阶段（TF1 < TargetTemp - StableThreshold，即 < 747°C）
═══════════════════════════════════════════════
  TF1 += HeatingRatePerSecond × 0.8 + Random(-1,1) × TempFluctuation
  TF2 += HeatingRatePerSecond × 0.8 + Random(-1,1) × TempFluctuation  // 与 TF1 独立噪声
  TS  = TF1 × 0.3 + Noise()                    // 非记录阶段低值跟随
  TC  = TF1 × 0.25 + Noise()                   // 比表面温更低
  TCal = TF1 + Noise() × 2

  若 TF1 >= 747°C → 进入稳定判定
    满足 745~755°C 且 StableCount > 3 → IsStable = true → Preparing→Ready

═══════════════════════════════════════════════
稳定阶段（Preparing/Ready/Complete 状态）
═══════════════════════════════════════════════
  TF1 = 750 + Noise()                           // 钳位到目标温度
  TF2 = 750 + Noise()
  TS  = TF1 × 0.3 + Noise()                    // 非记录阶段保持低值
  TC  = TF1 × 0.25 + Noise()
  TCal = TF1 + Noise() × 2

═══════════════════════════════════════════════
记录阶段（Recording 状态）
═══════════════════════════════════════════════
  TF1 = 750 + Noise()                           // 炉温保持稳定
  TF2 = 750 + Noise()

  surfaceTarget = min(TF1 × 0.95, 800)
  TS += (surfaceTarget - TS) × 0.02 + Noise()  // 指数接近，每帧偏移 2%

  centerTarget = min(TF1 × 0.85, 750)
  TC += (centerTarget - TC) × 0.01 + Noise()   // 比表面温慢一半，每帧偏移 1%

  TCal = TF1 + Noise() × 2

═══════════════════════════════════════════════
降温阶段（Idle 状态）
═══════════════════════════════════════════════
  TF1 -= 0.5 + Noise() × 0.1                   // 缓慢冷却
  TF2 -= 0.5 + Noise() × 0.1
  // 不低于 25°C（环境温度下限）
  TS  = TF1 × 0.3 + Noise()
  TC  = TF1 × 0.25 + Noise()

═══════════════════════════════════════════════
随机噪声
═══════════════════════════════════════════════
  Noise() = Random(-1, 1) × TempFluctuation    // TempFluctuation 默认 0.5°C
```

#### 样品温度重置逻辑

当用户点击「开始记录」时，模拟将常温样品放入 750°C 炉内的时刻：

```csharp
// TestController.StartRecording() 中调用
_simulator.ResetSampleTemps(AmbTemp);
// TS = AmbTemp + Noise();   // 从环境温度开始
// TC = AmbTemp + Noise();   // 然后指数爬升到炉温×系数
```

### 3. 数据采集调度器（DaqWorker）

```
DaqWorker 使用 System.Timers.Timer，每 800ms 触发一次

┌─────────────────────────────────────────────────────────┐
│                      OnTick() 回调                       │
│                                                          │
│  1. TestController.Tick()                                │
│     ├── SensorSimulator.Update(state) → 更新 5 通道温度  │
│     ├── 检查自动状态转换（Preparing→Ready, Ready→Preparing)│
│     ├── Ready 状态：TF1 加入 PID 队列（计算恒功率用）     │
│     └── Recording 状态：TF1 加入温漂历史、检查终止条件    │
│                                                          │
│  2. UpdateSensorValues()                                 │
│     └── DbHelper.UpdateSensorValue(id, temp, temp) × 5  │
│                                                          │
│  3. 秒边界检测                                           │
│     _accumulatedMs += 800                                │
│     if (_accumulatedMs >= 1000) → OnSecondTick()         │
│                                                          │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                    OnSecondTick()                         │
│                                                          │
│  1. TestController.SecondTick()                          │
│     ├── Recording: ElapsedSeconds++                     │
│     ├── 检查是否到达目标时长                             │
│     └── 装配 DataBroadcastEventArgs                     │
│                                                          │
│  2. Recording 状态：                                     │
│     ├── UpdateMaxTracking() → 追踪各通道最大值           │
│     └── WriteCsvRow() → 追加一行到 sensor_data.csv      │
│                                                          │
│  3. 触发 DataBroadcast 事件 → UI 订阅者收到数据         │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 4. 温漂计算

使用最小二乘法线性回归，直接在 TestController 中实现（不依赖 MathNet）：

```csharp
private double CalculateDrift()
{
    int n = _tempHistory.Count;  // 最多 600 个数据点（10分钟）
    if (n < 2) return 0;

    // y = a + b*x  线性回归
    double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
    for (int i = 0; i < n; i++)
    {
        double x = i - n + 1;       // 以最后一个点为基准
        double y = _tempHistory[i];
        sumX += x; sumY += y;
        sumXY += x * y; sumX2 += x * x;
    }

    double denominator = n * sumX2 - sumX * sumX;
    if (Math.Abs(denominator) < 1e-10) return 0;

    double slope = (n * sumXY - sumX * sumY) / denominator;

    // 斜率是 °C/秒，换算为 °C/10min
    return slope * 600;
}
```

### 5. 试验终止条件

```
标准 60 分钟模式：
  ├── 在第 30/35/40/45/50/55 分钟检查提前终止条件
  ├── 提前终止条件：
  │   ├── 温度历史 ≥ 600 个数据点（10 分钟）
  │   └── 10 分钟温漂 ≤ MaxTemperatureDriftPerTenMinutes（默认 2.0 °C/10min）
  └── 第 60 分钟无条件终止

固定时长模式：
  ├── 忽略提前终止检查
  └── ElapsedSeconds ≥ TargetDurationSeconds → 终止

手动终止：
  └── 用户点击「停止记录」→ 有有效样本则 Complete，否则回 Preparing
```

### 6. 恒功率计算

```
Ready 状态下：
  PID 队列持续追加 TF1 值（最多 600 个数据点）

用户点击「开始记录」时：
  恒功率 = 队列中所有值的平均值 → 写入试验记录
  队列清空，进入记录阶段
```

### 7. 判定结论逻辑

```
通过条件（三者同时满足）：
  ✅ 综合温升 ΔTf ≤ 30°C
  ✅ 失重率 ≤ 50%
  ✅ 火焰持续时间 ≤ 20 秒

任一不满足 → 不通过
```

---

## UI 层详解

### 登录界面（LoginForm）

```
┌──────────────────────────────────────────┐
│         ISO 11820 试验系统                │
│                                          │
│  ┌────────────────────────────────────┐  │
│  │ 选择角色                           │  │
│  │  ○ 管理员   ○ 试验员               │  │
│  └────────────────────────────────────┘  │
│                                          │
│  密码： [··················]              │
│                                          │
│        ┌──────────────┐                  │
│        │    登  录    │                  │
│        └──────────────┘                  │
└──────────────────────────────────────────┘

关键设计：
  - 没有用户名输入框，用户名由角色决定
  - 管理员 → admin, 试验员 → experimenter
  - 密码框支持 Enter 键提交
  - 登录失败：MessageBox 提示，选中密码文本
  - 登录成功：隐藏登录窗，显示主窗体
```

### 主界面布局（MainForm）

主界面使用 TabControl 分为 3 个 Tab，主布局采用可折叠的 SplitContainer。

```
┌──────────────────────────────────────────────────────────────────┐
│ 操作员：admin（admin）                                   [◀]     │
├──────────────────────────────────────────────────┬───────────────┤
│ ┌──────────────────────────────────────────────┐ │ 试验状态      │
│ │ 炉温1       炉温2     表面温   中心温 校准温 │ │ 当前状态: 就绪 │
│ │ 750.1°C   749.8°C  620.5°C 480.1°C 751.0°C │ │ 已记录: 120秒  │
│ └──────────────────────────────────────────────┘ │ 温漂: 0.05    │
│                                                  │ 样品编号: ... │
│ ┌──────────────────────────────────────────────┐ ├───────────────┤
│ │            温度曲线 (OxyPlot)                 │ │ 操作          │
│ │    800 ┤                                    │ │ [新建试验]    │
│ │        │     ╭────── 炉温1 (红)              │ │ [开始升温]    │
│ │    600 ┤    ╱       ╭── 炉温2 (蓝)          │ │ [停止升温]    │
│ │        │   ╱     ╭─╯                        │ │ [开始记录]    │
│ │    400 ┤  ╱   ╭─╯   表面温 (绿)            │ │ [停止记录]    │
│ │        │ ╱ ╭─╯                              │ │ [保存记录]    │
│ │    200 ┤╱╭╯        中心温 (橙)              │ │ [参数设置]    │
│ │        └────────────────────                 │ ├───────────────┤
│ │        0              600 (秒)                │ │ 系统消息:     │
│ └──────────────────────────────────────────────┘ │ 18:28:14 ... │
│ 炉温1 炉温2 表面温 中心温 (图例)                 │ 18:28:15 ... │
└──────────────────────────────────────────────────┴───────────────┘

Tab 页：
  [试验操作]  [记录查询]  [设备校准]
```

#### 温度 LED 面板

```csharp
// 5 个通道，黑底彩色大字
// 布局：深色背景 (#141414)，大号 Consolas 20pt 字体
// 颜色映射：
//   炉温1 → 红色   (#FF4D4F)
//   炉温2 → 蓝色   (#1890FF)
//   表面温 → 绿色  (#52C41A)
//   中心温 → 橙色  (#FA8C16)
//   校准温 → 绿色  (Lime)
//
// 炉温1 超范围告警：Ready 状态后偏离 740~760 → 变为 OrangeRed
```

#### 按钮状态控制

| 按钮 | Idle | Preparing | Ready | Recording | Complete |
|------|:----:|:---------:|:-----:|:---------:|:--------:|
| **新建试验** | ✅ | 有未保存❌ / 无✅ | ❌ | ❌ | 未保存❌ / 保存后✅ |
| **开始升温** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **停止升温** | ❌ | ✅ | ✅ | ❌ | ✅ |
| **开始记录** | ❌ | ❌ | ✅ | ❌ | ❌ |
| **停止记录** | ❌ | ❌ | ❌ | ✅ | ❌ |
| **保存记录** | ❌ | ❌ | ❌ | ❌ | ✅ |
| **参数设置** | ✅ | ✅ | ✅ | ❌ | ✅ |

#### 系统消息日志

```csharp
// 使用 RichTextBox，黑底白字 Consolas 9pt
// 三种消息类型：
//   Info    → 白色  普通状态变更（开始升温、开始记录等）
//   Warning → 黄色  提醒/警告（满足终止条件、导出失败等）
//   Success → 绿色  操作成功（试验记录已保存、报告已生成）

// 消息产生时机：
//   程序启动          → "系统初始化，操作员：admin"
//   StartHeating()   → "开始升温"
//   StopHeating()    → "停止升温，冷却中"
//   Preparing→Ready  → 自动（由状态转换触发）
//   Ready→Recording  → "开始记录"
//   终止条件满足      → 由 TestController 生成
//   到达 60 分钟      → 由 TestController 生成
//   手动停止          → "停止记录"
//   Complete 到达     → "试验完成，请点击【保存记录】"
//   保存成功          → "试验记录已保存"
//   报告生成          → "Excel 报告已生成" / "PDF 报告已生成"
```

### OxyPlot 曲线图（ChartPanel）

```csharp
// 4 条 LineSeries：
//   炉温1 → 红色  OxyColor.FromRgb(245, 34, 45)   StrokeThickness 1.5
//   炉温2 → 蓝色  OxyColor.FromRgb(24, 144, 255)  StrokeThickness 1.5
//   表面温 → 绿色 OxyColor.FromRgb(82, 196, 26)   StrokeThickness 2.0
//   中心温 → 橙色 OxyColor.FromRgb(250, 140, 22)  StrokeThickness 2.0
//
// X 轴：LinearAxis，标题 "时间 (秒)"，滚动显示最近 600 个点（10 分钟）
// Y 轴：LinearAxis，标题 "温度 (°C)"，动态范围（自动适配数据范围）
//
// 特殊处理：
//   - 非 Recording 状态下 TS/TC 以 NaN 占位（不显示线）
//   - Recording 状态下 TS/TC 才开始显示实际数据
//   - 支持缩放和拖拽（IsZoomEnabled, IsPanEnabled）
//   - 底部手绘彩色图例条
```

### 新建试验对话框（NewExperimentDialog）

```
┌──────────────────────────────────────┐
│ 新建试验                             │
│                                      │
│ 样品编号：  [20240613-001]           │
│ 试验标识：  20240613-143022 (自动)   │
│ 样品名称：  [岩棉隔热板]             │
│ 规格型号：  [100×50×25mm]           │
│ 高度(mm)：  [50]   直径(mm)： [45]   │
│ 试验前质量(g)： [50.00]              │
│ 环境温度(°C)： [25.0]  湿度(%)：[50]│
│ 操作员：    admin (自动)             │
│ ┌─试验时长模式────────────────────┐  │
│ │ ○ 标准 60 分钟                  │  │
│ │ ○ 自定义时长(分钟)：[30]        │  │
│ └────────────────────────────────┘  │
│ ┌─设备信息────────────────────────┐  │
│ │ 设备：一号试验炉 (FURNACE-01)    │  │
│ │ 检定日期：2024-01-01 ~ 2025-01-01│  │
│ └────────────────────────────────┘  │
│                                      │
│    [创建试验]      [取消]            │
└──────────────────────────────────────┘

验证规则：
  - 样品编号不能为空
  - 样品名称不能为空
  - 试验前质量必须 > 0
  - 创建时自动 UpsertProduct() + InsertTest()
```

### 试验记录对话框（ExperimentRecordDialog）

```
┌──────────────────────────────────────┐
│ 试验记录 — 填写试验现象              │
│                                      │
│ 样品编号：20240613-001               │
│ 试验前质量：50.00 g                  │
│                                      │
│ ☐ 是否出现持续火焰                   │
│ 火焰发生时刻(秒)：[0]  持续时间：[0]│
│ ─────────────────────────────────── │
│ 试验后质量(g)：[45.00]  ← 必填      │
│ 备注：[________________]             │
│                                      │
│ 预览 — 失重率：10.00%  温升：2.35°C │
│ 判定结论：通过 ✓                    │
│                                      │
│    [保存试验记录]   [取消]           │
└──────────────────────────────────────┘

联动逻辑：
  - ☐ 未勾选 → 火焰时刻/持续时间禁用
  - 试验后质量变化 → 实时更新失重率预览
  - 火焰持续时间变化 → 实时更新判定结论
  - 保存后自动计算全部统计字段 → UpdateTestResult() → 生成报告
```

### 历史查询（Tab 2）

```
筛选条件：
  开始日期：[2024-01-01]  结束日期：[2024-12-31]
  样品编号：[________]  操作员：[(全部) ▼]
  [查询]  [导出Excel]

结果列表（DataGridView）：
┌──────────────┬──────────┬──────────┬────────┬──────┬──────┬──────┬──────┬──────┐
│ 试验ID       │ 样品编号 │ 日期     │ 操作员 │ 时长 │ 前质量│ 后质量│ 失重率│ 状态 │
├──────────────┼──────────┼──────────┼────────┼──────┼──────┼──────┼──────┼──────┤
│ 20240613-... │ 202406.. │ 2024-06..│ admin  │ 3600 │ 50.00│ 45.00│ 10%  │ 已保存│
└──────────────┴──────────┴──────────┴────────┴──────┴──────┴──────┴──────┴──────┘

双击行 → 弹出详情 MessageBox（完整试验信息）
```

### 设备校准（Tab 3）

```
当前校准温度：750.1 °C
[记录当前校准数据]

校准历史记录（DataGridView）：
┌──────┬──────────┬──────┬────────┬──────┬──────┬──────┐
│ ID   │ 日期     │ 类型 │ 操作员 │ 平均 │ 最大偏差│ 判定 │
└──────┴──────────┴──────┴────────┴──────┴──────┴──────┘

记录校准时弹出 CalibrationDialog：
  - 校准类型：Surface / Center
  - 炉壁 9 测温点（A/B/C层 × 1/2/3轴），自动预填当前校准温度
  - 自动计算均温、最大偏差
  - 偏差 ≤ 10°C → 通过；否则不通过
```

---

## 配置说明

### appsettings.json 完整配置

```json
{
  "Database": {
    "Provider": "Sqlite",
    "SqlitePath": "Data\\ISO11820.db"
  },
  "Hardware": {
    "ConstPower": 2048,
    "PidTemperature": 750,
    "SensorProtocol": "ModbusRtu"
  },
  "Simulation": {
    "EnableSimulation": true,
    "SimulateSensors": true,
    "SimulatePidController": true,
    "InitialFurnaceTemp": 720.0,
    "TargetFurnaceTemp": 750.0,
    "HeatingRatePerSecond": 40.0,
    "TempFluctuation": 0.5,
    "StableThreshold": 3.0,
    "SimulateFlame": false,
    "MaxTemperatureDriftPerTenMinutes": 2.0,
    "UpdateIntervalMs": 800
  },
  "FileStorage": {
    "BaseDirectory": "D:\\ISO11820",
    "TestDataDirectory": "D:\\ISO11820\\TestData"
  },
  "Report": {
    "OutputDirectory": "D:\\ISO11820\\Reports",
    "EnablePdfExport": true
  }
}
```

### 配置项说明

#### Simulation 节（⭐ 最重要）

| 配置项 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `EnableSimulation` | bool | true | **仿真模式总开关**，开发阶段必须为 true |
| `SimulateSensors` | bool | true | 是否仿真传感器读数 |
| `SimulatePidController` | bool | true | 是否仿真 PID 控制器 |
| `InitialFurnaceTemp` | double | 720.0 | 初始炉温（°C），设置较低可延长升温演示时间 |
| `TargetFurnaceTemp` | double | 750.0 | 目标炉温（°C），ISO 11820 标准为 750°C |
| `HeatingRatePerSecond` | double | 40.0 | 升温速率（°C/s），课堂演示建议 40，慢速演示建议 5~10 |
| `TempFluctuation` | double | 0.5 | 温度随机波动幅度（°C） |
| `StableThreshold` | double | 3.0 | 稳定判定阈值（°C），TF1 ≥ 750-3 = 747 即进入稳定判定 |
| `SimulateFlame` | bool | false | 是否仿真火焰检测（Demo 中跳过） |
| `MaxTemperatureDriftPerTenMinutes` | double | 2.0 | 提前终止判定的温漂阈值（°C/10min） |
| `UpdateIntervalMs` | int | 800 | 仿真更新间隔（毫秒） |

#### 演示场景配置建议

```json
// 课堂快速演示（从 720°C 起步，约 1.5 秒即稳定）
"Simulation": {
    "InitialFurnaceTemp": 720.0,
    "HeatingRatePerSecond": 40.0
}

// 慢速演示（从 25°C 起步，让学生看到完整升温过程）
"Simulation": {
    "InitialFurnaceTemp": 25.0,
    "HeatingRatePerSecond": 5.0
}

// 真实感演示（接近实际加热炉行为）
"Simulation": {
    "InitialFurnaceTemp": 25.0,
    "HeatingRatePerSecond": 2.0,
    "TempFluctuation": 1.0
}
```

---

## 数据库设计

### 表结构概览

| 表名 | 主键 | 字段数 | 索引 | 说明 |
|------|------|--------|------|------|
| `operators` | 无主键 | 4 | 无 | ⚠️ 无主键，密码明文 |
| `apparatus` | `apparatusid` | 8 | 主键索引 | 全局唯一记录 |
| `productmaster` | `productid` | 6 | 主键索引 | |
| `testmaster` | `(productid, testid)` | 43 | 3 个索引 | ⭐ 核心表 |
| `sensors` | `sensorid` | 14 | 主键索引 | 17 条固定数据 |
| `CalibrationRecords` | `Id` (GUID) | 38 | 2 个索引 | ⚠️ 表名大写 |

### 关键设计注意事项

| 问题 | 说明 | 影响 |
|------|------|------|
| `operators` 无主键 | 登录查询用 `username + pwd`，不要用 `userid` | 可能插入重复用户 |
| `testmaster` 联合主键 | 查询/更新必须同时提供 `productid` + `testid` | WHERE 需两个条件 |
| `testmaster` 字段分两步写入 | 新建 INSERT 全填 0，完成后 UPDATE 统计值 | 中间状态字段为 0 |
| `testmaster.flag` 完成标记 | `"10000000"` = 已保存，`NULL` = 未保存 | 阻止覆盖未保存记录 |
| `CalibrationRecords.TemperatureData` | JSON 字符串，手动序列化/反序列化 | 不能直接 SQL 查询 |
| `CalibrationRecords` 表名大小写 | 大写"C"开头，与其他表不同 | SQL 中注意区分 |
| CSV 时序数据不入库 | 存在文件系统 `TestData/{pid}/{tid}/sensor_data.csv` | 报告生成需读取 CSV |
| 参数化 SQL | 全部使用 `$param` 占位符 | 防注入、类型安全 |

### 初始数据

```sql
-- 操作员
INSERT INTO operators VALUES ('1', 'admin',        '123456', 'admin');
INSERT INTO operators VALUES ('2', 'experimenter', '123456', 'operator');

-- 设备
INSERT INTO apparatus VALUES (0, 'FURNACE-01', '一号试验炉',
    date('now'), date('now', '+1 year'), 'COM9', 'COM9', 2048);

-- 传感器（17 个通道：0~3 主通道，16 校准通道，4~15 备用通道）
INSERT INTO sensors VALUES (0,  'Sensor0',  '炉温1',    '采集', '℃', '炉温1',    '启用', 0, 0, 0, 1000, 0, 0, 4);
INSERT INTO sensors VALUES (1,  'Sensor1',  '炉温2',    '采集', '℃', '炉温2',    '启用', 0, 0, 0, 1000, 0, 0, 4);
INSERT INTO sensors VALUES (2,  'Sensor2',  '表面温度', '采集', '℃', '表面温度', '启用', 0, 0, 0, 1000, 0, 0, 4);
INSERT INTO sensors VALUES (3,  'Sensor3',  '中心温度', '采集', '℃', '中心温度', '启用', 0, 0, 0, 1000, 0, 0, 4);
-- 4~15: 备用通道
INSERT INTO sensors VALUES (16, 'Sensor16', '校准温度', '校准', '℃', '校准温度', '启用', 0, 0, 0, 1000, 0, 0, 4);
```

---

## 开发指南

### 编译和运行

```bash
# 还原依赖
dotnet restore

# Debug 编译
dotnet build

# Release 编译
dotnet build -c Release

# 运行
dotnet run

# 发布为独立可执行文件（不需要安装 .NET Runtime）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```

### 关键设计原则

1. **分层依赖**：上层依赖下层，下层通过事件通知上层
   ```
   UI → Core → Data → SQLite
   UI 不能直接访问 SQLite（通过 DbHelper）
   Core 不能引用 UI 类型
   ```

2. **跨线程安全**：
   ```csharp
   // DaqWorker 在后台线程触发事件
   // UI 必须在回调中 Invoke 到 UI 线程
   private void OnDataBroadcast(object sender, DataBroadcastEventArgs e)
   {
       if (this.InvokeRequired)
       {
           this.BeginInvoke(() => OnDataBroadcast(sender, e));
           return;
       }
       // 安全操作 UI 控件...
   }
   ```

3. **全局上下文**：
   ```csharp
   // 通过 AppContext.Instance 访问所有核心对象
   AppContext.Instance.Config        // 配置
   AppContext.Instance.Db            // 数据库
   AppContext.Instance.Controller    // 状态机
   AppContext.Instance.DaqWorker     // 数据采集
   AppContext.Instance.CurrentOperator // 当前用户
   ```

4. **参数化 SQL**：
   ```csharp
   // ✅ 正确
   cmd.CommandText = "SELECT * FROM operators WHERE username=$name AND pwd=$pwd";
   cmd.Parameters.AddWithValue("$name", username);

   // ❌ 错误
   cmd.CommandText = $"SELECT * FROM operators WHERE username='{username}'";
   ```

5. **CSV 目录创建**：
   ```csharp
   // 写 CSV 前必须确保目录存在
   var dir = Path.Combine(config.TestDataDirectory, productId, testId);
   if (!Directory.Exists(dir))
       Directory.CreateDirectory(dir);
   ```

### 调试技巧

```bash
# 查看 Serilog 日志
type ISO11820.App\bin\Debug\net8.0-windows\Logs\iso11820-*.log

# 直接查询 SQLite 数据库
sqlite3 ISO11820.App\bin\Debug\net8.0-windows\Data\ISO11820.db
> .tables
> SELECT * FROM testmaster ORDER BY testdate DESC LIMIT 5;
> .quit

# 查看生成的 CSV 数据
type D:\ISO11820\TestData\{ProductId}\{TestId}\sensor_data.csv
```

### 常见修改场景

#### 调整仿真升温速度

编辑 `appsettings.json`：
```json
"Simulation": {
    "HeatingRatePerSecond": 5.0,    // 从 40 改为 5，升温更慢
    "InitialFurnaceTemp": 25.0      // 从 720 改为 25，从室温开始
}
```

#### 修改判定标准

在 `ExportService.cs` 和 `ExperimentRecordDialog.cs` 中：
```csharp
// 修改判定阈值
bool passed = test.DeltaTf <= 30      // 温升阈值
           && lostPer <= 50           // 失重率阈值
           && test.FlameDuration <= 20; // 火焰持续时间阈值
```

#### 启用硬件模式（需连接真实设备）

```json
"Simulation": {
    "EnableSimulation": false,   // 改为 false
}
// 然后在 DaqWorker 中实现真实的 Modbus 读取逻辑
```

---

## 团队分工

本项目按三层架构分工，各层有明确的接口约定：

```
┌─────────────────────────────────────────────────────────────────┐
│  Person C — UI 层 + 导出层                                      │
│  LoginForm / MainForm / 子窗体 / ExportService                  │
│  依赖：Person B 的 DataBroadcast 事件 + Person A 的 DbHelper    │
├─────────────────────────────────────────────────────────────────┤
│  Person B — 核心业务层                                          │
│  TestController / SensorSimulator / DaqWorker                   │
│  依赖：Person A 的 DbHelper + Models + AppContext               │
├─────────────────────────────────────────────────────────────────┤
│  Person A — 基础设施层                                          │
│  Models / DbHelper / AppContext / AppConfig / Serilog           │
│  依赖：无（最底层，最先完成）                                    │
└─────────────────────────────────────────────────────────────────┘
```

### Person A 提供给 Person B 的接口

```csharp
// 全局访问点
AppContext.Instance.Db          // DbHelper 实例
AppContext.Instance.Config       // AppConfig 实例

// 关键 DbHelper 方法
DbHelper.GetSensors()              // → List<Sensor>
DbHelper.UpdateSensorValue(id, v)  // 更新传感器值
DbHelper.InsertTest(test)          // 新建试验记录
DbHelper.GetCurrentTest()          // → TestMaster?
DbHelper.GetApparatus()            // → Apparatus?
```

### Person B 提供给 Person C 的接口

```csharp
// 试验控制
TestController.CurrentState         // TestState 枚举值
TestController.StartHeating()       // Idle → Preparing
TestController.StopHeating()        // Preparing/Ready → Idle
TestController.StartRecording()     // Ready → Recording
TestController.StopRecording()      // Recording → Complete
TestController.SetTestContext(...)  // 设置试验上下文
TestController.OnRecordSaved()      // Complete → Idle
TestController.BuildTestMasterForSave() // → TestMaster?

// 数据广播事件
TestController.DataBroadcast += OnDataBroadcast;
// DataBroadcastEventArgs 携带:
//   TF1, TF2, TS, TC, TCal  → 温度显示
//   ElapsedSeconds           → 计时器
//   StatusText                → 状态标签
//   TemperatureDrift         → 温漂显示
//   CurrentState             → 按钮状态
//   Messages                 → 消息日志
```

### Person C 也需要直接调用 Person A

```csharp
// 登录
DbHelper.Login(username, pwd) → (bool, userid, usertype)

// 试验新建与保存
DbHelper.InsertTest(test)
DbHelper.UpdateTestResult(test)
DbHelper.UpsertProduct(product)
DbHelper.GetApparatus()

// 查询
DbHelper.QueryTests(from, to, productId, operator)
DbHelper.GetAllOperators()

// 校准
DbHelper.InsertCalibrationRecord(record)
DbHelper.QueryCalibrationRecords()
```

### 开发顺序

| 步骤 | 负责人 | 产出 | 依赖 |
|------|--------|------|------|
| 1 | A | Model 类 + appsettings.json + AppConfig | 无 |
| 2 | A | DbHelper（建表 + 初始数据 + 全部 CRUD） | 1 |
| 3 | A | AppContext + Serilog 日志 + Program.cs | 2 |
| 4 | B | TestState 枚举 + 事件参数类 + MasterMessage | 3 |
| 5 | B | SensorSimulator（5通道仿真算法） | 4 |
| 6 | B | TestController（状态机 + 终止判定 + 温漂） | 5 |
| 7 | B | DaqWorker（800ms 定时器 + CSV 写入） | 6 |
| 8 | C | LoginForm | 3 |
| 9 | C | MainForm 框架 + DataBroadcast 订阅 | 7 |
| 10 | C | OxyPlot 曲线 + 按钮状态控制 | 9 |
| 11 | C | NewExperimentDialog + ExperimentRecordDialog | 10 |
| 12 | C | ExportService（Excel + PDF） | 11 |
| 13 | C | 历史查询 Tab + 设备校准 Tab | 12 |

---

## API 参考

### AppContext（全局上下文）

```csharp
public class AppContext
{
    public static AppContext Instance { get; }  // 单例

    public AppConfig Config { get; set; }        // 配置管理器
    public DbHelper Db { get; set; }             // 数据库操作助手
    public string CurrentOperator { get; set; }  // 当前登录用户名
    public string CurrentUserType { get; set; }  // 当前角色 (admin/operator)
    public TestController Controller { get; set; }  // 试验控制器
    public DaqWorker DaqWorker { get; set; }         // 数据采集器
}
```

### TestController（状态机 API）

```csharp
public class TestController
{
    // 状态属性
    public TestState CurrentState { get; }     // 当前状态
    public bool IsStable { get; }              // 温度是否稳定
    public int ElapsedSeconds { get; }         // 记录阶段已过秒数
    public double TemperatureDrift { get; }    // 温度漂移 (°C/10min)

    // 试验上下文
    public string? CurrentProductId { get; }
    public string? CurrentTestId { get; }
    public double PreWeight { get; }
    public double AmbTemp { get; }
    public double AmbHumi { get; }
    public bool IsFixedDuration { get; }
    public int TargetDurationSeconds { get; }
    public int CalculatedConstPower { get; }

    // 温度快照
    public (double tf1, double tf2, double ts, double tc, double tcal) CurrentTemperatures { get; }

    // 状态操作
    public bool StartHeating();                    // Idle → Preparing
    public bool StopHeating();                     // Preparing/Ready → Idle
    public bool StartRecording();                  // Ready → Recording
    public bool StopRecording(bool hasValidSamples = true); // Recording → Complete
    public void OnRecordSaved();                   // Complete → Idle

    // 试验上下文
    public void SetTestContext(string productId, string testId,
        double preWeight, double ambTemp, double ambHumi,
        bool isFixedDuration = false, int targetSeconds = 3600);

    // 查询
    public bool HasUnsavedCompletedTest();         // 存在已完成未保存的试验
    public TestMaster? BuildTestMasterForSave();   // 构建保存用的 TestMaster

    // 内部（由 DaqWorker 调用）
    public void Tick();                            // 每帧（800ms）
    public DataBroadcastEventArgs? SecondTick();   // 每秒

    // 事件
    public event EventHandler<TestState>? StateChanged;
    public event EventHandler<DataBroadcastEventArgs>? DataBroadcast;
}
```

### DbHelper（数据库 API）

```csharp
public class DbHelper
{
    public DbHelper(string dbPath);

    // 初始化
    public void InitializeDatabase();              // 建表 + 初始数据

    // 登录
    public bool Login(string username, string pwd,
        out string userid, out string usertype);

    // 设备
    public Apparatus? GetApparatus();
    public void UpdateApparatus(Apparatus apparatus);

    // 样品
    public void UpsertProduct(ProductMaster product);
    public List<ProductMaster> GetAllProducts();
    public ProductMaster? GetProduct(string productId);

    // 传感器
    public List<Sensor> GetSensors();
    public Sensor? GetSensor(int sensorId);
    public void UpdateSensorValue(int sensorId,
        double outputValue, double inputValue);

    // 试验记录
    public void InsertTest(TestMaster test);
    public void UpdateTestResult(TestMaster test);
    public TestMaster? GetCurrentTest();
    public TestMaster? GetTest(string productId, string testId);
    public List<TestMaster> QueryTests(DateTime from, DateTime to,
        string? productId = null, string? operatorName = null);
    public List<string> GetAllOperators();

    // 校准
    public void InsertCalibrationRecord(CalibrationRecord record);
    public List<CalibrationRecord> QueryCalibrationRecords(
        DateTime? from = null, DateTime? to = null,
        string? operatorName = null);
}
```

### ExportService（导出 API）

```csharp
public static class ExportService
{
    // 导出单个试验为 Excel 报告（3 个 Sheet）
    public static string ExportExcel(TestMaster test);

    // 导出查询结果列表为 Excel
    public static string ExportQueryResult(List<TestMaster> tests);

    // 导出试验报告为 PDF（PdfSharp 直接绘制，嵌入中文字体）
    public static string ExportPdf(TestMaster test);
}
```

### DataBroadcastEventArgs（广播数据）

```csharp
public class DataBroadcastEventArgs : EventArgs
{
    public double TF1 { get; set; }              // 炉温1
    public double TF2 { get; set; }              // 炉温2
    public double TS { get; set; }               // 表面温度
    public double TC { get; set; }               // 中心温度
    public double TCal { get; set; }             // 校准温度
    public int ElapsedSeconds { get; set; }      // 已记录秒数
    public string StatusText { get; set; }       // 状态中文描述
    public TestState CurrentState { get; set; }  // 状态枚举
    public double TemperatureDrift { get; set; } // 温漂 (°C/10min)
    public bool IsStable { get; set; }           // 温度是否稳定
    public List<MasterMessage> Messages { get; set; } // 系统消息
    public string? ProductId { get; set; }       // 样品编号
    public string? TestId { get; set; }          // 试验标识
}
```

---

## 常见问题

### Q: 为什么登录没有用户名输入框？

A: 设计如此。用户名由角色选择自动确定——选择"管理员"即使用 `admin`，选择"试验员"即使用 `experimenter`。这是简化设计，真实使用场景下操作员身份是固定的。

### Q: 如何修改密码？

A: 当前版本密码存储在 `operators` 表的 `pwd` 字段中（明文）。可以通过 SQLite 工具直接修改：
```sql
UPDATE operators SET pwd = '新密码' WHERE username = 'admin';
```

### Q: 如何更改数据存储路径？

A: 编辑 `appsettings.json` 中的 `FileStorage` 和 `Report` 配置节：
```json
"FileStorage": {
    "BaseDirectory": "E:\\MyData\\ISO11820",
    "TestDataDirectory": "E:\\MyData\\ISO11820\\TestData"
},
"Report": {
    "OutputDirectory": "E:\\MyData\\ISO11820\\Reports"
}
```

### Q: 如何让演示更慢/更快？

A: 调整 `HeatingRatePerSecond`（升温速率）和 `InitialFurnaceTemp`（初始炉温）：
- 慢速演示：`"InitialFurnaceTemp": 25.0, "HeatingRatePerSecond": 5.0`
- 快速演示：`"InitialFurnaceTemp": 720.0, "HeatingRatePerSecond": 40.0`

### Q: 能连接真实硬件吗？

A: 当前版本仅支持仿真模式。要连接真实硬件，需要：
1. 将 `EnableSimulation` 设为 `false`
2. 在 `DaqWorker` 中实现真实的 Modbus RTU 串口读取逻辑
3. 安装 `System.IO.Ports` 和 `FluentModbus` NuGet 包

### Q: EPPlus 报告导出报错？

A: EPPlus 7.x 在商业用途需要许可证。开发/学习用途需在代码中设置：
```csharp
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
```
已在 `ExportService` 静态构造中设置。

### Q: PDF 中文显示为方块？

A: PDF 导出依赖 Windows 字体目录下的中文字体文件（`simhei.ttf`）。如果系统缺少中文字体，PDF 中的中文会显示异常。确保 Windows 系统已安装黑体等中文字体。

### Q: 数据库文件在哪里？

A: 默认在编译输出目录的 `Data\ISO11820.db`，例如：
```
ISO11820.App\bin\Debug\net8.0-windows\Data\ISO11820.db
```

### Q: 如何重置数据库？

A: 删除数据库文件后重新运行程序，DbHelper 会自动重建：
```bash
del ISO11820.App\bin\Debug\net8.0-windows\Data\ISO11820.db
dotnet run
```

### Q: 程序崩溃了怎么办？

A: 查看 Serilog 日志文件定位问题：
```bash
type ISO11820.App\bin\Debug\net8.0-windows\Logs\iso11820-*.log
```

---

## 📚 相关文档

| 文档 | 说明 |
|------|------|
| [ISO11820-开发文档.md](ISO11820-开发文档.md) | 完整功能需求、仿真算法、技术设计 |
| [DB-数据库设计.md](DB-数据库设计.md) | SQLite 完整建表 SQL、初始数据、操作示例 |
| [ISO11820-三人分工方案.md](ISO11820-三人分工方案.md) | 三层架构分工、接口约定、开发顺序 |

---

## 📄 许可

本项目仅用于教学和学习目的。

EPPlus 使用非商业许可（`LicenseContext.NonCommercial`）。

---

<p align="center">
  <b>ISO 11820 建筑材料不燃性试验仿真系统</b><br>
  Built with C# / .NET 8 / WinForms / SQLite<br>
  Made for Education 🎓
</p>
