using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
 
// Aliases
using X   = DocumentFormat.OpenXml.Spreadsheet;          
using C   = DocumentFormat.OpenXml.Drawing.Charts;       
using XDR = DocumentFormat.OpenXml.Drawing.Spreadsheet;  
using A   = DocumentFormat.OpenXml.Drawing;              

namespace ConcertEventSystemUI;

public static class ExcelExporter
{
    private const string CompanyName    = "Lance Fest Concert Events";
    private const string CompanyTagline = "Concert Event Information System";
    private const string SignerName     = "Lance Christopher T. Delos Reyes";
    private const string SignerTitle    = "System Developer";


    public static void Export(
        DataGridView grid,
        string       reportTitle,
        int          chartLabelCol,
        int          chartValueCol)
    {
        using var dlg = new SaveFileDialog
        {
            Title      = "Save Excel Report",
            Filter     = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName   = $"{reportTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            DefaultExt = "xlsx"
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            string tempPath = Path.Combine(
                Path.GetTempPath(), Path.GetRandomFileName() + ".xlsx");

            BuildSheet1(grid, reportTitle, tempPath);
            InjectChart(tempPath, dlg.FileName, grid, reportTitle,
                        chartLabelCol, chartValueCol);

            if (File.Exists(tempPath)) File.Delete(tempPath);

            var open = MessageBox.Show(
                $"Report saved to:\n{dlg.FileName}\n\nOpen now?",
                "Export Successful",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (open == DialogResult.Yes)
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName        = dlg.FileName,
                        UseShellExecute = true
                    });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    
    // SHEET 1 — header, data table, signature

    private static void BuildSheet1(
        DataGridView grid, string reportTitle, string path)
    {
        using var wb = new XLWorkbook();
        var ws  = wb.Worksheets.Add("Report");
        int col = grid.Columns.Count;

        Merge(ws, 1, col).Value = CompanyName;
        ws.Cell(1, 1).Style
            .Font.SetBold(true).Font.SetFontSize(20)
            .Font.SetFontColor(XLColor.FromArgb(54, 74, 112))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Row(1).Height = 30;

        Merge(ws, 2, col).Value = CompanyTagline;
        ws.Cell(2, 1).Style
            .Font.SetFontSize(10)
            .Font.SetFontColor(XLColor.FromArgb(105, 120, 145))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        Merge(ws, 3, col).Value = reportTitle;
        ws.Cell(3, 1).Style
            .Font.SetBold(true).Font.SetFontSize(13)
            .Font.SetFontColor(XLColor.FromArgb(54, 74, 112))
            .Fill.SetBackgroundColor(XLColor.FromArgb(213, 232, 255))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Row(3).Height = 22;

        Merge(ws, 4, col).Value = $"Generated: {DateTime.Now:MMMM d, yyyy  h:mm tt}";
        ws.Cell(4, 1).Style
            .Font.SetFontSize(9).Font.SetItalic(true)
            .Font.SetFontColor(XLColor.FromArgb(105, 120, 145))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Row(5).Height = 8;

        // Column headers
        for (int c = 0; c < grid.Columns.Count; c++)
        {
            ws.Cell(6, c + 1).Value = grid.Columns[c].HeaderText;
            ws.Cell(6, c + 1).Style
                .Font.SetBold(true)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromArgb(100, 149, 220))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Border.SetBottomBorder(XLBorderStyleValues.Thin);
        }
        ws.Row(6).Height = 22;

        // Data rows
        for (int row = 0; row < grid.Rows.Count; row++)
        {
            bool alt = row % 2 == 1;
            for (int c = 0; c < grid.Columns.Count; c++)
            {
                var cell  = ws.Cell(7 + row, c + 1);
                string val   = grid.Rows[row].Cells[c].Value?.ToString() ?? "";
                string clean = val.Replace("₱", "").Replace(",", "")
                                  .Replace("%", "").Trim();
                if (double.TryParse(clean, out double num)) cell.Value = num;
                else cell.Value = val;
                if (alt)
                    cell.Style.Fill.SetBackgroundColor(XLColor.FromArgb(247, 250, 255));
                cell.Style
                    .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                    .Border.SetBottomBorderColor(XLColor.FromArgb(220, 232, 248));
            }
        }

        ws.Columns().AdjustToContents();
        foreach (var c in ws.ColumnsUsed()) if (c.Width > 45) c.Width = 45;

        int endRow = 7 + grid.Rows.Count + 1;
        Merge(ws, endRow, col).Value = "— End of Report —";
        ws.Cell(endRow, 1).Style
            .Font.SetItalic(true)
            .Font.SetFontColor(XLColor.FromArgb(105, 120, 145))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // Signature block
        int sig = endRow + 4;
        ws.Cell(sig,     col).Value = "Prepared and Signed by:";
        ws.Cell(sig,     col).Style.Font.SetFontSize(9)
            .Font.SetFontColor(XLColor.FromArgb(105, 120, 145));

        ws.Cell(sig + 1, col).Value = SignerName;
        ws.Cell(sig + 1, col).Style
            .Font.SetBold(true).Font.SetFontSize(12)
            .Font.SetFontColor(XLColor.FromArgb(54, 74, 112));

        ws.Cell(sig + 2, col).Value = SignerTitle;
        ws.Cell(sig + 2, col).Style.Font.SetFontSize(9)
            .Font.SetFontColor(XLColor.FromArgb(105, 120, 145));

        ws.Cell(sig + 3, col).Style
            .Border.SetBottomBorder(XLBorderStyleValues.Thick)
            .Border.SetBottomBorderColor(XLColor.FromArgb(54, 74, 112));
        ws.Row(sig + 3).Height = 36;

        ws.Cell(sig + 4, col).Value = "Authorized Signature";
        ws.Cell(sig + 4, col).Style.Font.SetFontSize(8)
            .Font.SetFontColor(XLColor.FromArgb(105, 120, 145));

        ws.Cell(sig + 6, col).Value = $"Date: {DateTime.Now:MMMM d, yyyy}";
        ws.Cell(sig + 6, col).Style.Font.SetFontSize(9)
            .Font.SetFontColor(XLColor.FromArgb(105, 120, 145));

        wb.SaveAs(path);
    }

