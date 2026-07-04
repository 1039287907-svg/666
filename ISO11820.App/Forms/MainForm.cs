using ISO11820.App.Core;
using AppContext = ISO11820.App.Core.AppContext;

namespace ISO11820.App.Forms;

// ====================================================================
// MainForm.cs — 核心字段、构造、生命周期
// ====================================================================

public partial class MainForm : Form
{
    // ---- 核心依赖 ----
    private readonly TestController _controller = AppContext.Instance.Controller;
    private readonly DaqWorker _daqWorker = AppContext.Instance.DaqWorker;

    // ---- Tab 控件 ----
    private TabControl _tabControl;
    private TabPage _tabExperiment;
    private TabPage _tabQuery;
    private TabPage _tabCalibration;

    // ---- 图表（独立封装） ----
    private ChartPanel _chartPanel;

    // ---- 按钮 ----
    private Button _btnNewTest;
    private Button _btnStartHeating;
    private Button _btnStopHeating;
    private Button _btnStartRecording;
    private Button _btnStopRecording;
    private Button _btnSaveRecord;
    private Button _btnSettings;

    // ---- 显示标签 ----
    private Label _lblStatus;
    private Label _lblTimer;
    private Label _lblDrift;
    private Label _lblProductId;
    private Label _lblTF1, _lblTF2, _lblTS, _lblTC, _lblTCal;

    // ---- 系统消息 ----
    private RichTextBox _rtbMessages;

    // ---- 可折叠右侧面板 ----
    private SplitContainer _splitContainer;
    private Panel _rightPanelInner;
    private Button _btnTogglePanel;

    // ---- 状态追踪 ----
    private TestState _prevState;

    // ---- 查询 Tab ----
    private DataGridView _dgvTests;
    private DateTimePicker _dtpFrom, _dtpTo;
    private TextBox _txtSearchProduct;
    private Button _btnSearch, _btnExportQuery;
    private ComboBox _cmbOperatorFilter;

    // ---- 校准 Tab ----
    private DataGridView _dgvCalibration;
    private Label _lblCalibTemp;

    // ================================================================
    // 构造
    // ================================================================

    public MainForm()
    {
        this.Text = "ISO 11820 — 建筑材料不燃性试验仿真系统";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Microsoft YaHei", 9F);
        this.FormClosing += OnFormClosing;

        BuildTabs();
        BuildExperimentTab();
        BuildQueryTab();
        BuildCalibrationTab();

        _daqWorker.DataBroadcast += OnDataBroadcast;
        UpdateButtonStates();
    }

    // ================================================================
    // 生命周期
    // ================================================================

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        _daqWorker.Stop();
        Application.Exit();
    }
}
