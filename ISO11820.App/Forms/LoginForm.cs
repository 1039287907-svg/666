using ISO11820.App.Core;
using AppContext = ISO11820.App.Core.AppContext;

namespace ISO11820.App.Forms;

public partial class LoginForm : Form
{
    private RadioButton rbAdmin;
    private RadioButton rbExperimenter;
    private TextBox txtPassword;
    private Button btnLogin;
    private Label lblTitle;
    private Label lblPassword;
    private GroupBox gbRole;

    public LoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "ISO 11820 — 建筑材料不燃性试验仿真系统";
        this.Size = new Size(460, 360);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = Color.FromArgb(240, 242, 245);
        this.Font = new Font("Microsoft YaHei", 10F);

        // 标题
        lblTitle = new Label
        {
            Text = "ISO 11820 试验系统",
            Font = new Font("Microsoft YaHei", 22F, FontStyle.Bold),
            ForeColor = Color.FromArgb(24, 144, 255),
            Size = new Size(400, 50),
            Location = new Point(30, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 角色选择
        gbRole = new GroupBox
        {
            Text = "选择角色",
            Size = new Size(320, 70),
            Location = new Point(70, 95),
            BackColor = Color.White
        };

        rbAdmin = new RadioButton
        {
            Text = "管理员",
            Location = new Point(35, 30),
            Size = new Size(110, 24),
            Checked = true,
            BackColor = Color.White
        };

        rbExperimenter = new RadioButton
        {
            Text = "试验员",
            Location = new Point(170, 30),
            Size = new Size(110, 24),
            BackColor = Color.White
        };

        gbRole.Controls.Add(rbAdmin);
        gbRole.Controls.Add(rbExperimenter);

        // 密码
        lblPassword = new Label
        {
            Text = "密码：",
            Size = new Size(60, 30),
            Location = new Point(90, 185),
            TextAlign = ContentAlignment.MiddleRight
        };

        txtPassword = new TextBox
        {
            Size = new Size(200, 30),
            Location = new Point(155, 188),
            PasswordChar = '*',
            BorderStyle = BorderStyle.FixedSingle
        };
        txtPassword.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e);
        };

        // 登录按钮
        btnLogin = new Button
        {
            Text = "登  录",
            Size = new Size(200, 42),
            Location = new Point(130, 240),
            Font = new Font("Microsoft YaHei", 11F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += BtnLogin_Click;

        // 添加到窗体
        this.Controls.Add(lblTitle);
        this.Controls.Add(gbRole);
        this.Controls.Add(lblPassword);
        this.Controls.Add(txtPassword);
        this.Controls.Add(btnLogin);
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        // 根据角色确定用户名
        string username = rbAdmin.Checked ? "admin" : "experimenter";
        string password = txtPassword.Text.Trim();

        if (string.IsNullOrEmpty(password))
        {
            MessageBox.Show("请输入密码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // 调用 DbHelper 验证登录
        bool success = AppContext.Instance.Db.Login(username, password,
            out string userid, out string usertype);

        if (success)
        {
            AppContext.Instance.CurrentOperator = username;
            AppContext.Instance.CurrentUserType = usertype;

            var mainForm = new MainForm();
            mainForm.Show();
            this.Hide();
        }
        else
        {
            MessageBox.Show("密码错误，请重新输入", "登录失败",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtPassword.SelectAll();
            txtPassword.Focus();
        }
    }
}
