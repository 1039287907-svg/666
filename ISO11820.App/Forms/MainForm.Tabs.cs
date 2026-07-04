using ISO11820.App.Models;
using AppContext = ISO11820.App.Core.AppContext;

namespace ISO11820.App.Forms;

// ====================================================================
// MainForm.Tabs.cs — 记录查询 Tab + 设备校准 Tab
// ====================================================================

public partial class MainForm
{
    // ================================================================
    // Tab 2：记录查询
    // ================================================================

    private void BuildQueryTab()
    {
        int y = 12, flw = 75;

        var lblFrom = new Label { Text = "开始日期：", Location = new Point(12, y + 4), Size = new Size(flw, 20), TextAlign = ContentAlignment.MiddleRight };
        _dtpFrom = new DateTimePicker { Location = new Point(90, y), Size = new Size(125, 24), Format = DateTimePickerFormat.Short };

        var lblTo = new Label { Text = "结束日期：", Location = new Point(222, y + 4), Size = new Size(flw, 20), TextAlign = ContentAlignment.MiddleRight };
        _dtpTo = new DateTimePicker { Location = new Point(300, y), Size = new Size(125, 24), Format = DateTimePickerFormat.Short };
        _dtpTo.Value = DateTime.Today;

        var lblPid = new Label { Text = "样品编号：", Location = new Point(432, y + 4), Size = new Size(flw, 20), TextAlign = ContentAlignment.MiddleRight };
        _txtSearchProduct = new TextBox { Location = new Point(510, y), Size = new Size(125, 24) };

        var lblOp = new Label { Text = "操作员：", Location = new Point(642, y + 4), Size = new Size(flw, 20), TextAlign = ContentAlignment.MiddleRight };
        _cmbOperatorFilter = new ComboBox { Location = new Point(720, y), Size = new Size(95, 24), DropDownStyle = ComboBoxStyle.DropDownList };
        try
        {
            var ops = AppContext.Instance.Db.GetAllOperators();
            _cmbOperatorFilter.Items.Add("(全部)");
            foreach (var op in ops) _cmbOperatorFilter.Items.Add(op);
            _cmbOperatorFilter.SelectedIndex = 0;
        }
        catch { _cmbOperatorFilter.Items.Add("(全部)"); _cmbOperatorFilter.SelectedIndex = 0; }

        _btnSearch = new Button { Text = "查询", Location = new Point(825, y - 1), Size = new Size(75, 28) };
        _btnSearch.Click += (_, _) => RefreshQueryTab();

        _btnExportQuery = new Button { Text = "导出Excel", Location = new Point(908, y - 1), Size = new Size(95, 28) };
        _btnExportQuery.Click += (_, _) => ExportQuery();

        foreach (var c in new Control[] { lblFrom, _dtpFrom, lblTo, _dtpTo, lblPid, _txtSearchProduct, lblOp, _cmbOperatorFilter, _btnSearch, _btnExportQuery })
            _tabQuery.Controls.Add(c);

        _dgvTests = new DataGridView
        {
            Location = new Point(12, 48),
            Size = new Size(1200, 600),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            Font = new Font("Microsoft YaHei", 9F),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        _dgvTests.DoubleClick += (_, _) =>
        {
            if (_dgvTests.SelectedRows.Count == 0) return;
            var pid = _dgvTests.SelectedRows[0].Cells["ProductId"].Value?.ToString();
            var tid = _dgvTests.SelectedRows[0].Cells["TestId"].Value?.ToString();
            if (pid == null || tid == null) return;
            var t = AppContext.Instance.Db.GetTest(pid, tid);
            if (t == null) return;
            MessageBox.Show(
                $"试验ID: {t.TestId}\n样品编号: {t.ProductId}\n日期: {t.TestDate:yyyy-MM-dd}\n" +
                $"操作员: {t.Operator}\n前质量: {t.PreWeight:F2}g  后质量: {t.PostWeight:F2}g\n" +
                $"失重率: {t.LostWeightPer:F2}%  样品温升: {t.DeltaTf:F2}°C\n" +
                $"总时长: {t.TotalTestTime}s  火焰: {t.FlameDuration}s\n" +
                $"备注: {t.Memo ?? "无"}  状态: {(t.IsSaved ? "已保存" : "未保存")}",
                "试验详情", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        _tabQuery.Controls.Add(_dgvTests);

        RefreshQueryTab();
    }

    private void RefreshQueryTab()
    {
        try
        {
            var from = _dtpFrom.Value.Date;
            var to   = _dtpTo.Value.Date.AddDays(1).AddSeconds(-1);
            var pid  = _txtSearchProduct.Text.Trim();
            var op   = _cmbOperatorFilter.SelectedIndex > 0 ? _cmbOperatorFilter.Text : null;
            var tests = AppContext.Instance.Db.QueryTests(from, to, pid, op);

            _dgvTests.DataSource = null;
            _dgvTests.Columns.Clear();
            _dgvTests.DataSource = tests.Select(t => new
            {
                t.TestId,
                t.ProductId,
                TestDate = t.TestDate.ToString("yyyy-MM-dd"),
                t.Operator,
                t.TotalTestTime,
                PreWt  = $"{t.PreWeight:F2}",
                PostWt = $"{t.PostWeight:F2}",
                Lost   = $"{t.LostWeightPer:F2}%",
                Delta  = $"{t.DeltaTf:F2}°C",
                Saved  = t.IsSaved ? "已保存" : "未保存"
            }).ToList();

            if (_dgvTests.Columns.Count > 0)
            {
                _dgvTests.Columns["TestId"].HeaderText  = "试验ID";
                _dgvTests.Columns["ProductId"].HeaderText = "样品编号";
                _dgvTests.Columns["TestDate"].HeaderText = "日期";
                _dgvTests.Columns["Operator"].HeaderText = "操作员";
                _dgvTests.Columns["TotalTestTime"].HeaderText = "时长(s)";
                _dgvTests.Columns["PreWt"].HeaderText  = "前质量(g)";
                _dgvTests.Columns["PostWt"].HeaderText = "后质量(g)";
                _dgvTests.Columns["Saved"].HeaderText  = "状态";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Query] {ex.Message}");
        }
    }

    private void ExportQuery()
    {
        if (_dgvTests.Rows.Count == 0) { MessageBox.Show("没有可导出的数据。"); return; }
        try
        {
            var from = _dtpFrom.Value.Date;
            var to   = _dtpTo.Value.Date.AddDays(1).AddSeconds(-1);
            var pid  = _txtSearchProduct.Text.Trim();
            var op   = _cmbOperatorFilter.SelectedIndex > 0 ? _cmbOperatorFilter.Text : null;
            var tests = AppContext.Instance.Db.QueryTests(from, to, pid, op);
            var path = ExportService.ExportQueryResult(tests);
            MessageBox.Show($"导出成功。\n{path}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"导出失败: {ex.Message}"); }
    }

    // ================================================================
    // Tab 3：设备校准
    // ================================================================

    private void BuildCalibrationTab()
    {
        int y = 12;

        _lblCalibTemp = new Label
        {
            Text = "0.0 °C",
            Location = new Point(140, y),
            AutoSize = true,
            Font = new Font("Consolas", 18F, FontStyle.Bold),
            ForeColor = Color.DarkGreen
        };
        _tabCalibration.Controls.Add(new Label
        {
            Text = "当前校准温度：",
            Location = new Point(12, y + 4),
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 11F, FontStyle.Bold)
        });
        _tabCalibration.Controls.Add(_lblCalibTemp);

        var btnRecord = new Button
        {
            Text = "记录当前校准数据",
            Location = new Point(12, y + 38),
            Size = new Size(160, 32)
        };
        btnRecord.Click += (_, _) =>
        {
            var calTemp = _controller.CurrentTemperatures.tcal;
            using var dlg = new CalibrationDialog(calTemp);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                AppContext.Instance.Db.InsertCalibrationRecord(dlg.Record);
                RefreshCalibrationTab();
                MessageBox.Show("校准记录已保存。");
            }
        };
        _tabCalibration.Controls.Add(btnRecord);

        _tabCalibration.Controls.Add(new Label
        {
            Text = "校准历史记录：",
            Location = new Point(12, y + 82),
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold)
        });

        _dgvCalibration = new DataGridView
        {
            Location = new Point(12, y + 108),
            Size = new Size(1200, 500),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Microsoft YaHei", 9F),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        _dgvCalibration.DoubleClick += (_, _) =>
        {
            if (_dgvCalibration.SelectedRows.Count == 0) return;
            var id = _dgvCalibration.SelectedRows[0].Cells["Id"].Value?.ToString();
            if (id == null) return;
            var r = AppContext.Instance.Db.QueryCalibrationRecords().FirstOrDefault(x => x.Id == id);
            if (r == null) return;
            MessageBox.Show(
                $"校准ID: {r.Id}\n日期: {r.CalibrationDate}\n类型: {r.CalibrationType}\n" +
                $"操作员: {r.Operator}\n通过: {(r.PassedCriteria == 1 ? "是" : "否")}\n备注: {r.Remarks}",
                "校准详情");
        };
        _tabCalibration.Controls.Add(_dgvCalibration);

        RefreshCalibrationTab();
    }

    private void RefreshCalibrationTab()
    {
        try
        {
            var records = AppContext.Instance.Db.QueryCalibrationRecords();
            _dgvCalibration.DataSource = null;
            _dgvCalibration.Columns.Clear();
            _dgvCalibration.DataSource = records.Select(r => new
            {
                r.Id,
                r.CalibrationDate,
                r.CalibrationType,
                r.Operator,
                r.AverageTemperature,
                r.MaxDeviation,
                Passed = r.PassedCriteria == 1 ? "通过" : "未通过",
                r.Remarks
            }).ToList();

            if (_dgvCalibration.Columns.Count > 0)
            {
                _dgvCalibration.Columns["Id"].HeaderText              = "ID";
                _dgvCalibration.Columns["CalibrationDate"].HeaderText = "日期";
                _dgvCalibration.Columns["CalibrationType"].HeaderText = "类型";
                _dgvCalibration.Columns["Operator"].HeaderText        = "操作员";
                _dgvCalibration.Columns["AverageTemperature"].HeaderText = "平均温度";
                _dgvCalibration.Columns["MaxDeviation"].HeaderText    = "最大偏差";
                _dgvCalibration.Columns["Passed"].HeaderText          = "判定";
                _dgvCalibration.Columns["Remarks"].HeaderText         = "备注";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Calib] {ex.Message}");
        }
    }
}
