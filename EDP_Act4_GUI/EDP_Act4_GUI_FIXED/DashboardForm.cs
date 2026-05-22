namespace ConcertEventSystemUI;

public class DashboardForm : ShellForm
{
    public DashboardForm() : base("Lance Fest | Dashboard", "Dashboard")
    {
        BuildUi();
    }

    private void BuildUi()
    {
        // Page header
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Dashboard",
            Font      = Theme.TitleFont(28),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 26)
        });
        ContentPanel.Controls.Add(new Label
        {
            Text      = "A visual summary of upcoming concerts, ticket details, and other activities.",
            Font      = Theme.SubtitleFont(11),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(40, 70)
        });

        // loaded from Database
        string totalEvents  = "—";
        string totalVenues  = "—";
        string totalArtists = "—";
        string totalRevenue = "—";

        try
        {
            totalEvents  = DatabaseConnection.GetTotalEvents().ToString();
            totalVenues  = DatabaseConnection.GetTotalVenues().ToString();
            totalArtists = DatabaseConnection.GetTotalArtists().ToString();

            decimal rev  = DatabaseConnection.GetTotalRevenue();
            // Format is like P120.5M, P6.7B, etc.
            totalRevenue = rev >= 1_000_000_000 ? $"₱{rev / 1_000_000_000:0.#}B"
                         : rev >= 1_000_000     ? $"₱{rev / 1_000_000:0.#}M"
                         : rev >= 1_000         ? $"₱{rev / 1_000:0.#}K"
                         : $"₱{rev:0}";
        }
        catch
        {
        }

        ContentPanel.Controls.Add(CreateMetricCard("Total Events",   totalEvents,  40,  112, Theme.AccentBlue, Color.White));
        ContentPanel.Controls.Add(CreateMetricCard("Partner Venues", totalVenues,  285, 112, Theme.AccentPink, Theme.DeepText));
        ContentPanel.Controls.Add(CreateMetricCard("Artists Booked", totalArtists, 530, 112, Theme.PanelBlue,  Theme.DeepText));
        ContentPanel.Controls.Add(CreateMetricCard("Total Revenue",  totalRevenue, 775, 112, Theme.SoftPink,   Theme.DeepText));

        // Next upcoming event card
        Panel lineupCard = Theme.CreateCard(40, 262, 510, 330);
        lineupCard.Controls.Add(new Label
        {
            Text      = "Next Upcoming Event",
            Font      = Theme.TitleFont(18),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(24, 22)
        });

        // Defaults shown while loading or error
        string eventLine1 = "No upcoming events found.";
        string eventLine2 = "";

        try
        {
            var ev = DatabaseConnection.GetNextUpcomingEvent();
            if (ev != null)
            {
                eventLine1 = $"{ev.EventDate:MMMM d, yyyy}  •  {ev.VenueName}";
                eventLine2 = $"{ev.EventName}\n{ev.Location}";
            }
        }
        catch { eventLine1 = "Could not load event data."; }

        lineupCard.Controls.Add(new Label
        {
            Text      = eventLine1,
            Font      = Theme.SubtitleFont(10),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(28, 70)
        });
        lineupCard.Controls.Add(new Label
        {
            Text      = eventLine2,
            Font      = Theme.TitleFont(16),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(28, 96)
        });

        Button reportButton = Theme.CreateSecondaryButton("Open Reports", 28, 262, 180, 42);
        reportButton.Click += (_, _) => Navigation.Open(this, new ReportGeneratorForm());
        lineupCard.Controls.Add(reportButton);

        ContentPanel.Controls.Add(lineupCard);

        // Recent ticket sales card
        Panel salesCard = Theme.CreateCard(574, 262, 420, 330, Theme.SoftPink);
        salesCard.Controls.Add(new Label
        {
            Text      = "Recent Ticket Sales",
            Font      = Theme.TitleFont(18),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(24, 22)
        });

        try
        {
            var sales = DatabaseConnection.GetRecentSales(5);
            int sy    = 68;

            if (sales.Count == 0)
            {
                salesCard.Controls.Add(new Label
                {
                    Text      = "No sales data found.",
                    Font      = Theme.BodyFont(10),
                    ForeColor = Theme.MutedText,
                    AutoSize  = true,
                    Location  = new Point(28, sy)
                });
            }
            else
            {
                foreach (var s in sales)
                {
                    // Row: event name left, tickets sold right
                    salesCard.Controls.Add(new Label
                    {
                        Text      = $"• {s.EventName}  ({s.ArtistName})",
                        Font      = Theme.BodyFont(10),
                        ForeColor = Theme.DeepText,
                        AutoSize  = true,
                        Location  = new Point(28, sy)
                    });
                    salesCard.Controls.Add(new Label
                    {
                        Text      = $"{s.TicketsSold:N0} tickets",
                        Font      = Theme.SubtitleFont(9.5f),
                        ForeColor = Theme.MutedText,
                        AutoSize  = true,
                        Location  = new Point(28, sy + 18)
                    });
                    sy += 46;
                }
            }
        }
        catch
        {
            salesCard.Controls.Add(new Label
            {
                Text      = "Could not load sales data.",
                Font      = Theme.BodyFont(10),
                ForeColor = Theme.MutedText,
                AutoSize  = true,
                Location  = new Point(28, 68)
            });
        }

        Button aboutButton = Theme.CreatePrimaryButton("View System Info", 28, 262, 180, 42);
        aboutButton.Click += (_, _) => Navigation.Open(this, new AboutProgramForm());
        salesCard.Controls.Add(aboutButton);

        ContentPanel.Controls.Add(salesCard);
    }

    private static Panel CreateMetricCard(string title, string value,
                                          int x, int y, Color fill, Color textColor)
    {
        Panel panel = Theme.CreateCard(x, y, 220, 120, fill);
        panel.Controls.Add(new Label
        {
            Text      = title,
            Font      = Theme.SubtitleFont(10),
            ForeColor = textColor,
            AutoSize  = true,
            Location  = new Point(20, 24)
        });
        panel.Controls.Add(new Label
        {
            Text      = value,
            Font      = Theme.TitleFont(30),
            ForeColor = textColor,
            AutoSize  = true,
            Location  = new Point(18, 48)
        });
        return panel;
    }
}