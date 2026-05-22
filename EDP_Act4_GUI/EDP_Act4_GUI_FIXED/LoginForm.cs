namespace ConcertEventSystemUI;

public class LoginForm : Form
{
    public LoginForm()
    {
        Size = new Size(1280, 760);
        Theme.ApplyFormStyle(this, "Lance Fest | Login");
        BuildUi();
    }

    private void BuildUi()
    {
        // Left panel
        Panel left = new()
        {
            Dock      = DockStyle.Left,
            Width     = 560,
            BackColor = Theme.PanelBlue
        };

        Label title = new()
        {
            Text      = "Lance Fest",
            Font      = Theme.TitleFont(34),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(56, 80)
        };

        Label subtitle = new()
        {
            Text      = "Manage concert schedules, artist lineups,\nand audience reports — all in one place.",
            Font      = Theme.SubtitleFont(12),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(60, 138)
        };

        Panel highlight = Theme.CreateCard(56, 250, 400, 210, Theme.WhiteCard);
        highlight.Controls.Add(new Label
        {
            Text      = "What's Inside",
            Font      = Theme.TitleFont(16),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(28, 24)
        });
        highlight.Controls.Add(new Label
        {
            Text =
                "→  Dashboard & event overview\n" +
                "→  User account management\n" +
                "→  Artist and venue reports\n" +
                "→  Secure login & recovery",
            Font      = Theme.BodyFont(11),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(32, 70)
        });

        left.Controls.Add(title);
        left.Controls.Add(subtitle);
        left.Controls.Add(highlight);
        left.Controls.Add(CreateBubble("Coachella 2026",   70,  500, Theme.AccentPink,  Theme.DeepText));
        left.Controls.Add(CreateBubble("Hamilton Musical", 256, 558, Theme.AccentBlue,  Color.White));

        // Right panel
        Panel right = new()
        {
            Dock      = DockStyle.Fill,
            BackColor = Theme.Background
        };

        Panel card = Theme.CreateCard(120, 100, 420, 500);

        Label loginTitle = new()
        {
            Text      = "Welcome back",
            Font      = Theme.TitleFont(24),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 42)
        };

        Label loginText = new()
        {
            Text      = "Sign in to access the concert event system.",
            Font      = Theme.SubtitleFont(10.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(38, 88)
        };

        TextBox username = Theme.CreateTextBox("Username", 40, 150, 320);
        Panel   usernameWrap = Theme.WrapInput(username);

        TextBox password = Theme.CreateTextBox("Password", 40, 226, 320, true);
        Panel   passwordWrap = Theme.WrapInput(password);

        // error label — hidden until needed
        Label errorLabel = new()
        {
            Text      = "",
            Font      = Theme.SubtitleFont(9),
            ForeColor = Color.FromArgb(200, 60, 80),
            AutoSize  = true,
            Location  = new Point(42, 294),
            Visible   = false
        };

        LinkLabel forgot = new()
        {
            Text             = "Forgot password?",
            AutoSize         = true,
            LinkColor        = Theme.AccentBlue,
            ActiveLinkColor  = Theme.AccentBlue,
            VisitedLinkColor = Theme.AccentBlue,
            Font             = Theme.SubtitleFont(9.5f),
            Location         = new Point(236, 295)
        };
        forgot.Click += (_, _) => Navigation.Open(this, new PasswordRecoveryForm());

        Button loginButton = Theme.CreatePrimaryButton("Sign In", 40, 330, 320, 46);
        loginButton.Click += (_, _) =>
        {
            string uname = username.Text.Trim();
            string pass  = password.Text;

            if (uname == "Username" || string.IsNullOrWhiteSpace(uname) ||
                pass  == "Password" || string.IsNullOrWhiteSpace(pass))
            {
                errorLabel.Text    = "Please enter your username and password.";
                errorLabel.Visible = true;
                return;
            }

            try
            {
                UserRecord? user = DatabaseConnection.ValidateLogin(uname, pass);
                if (user is null)
                {
                    errorLabel.Text    = "Incorrect username or password, or account is inactive.";
                    errorLabel.Visible = true;
                }
                else
                {
                    errorLabel.Visible = false;
                    Navigation.Open(this, new DashboardForm());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not connect to the database.\n\n{ex.Message}",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        Button aboutButton = Theme.CreateSecondaryButton("About the Program", 40, 392, 320, 46);
        aboutButton.Click += (_, _) => Navigation.Open(this, new AboutProgramForm());

        card.Controls.Add(loginTitle);
        card.Controls.Add(loginText);
        card.Controls.Add(usernameWrap);
        card.Controls.Add(passwordWrap);
        card.Controls.Add(errorLabel);
        card.Controls.Add(forgot);
        card.Controls.Add(loginButton);
        card.Controls.Add(aboutButton);

        right.Controls.Add(card);
        Controls.Add(right);
        Controls.Add(left);
    }

    private static Label CreateBubble(string text, int x, int y, Color fill, Color fg)
    {
        Label label = new()
        {
            Text      = text,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = fill,
            ForeColor = fg,
            Font      = Theme.ButtonFont(10),
            Location  = new Point(x, y),
            Size      = new Size(158, 48)
        };

        label.Paint += (_, e) =>
        {
            using var brush = new SolidBrush(fill);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(brush, 0, 0, label.Width - 1, label.Height - 1);
            TextRenderer.DrawText(
                e.Graphics, label.Text, label.Font, label.ClientRectangle, label.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };

        return label;
    }
}