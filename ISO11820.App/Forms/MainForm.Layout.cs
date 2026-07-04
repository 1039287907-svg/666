using ISO11820.App.Core;
using AppContext = ISO11820.App.Core.AppContext;

namespace ISO11820.App.Forms;

// ====================================================================
// MainForm.Layout.cs — 所有 UI 控件创建与布局
// ====================================================================

public partial class MainForm
{
    private void BuildTabs()
    {
        _tabControl = new TabControl { Dock = DockStyle.Fill };

        _tabExperiment   = new TabPage { Text = "试验操作" };
        _tabQuery        = new TabPage { Text = "记录查询" };
        _tabCalibration  = new TabPage { Text = "设备校准" };

        _tabControl.TabPages.Add(_tabExperiment);
        _tabControl.TabPages.Add(_tabQuery);
        _tabControl.TabPages.Add(_tabCalibration);

        this.Controls.Add(_tabControl);
    }

    // ================================================================
    // Tab 1：试验操作
    // ================================================================

    private void BuildExperimentTab()
    {
        // === SplitContainer：左=图表，右=操作面板（可拖拽 + 可折叠） ===
        _splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 6,
            FixedPanel = FixedPanel.None,
            Panel1MinSize = 600,  // 图表最小宽度
            Panel2MinSize = 30    // 右面板最小（折叠后只剩按钮）
        };

        // ---- 左面板（图表） ----
        var leftPanel = _splitContainer.Panel1;
        leftPanel.Padding = new Padding(4, 0, 0, 4);

