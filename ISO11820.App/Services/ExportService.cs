using ISO11820.App.Config;
using ISO11820.App.Core;
using ISO11820.App.Models;
using AppContext = ISO11820.App.Core.AppContext;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace ISO11820.App.Forms;

/// <summary>
/// 试验报告导出服务：Excel + PDF
/// </summary>
public static class ExportService
{
    static ExportService()
    {
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
    }

    private static AppConfig Config => AppContext.Instance.Config;

    // ================================================================
    // Excel 导出
    // ================================================================

    /// <summary>
    /// 导出单个试验为 Excel 报告
    /// </summary>
    public static string ExportExcel(TestMaster test)
    {
        var dir = Config.OutputDirectory;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, $"{test.TestId}_报告.xlsx");

        using var package = new ExcelPackage();
        bool passed = test.DeltaTf <= 30 && test.LostWeightPer <= 50 && test.FlameDuration <= 20;

        // ===== Sheet 1: 试验信息 =====
        var sheet1 = package.Workbook.Worksheets.Add("试验信息");
        sheet1.Cells["A1"].Value = "ISO 11820 建筑材料不燃性试验报告";
        sheet1.Cells["A1:H1"].Merge = true;
        sheet1.Cells["A1"].Style.Font.Size = 16;
        sheet1.Cells["A1"].Style.Font.Bold = true;
        sheet1.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        sheet1.Cells["A2"].Value = $"报告生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        int row = 4;
        WriteInfoRow(sheet1, ref row, "样品编号", test.ProductId);
        WriteInfoRow(sheet1, ref row, "试验ID", test.TestId);
        WriteInfoRow(sheet1, ref row, "试验日期", test.TestDate.ToString("yyyy-MM-dd"));
        WriteInfoRow(sheet1, ref row, "操作员", test.Operator);
        WriteInfoRow(sheet1, ref row, "设备", $"{test.ApparatusName} ({test.ApparatusId})");
        WriteInfoRow(sheet1, ref row, "环境温度", $"{test.AmbTemp:F1} °C");
        WriteInfoRow(sheet1, ref row, "环境湿度", $"{test.AmbHumi:F1} %");

        row++;
        WriteInfoRow(sheet1, ref row, "试验前质量", $"{test.PreWeight:F2} g");
        WriteInfoRow(sheet1, ref row, "试验后质量", $"{test.PostWeight:F2} g");
        WriteInfoRow(sheet1, ref row, "失重量", $"{test.LostWeight:F2} g");
        WriteInfoRow(sheet1, ref row, "失重率", $"{test.LostWeightPer:F2} %");
        WriteInfoRow(sheet1, ref row, "总试验时长", $"{test.TotalTestTime} 秒");
        WriteInfoRow(sheet1, ref row, "火焰持续时间", $"{test.FlameDuration} 秒");

        row++;
        WriteInfoRow(sheet1, ref row, "炉温1温升", $"{test.DeltaTf1:F2} °C");
        WriteInfoRow(sheet1, ref row, "炉温2温升", $"{test.DeltaTf2:F2} °C");
        WriteInfoRow(sheet1, ref row, "表面温升", $"{test.DeltaTs:F2} °C");
        WriteInfoRow(sheet1, ref row, "中心温升", $"{test.DeltaTc:F2} °C");
        WriteInfoRow(sheet1, ref row, "样品温升 (ΔTf)", $"{test.DeltaTf:F2} °C");
        WriteInfoRow(sheet1, ref row, "恒功率值", test.ConstPower.ToString());

        row += 2;
        sheet1.Cells[$"A{row}"].Value = $"判定结论：{(passed ? "通过" : "不通过")}";
        sheet1.Cells[$"A{row}"].Style.Font.Size = 14;
        sheet1.Cells[$"A{row}"].Style.Font.Bold = true;
        sheet1.Cells[$"A{row}"].Style.Font.Color.SetColor(passed
            ? System.Drawing.Color.Green : System.Drawing.Color.Red);

        sheet1.Columns[1].Width = 22;
        sheet1.Columns[2].Width = 30;

