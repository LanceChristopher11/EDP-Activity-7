namespace ConcertEventSystemUI;

public class TicketSalesForm : ShellForm
{
    private DataGridView _grid      = null!;
    private Label        _statusLbl = null!;
    private ComboBox     _cbEvent   = null!;
    private ComboBox     _cbArtist  = null!;
    private TextBox      _tbPrice   = null!;
    private TextBox      _tbQty     = null!;
    private Label        _totalLbl  = null!;

    private List<(int Id, string Name)> _events  = [];
    private List<(int Id, string Name)> _artists = [];

    public TicketSalesForm() : base("Lance Fest | Ticket Sales", "Ticket Sales")
    {
        this.MaximizeBox     = true;
        this.FormBorderStyle = FormBorderStyle.Sizable;

        BuildUi();
        LoadDropdowns();
        LoadGrid();
    }

    private void BuildUi()
    {
        // Page header
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Ticket Sales",
            Font      = Theme.TitleFont(26),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 20)
        });
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Record ticket sales for events. All transactions are saved to the database.",
            Font      = Theme.SubtitleFont(10.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(40, 60)
        });

        // Input form card
        Panel form = Theme.CreateCard(36, 90, 940, 180);

        form.Controls.Add(new Label
        {
            Text      = "New Sale",
            Font      = Theme.TitleFont(14),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(20, 16)
        });

        // Row of inputs
        int fx = 20;

        form.Controls.Add(MakeLabel("Event", fx, 48));
        _cbEvent = MakeCombo(fx, 68, 210);
        form.Controls.Add(_cbEvent);
        fx += 226;

        form.Controls.Add(MakeLabel("Artist", fx, 48));
        _cbArtist = MakeCombo(fx, 68, 210);
        form.Controls.Add(_cbArtist);
        fx += 226;

        form.Controls.Add(MakeLabel("Ticket Price (₱)", fx, 48));
        _tbPrice = Theme.CreateTextBox("0.00", fx, 68, 120);
        Panel pw = Theme.WrapInput(_tbPrice); pw.Location = new Point(fx, 64);
        form.Controls.Add(pw);
        fx += 136;

        form.Controls.Add(MakeLabel("Qty Sold", fx, 48));
        _tbQty = Theme.CreateTextBox("0", fx, 68, 100);
        Panel qw = Theme.WrapInput(_tbQty); qw.Location = new Point(fx, 64);
        form.Controls.Add(qw);
        fx += 116;

        _totalLbl = new Label
        {
            Text      = "Total: ₱0.00",
            Font      = Theme.TitleFont(12),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(fx, 72)
        };
        form.Controls.Add(_totalLbl);

        EventHandler recalc = (_, _) =>
        {
            decimal.TryParse(_tbPrice.Text, out decimal p);
            int.TryParse(_tbQty.Text, out int q);
            _totalLbl.Text = $"Total: ₱{p * q:N2}";
        };
        _tbPrice.TextChanged += recalc;
        _tbQty.TextChanged   += recalc;

        Button saveBtn = Theme.CreatePrimaryButton("Save Sale", fx, 110, 150, 42);
        saveBtn.Click += SaveSale_Click;
        form.Controls.Add(saveBtn);

        ContentPanel.Controls.Add(form);

        // Section label
        ContentPanel.Controls.Add(new Label
        {
            Text      = "All Ticket Sales Records",
            Font      = Theme.TitleFont(16),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 286)
        });

        _grid = BuildGrid();
        _grid.Location = new Point(36, 316);
        _grid.Size     = new Size(940, 300);
        _grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",      HeaderText = "ID",          FillWeight = 40  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEvent",   HeaderText = "Event Name",  FillWeight = 200 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDate",    HeaderText = "Event Date",  FillWeight = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colArtist",  HeaderText = "Artist",      FillWeight = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice",   HeaderText = "Price (₱)",   FillWeight = 90  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQty",     HeaderText = "Tickets Sold",FillWeight = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRevenue", HeaderText = "Revenue (₱)", FillWeight = 110 });

        ContentPanel.Controls.Add(_grid);

        // Status + Export button
        _statusLbl = new Label
        {
            Text      = "",
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(36, 626),
            Anchor    = AnchorStyles.Bottom | AnchorStyles.Left
        };
        ContentPanel.Controls.Add(_statusLbl);

        Button exportBtn = Theme.CreateSecondaryButton("Generate Report / Export to Excel", 36, 656, 340, 46);
        exportBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        exportBtn.Click += (_, _) =>
        {
            if (_grid.Rows.Count == 0)
            { ShowMsg("No records to export.", true); return; }
            ExcelExporter.Export(_grid, "Ticket Sales Report",
                chartLabelCol: 1, chartValueCol: 6);
        };
        ContentPanel.Controls.Add(exportBtn);
    }

    private void LoadDropdowns()
    {
        try { _events = DatabaseConnection.GetEventList(); _artists = DatabaseConnection.GetArtistList(); } catch { }
        _cbEvent.Items.Clear();
        foreach (var e in _events) _cbEvent.Items.Add(e.Name);
        if (_cbEvent.Items.Count > 0) _cbEvent.SelectedIndex = 0;
        _cbArtist.Items.Clear();
        foreach (var a in _artists) _cbArtist.Items.Add(a.Name);
        if (_cbArtist.Items.Count > 0) _cbArtist.SelectedIndex = 0;
    }

    private void LoadGrid()
    {
        _grid.Rows.Clear();
        try
        {
            var rows = DatabaseConnection.GetTicketSales();
            foreach (var r in rows)
                _grid.Rows.Add(r.TicketId, r.EventName,
                    r.EventDate.ToString("MMM d, yyyy"),
                    r.ArtistName,
                    $"₱{r.TicketPrice:N2}",
                    $"{r.TicketsSold:N0}",
                    $"₱{r.Revenue:N2}");
            ShowMsg($"{rows.Count} record(s) loaded.", false);
        }
        catch (Exception ex) { ShowMsg($"Error: {ex.Message}", true); }
    }

    private void SaveSale_Click(object? sender, EventArgs e)
    {
        if (_cbEvent.SelectedIndex < 0 || _cbArtist.SelectedIndex < 0)
        { ShowMsg("Please select an event and artist.", true); return; }
        if (!decimal.TryParse(_tbPrice.Text, out decimal price) || price <= 0)
        { ShowMsg("Enter a valid ticket price.", true); return; }
        if (!int.TryParse(_tbQty.Text, out int qty) || qty <= 0)
        { ShowMsg("Enter a valid quantity.", true); return; }

        try
        {
            DatabaseConnection.AddTicketSale(
                _events[_cbEvent.SelectedIndex].Id,
                _artists[_cbArtist.SelectedIndex].Id,
                price, qty);
            ShowMsg($"Sale saved! Revenue: ₱{price * qty:N2}", false);
            LoadGrid();
            _tbPrice.Text = "0.00"; _tbQty.Text = "0"; _totalLbl.Text = "Total: ₱0.00";
        }
        catch (Exception ex) { ShowMsg($"Error: {ex.Message}", true); }
    }

    private void ShowMsg(string msg, bool isError)
    {
        _statusLbl.ForeColor = isError ? Color.FromArgb(200, 60, 80) : Color.FromArgb(50, 160, 110);
        _statusLbl.Text      = msg;
    }

    private static Label MakeLabel(string text, int x, int y) => new()
    {
        Text = text, Font = Theme.SubtitleFont(9.5f),
        ForeColor = Theme.MutedText, AutoSize = true, Location = new Point(x, y)
    };

    private static ComboBox MakeCombo(int x, int y, int w) => new()
    {
        Location = new Point(x, y), Size = new Size(w, 30),
        FlatStyle = FlatStyle.Flat, BackColor = Color.White,
        ForeColor = Theme.DeepText, Font = Theme.BodyFont(10),
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private static DataGridView BuildGrid()
    {
        var g = new DataGridView
        {
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.None,
            RowHeadersVisible     = false,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            ReadOnly              = true,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            Font                  = Theme.BodyFont(10),
            GridColor             = Color.FromArgb(220, 232, 248),
            CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal
        };
        g.ColumnHeadersDefaultCellStyle.BackColor           = Theme.PanelBlue;
        g.ColumnHeadersDefaultCellStyle.ForeColor           = Theme.DeepText;
        g.ColumnHeadersDefaultCellStyle.Font                = Theme.ButtonFont(10);
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor  = Theme.PanelBlue;
        g.ColumnHeadersHeight       = 40;
        g.EnableHeadersVisualStyles  = false;
        g.DefaultCellStyle.SelectionBackColor = Theme.AccentPink;
        g.DefaultCellStyle.SelectionForeColor = Theme.DeepText;
        g.RowTemplate.Height = 36;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);
        return g;
    }
}