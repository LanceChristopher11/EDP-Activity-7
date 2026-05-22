namespace ConcertEventSystemUI;

public class ReportGeneratorForm : ShellForm
{
    // State
    private ComboBox _reportTypeBox  = null!;
    private ComboBox _venueFilterBox = null!;
    private ComboBox _artistFilterBox= null!;
    private Label    _venueLabel     = null!;
    private Label    _artistLabel    = null!;
    private DataGridView _grid       = null!;
    private Label    _summaryLabel   = null!;

    private List<(int Id, string Name)> _venueList  = [];
    private List<(int Id, string Name)> _artistList = [];

    public ReportGeneratorForm() : base("Lance Fest | Report Generator", "Report Generator")
    {
        BuildUi();
        LoadFilterData();
        RunReport(); // show data immediately on open
    }

    // UI
    private void BuildUi()
    {
        // Page header
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Report Generator",
            Font      = Theme.TitleFont(28),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 26)
        });
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Live reports pulled directly from concert_event_db.",
            Font      = Theme.SubtitleFont(11),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(40, 70)
        });

        // Filter panel
        Panel filters = Theme.CreateCard(40, 112, 310, 460);

        filters.Controls.Add(new Label
        {
            Text      = "Report Filters",
            Font      = Theme.TitleFont(16),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(24, 22)
        });

        // Report type
        filters.Controls.Add(MakeLabel("Report Type", 24, 66));
        _reportTypeBox = MakeCombo(24, 88, 260, new[]
        {
            "Event List",
            "Sales Overview",
            "Artist Schedule",
            "Venue Utilisation"
        });
        _reportTypeBox.SelectedIndexChanged += (_, _) => UpdateFilterVisibility();
        filters.Controls.Add(_reportTypeBox);

        // Venue filter (shown for Event List + Sales Overview)
        _venueLabel = MakeLabel("Filter by Venue", 24, 140);
        _venueFilterBox = MakeCombo(24, 162, 260, Array.Empty<string>());
        filters.Controls.Add(_venueLabel);
        filters.Controls.Add(_venueFilterBox);

        // Artist filter (shown for Artist Schedule only)
        _artistLabel = MakeLabel("Filter by Artist", 24, 140);
        _artistFilterBox = MakeCombo(24, 162, 260, Array.Empty<string>());
        filters.Controls.Add(_artistLabel);
        filters.Controls.Add(_artistFilterBox);

        Button runBtn = Theme.CreatePrimaryButton("Generate Report", 24, 230, 262, 44);
        runBtn.Click += (_, _) => RunReport();
        filters.Controls.Add(runBtn);

        // Summary stats panel inside filter card
        _summaryLabel = new Label
        {
            Text      = "",
            Font      = Theme.BodyFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = false,
            Size      = new Size(262, 140),
            Location  = new Point(24, 292)
        };
        filters.Controls.Add(_summaryLabel);

        ContentPanel.Controls.Add(filters);

        // Results grid
        _grid = new DataGridView
        {
            Location                = new Point(370, 112),
            Size                    = new Size(630, 460),
            BackgroundColor         = Color.White,
            BorderStyle             = BorderStyle.None,
            RowHeadersVisible       = false,
            AllowUserToAddRows      = false,
            AllowUserToDeleteRows   = false,
            ReadOnly                = true,
            SelectionMode           = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode     = DataGridViewAutoSizeColumnsMode.Fill,
            Font                    = Theme.BodyFont(9.5f),
            GridColor               = Color.FromArgb(220, 232, 248),
            CellBorderStyle         = DataGridViewCellBorderStyle.SingleHorizontal
        };

        _grid.ColumnHeadersDefaultCellStyle.BackColor           = Theme.PanelBlue;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor           = Theme.DeepText;
        _grid.ColumnHeadersDefaultCellStyle.Font                = Theme.ButtonFont(9.5f);
        _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor  = Theme.PanelBlue;
        _grid.ColumnHeadersHeight       = 40;
        _grid.EnableHeadersVisualStyles  = false;
        _grid.DefaultCellStyle.SelectionBackColor = Theme.AccentPink;
        _grid.DefaultCellStyle.SelectionForeColor = Theme.DeepText;
        _grid.RowTemplate.Height = 34;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);

        ContentPanel.Controls.Add(_grid);

        UpdateFilterVisibility();
    }

    // Load filter dropdowns from Database
    private void LoadFilterData()
    {
        try
        {
            _venueList  = DatabaseConnection.GetVenueList();
            _artistList = DatabaseConnection.GetArtistList();
        }
        catch { /* dropdowns stay empty; All option still works */ }

        // Venue combo: first item = All Venues
        _venueFilterBox.Items.Clear();
        _venueFilterBox.Items.Add("All Venues");
        foreach (var v in _venueList) _venueFilterBox.Items.Add(v.Name);
        _venueFilterBox.SelectedIndex = 0;

        // Artist combo: first item = All Artists
        _artistFilterBox.Items.Clear();
        _artistFilterBox.Items.Add("All Artists");
        foreach (var a in _artistList) _artistFilterBox.Items.Add(a.Name);
        _artistFilterBox.SelectedIndex = 0;
    }

    // Show/hide the right filter based on report type
    private void UpdateFilterVisibility()
    {
        int idx = _reportTypeBox.SelectedIndex;

        bool showVenue  = idx == 0 || idx == 1; // Event List, Sales Overview
        bool showArtist = idx == 2;              // Artist Schedule

        _venueLabel.Visible      = showVenue;
        _venueFilterBox.Visible  = showVenue;
        _artistLabel.Visible     = showArtist;
        _artistFilterBox.Visible = showArtist;
    }

    // Run the selected report
    private void RunReport()
    {
        _grid.Columns.Clear();
        _grid.Rows.Clear();
        _summaryLabel.Text = "";

        try
        {
            int reportIdx = _reportTypeBox.SelectedIndex;

            switch (reportIdx)
            {
                case 0: RunEventListReport();      break;
                case 1: RunSalesReport();          break;
                case 2: RunArtistScheduleReport(); break;
                case 3: RunVenueReport();          break;
            }
        }
        catch (Exception ex)
        {
            _summaryLabel.ForeColor = Color.FromArgb(200, 60, 80);
            _summaryLabel.Text      = $"Error loading report:\n{ex.Message}";
        }
    }

    // Report 1: Event List
    private void RunEventListReport()
    {
        int venueId = SelectedVenueId();
        var rows    = DatabaseConnection.GetEventReport(venueId);

        AddColumns("Event Name", "Date", "Venue", "Location", "Capacity", "Organizer");

        foreach (var r in rows)
            _grid.Rows.Add(
                r.EventName,
                r.EventDate.ToString("MMM d, yyyy"),
                r.VenueName,
                r.Location,
                $"{r.Capacity:N0}",
                r.OrganizerName
            );

        _summaryLabel.ForeColor = Theme.MutedText;
        _summaryLabel.Text =
            $"Total events shown:  {rows.Count}\n" +
            $"Earliest:  {(rows.Count > 0 ? rows[0].EventDate.ToString("MMM d, yyyy") : "—")}\n" +
            $"Latest:    {(rows.Count > 0 ? rows[^1].EventDate.ToString("MMM d, yyyy") : "—")}";
    }

    // Report 2: Sales Overview
    private void RunSalesReport()
    {
        int venueId = SelectedVenueId();
        var rows    = DatabaseConnection.GetSalesReport(venueId);

        AddColumns("Event", "Date", "Venue", "Artist", "Price (₱)", "Tickets Sold", "Revenue (₱)");

        decimal totalRevenue = 0;
        int     totalTickets = 0;

        foreach (var r in rows)
        {
            _grid.Rows.Add(
                r.EventName,
                r.EventDate.ToString("MMM d, yyyy"),
                r.VenueName,
                r.ArtistName,
                $"{r.TicketPrice:N2}",
                $"{r.TicketsSold:N0}",
                $"{r.Revenue:N2}"
            );
            totalRevenue += r.Revenue;
            totalTickets += r.TicketsSold;
        }

        _summaryLabel.ForeColor = Theme.MutedText;
        _summaryLabel.Text =
            $"Records shown:    {rows.Count}\n" +
            $"Total tickets:    {totalTickets:N0}\n" +
            $"Total revenue:    ₱{totalRevenue:N2}\n" +
            $"Avg per event:    ₱{(rows.Count > 0 ? totalRevenue / rows.Count : 0):N2}";
    }

    // Report 3: Artist Schedule
    private void RunArtistScheduleReport()
    {
        int artistId = SelectedArtistId();
        var rows     = DatabaseConnection.GetArtistReport(artistId);

        AddColumns("Artist", "Genre", "Event", "Date", "Venue", "Location");

        foreach (var r in rows)
            _grid.Rows.Add(
                r.ArtistName,
                r.Genre,
                r.EventName,
                r.EventDate.ToString("MMM d, yyyy"),
                r.VenueName,
                r.Location
            );

        _summaryLabel.ForeColor = Theme.MutedText;
        _summaryLabel.Text =
            $"Bookings shown:  {rows.Count}\n" +
            $"Unique artists:  {rows.Select(r => r.ArtistName).Distinct().Count()}";
    }

    // Report 4: Venue Utilisation
    private void RunVenueReport()
    {
        var rows = DatabaseConnection.GetVenueReport();

        AddColumns("Venue", "Location", "Capacity", "Events Held", "Tickets Sold", "Fill Rate");

        foreach (var r in rows)
        {
            // Fill rate = total tickets sold / (capacity × events held)
            double fillRate = r.EventCount > 0 && r.Capacity > 0
                ? (double)r.TotalSold / (r.Capacity * r.EventCount) * 100
                : 0;

            _grid.Rows.Add(
                r.VenueName,
                r.Location,
                $"{r.Capacity:N0}",
                r.EventCount,
                $"{r.TotalSold:N0}",
                $"{fillRate:0.0}%"
            );
        }

        int totalSold   = rows.Sum(r => r.TotalSold);
        int totalEvents = rows.Sum(r => r.EventCount);

        _summaryLabel.ForeColor = Theme.MutedText;
        _summaryLabel.Text =
            $"Venues shown:     {rows.Count}\n" +
            $"Total events:     {totalEvents}\n" +
            $"Total tickets:    {totalSold:N0}";
    }

    private void AddColumns(params string[] headers)
    {
        foreach (var h in headers)
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = h,
                ReadOnly   = true
            });
    }

    private int SelectedVenueId()
    {
        int idx = _venueFilterBox.SelectedIndex;
        // Index 0 = "All Venues" (id 0 = no filter)
        if (idx <= 0 || idx - 1 >= _venueList.Count) return 0;
        return _venueList[idx - 1].Id;
    }

    private int SelectedArtistId()
    {
        int idx = _artistFilterBox.SelectedIndex;
        if (idx <= 0 || idx - 1 >= _artistList.Count) return 0;
        return _artistList[idx - 1].Id;
    }

    private static Label MakeLabel(string text, int x, int y) => new()
    {
        Text      = text,
        Font      = Theme.SubtitleFont(9.5f),
        ForeColor = Theme.MutedText,
        AutoSize  = true,
        Location  = new Point(x, y)
    };

    private static ComboBox MakeCombo(int x, int y, int w, string[] items)
    {
        var cb = new ComboBox
        {
            Location      = new Point(x, y),
            Size          = new Size(w, 30),
            FlatStyle     = FlatStyle.Flat,
            BackColor     = Color.White,
            ForeColor     = Theme.DeepText,
            Font          = Theme.BodyFont(10),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cb.Items.AddRange(items);
        if (items.Length > 0) cb.SelectedIndex = 0;
        return cb;
    }
}