        // ===== Sheet 2: 温度数据 =====
        var sheet2 = package.Workbook.Worksheets.Add("温度数据");
        sheet2.Cells["A1"].Value = "时间(秒)";
        sheet2.Cells["B1"].Value = "炉温1(°C)";
        sheet2.Cells["C1"].Value = "炉温2(°C)";
        sheet2.Cells["D1"].Value = "表面温(°C)";
        sheet2.Cells["E1"].Value = "中心温(°C)";
        sheet2.Cells["F1"].Value = "校准温(°C)";

        // 读取 CSV 数据
        var csvPath = Path.Combine(Config.TestDataDirectory, test.ProductId, test.TestId, "sensor_data.csv");
        if (File.Exists(csvPath))
        {
            var lines = File.ReadAllLines(csvPath);
            for (int i = 1; i < lines.Length; i++) // 跳过表头
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 6)
                {
                    sheet2.Cells[$"A{i + 1}"].Value = int.Parse(parts[0]);
                    sheet2.Cells[$"B{i + 1}"].Value = double.Parse(parts[1]);
                    sheet2.Cells[$"C{i + 1}"].Value = double.Parse(parts[2]);
                    sheet2.Cells[$"D{i + 1}"].Value = double.Parse(parts[3]);
                    sheet2.Cells[$"E{i + 1}"].Value = double.Parse(parts[4]);
                    sheet2.Cells[$"F{i + 1}"].Value = double.Parse(parts[5]);
                }
            }
        }

        // ===== Sheet 3: 温度曲线图 =====
        var sheet3 = package.Workbook.Worksheets.Add("温度曲线");
        if (File.Exists(csvPath))
        {
            var lines = File.ReadAllLines(csvPath);
            // 将数据写入 sheet3（图表需要数据源）
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (i == 0)
                {
                    sheet3.Cells["A1"].Value = parts[0];
                    sheet3.Cells["B1"].Value = parts[1];
                    sheet3.Cells["C1"].Value = parts[2];
                    sheet3.Cells["D1"].Value = parts[3];
                    sheet3.Cells["E1"].Value = parts[4];
                }
                else if (parts.Length >= 6)
                {
                    sheet3.Cells[$"A{i + 1}"].Value = int.Parse(parts[0]);
                    sheet3.Cells[$"B{i + 1}"].Value = double.Parse(parts[1]);
                    sheet3.Cells[$"C{i + 1}"].Value = double.Parse(parts[2]);
                    sheet3.Cells[$"D{i + 1}"].Value = double.Parse(parts[3]);
                    sheet3.Cells[$"E{i + 1}"].Value = double.Parse(parts[4]);
                }
            }

            int dataRows = lines.Length;
            if (dataRows > 1)
            {
                // 添加折线图
                var chart = sheet3.Drawings.AddChart("TempChart", eChartType.Line);
                chart.SetSize(900, 500);
                chart.SetPosition(2, 0, 0, 0);

                var series1 = (ExcelLineChartSerie)chart.Series.Add(
                    sheet3.Cells[$"B2:B{dataRows}"], sheet3.Cells[$"A2:A{dataRows}"]);
                series1.Header = "炉温1";

                var series2 = (ExcelLineChartSerie)chart.Series.Add(
                    sheet3.Cells[$"C2:C{dataRows}"], sheet3.Cells[$"A2:A{dataRows}"]);
                series2.Header = "炉温2";

                var series3 = (ExcelLineChartSerie)chart.Series.Add(
                    sheet3.Cells[$"D2:D{dataRows}"], sheet3.Cells[$"A2:A{dataRows}"]);
                series3.Header = "表面温";

                var series4 = (ExcelLineChartSerie)chart.Series.Add(
                    sheet3.Cells[$"E2:E{dataRows}"], sheet3.Cells[$"A2:A{dataRows}"]);
                series4.Header = "中心温";

                chart.Title.Text = "温度曲线";
                chart.XAxis.Title.Text = "时间 (秒)";
                chart.YAxis.Title.Text = "温度 (°C)";
                chart.YAxis.MaxValue = 800;
                chart.YAxis.MinValue = 0;
            }
        }

        package.SaveAs(new FileInfo(path));
        return path;
    }

    private static void WriteInfoRow(ExcelWorksheet sheet, ref int row, string label, string value)
    {
        sheet.Cells[$"A{row}"].Value = label;
        sheet.Cells[$"A{row}"].Style.Font.Bold = true;
        sheet.Cells[$"B{row}"].Value = value;
        row++;
    }

    /// <summary>
    /// 导出查询结果为 Excel
    /// </summary>
    public static string ExportQueryResult(List<TestMaster> tests)
    {
        var dir = Config.OutputDirectory;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, $"查询结果_{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");

        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("试验列表");

        // 表头
        var headers = new[] { "试验ID", "样品编号", "试验日期", "操作员", "时长(秒)",
            "前质量(g)", "后质量(g)", "失重率(%)", "温升(°C)", "火焰(秒)", "判定" };
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cells[1, i + 1].Value = headers[i];
            sheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        // 数据行
        for (int i = 0; i < tests.Count; i++)
        {
            var t = tests[i];
            bool passed = t.DeltaTf <= 30 && t.LostWeightPer <= 50 && t.FlameDuration <= 20;
            sheet.Cells[i + 2, 1].Value = t.TestId;
            sheet.Cells[i + 2, 2].Value = t.ProductId;
            sheet.Cells[i + 2, 3].Value = t.TestDate.ToString("yyyy-MM-dd");
            sheet.Cells[i + 2, 4].Value = t.Operator;
            sheet.Cells[i + 2, 5].Value = t.TotalTestTime;
            sheet.Cells[i + 2, 6].Value = t.PreWeight;
            sheet.Cells[i + 2, 7].Value = t.PostWeight;
            sheet.Cells[i + 2, 8].Value = $"{t.LostWeightPer:F2}";
            sheet.Cells[i + 2, 9].Value = $"{t.DeltaTf:F2}";
            sheet.Cells[i + 2, 10].Value = t.FlameDuration;
            sheet.Cells[i + 2, 11].Value = passed ? "通过" : "不通过";
        }

        sheet.Columns[1].AutoFit();
        sheet.Columns[2].AutoFit();

        package.SaveAs(new FileInfo(path));
        return path;
    }

    private static bool _fontResolverSet;

    private static void EnsureFontResolver()
    {
        if (_fontResolverSet) return;
        _fontResolverSet = true;
        GlobalFontSettings.FontResolver = new SystemFontResolver();
    }

    // ================================================================
    // PDF 导出（PdfSharp 直接绘制，嵌入中文字体）
    // ================================================================

    /// <summary>
    /// 导出试验报告为 PDF（PdfSharp 直接绘制，正确嵌入中文字体）
    /// </summary>
    public static string ExportPdf(TestMaster test)
    {
        if (!Config.EnablePdfExport) return string.Empty;

        try
        {
            return ExportPdfInternal(test);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PDF] {ex}");
            throw new Exception($"PDF导出失败: {ex.Message}\n{ex.StackTrace}", ex);
        }
    }

    private static string ExportPdfInternal(TestMaster test)
    {
        EnsureFontResolver();

        var dir = Config.OutputDirectory;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, $"{test.TestId}_报告.pdf");
        bool passed = test.DeltaTf <= 30 && test.LostWeightPer <= 50 && test.FlameDuration <= 20;

        using var doc = new PdfSharp.Pdf.PdfDocument();
        doc.Info.Title = $"ISO 11820 试验报告 — {test.TestId}";

        var page = doc.AddPage();
        page.Size = PdfSharp.PageSize.A4;
        using var gfx = XGraphics.FromPdfPage(page);

        var titleFont  = new XFont("SimHei", 16, XFontStyleEx.Bold);
        var normalFont = new XFont("SimHei", 10, XFontStyleEx.Regular);
        var boldFont   = new XFont("SimHei", 10, XFontStyleEx.Bold);
        var bigFont    = new XFont("SimHei", 14, XFontStyleEx.Bold);
        var smallFont  = new XFont("SimHei", 9, XFontStyleEx.Regular);

        double y = 40;
        double left = 40, right = page.Width.Point - 40;

        // 标题
        gfx.DrawString("ISO 11820 建筑材料不燃性试验报告",
            titleFont, XBrushes.Black,
            new XRect(left, y, right - left, 30), XStringFormats.TopCenter);
        y += 32;

        gfx.DrawString($"报告生成：{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            smallFont, XBrushes.Gray,
            new XRect(left, y, right - left, 18), XStringFormats.TopCenter);
        y += 26;

        // 分割线
        gfx.DrawLine(XPens.Gray, left, y, right, y);
        y += 14;

        // 试验信息（键值对）
        var info = new (string, string)[]
        {
            ("样品编号", test.ProductId),
            ("试验ID", test.TestId),
            ("试验日期", test.TestDate.ToString("yyyy-MM-dd")),
            ("操作员", test.Operator),
            ("设备", $"{test.ApparatusName} ({test.ApparatusId})"),
            ("环境温度", $"{test.AmbTemp:F1} °C"),
            ("环境湿度", $"{test.AmbHumi:F1} %"),
            ("试验前质量", $"{test.PreWeight:F2} g"),
            ("试验后质量", $"{test.PostWeight:F2} g"),
            ("失重量", $"{test.LostWeight:F2} g"),
            ("失重率", $"{test.LostWeightPer:F2} %"),
            ("炉温升", $"{test.DeltaTf:F2} °C"),
            ("总试验时长", $"{test.TotalTestTime} 秒"),
            ("火焰持续时间", $"{test.FlameDuration} 秒"),
        };

        double labelW = 120, valueW = right - left - labelW;
        foreach (var kv in info)
        {
            gfx.DrawString(kv.Item1, boldFont, XBrushes.Black,
                new XRect(left, y, labelW, 20), XStringFormats.TopLeft);
            gfx.DrawString(kv.Item2, normalFont, XBrushes.Black,
                new XRect(left + labelW, y, valueW, 20), XStringFormats.TopLeft);
            y += 22;
        }

        y += 10;
        gfx.DrawLine(XPens.Gray, left, y, right, y);
        y += 16;

        // 判定结论
        var conclusionText = passed ? "通过 ✓" : "不通过 ✗";
        var conclusionColor = passed ? XBrushes.Green : XBrushes.Red;
        gfx.DrawString($"判定结论：{conclusionText}",
            bigFont, conclusionColor,
            new XRect(left, y, right - left, 24), XStringFormats.TopLeft);
        y += 28;

        gfx.DrawString("依据：ΔTf ≤ 30°C  且  失重率 ≤ 50%  且  火焰持续时间 ≤ 20 秒",
            smallFont, XBrushes.Gray,
            new XRect(left, y, right - left, 18), XStringFormats.TopLeft);

        doc.Save(path);
        return path;
    }
}

