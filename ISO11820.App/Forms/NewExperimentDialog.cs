using ISO11820.App.Core;
using ISO11820.App.Models;
using AppContext = ISO11820.App.Core.AppContext;

namespace ISO11820.App.Forms;

/// <summary>
/// 新建试验对话框
/// </summary>
public partial class NewExperimentDialog : Form
{
    private TextBox txtProductId, txtProductName, txtSpecific;
    private NumericUpDown nudHeight, nudDiameter, nudPreWeight, nudAmbTemp, nudAmbHumi;
    private NumericUpDown nudCustomDuration;
    private Label lblOperator, lblApparatus, lblApparatusChkDate;
    private RadioButton rbStandard, rbCustom;
    private Button btnCreate, btnCancel;

    public string ProductId => txtProductId.Text.Trim();
    public string TestId { get; private set; } = string.Empty;
    public string SampleName => txtProductName.Text.Trim();
    public string Specification => txtSpecific.Text.Trim();
    public double SampleHeight => (double)nudHeight.Value;
    public double Diameter => (double)nudDiameter.Value;
    public double PreWeight => (double)nudPreWeight.Value;
    public double AmbTemp => (double)nudAmbTemp.Value;
    public double AmbHumi => (double)nudAmbHumi.Value;
    public bool IsFixedDuration => rbCustom.Checked;
    public int TargetDurationSeconds => (int)nudCustomDuration.Value * 60;

    public NewExperimentDialog()
    {
        TestId = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        InitializeComponent();
        LoadAutoFillData();
    }

