namespace ConcertEventSystemUI;

public class ArtistBookingForm : ShellForm
{
    private DataGridView _grid       = null!;
    private Label        _statusLbl  = null!;
    private ComboBox     _cbEvent    = null!;
    private ComboBox     _cbArtist   = null!;
    private ComboBox     _cbStatus   = null!;
    private TextBox      _tbFee      = null!;
    private TextBox      _tbNotes    = null!;
    private TextBox      _tbBookedBy = null!;

    private List<(int Id, string Name)> _events  = [];
    private List<(int Id, string Name)> _artists = [];

    public ArtistBookingForm() : base("Lance Fest | Artist Booking", "Artist Booking")
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
            Text      = "Artist Booking",
            Font      = Theme.TitleFont(26),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 20)
        });
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Assign an artist to a concert event with booking fee and status.",
            Font      = Theme.SubtitleFont(10.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(40, 60)
        });

        // Input form card
        Panel form = Theme.CreateCard(36, 90, 940, 200);

        form.Controls.Add(new Label
        {
            Text      = "New Artist Booking",
            Font      = Theme.TitleFont(14),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(20, 14)
        });

        int fx = 20;

        // Event
        form.Controls.Add(MakeLabel("Event", fx, 48));
        _cbEvent = MakeCombo(fx, 68, 200);
        form.Controls.Add(_cbEvent);
        fx += 216;

        // Artist
        form.Controls.Add(MakeLabel("Artist", fx, 48));
        _cbArtist = MakeCombo(fx, 68, 180);
        form.Controls.Add(_cbArtist);
        fx += 196;

        // Booking fee
        form.Controls.Add(MakeLabel("Booking Fee (₱)", fx, 48));
        _tbFee = Theme.CreateTextBox("0.00", fx, 68, 110);
        Panel fw = Theme.WrapInput(_tbFee); fw.Location = new Point(fx, 64);
        form.Controls.Add(fw);
        fx += 126;

        // Status
        form.Controls.Add(MakeLabel("Status", fx, 48));
        _cbStatus = new ComboBox
        {
            Location      = new Point(fx, 68),
            Size          = new Size(130, 30),
            FlatStyle     = FlatStyle.Flat,
            BackColor     = Color.White,
            ForeColor     = Theme.DeepText,
            Font          = Theme.BodyFont(10),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cbStatus.Items.AddRange(["Confirmed", "Pending", "Cancelled"]);
        _cbStatus.SelectedIndex = 0;
        form.Controls.Add(_cbStatus);
        fx += 146;

        // Notes
        form.Controls.Add(MakeLabel("Notes", fx, 48));
        _tbNotes = Theme.CreateTextBox("Optional notes", fx, 68, 130);
        Panel nw = Theme.WrapInput(_tbNotes); nw.Location = new Point(fx, 64);
        form.Controls.Add(nw);
        fx += 146;

        // Booked by
        form.Controls.Add(MakeLabel("Booked By", 20, 116));
        _tbBookedBy = Theme.CreateTextBox("Your name", 20, 136, 200);
        Panel bw = Theme.WrapInput(_tbBookedBy); bw.Location = new Point(20, 132);
        form.Controls.Add(bw);

        Button saveBtn = Theme.CreatePrimaryButton("Confirm Booking", 236, 138, 180, 42);
        saveBtn.Click += SaveBooking_Click;
        form.Controls.Add(saveBtn);

        ContentPanel.Controls.Add(form);

        // Section label
        ContentPanel.Controls.Add(new Label
        {
            Text      = "All Artist Bookings",
            Font      = Theme.TitleFont(16),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 306)
        });

        // Grid
        _grid = BuildGrid();
        _grid.Location = new Point(36, 336);
        _grid.Size     = new Size(940, 296);
        _grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",     HeaderText = "ID",          FillWeight = 40  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colArtist", HeaderText = "Artist",      FillWeight = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colGenre",  HeaderText = "Genre",       FillWeight = 80  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEvent",  HeaderText = "Event",       FillWeight = 180 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDate",   HeaderText = "Event Date",  FillWeight = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVenue",  HeaderText = "Venue",       FillWeight = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "Status",      FillWeight = 85  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFee",    HeaderText = "Fee (₱)",     FillWeight = 90  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBy",     HeaderText = "Booked By",   FillWeight = 95  });

        _grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            if (_grid.Columns[e.ColumnIndex].Name == "colStatus" && e.Value != null)
            {
                e.CellStyle.ForeColor = e.Value.ToString() switch
                {
                    "Confirmed" => Color.FromArgb(50, 160, 110),
                    "Cancelled" => Color.FromArgb(200, 60, 80),
                    _           => Color.FromArgb(200, 140, 40)
                };
                e.CellStyle.Font = Theme.ButtonFont(9.5f);
            }
        };

        ContentPanel.Controls.Add(_grid);

        // Status label
        _statusLbl = new Label
        {
            Text      = "",
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(36, 642),
            Anchor    = AnchorStyles.Bottom | AnchorStyles.Left
        };
        ContentPanel.Controls.Add(_statusLbl);

        // Export button
        Button exportBtn = Theme.CreateSecondaryButton(
            "Generate Report / Export to Excel", 36, 672, 340, 46);
        exportBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        exportBtn.Click += (_, _) =>
        {
            if (_grid.Rows.Count == 0)
            { ShowMsg("No records to export.", true); return; }
            ExcelExporter.Export(
                _grid,
                "Artist Booking Report",
                chartLabelCol: 1,   // Artist name
                chartValueCol: 7);  // Fee
        };
        ContentPanel.Controls.Add(exportBtn);
    }

    // Data
    private void LoadDropdowns()
    {
        try { _events = DatabaseConnection.GetEventList(); _artists = DatabaseConnection.GetArtistList(); }
        catch { }

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
            var rows = DatabaseConnection.GetArtistBookings();
            foreach (var r in rows)
                _grid.Rows.Add(r.BookingId, r.ArtistName, r.Genre,
                    r.EventName, r.EventDate.ToString("MMM d, yyyy"),
                    r.VenueName, r.Status,
                    $"₱{r.BookingFee:N2}", r.BookedBy);
            ShowMsg($"{rows.Count} booking(s) loaded.", false);
        }
        catch (Exception ex) { ShowMsg($"Error: {ex.Message}", true); }
    }

    private void SaveBooking_Click(object? sender, EventArgs e)
    {
        if (_cbEvent.SelectedIndex < 0 || _cbArtist.SelectedIndex < 0)
        { ShowMsg("Select an event and artist.", true); return; }
        if (!decimal.TryParse(_tbFee.Text, out decimal fee) || fee < 0)
        { ShowMsg("Enter a valid booking fee.", true); return; }

        string status   = _cbStatus.SelectedItem?.ToString() ?? "Pending";
        string notes    = _tbNotes.Text.Trim()    == "Optional notes" ? "" : _tbNotes.Text.Trim();
        string bookedBy = _tbBookedBy.Text.Trim() == "Your name"      ? "System User" : _tbBookedBy.Text.Trim();

        try
        {
            int newId = DatabaseConnection.AddArtistBooking(
                _events[_cbEvent.SelectedIndex].Id,
                _artists[_cbArtist.SelectedIndex].Id,
                fee, status, notes, bookedBy);
            ShowMsg($"Booking confirmed! ID: {newId}", false);
            LoadGrid();
            _tbFee.Text      = "0.00";
            _tbNotes.Text    = "Optional notes";    _tbNotes.ForeColor    = Theme.MutedText;
            _tbBookedBy.Text = "Your name";         _tbBookedBy.ForeColor = Theme.MutedText;
            _cbStatus.SelectedIndex = 0;
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