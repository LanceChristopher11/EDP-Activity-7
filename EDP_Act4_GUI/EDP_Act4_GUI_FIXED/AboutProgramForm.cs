namespace ConcertEventSystemUI;

public class AboutProgramForm : ShellForm
{
    public AboutProgramForm() : base("Lance Fest | About the Program", "About Program")
    {
        BuildUi();
    }

    private void BuildUi()
    {
        Label title = new()
        {
            Text = "About the Program",
            Font = Theme.TitleFont(28),
            ForeColor = Theme.DeepText,
            AutoSize = true,
            Location = new Point(36, 26)
        };

        Panel overview = Theme.CreateCard(40, 90, 620, 230);
        overview.Controls.Add(new Label
        {
            Text = "Lance Fest is a concert event information system designed to organize shows, venues, artists, and reports in one user-friendly desktop interface.",
            Font = Theme.BodyFont(11),
            ForeColor = Theme.MutedText,
            AutoSize = false,
            Size = new Size(560, 80),
            Location = new Point(28, 34)
        });
        overview.Controls.Add(new Label
        {
            Text = "Purpose:\nProvide event staff and administrators with an easy visual platform for monitoring event operations.",
            Font = Theme.BodyFont(11),
            ForeColor = Theme.DeepText,
            AutoSize = false,
            Size = new Size(540, 70),
            Location = new Point(28, 120)
        });

        Panel modules = Theme.CreateCard(690, 90, 260, 380, Theme.SoftPink);
        modules.Controls.Add(new Label
        {
            Text = "Included Forms",
            Font = Theme.TitleFont(18),
            ForeColor = Theme.DeepText,
            AutoSize = true,
            Location = new Point(24, 24)
        });
        modules.Controls.Add(new Label
        {
            Text = "1. Login\n2. Password Recovery\n3. About the Program\n4. Dashboard\n5. Report Generator",
            Font = Theme.BodyFont(11),
            ForeColor = Theme.MutedText,
            AutoSize = true,
            Location = new Point(28, 78)
        });

        Panel design = Theme.CreateCard(40, 350, 620, 190, Theme.PanelBlue);
        design.Controls.Add(new Label
        {
            Text = "Design Direction",
            Font = Theme.TitleFont(18),
            ForeColor = Theme.DeepText,
            AutoSize = true,
            Location = new Point(24, 24)
        });
        design.Controls.Add(new Label
        {
            Text = "The interface uses pastel blue and pink because I like those colors.",
            Font = Theme.BodyFont(11),
            ForeColor = Theme.DeepText,
            AutoSize = false,
            Size = new Size(550, 60),
            Location = new Point(28, 68)
        });

        Button dashboardButton = Theme.CreatePrimaryButton("Go to Dashboard", 28, 300, 180, 42);
        dashboardButton.Click += (_, _) => Navigation.Open(this, new DashboardForm());
        modules.Controls.Add(dashboardButton);

        Button act7Button = Theme.CreateSecondaryButton("Just added for activity 7 lol", 40, 560, 280, 44);
        // Button is intended to not do anything HAHSHA
        ContentPanel.Controls.Add(act7Button);

        ContentPanel.Controls.Add(title);
        ContentPanel.Controls.Add(overview);
        ContentPanel.Controls.Add(modules);
        ContentPanel.Controls.Add(design);
    }
}