    private void InitializeComponent()
    {
        this.Text = "新建试验";
        this.Size = new Size(520, 520);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.FromArgb(240, 242, 245);
        this.Font = new Font("Microsoft YaHei", 9F);

        int y = 12, rowH = 30, gap = 6;
        int lblW = 110, ctrlW = 200;

        // 样品编号
        AddLabel("样品编号：", 12, y + 4, lblW);
        txtProductId = AddTextBox(130, y, ctrlW);
        // 试验ID（自动生成）
        AddLabel("试验标识：", 340, y + 4, lblW);
        var lblTestId = new Label
        {
            Text = TestId,
            Location = new Point(460, y + 4),
            AutoSize = true,
            Font = new Font("Consolas", 9F, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        };
        this.Controls.Add(lblTestId);
        y += rowH + gap;

        // 样品名称
        AddLabel("样品名称：", 12, y + 4, lblW);
        txtProductName = AddTextBox(130, y, ctrlW);
        y += rowH + gap;

        // 规格型号
        AddLabel("规格型号：", 12, y + 4, lblW);
        txtSpecific = AddTextBox(130, y, ctrlW);
        y += rowH + gap;

        // 尺寸
        AddLabel("高度 (mm)：", 12, y + 4, lblW);
        nudHeight = new NumericUpDown
        {
            Location = new Point(130, y), Size = new Size(90, 24),
            Minimum = 0, Maximum = 500, DecimalPlaces = 1, Value = 50
        };
        AddLabel("直径 (mm)：", 240, y + 4, lblW);
        nudDiameter = new NumericUpDown
        {
            Location = new Point(355, y), Size = new Size(90, 24),
            Minimum = 0, Maximum = 500, DecimalPlaces = 1, Value = 45
        };
        this.Controls.Add(nudHeight);
        this.Controls.Add(nudDiameter);
        y += rowH + gap;

        // 试验前质量
        AddLabel("试验前质量 (g)：", 12, y + 4, lblW);
        nudPreWeight = new NumericUpDown
        {
            Location = new Point(130, y), Size = new Size(90, 24),
            Minimum = 0, Maximum = 9999, DecimalPlaces = 2, Value = 50
        };
        this.Controls.Add(nudPreWeight);
        y += rowH + gap;

        // 环境信息
        AddLabel("环境温度 (°C)：", 12, y + 4, lblW);
        nudAmbTemp = new NumericUpDown
        {
            Location = new Point(130, y), Size = new Size(90, 24),
            Minimum = -20, Maximum = 60, DecimalPlaces = 1, Value = 25
        };
        AddLabel("环境湿度 (%)：", 240, y + 4, lblW);
        nudAmbHumi = new NumericUpDown
        {
            Location = new Point(355, y), Size = new Size(90, 24),
            Minimum = 0, Maximum = 100, DecimalPlaces = 1, Value = 50
        };
        this.Controls.Add(nudAmbTemp);
        this.Controls.Add(nudAmbHumi);
        y += rowH + gap;

        // 操作员（自动填入）
        AddLabel("操作员：", 12, y + 4, lblW);
        lblOperator = new Label
        {
            Text = AppContext.Instance.CurrentOperator,
            Location = new Point(130, y + 4),
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold)
        };
        this.Controls.Add(lblOperator);
        y += rowH + gap;

        // 试验时长模式
        var gbDuration = new GroupBox
        {
            Text = "试验时长模式",
            Location = new Point(12, y),
            Size = new Size(340, 70)
        };
        rbStandard = new RadioButton
        {
            Text = "标准 60 分钟", Location = new Point(15, 25), Size = new Size(130, 24), Checked = true
        };
        rbCustom = new RadioButton
        {
            Text = "自定义时长 (分钟)：", Location = new Point(15, 48), Size = new Size(140, 24)
        };
        nudCustomDuration = new NumericUpDown
        {
            Location = new Point(165, 46), Size = new Size(70, 24),
            Minimum = 1, Maximum = 360, Value = 30, Enabled = false
        };
        rbCustom.CheckedChanged += (s, e) => nudCustomDuration.Enabled = rbCustom.Checked;
        gbDuration.Controls.Add(rbStandard);
        gbDuration.Controls.Add(rbCustom);
        gbDuration.Controls.Add(nudCustomDuration);
        this.Controls.Add(gbDuration);
        y += 80;

        // 设备信息（自动填入）
        var gbApp = new GroupBox
        {
            Text = "设备信息",
            Location = new Point(12, y),
            Size = new Size(480, 70)
        };
        lblApparatus = new Label
        {
            Text = "设备：—", Location = new Point(15, 25), AutoSize = true
        };
        lblApparatusChkDate = new Label
        {
            Text = "检定日期：—", Location = new Point(240, 25), AutoSize = true
        };
        gbApp.Controls.Add(lblApparatus);
        gbApp.Controls.Add(lblApparatusChkDate);
        this.Controls.Add(gbApp);
        y += 85;

        // 按钮
        btnCreate = new Button
        {
            Text = "创建试验",
            Location = new Point(130, y),
            Size = new Size(120, 36),
            Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnCreate.FlatAppearance.BorderSize = 0;
        btnCreate.Click += BtnCreate_Click;

        btnCancel = new Button
        {
            Text = "取消",
            Location = new Point(270, y),
            Size = new Size(100, 36),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = Color.FromArgb(217, 217, 217);
        btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        this.Controls.Add(btnCreate);
        this.Controls.Add(btnCancel);
    }

    private void LoadAutoFillData()
    {
        try
        {
            var apparatus = AppContext.Instance.Db.GetApparatus();
            if (apparatus != null)
            {
                lblApparatus.Text = $"设备：{apparatus.ApparatusName} ({apparatus.InnerNumber})";
                lblApparatusChkDate.Text = $"检定日期：{apparatus.CheckDateF:yyyy-MM-dd} ~ {apparatus.CheckDateT:yyyy-MM-dd}";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NewExperimentDialog] LoadAutoFillData: {ex.Message}");
            lblApparatus.Text = "设备：数据库读取失败";
        }
    }

    private void BtnCreate_Click(object? sender, EventArgs e)
    {
        // 验证
        if (string.IsNullOrWhiteSpace(txtProductId.Text))
        {
            MessageBox.Show("请输入样品编号。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtProductId.Focus();
            return;
        }
        if (string.IsNullOrWhiteSpace(txtProductName.Text))
        {
            MessageBox.Show("请输入样品名称。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtProductName.Focus();
            return;
        }
        if (nudPreWeight.Value <= 0)
        {
            MessageBox.Show("试验前质量必须大于 0。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            nudPreWeight.Focus();
            return;
        }

        // 保存样品信息到 productmaster
        var apparatus = AppContext.Instance.Db.GetApparatus();
        var product = new ProductMaster
        {
            ProductId = txtProductId.Text.Trim(),
            ProductName = txtProductName.Text.Trim(),
            Specific = txtSpecific.Text.Trim(),
            Diameter = (double)nudDiameter.Value,
            Height = (double)nudHeight.Value
        };
        AppContext.Instance.Db.UpsertProduct(product);

        // 创建试验记录到 testmaster
        var test = new TestMaster
        {
            ProductId = txtProductId.Text.Trim(),
            TestId = TestId,
            TestDate = DateTime.Today,
            AmbTemp = (double)nudAmbTemp.Value,
            AmbHumi = (double)nudAmbHumi.Value,
            According = "ISO 11820:2022",
            Operator = AppContext.Instance.CurrentOperator,
            ApparatusId = apparatus?.InnerNumber ?? "FURNACE-01",
            ApparatusName = apparatus?.ApparatusName ?? "一号试验炉",
            ApparatusChkDate = apparatus?.CheckDateF ?? DateTime.Today,
            RptNo = txtProductId.Text.Trim(),
            PreWeight = (double)nudPreWeight.Value
        };
        AppContext.Instance.Db.InsertTest(test);

        DialogResult = DialogResult.OK;
        Close();
    }

    private Label AddLabel(string text, int x, int y, int w)
    {
        var lbl = new Label { Text = text, Location = new Point(x, y), Size = new Size(w, 20), TextAlign = ContentAlignment.MiddleRight };
        this.Controls.Add(lbl);
        return lbl;
    }

    private TextBox AddTextBox(int x, int y, int w)
    {
        var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 24) };
        this.Controls.Add(tb);
        return tb;
    }
}