/// <summary>
/// Windows 系统字体解析器 — 让 PDFsharp 能嵌入中文字体
/// </summary>
internal sealed class SystemFontResolver : IFontResolver
{
    public string DefaultFontName => "SimHei";

    public byte[] GetFont(string faceName)
    {
        // 从 Windows Fonts 目录加载字体文件
        string fontPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
            faceName switch
            {
                "SimHei" => "simhei.ttf",
                "SimSun" => "simsun.ttc",
                "KaiTi"  => "simkai.ttf",
                _        => "simhei.ttf"
            });
        if (File.Exists(fontPath))
            return File.ReadAllBytes(fontPath);

        // 回退：尝试直接匹配文件名
        fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), faceName + ".ttf");
        if (File.Exists(fontPath))
            return File.ReadAllBytes(fontPath);

        fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), faceName + ".ttc");
        if (File.Exists(fontPath))
            return File.ReadAllBytes(fontPath);

        throw new FileNotFoundException($"找不到字体: {faceName}");
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        string name = familyName switch
        {
            "SimHei" => "SimHei",
            _        => familyName
        };

        var sim = (isBold, isItalic) switch
        {
            (true, true)  => XStyleSimulations.BoldItalicSimulation,
            (true, false)  => XStyleSimulations.BoldSimulation,
            (false, true)  => XStyleSimulations.ItalicSimulation,
            _              => XStyleSimulations.None
        };
        return new FontResolverInfo(name, sim);
    }
}