        // 操作员信息条 + 折叠按钮
        var infoBar = new Panel { Height = 22, Dock = DockStyle.Top };
        infoBar.Controls.Add(new Label
        {
            Text = $"操作员：{AppContext.Instance.CurrentOperator}（{AppContext.Instance.CurrentUserType}）",
            Location = new Point(2, 2),
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold)
        });
        // 折叠按钮（在 Panel1 里，始终可见）
        _btnTogglePanel = new Button
        {
            Text = "◀",
            Width = 22, Height = 22,
            Location = new Point(950, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.LightGray,
            Cursor = Cursors.Hand
        };
        _btnTogglePanel.Click += (_, _) => ToggleRightPanel();
        infoBar.Controls.Add(_btnTogglePanel);
        infoBar.Layout += (_, _) => { _btnTogglePanel.Left = infoBar.ClientSize.Width - 24; };

        // 温度 LED 面板
        var tempPanel = BuildTemperaturePanel();

        // 图表面板
        var chartContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 4, 2) };
        _chartPanel = new ChartPanel();
        chartContainer.Controls.Add(_chartPanel);

        leftPanel.Controls.Add(chartContainer);
        leftPanel.Controls.Add(tempPanel);
        leftPanel.Controls.Add(infoBar);

        // ---- 右面板（操作）----
        var rightPanel = _splitContainer.Panel2;
        rightPanel.Padding = new Padding(4, 0, 4, 0);

        // 右面板内部
        _rightPanelInner = new Panel
        {
            Dock = DockStyle.Fill
        };

        int iw = 370; // inner width

        // 状态信息
        var grpInfo = new GroupBox
        {
            Text = "试验状态",
            Location = new Point(0, 0),
            Size = new Size(iw, 160)
        };
        AddInfoRow(grpInfo, "当前状态：",    0, out _lblStatus,    "空闲",  14, Color.Gray);
        AddInfoRow(grpInfo, "已记录：",      1, out _lblTimer,     "0 秒",  14, Color.Blue);
        AddInfoRow(grpInfo, "温漂：",        2, out _lblDrift,     "0.00 °C/10min", 12, Color.Black);
        AddInfoRow(grpInfo, "样品编号：",    3, out _lblProductId, "—",     12, Color.DarkBlue);
        _rightPanelInner.Controls.Add(grpInfo);

        // 操作按钮
        var grpButtons = new GroupBox
        {
            Text = "操作",
            Location = new Point(0, 168),
            Size = new Size(iw, 130)
        };
        int bw = 100, bh = 32, g = 8, x0 = 8;
        _btnNewTest        = MakeBtn("新建试验",  x0,                 22, bw, bh);
        _btnStartHeating   = MakeBtn("开始升温",  x0 + bw + g,        22, bw, bh);
        _btnStopHeating    = MakeBtn("停止升温",  x0 + (bw + g) * 2,  22, bw, bh);
        _btnStartRecording = MakeBtn("开始记录",  x0,                 60, bw, bh);
        _btnStopRecording  = MakeBtn("停止记录",  x0 + bw + g,        60, bw, bh);
        _btnSaveRecord     = MakeBtn("保存记录",  x0 + (bw + g) * 2,  60, bw, bh);
        _btnSettings       = MakeBtn("参数设置",  x0,                 98, bw, bh);
        foreach (var b in new[] { _btnNewTest, _btnStartHeating, _btnStopHeating,
                                  _btnStartRecording, _btnStopRecording, _btnSaveRecord,
                                  _btnSettings })
            grpButtons.Controls.Add(b);
        _rightPanelInner.Controls.Add(grpButtons);

        // 系统消息
        var lblMsg = new Label
        {
            Text = "系统消息：",
            Location = new Point(0, 303),
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold)
        };
        _rightPanelInner.Controls.Add(lblMsg);
        _rtbMessages = new RichTextBox
        {
            Location = new Point(0, 325),
            Size = new Size(iw, 200),
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        _rightPanelInner.Controls.Add(_rtbMessages);

        rightPanel.Controls.Add(_rightPanelInner);

        // 初始：右面板约 380px
        _splitContainer.SplitterDistance = Math.Max(600,
            _splitContainer.Width - 390);

        _tabExperiment.Controls.Add(_splitContainer);
    }

    private void ToggleRightPanel()
    {
        _splitContainer.Panel2Collapsed = !_splitContainer.Panel2Collapsed;
        _btnTogglePanel.Text = _splitContainer.Panel2Collapsed ? "▶" : "◀";
    }

    private Panel BuildTemperaturePanel()
    {
        var p = new Panel
        {
            Height = 72,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(20, 20, 20),
            BorderStyle = BorderStyle.FixedSingle
        };

        string[] titles = { "炉温1", "炉温2", "表面温", "中心温", "校准温" };
        var labels = new List<Label>();
        int tx = 6, tw = 150;

        foreach (var title in titles)
        {
            p.Controls.Add(new Label
            {
                Text = title, ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent, Location = new Point(tx, 4),
                Size = new Size(tw - 10, 18), Font = new Font("Microsoft YaHei", 9F)
            });
            var val = new Label
            {
                Text = "0.0 °C", ForeColor = Color.Lime,
                BackColor = Color.Transparent, Location = new Point(tx, 26),
                Size = new Size(tw - 10, 40), Font = new Font("Consolas", 20F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            p.Controls.Add(val);
            labels.Add(val);
            tx += tw;
        }
        // 温度标签颜色
        _lblTF1  = labels[0]; _lblTF1.ForeColor  = Color.FromArgb(255, 77, 79);
        _lblTF2  = labels[1]; _lblTF2.ForeColor  = Color.FromArgb(24, 144, 255);
        _lblTS   = labels[2]; _lblTS.ForeColor   = Color.FromArgb(82, 196, 26);
        _lblTC   = labels[3]; _lblTC.ForeColor   = Color.FromArgb(250, 140, 22);
        _lblTCal = labels[4];

        return p;
    }

    // ================================================================
    // 辅助
    // ================================================================

    private static void AddInfoRow(GroupBox parent, string title, int row,
        out Label valueLabel, string defaultText, int fontSize, Color color)
    {
        int y = row * 32 + 20;
        parent.Controls.Add(new Label
        {
            Text = title,
            Location = new Point(8, y + 3),
            Size = new Size(90, 24),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold)
        });
        valueLabel = new Label
        {
            Text = defaultText,
            Location = new Point(115, y),
            AutoSize = true,
            Font = new Font(fontSize >= 14 ? "Consolas" : "Microsoft YaHei",
                            fontSize, FontStyle.Bold),
            ForeColor = color
        };
        parent.Controls.Add(valueLabel);
    }

    private Button MakeBtn(string text, int x, int y, int w, int h)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(w, h),
            Font = new Font("Microsoft YaHei", 9F),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = Color.FromArgb(217, 217, 217);
        btn.Click += (s, e) => HandleButton(text);
        return btn;
    }
}
