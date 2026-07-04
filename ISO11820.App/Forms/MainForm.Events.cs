using ISO11820.App.Core;
using AppContext = ISO11820.App.Core.AppContext;

namespace ISO11820.App.Forms;

// ====================================================================
// MainForm.Events.cs — 按钮事件、数据广播、状态更新
// ====================================================================

public partial class MainForm
{
    // ---- 当前试验上下文 ----
    private string? _currentProductId;
    private string? _currentTestId;

    // ================================================================
    // 按钮路由
    // ================================================================

    private void HandleButton(string text)
    {
        switch (text)
        {
            case "新建试验":   BtnNewTest();       break;
            case "开始升温":   BtnStartHeating();   break;
            case "停止升温":   BtnStopHeating();    break;
            case "开始记录":   BtnStartRecording(); break;
            case "停止记录":   BtnStopRecording();  break;
            case "保存记录":   BtnSaveRecord();     break;
            case "参数设置":   BtnSettings();       break;
        }
        UpdateButtonStates();
    }

    // ================================================================
    // 按钮逻辑
    // ================================================================

    private void BtnNewTest()
    {
        // 处理未保存试验
        var existing = AppContext.Instance.Db.GetCurrentTest();
        if (existing != null && existing.IsFinished && !existing.IsSaved)
        {
            var result = MessageBox.Show(
                "存在已完成但未保存的试验记录。\n\n点「是」现在保存\n点「否」丢弃并新建",
                "未保存的试验记录", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes) return;
            existing.Flag = "10000000";
            AppContext.Instance.Db.UpdateTestResult(existing);
            _controller.OnRecordSaved();
        }

        using var dialog = new NewExperimentDialog();
        if (dialog.ShowDialog() != DialogResult.OK) return;

        // 新建完成后才启动数据采集
        _daqWorker.Start();

        _currentProductId = dialog.ProductId;
        _currentTestId    = dialog.TestId;

        _daqWorker.PrepareCsv(dialog.ProductId, dialog.TestId);
        _controller.SetTestContext(dialog.ProductId, dialog.TestId,
            dialog.PreWeight, dialog.AmbTemp, dialog.AmbHumi,
            dialog.IsFixedDuration, dialog.TargetDurationSeconds);

        _lblProductId.Text = dialog.ProductId;
        _chartPanel.Reset();

        AddMessage($"新建试验：{dialog.ProductId} / {dialog.TestId}", MessageType.Success);
    }

