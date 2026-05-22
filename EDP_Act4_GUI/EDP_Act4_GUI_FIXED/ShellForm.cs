namespace ConcertEventSystemUI;

public abstract class ShellForm : Form
{
    protected Panel ContentPanel = null!;

    protected ShellForm(string title, string pageName)
    {
        Size = new Size(1280, 760);
        Theme.ApplyFormStyle(this, title);
        BuildLayout(pageName);
    }

    private void BuildLayout(string pageName)
    {
        Panel sidebar = new()
        {
            Dock      = DockStyle.Left,
            Width     = 250,
            BackColor = Theme.PanelBlue
        };

        sidebar.Controls.Add(new Label
        {
            Text      = "Lance Fest",
            Font      = Theme.TitleFont(24),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(32, 36)
        });
        sidebar.Controls.Add(new Label
        {
            Text      = "Concert Event Information System",
            Font      = Theme.SubtitleFont(10),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(34, 74)
        });

        int top = 118;

        // Main nav
        sidebar.Controls.Add(CreateNavButton("Dashboard",        top,       () => Navigation.Open(this, new DashboardForm()),       pageName == "Dashboard"));
        sidebar.Controls.Add(CreateNavButton("User Management",  top += 54, () => Navigation.Open(this, new UserManagementForm()),  pageName == "User Management"));
        sidebar.Controls.Add(CreateNavButton("Password Recovery",top += 54, () => Navigation.Open(this, new PasswordRecoveryForm()),pageName == "Password Recovery"));
        sidebar.Controls.Add(CreateNavButton("Report Generator", top += 54, () => Navigation.Open(this, new ReportGeneratorForm()), pageName == "Report Generator"));

        // Transactions section label
        top += 54;
        sidebar.Controls.Add(new Label
        {
            Text      = "TRANSACTIONS",
            Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(34, top)
        });
        top += 22;

        sidebar.Controls.Add(CreateNavButton("Ticket Sales",    top,       () => Navigation.Open(this, new TicketSalesForm()),    pageName == "Ticket Sales"));
        sidebar.Controls.Add(CreateNavButton("Event Booking",   top += 54, () => Navigation.Open(this, new EventBookingForm()),   pageName == "Event Booking"));
        sidebar.Controls.Add(CreateNavButton("Artist Booking",  top += 54, () => Navigation.Open(this, new ArtistBookingForm()),  pageName == "Artist Booking"));

        top += 54;
        sidebar.Controls.Add(CreateNavButton("Back to Login", top, () => Navigation.Open(this, new LoginForm()), false));

        sidebar.Controls.Add(new Label
        {
            Text      = "Lance Fest Concert System",
            ForeColor = Theme.MutedText,
            Font      = Theme.SubtitleFont(9f),
            AutoSize  = true,
            Location  = new Point(32, 690)
        });

        ContentPanel = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Theme.Background
        };

        Controls.Add(ContentPanel);
        Controls.Add(sidebar);
    }

    private Button CreateNavButton(string text, int top, Action onClick, bool active)
    {
        Button button = new()
        {
            Text      = text,
            Location  = new Point(28, top),
            Size      = new Size(194, 44),
            FlatStyle = FlatStyle.Flat,
            Font      = Theme.ButtonFont(10),
            ForeColor = Theme.DeepText,
            BackColor = active ? Theme.AccentPink : Color.FromArgb(238, 245, 255),
            Cursor    = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => onClick();
        return button;
    }
}