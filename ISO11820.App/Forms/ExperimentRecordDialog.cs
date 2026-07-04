using ISO11820.App.Models;

namespace ISO11820.App.Forms;

/// <summary>
/// 试验记录对话框 — 填写火焰现象、试验后质量等
/// 保存后自动计算失重率、温升，生成报告
/// </summary>
public partial class ExperimentRecordDialog : Form
{
    private readonly TestMaster _test;
    private CheckBox chkFlame;
    private NumericUpDown nudFlameTime, nudFlameDuration, nudPostWeight;
    private TextBox txtMemo;
    private Button btnSave, btnCancel;

    public TestMaster Test => _test;

    public ExperimentRecordDialog(TestMaster test)
    {
        _test = test;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "试验记录 — 填写试验现象";
        this.Size = new Size(500, 480);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.FromArgb(240, 242, 245);
        this.Font = new Font("Microsoft YaHei", 9F);

        int y = 12, rowH = 32, gap = 6;

        // 样品信息（只读）
        var lblProduct = new Label
        {
            Text = $"样品编号：{_test.ProductId}    试验ID：{_test.TestId}",
            Location = new Point(12, y),
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold)
        };
        this.Controls.Add(lblProduct);
        y += rowH;

        var lblPreWeight = new Label
        {
            Text = $"试验前质量：{_test.PreWeight:F2} g",
            Location = new Point(12, y),
            AutoSize = true
        };
        this.Controls.Add(lblPreWeight);
        y += rowH + gap;

        // 火焰现象
        chkFlame = new CheckBox
        {
            Text = "是否出现持续火焰",
            Location = new Point(12, y),
            Size = new Size(160, 24)
        };
        chkFlame.CheckedChanged += (s, e) =>
        {
            nudFlameTime.Enabled = chkFlame.Checked;
            nudFlameDuration.Enabled = chkFlame.Checked;
        };
        this.Controls.Add(chkFlame);

        AddLabel("火焰发生时刻 (秒)：", 180, y - 2, 160);
        nudFlameTime = new NumericUpDown
        {
            Location = new Point(340, y - 2), Size = new Size(80, 24),
            Minimum = 0, Maximum = 9999, Enabled = false
        };
        this.Controls.Add(nudFlameTime);
        y += rowH;

        AddLabel("火焰持续时间 (秒)：", 180, y + 2, 160);
        nudFlameDuration = new NumericUpDown
        {
            Location = new Point(340, y), Size = new Size(80, 24),
            Minimum = 0, Maximum = 9999, Enabled = false
        };
        this.Controls.Add(nudFlameDuration);
        y += rowH + gap;

        // 分隔线
        var separator = new Label
        {
            Text = "",
            Location = new Point(12, y),
            Size = new Size(440, 2),
            BorderStyle = BorderStyle.Fixed3D
        };
        this.Controls.Add(separator);
        y += 10;

        // 试验后质量（必填）
        AddLabel("试验后质量 (g)：", 12, y + 4, 130);
        nudPostWeight = new NumericUpDown
        {
            Location = new Point(145, y), Size = new Size(100, 24),
            Minimum = 0, Maximum = 9999, DecimalPlaces = 2, Value = (decimal)(_test.PreWeight * 0.9)
        };
        this.Controls.Add(nudPostWeight);
        y += rowH + gap;

        // 备注
        AddLabel("备注：", 12, y + 4, 130);
        txtMemo = new TextBox
        {
            Location = new Point(145, y), Size = new Size(300, 60),
            Multiline = true, ScrollBars = ScrollBars.Vertical
        };
        this.Controls.Add(txtMemo);
        y += 72;

        // 自动计算结果预览
        var previewText = $"预览 — 失重率：{(_test.PreWeight - (double)nudPostWeight.Value) / _test.PreWeight * 100:F2}%";
        if (_test.LostWeightPer > 0) previewText += $"    样品温升：{_test.DeltaTs:F2}°C";