    private void BtnStartHeating()
    {
        if (string.IsNullOrEmpty(_currentProductId))
        {
            MessageBox.Show("请先新建试验。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_controller.StartHeating())
            AddMessage("开始升温", MessageType.Info);
    }

    private void BtnStopHeating()
    {
        if (_controller.StopHeating())
            AddMessage("停止升温，冷却中", MessageType.Info);
    }

    private void BtnStartRecording()
    {
        if (_controller.StartRecording())
        {
            _daqWorker.ResetMaxTracking();
            AddMessage("开始记录", MessageType.Info);
        }
    }

    private void BtnStopRecording()
    {
        if (_controller.StopRecording(_controller.ElapsedSeconds > 0))
        {
            _daqWorker.Stop();
            AddMessage("停止记录", MessageType.Info);
        }
    }

    private void BtnSaveRecord()
    {
        var test = _controller.BuildTestMasterForSave();
        if (test == null)
        {
            MessageBox.Show("没有可保存的试验记录。", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new ExperimentRecordDialog(test);
        if (dialog.ShowDialog() != DialogResult.OK) return;

        AppContext.Instance.Db.UpdateTestResult(dialog.Test);

        try { ExportService.ExportExcel(dialog.Test); AddMessage("Excel 报告已生成", MessageType.Success); }
        catch (Exception ex) { AddMessage($"Excel 导出失败: {ex.Message}", MessageType.Warning); }

        try { ExportService.ExportPdf(dialog.Test); AddMessage("PDF 报告已生成", MessageType.Success); }
        catch (Exception ex) { AddMessage($"PDF 导出失败: {ex.Message}", MessageType.Warning); }

        _controller.OnRecordSaved();
        _currentProductId = null;
        _currentTestId    = null;

        AddMessage("试验记录已保存", MessageType.Success);
        RefreshQueryTab();

        MessageBox.Show("试验记录保存成功。可以继续新建试验。",
            "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ================================================================
    // 数据广播回调（后台线程 → Invoke 到 UI 线程）
    // ================================================================

    private void OnDataBroadcast(object? sender, DataBroadcastEventArgs e)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(() => OnDataBroadcast(sender, e));
            return;
        }

        // 温度面板
        _lblTF1.Text  = $"{e.TF1:F1} °C";
        _lblTF2.Text  = $"{e.TF2:F1} °C";
        _lblTS.Text   = $"{e.TS:F1} °C";
        _lblTC.Text   = $"{e.TC:F1} °C";
        _lblTCal.Text = $"{e.TCal:F1} °C";
        // 炉温1 超范围告警
        _lblTF1.ForeColor = (e.TF1 > 760 || e.TF1 < 740) && e.CurrentState >= TestState.Ready
            ? Color.OrangeRed : Color.FromArgb(255, 77, 79);

        // 状态信息
        _lblTimer.Text     = $"{e.ElapsedSeconds} 秒";
        _lblStatus.Text    = e.StatusText;
        _lblStatus.ForeColor = e.CurrentState switch
        {
            TestState.Idle      => Color.Gray,
            TestState.Preparing => Color.Orange,
            TestState.Ready     => Color.Green,
            TestState.Recording => Color.Blue,
            TestState.Complete  => Color.DarkGreen,
            _                   => Color.Black
        };
        _lblDrift.Text     = $"{e.TemperatureDrift:F2} °C/10min";
        _lblProductId.Text = e.ProductId ?? "—";

        if (_lblCalibTemp != null)
            _lblCalibTemp.Text = $"{e.TCal:F1} °C";

        // 图表
        _chartPanel.AddData(e.TF1, e.TF2, e.TS, e.TC, e.ElapsedSeconds,
            e.CurrentState == TestState.Recording);

        // 系统消息
        foreach (var msg in e.Messages)
        {
            Color c = msg.Type switch
            {
                MessageType.Warning => Color.Yellow,
                MessageType.Success => Color.Lime,
                _                   => Color.White
            };
            AppendMessage(msg.Time, msg.Message, c);
        }

        // 试验完成 → 自动冻结图表
        if (e.CurrentState == TestState.Complete && _prevState != TestState.Complete)
        {
            _daqWorker.Stop();
            AddMessage("试验完成，请点击【保存记录】", MessageType.Warning);
        }
        _prevState = e.CurrentState;

        UpdateButtonStates();
    }

    // ================================================================
    // 按钮状态
    // ================================================================

    private void UpdateButtonStates()
    {
        var state = _controller.CurrentState;
        bool hasUnsaved = _controller.HasUnsavedCompletedTest();

        _btnNewTest.Enabled        = state == TestState.Idle || (state == TestState.Preparing && !hasUnsaved);
        _btnStartHeating.Enabled   = state == TestState.Idle;
        _btnStopHeating.Enabled    = state is TestState.Preparing or TestState.Ready or TestState.Complete;
        _btnStartRecording.Enabled = state == TestState.Ready;
        _btnStopRecording.Enabled  = state == TestState.Recording;
        _btnSaveRecord.Enabled     = state == TestState.Complete || hasUnsaved;
        _btnSettings.Enabled       = state != TestState.Recording;
    }

    private void BtnSettings()
    {
        var cfg = AppContext.Instance.Config;
        var msg = $"当前仿真参数：\n\n" +
                  $"  目标炉温：{cfg.TargetFurnaceTemp} °C\n" +
                  $"  初始炉温：{cfg.InitialFurnaceTemp} °C\n" +
                  $"  升温速率：{cfg.HeatingRatePerSecond} °C/s\n" +
                  $"  稳定阈值：{cfg.StableThreshold} °C\n" +
                  $"  温度波动：±{cfg.TempFluctuation} °C\n" +
                  $"  更新间隔：{cfg.UpdateIntervalMs} ms\n" +
                  $"  恒功率值：{cfg.ConstPower}";
        MessageBox.Show(msg, "仿真参数设置",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ================================================================
    // 消息
    // ================================================================

    private void AddMessage(string msg, MessageType type)
    {
        Color c = type switch
        {
            MessageType.Warning => Color.Yellow,
            MessageType.Success => Color.Lime,
            _                   => Color.White
        };
        AppendMessage(DateTime.Now.ToString("HH:mm:ss"), msg, c);
    }

    private void AppendMessage(string time, string msg, Color color)
    {
        if (_rtbMessages.InvokeRequired)
        {
            _rtbMessages.Invoke(() => AppendMessage(time, msg, color));
            return;
        }
        _rtbMessages.SelectionColor = color;
        _rtbMessages.AppendText($"{time}  {msg}\n");
        _rtbMessages.ScrollToCaret();
    }
}
