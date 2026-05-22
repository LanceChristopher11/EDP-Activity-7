namespace ConcertEventSystemUI;

public class EventBookingForm : ShellForm
{
    private DataGridView _grid        = null!;
    private Label        _statusLbl   = null!;
    private TextBox      _tbEventName = null!;
    private DateTimePicker _dtpDate   = null!;
    private ComboBox     _cbVenue     = null!;
    private ComboBox     _cbOrganizer = null!;
    private TextBox      _tbCreatedBy = null!;

    private List<(int Id, string Name)> _venues     = [];
    private List<(int Id, string Name)> _organizers = [];

    public EventBookingForm() : base("Lance Fest | Event Booking", "Event Booking")
    {
        this.MaximizeBox     = true;
        this.FormBorderStyle = FormBorderStyle.Sizable;

        BuildUi();
        LoadDropdowns();
        LoadGrid();
    }

    private void BuildUi()
    {
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Event Booking",
            Font      = Theme.TitleFont(26),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 20)
        });
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Schedule a new concert event. All bookings are saved to the database.",
            Font      = Theme.SubtitleFont(10.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(40, 60)
        });

        // Input form card
        Panel form = Theme.CreateCard(36, 90, 940, 180);

        form.Controls.Add(new Label
        {
            Text      = "New Event",
            Font      = Theme.TitleFont(14),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(20, 16)
        });

        int fx = 20;

        // Event name
        form.Controls.Add(MakeLabel("Event Name", fx, 48));
        _tbEventName = Theme.CreateTextBox("Event name", fx, 68, 220);
        Panel nw = Theme.WrapInput(_tbEventName); nw.Location = new Point(fx, 64);
        form.Controls.Add(nw);
        fx += 236;

        // Date
        form.Controls.Add(MakeLabel("Event Date", fx, 48));
        _dtpDate = new DateTimePicker
        {
            Location = new Point(fx, 68),
            Size     = new Size(150, 30),
            Format   = DateTimePickerFormat.Short,
            Font     = Theme.BodyFont(10)
        };
        form.Controls.Add(_dtpDate);
        fx += 166;

        // Venue
        form.Controls.Add(MakeLabel("Venue", fx, 48));
        _cbVenue = MakeCombo(fx, 68, 180);
        form.Controls.Add(_cbVenue);
        fx += 196;

        // Organizer
        form.Controls.Add(MakeLabel("Organizer", fx, 48));
        _cbOrganizer = MakeCombo(fx, 68, 160);
        form.Controls.Add(_cbOrganizer);
        fx += 176;

        // Booked by
        form.Controls.Add(MakeLabel("Booked By", fx, 48));
        _tbCreatedBy = Theme.CreateTextBox("Your name", fx, 68, 120);
        Panel bw = Theme.WrapInput(_tbCreatedBy); bw.Location = new Point(fx, 64);
        form.Controls.Add(bw);
        fx += 136;

        Button saveBtn = Theme.CreatePrimaryButton("Book Event", 20, 118, 160, 42);
        saveBtn.Click += SaveEvent_Click;
        form.Controls.Add(saveBtn);

        ContentPanel.Controls.Add(form);

        // Section label
        ContentPanel.Controls.Add(new Label
        {
            Text      = "All Booked Events",
            Font      = Theme.TitleFont(16),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 286)
        });

        // Grid
        _grid = BuildGrid();
        _grid.Location = new Point(36, 316);
        _grid.Size     = new Size(940, 300);
        _grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",   HeaderText = "ID",         FillWeight = 40  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "Event Name", FillWeight = 200 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDate", HeaderText = "Date",       FillWeight = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVen",  HeaderText = "Venue",      FillWeight = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLoc",  HeaderText = "Location",   FillWeight = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colOrg",  HeaderText = "Organizer",  FillWeight = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStat", HeaderText = "Status",     FillWeight = 90  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBy",   HeaderText = "Booked By",  FillWeight = 100 });

        _grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            if (_grid.Columns[e.ColumnIndex].Name == "colStat" && e.Value != null)
            {
                e.CellStyle.ForeColor = e.Value.ToString() switch
                {
                    "Scheduled" => Color.FromArgb(50, 160, 110),
                    "Cancelled" => Color.FromArgb(200, 60, 80),
                    _           => Theme.MutedText
                };
                e.CellStyle.Font = Theme.ButtonFont(9.5f);
            }
        };

        ContentPanel.Controls.Add(_grid);

        // Status + Export button
        _statusLbl = new Label
        {
            Text     = "",
            Font     = Theme.SubtitleFont(9.5f),
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
            ExcelExporter.Export(_grid, "Event Booking Report",
                chartLabelCol: 1, chartValueCol: 2);
        };
        ContentPanel.Controls.Add(exportBtn);
    }

    private void LoadDropdowns()
    {
        try { _venues = DatabaseConnection.GetVenueList(); _organizers = DatabaseConnection.GetOrganizerList(); } catch { }
        _cbVenue.Items.Clear();
        foreach (var v in _venues) _cbVenue.Items.Add(v.Name);
        if (_cbVenue.Items.Count > 0) _cbVenue.SelectedIndex = 0;
        _cbOrganizer.Items.Clear();
        foreach (var o in _organizers) _cbOrganizer.Items.Add(o.Name);
        if (_cbOrganizer.Items.Count > 0) _cbOrganizer.SelectedIndex = 0;
    }

    private void LoadGrid()
    {
        _grid.Rows.Clear();
        try
        {
            var rows = DatabaseConnection.GetEventBookings();
            foreach (var r in rows)
                _grid.Rows.Add(r.EventId, r.EventName,
                    r.EventDate.ToString("MMM d, yyyy"),
                    r.VenueName, r.Location, r.OrganizerName,
                    r.Status, r.CreatedBy);
            ShowMsg($"{rows.Count} event(s) loaded.", false);
        }
        catch (Exception ex) { ShowMsg($"Error: {ex.Message}", true); }
    }

    private void SaveEvent_Click(object? sender, EventArgs e)
    {
        string name = _tbEventName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name) || name == "Event name")
        { ShowMsg("Enter an event name.", true); return; }
        if (_cbVenue.SelectedIndex < 0 || _cbOrganizer.SelectedIndex < 0)
        { ShowMsg("Select a venue and organizer.", true); return; }

        string bookedBy = _tbCreatedBy.Text.Trim();
        if (string.IsNullOrWhiteSpace(bookedBy) || bookedBy == "Your name") bookedBy = "System User";

        try
        {
            int newId = DatabaseConnection.AddEvent(name, _dtpDate.Value,
                _venues[_cbVenue.SelectedIndex].Id,
                _organizers[_cbOrganizer.SelectedIndex].Id,
                bookedBy);
            ShowMsg($"Event booked! ID: {newId}", false);
            LoadGrid();
            _tbEventName.Text = "Event name"; _tbEventName.ForeColor = Theme.MutedText;
            _tbCreatedBy.Text = "Your name";  _tbCreatedBy.ForeColor = Theme.MutedText;
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