    // SHEET 2 — bar chart

    private static void InjectChart(
        string tempPath, string finalPath,
        DataGridView grid, string reportTitle,
        int labelCol, int valueCol)
    {
        File.Copy(tempPath, finalPath, overwrite: true);

        using var doc = SpreadsheetDocument.Open(finalPath, isEditable: true);
        var wbPart    = doc.WorkbookPart!;

        var sheet2Part = wbPart.AddNewPart<WorksheetPart>();
        sheet2Part.Worksheet = new X.Worksheet(new X.SheetData());

        X.Sheets sheets;
        var existing = wbPart.Workbook!.GetFirstChild<X.Sheets>();
        if (existing != null)
        {
            sheets = existing;
        }
        else
        {
            sheets = new X.Sheets();
            wbPart.Workbook.AppendChild(sheets);
        }
        uint nextId = (uint)(sheets.Elements<X.Sheet>().Count() + 1);

        sheets.AppendChild(new X.Sheet
        {
            Id      = wbPart.GetIdOfPart(sheet2Part),
            SheetId = nextId,
            Name    = "Chart"
        });

        // Collects the data from grid
        var labels = new List<string>();
        var values = new List<double>();
        foreach (DataGridViewRow row in grid.Rows)
        {
            string lbl   = row.Cells[labelCol].Value?.ToString() ?? "";
            string raw   = row.Cells[valueCol].Value?.ToString() ?? "0";
            string clean = raw.Replace("₱","").Replace(",","").Replace("%","").Trim();
            double.TryParse(clean, out double v);
            labels.Add(lbl.Length > 20 ? lbl[..20] + "…" : lbl);
            values.Add(v);
        }

        // the chart part
        var drawingsPart = sheet2Part.AddNewPart<DrawingsPart>();
        sheet2Part.Worksheet.AppendChild(
            new X.Drawing { Id = sheet2Part.GetIdOfPart(drawingsPart) });

        var chartPart  = drawingsPart.AddNewPart<ChartPart>();
        var chartSpace = new C.ChartSpace();
        chartSpace.AddNamespaceDeclaration("c",
            "http://schemas.openxmlformats.org/drawingml/2006/chart");
        chartSpace.AddNamespaceDeclaration("a",
            "http://schemas.openxmlformats.org/drawingml/2006/main");
        chartSpace.AddNamespaceDeclaration("r",
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

        var chart = new C.Chart();

        // Title
        var title   = new C.Title();
        var titleTx = new C.ChartText();
        var richTx  = new C.RichText();
        richTx.AppendChild(new A.BodyProperties());
        richTx.AppendChild(new A.ListStyle());
        var para = new A.Paragraph();
        var run  = new A.Run();
        run.AppendChild(new A.RunProperties { Bold = true, FontSize = 1400 });
        run.AppendChild(new A.Text(reportTitle));
        para.AppendChild(run);
        richTx.AppendChild(para);
        titleTx.AppendChild(richTx);
        title.AppendChild(titleTx);
        title.AppendChild(new C.Overlay { Val = false });
        chart.AppendChild(title);
        chart.AppendChild(new C.AutoTitleDeleted { Val = false });

        // Plot area
        var plotArea = new C.PlotArea();
        plotArea.AppendChild(new C.Layout());

        var barChart = new C.BarChart();
        barChart.AppendChild(new C.BarDirection  { Val = C.BarDirectionValues.Column });
        barChart.AppendChild(new C.BarGrouping   { Val = C.BarGroupingValues.Clustered });
        barChart.AppendChild(new C.VaryColors    { Val = false });

        // Series
        var ser = new C.BarChartSeries();
        ser.AppendChild(new C.Index { Val = 0 });
        ser.AppendChild(new C.Order { Val = 0 });

        // Series label
        var serTx    = new C.SeriesText();
        var serCache = new C.StringCache();
        serCache.AppendChild(new C.PointCount { Val = 1 });
        serCache.AppendChild(new C.StringPoint
            { Index = 0, NumericValue = new C.NumericValue(reportTitle) });
        var serRef = new C.StringReference();
        serRef.AppendChild(serCache);
        serTx.AppendChild(serRef);
        ser.AppendChild(serTx);

        // Bar fill — blue (iloveblue)
        var spPr = new C.ShapeProperties();
        var fill = new A.SolidFill();
        fill.AppendChild(new A.RgbColorModelHex { Val = "7EB4FF" });
        spPr.AppendChild(fill);
        ser.AppendChild(spPr);

        // Category axis data
        var cat      = new C.CategoryAxisData();
        var catRef   = new C.StringReference();
        var catCache = new C.StringCache();
        catCache.AppendChild(new C.PointCount { Val = (uint)labels.Count });
        for (int i = 0; i < labels.Count; i++)
            catCache.AppendChild(new C.StringPoint
                { Index = (uint)i, NumericValue = new C.NumericValue(labels[i]) });
        catRef.AppendChild(catCache);
        cat.AppendChild(catRef);
        ser.AppendChild(cat);

        // Values
        var vals     = new C.Values();
        var valRef   = new C.NumberReference();
        var valCache = new C.NumberingCache();
        valCache.AppendChild(new C.FormatCode("General"));
        valCache.AppendChild(new C.PointCount { Val = (uint)values.Count });
        for (int i = 0; i < values.Count; i++)
            valCache.AppendChild(new C.NumericPoint
                { Index = (uint)i, NumericValue = new C.NumericValue(values[i].ToString("G")) });
        valRef.AppendChild(valCache);
        vals.AppendChild(valRef);
        ser.AppendChild(vals);

        barChart.AppendChild(ser);

        // Data labels
        var dLbls = new C.DataLabels();
        dLbls.AppendChild(new C.ShowLegendKey    { Val = false });
        dLbls.AppendChild(new C.ShowValue        { Val = true  });
        dLbls.AppendChild(new C.ShowCategoryName { Val = false });
        dLbls.AppendChild(new C.ShowSeriesName   { Val = false });
        dLbls.AppendChild(new C.ShowPercent      { Val = false });
        dLbls.AppendChild(new C.ShowBubbleSize   { Val = false });
        barChart.AppendChild(dLbls);

        barChart.AppendChild(new C.AxisId { Val = 1 });
        barChart.AppendChild(new C.AxisId { Val = 2 });
        plotArea.AppendChild(barChart);

        // Category (X) axis
        var catAx = new C.CategoryAxis();
        catAx.AppendChild(new C.AxisId { Val = 1 });
        catAx.AppendChild(new C.Scaling(
            new C.Orientation { Val = C.OrientationValues.MinMax }));
        catAx.AppendChild(new C.Delete       { Val = false });
        catAx.AppendChild(new C.AxisPosition { Val = C.AxisPositionValues.Bottom });
        catAx.AppendChild(new C.CrossingAxis { Val = 2 });
        catAx.AppendChild(new C.Crosses      { Val = C.CrossesValues.AutoZero });
        catAx.AppendChild(new C.AutoLabeled  { Val = true });
        catAx.AppendChild(new C.LabelAlignment  { Val = C.LabelAlignmentValues.Center });
        catAx.AppendChild(new C.LabelOffset  { Val = 100 });
        plotArea.AppendChild(catAx);

        // Value (Y) axis
        var valAx = new C.ValueAxis();
        valAx.AppendChild(new C.AxisId { Val = 2 });
        valAx.AppendChild(new C.Scaling(
            new C.Orientation { Val = C.OrientationValues.MinMax }));
        valAx.AppendChild(new C.Delete       { Val = false });
        valAx.AppendChild(new C.AxisPosition { Val = C.AxisPositionValues.Left });
        valAx.AppendChild(new C.NumberingFormat
            { FormatCode = "General", SourceLinked = true });
        valAx.AppendChild(new C.MajorGridlines());
        valAx.AppendChild(new C.CrossingAxis { Val = 1 });
        valAx.AppendChild(new C.Crosses      { Val = C.CrossesValues.AutoZero });
        valAx.AppendChild(new C.CrossBetween { Val = C.CrossBetweenValues.Between });
        plotArea.AppendChild(valAx);

        chart.AppendChild(plotArea);

        var legend = new C.Legend();
        legend.AppendChild(new C.LegendPosition { Val = C.LegendPositionValues.Bottom });
        legend.AppendChild(new C.Overlay        { Val = false });
        chart.AppendChild(legend);

        chart.AppendChild(new C.PlotVisibleOnly { Val = true });
        chart.AppendChild(new C.DisplayBlanksAs { Val = C.DisplayBlanksAsValues.Gap });

        chartSpace.AppendChild(chart);
        chartPart.ChartSpace = chartSpace;

        // Anchor chart on the sheet
        var wsDr          = new XDR.WorksheetDrawing();
        var twoCellAnchor = new XDR.TwoCellAnchor();

        twoCellAnchor.AppendChild(new XDR.FromMarker
        {
            ColumnId     = new XDR.ColumnId("1"),
            ColumnOffset = new XDR.ColumnOffset("0"),
            RowId        = new XDR.RowId("1"),
            RowOffset    = new XDR.RowOffset("0")
        });
        twoCellAnchor.AppendChild(new XDR.ToMarker
        {
            ColumnId     = new XDR.ColumnId("14"),
            ColumnOffset = new XDR.ColumnOffset("0"),
            RowId        = new XDR.RowId("30"),
            RowOffset    = new XDR.RowOffset("0")
        });

        var frame    = new XDR.GraphicFrame();
        var nvProps  = new XDR.NonVisualGraphicFrameProperties();
        nvProps.AppendChild(new XDR.NonVisualDrawingProperties { Id = 1, Name = "Chart 1" });
        nvProps.AppendChild(new XDR.NonVisualGraphicFrameDrawingProperties());
        frame.AppendChild(nvProps);
        frame.AppendChild(new XDR.Transform());

        var graphic     = new A.Graphic();
        var graphicData = new A.GraphicData
        {
            Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart"
        };
        graphicData.AppendChild(new C.ChartReference
        {
            Id = drawingsPart.GetIdOfPart(chartPart)
        });
        graphic.AppendChild(graphicData);
        frame.AppendChild(graphic);

        twoCellAnchor.AppendChild(frame);
        twoCellAnchor.AppendChild(new XDR.ClientData());
        wsDr.AppendChild(twoCellAnchor);
        drawingsPart.WorksheetDrawing = wsDr;

        sheet2Part.Worksheet.Save();
        wbPart.Workbook.Save();
    }

    private static IXLCell Merge(IXLWorksheet ws, int row, int colCount)
    {
        ws.Range(row, 1, row, colCount).Merge();
        return ws.Cell(row, 1);
    }
}