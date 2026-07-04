using ISO11820.App.Core;
using ISO11820.App.Models;
using AppContext = ISO11820.App.Core.AppContext;
using System.Text.Json;

namespace ISO11820.App.Forms;

/// <summary>
/// 校准数据录入对话框 — 9点测温输入，保存到 CalibrationRecords
/// </summary>
public partial class CalibrationDialog : Form
{
    private readonly double _currentCalibTemp;
    private ComboBox cmbType;
    private NumericUpDown[,] tempGrid = new NumericUpDown[3, 3];
    private TextBox txtRemarks;
    private Button btnSave, btnCancel;

    public CalibrationRecord Record { get; private set; } = null!;

    public CalibrationDialog(double currentCalibTemp)
    {
        _currentCalibTemp = currentCalibTemp;
        InitializeComponent();
        AutoFillFromCurrentTemp();
    }

    private void InitializeComponent()
    {
        this.Text = "记录校准数据";
        this.Size = new Size(560, 480);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.FromArgb(240, 242, 245);
        this.Font = new Font("Microsoft YaHei", 9F);

        int y = 12;

        // 校准类型
        var lblType = new Label { Text = "校准类型：", Location = new Point(12, y + 4), AutoSize = true };
        cmbType = new ComboBox
        {
            Location = new Point(90, y),
            Size = new Size(130, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbType.Items.Add("Surface");
        cmbType.Items.Add("Center");
        cmbType.SelectedIndex = 0;
        this.Controls.Add(lblType);
        this.Controls.Add(cmbType);
        y += 36;

        // 当前校准温度
        var lblCurrTemp = new Label
        {
            Text = $"当前校准温度：{_currentCalibTemp:F1} °C",
            Location = new Point(12, y),
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold)
        };
        this.Controls.Add(lblCurrTemp);
        y += 28;

        // 9 点测温网格
        var gbGrid = new GroupBox
        {
            Text = "炉壁测温点 (°C) — A/B/C层 × 1/2/3轴",
            Location = new Point(12, y),
            Size = new Size(520, 175)
        };
        int gridY = 22;
        string[] layers = { "A层 (上)", "B层 (中)", "C层 (下)" };
        for (int row = 0; row < 3; row++)
        {
            var lblLayer = new Label
            {
                Text = layers[row],
                Location = new Point(10, gridY + row * 45 + 6),
                Size = new Size(60, 20),
                TextAlign = ContentAlignment.MiddleRight
            };
            gbGrid.Controls.Add(lblLayer);

            for (int col = 0; col < 3; col++)
            {
                var lbl = new Label
                {
                    Text = $"轴{col + 1}:",
                    Location = new Point(80 + col * 150, gridY + row * 45),
                    Size = new Size(30, 20),
                    TextAlign = ContentAlignment.MiddleRight
                };
                var nud = new NumericUpDown
                {
                    Location = new Point(115 + col * 150, gridY + row * 45 - 1),
                    Size = new Size(80, 24),
                    Minimum = 0,
                    Maximum = 1200,
                    DecimalPlaces = 1,
                    Value = (decimal)_currentCalibTemp
                };
                tempGrid[row, col] = nud;
                gbGrid.Controls.Add(lbl);
                gbGrid.Controls.Add(nud);
            }
        }
        this.Controls.Add(gbGrid);
        y += 190;

        // 备注
        var lblRemarks = new Label { Text = "备注：", Location = new Point(12, y + 4), AutoSize = true };
        txtRemarks = new TextBox
        {
            Location = new Point(70, y),
            Size = new Size(440, 50),
            Multiline = true
        };
        this.Controls.Add(lblRemarks);
        this.Controls.Add(txtRemarks);
        y += 60;

        // 按钮
        btnSave = new Button
        {
            Text = "保存校准记录",
            Location = new Point(160, y),
            Size = new Size(130, 36),
            Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button
        {
            Text = "取消",
            Location = new Point(310, y),
            Size = new Size(100, 36),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = Color.FromArgb(217, 217, 217);
        btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        this.Controls.Add(btnSave);
        this.Controls.Add(btnCancel);
    }

    private void AutoFillFromCurrentTemp()
    {
        // 将当前校准温度预填到 9 个点
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                if (tempGrid[r, c] != null)
                    tempGrid[r, c].Value = (decimal)_currentCalibTemp;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        // 收集 9 点数据
        double a1 = (double)tempGrid[0, 0].Value;
        double a2 = (double)tempGrid[0, 1].Value;
        double a3 = (double)tempGrid[0, 2].Value;
        double b1 = (double)tempGrid[1, 0].Value;
        double b2 = (double)tempGrid[1, 1].Value;
        double b3 = (double)tempGrid[1, 2].Value;
        double c1 = (double)tempGrid[2, 0].Value;
        double c2 = (double)tempGrid[2, 1].Value;
        double c3 = (double)tempGrid[2, 2].Value;

        // 简单计算
        double allSum = a1 + a2 + a3 + b1 + b2 + b3 + c1 + c2 + c3;
        double tAvg = allSum / 9;
        double maxDev = new[] { a1, a2, a3, b1, b2, b3, c1, c2, c3 }
            .Max(v => Math.Abs(v - tAvg));
        bool passed = maxDev <= 10; // 偏差 <= 10°C 判定通过

        // 组装 JSON 温度数据
        var tempData = new { A1 = a1, A2 = a2, A3 = a3, B1 = b1, B2 = b2, B3 = b3, C1 = c1, C2 = c2, C3 = c3 };

        Record = new CalibrationRecord
        {
            Id = Guid.NewGuid().ToString(),
            CalibrationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            CalibrationType = cmbType.SelectedItem?.ToString() ?? "Surface",
            ApparatusId = 0,
            Operator = AppContext.Instance.CurrentOperator,
            TemperatureData = JsonSerializer.Serialize(tempData),
            TempA1 = a1, TempA2 = a2, TempA3 = a3,
            TempB1 = b1, TempB2 = b2, TempB3 = b3,
            TempC1 = c1, TempC2 = c2, TempC3 = c3,
            TAvg = tAvg,
            MaxDeviation = maxDev,
            AverageTemperature = tAvg,
            PassedCriteria = passed ? 1 : 0,
            Remarks = txtRemarks.Text.Trim(),
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