        var lblPreview = new Label
        {
            Text = previewText,
            Location = new Point(12, y),
            AutoSize = true,
            ForeColor = Color.Gray
        };
        nudPostWeight.ValueChanged += (s, e) =>
        {
            double postWt = (double)nudPostWeight.Value;
            double lostPer = (_test.PreWeight - postWt) / _test.PreWeight * 100;
            lblPreview.Text = $"预览 — 失重率：{lostPer:F2}%    样品温升：{_test.DeltaTs:F2}°C";
        };
        this.Controls.Add(lblPreview);
        y += rowH + gap;

        // 判定结论预览
        double lostPerPreview = (_test.PreWeight - (double)nudPostWeight.Value) / _test.PreWeight * 100;
        bool passed = _test.DeltaTf <= 30 && lostPerPreview <= 50 && _test.FlameDuration <= 20;
        var lblConclusion = new Label
        {
            Text = $"判定结论：{(passed ? "通过 ✓" : "不通过 ✗")}",
            Location = new Point(12, y),
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold),
            ForeColor = passed ? Color.Green : Color.Red
        };
        nudPostWeight.ValueChanged += (s, e) =>
        {
            double pw = (double)nudPostWeight.Value;
            double lp = (_test.PreWeight - pw) / _test.PreWeight * 100;
            bool p = _test.DeltaTf <= 30 && lp <= 50 && (int)nudFlameDuration.Value <= 20;
            lblConclusion.Text = $"判定结论：{(p ? "通过 ✓" : "不通过 ✗")}";
            lblConclusion.ForeColor = p ? Color.Green : Color.Red;
        };
        chkFlame.CheckedChanged += (s, e) =>
        {
            if (!chkFlame.Checked) nudFlameDuration.Value = 0;
        };
        nudFlameDuration.ValueChanged += (s, e) =>
        {
            double pw = (double)nudPostWeight.Value;
            double lp = (_test.PreWeight - pw) / _test.PreWeight * 100;
            bool p = _test.DeltaTf <= 30 && lp <= 50 && (int)nudFlameDuration.Value <= 20;
            lblConclusion.Text = $"判定结论：{(p ? "通过 ✓" : "不通过 ✗")}";
            lblConclusion.ForeColor = p ? Color.Green : Color.Red;
        };
        this.Controls.Add(lblConclusion);
        y += rowH + gap;

        // 按钮
        btnSave = new Button
        {
            Text = "保存试验记录",
            Location = new Point(130, y),
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
            Location = new Point(280, y),
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

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (nudPostWeight.Value <= 0)
        {
            MessageBox.Show("请输入试验后质量。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            nudPostWeight.Focus();
            return;
        }

        double postWt = (double)nudPostWeight.Value;
        double preWt = _test.PreWeight;
        double lostWt = preWt - (double)nudPostWeight.Value;
        double lostPer = lostWt / preWt * 100;

        _test.PostWeight = postWt;
        _test.LostWeight = lostWt;
        _test.LostWeightPer = lostPer;
        _test.FlameTime = chkFlame.Checked ? (int)nudFlameTime.Value : 0;
        _test.FlameDuration = chkFlame.Checked ? (int)nudFlameDuration.Value : 0;
        _test.PhenoCode = chkFlame.Checked ? $"flame:{_test.FlameTime}s,{_test.FlameDuration}s" : "no-flame";
        _test.Memo = txtMemo.Text.Trim();

        // 判定结论
        bool passed = _test.DeltaTf <= 30 && lostPer <= 50 && _test.FlameDuration <= 20;

        DialogResult = DialogResult.OK;
        Close();
    }

    private Label AddLabel(string text, int x, int y, int w)
    {
        var lbl = new Label { Text = text, Location = new Point(x, y), Size = new Size(w, 20), TextAlign = ContentAlignment.MiddleRight };
        this.Controls.Add(lbl);
        return lbl;
    }
}
