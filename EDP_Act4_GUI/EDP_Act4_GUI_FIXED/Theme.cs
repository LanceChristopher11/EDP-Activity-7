using System.Drawing.Drawing2D;

namespace ConcertEventSystemUI;

internal static class Theme
{
    public static readonly Color Background = Color.FromArgb(247, 250, 255);
    public static readonly Color PanelBlue = Color.FromArgb(213, 232, 255);
    public static readonly Color AccentBlue = Color.FromArgb(126, 180, 255);
    public static readonly Color AccentPink = Color.FromArgb(255, 191, 216);
    public static readonly Color SoftPink = Color.FromArgb(255, 227, 238);
    public static readonly Color DeepText = Color.FromArgb(54, 74, 112);
    public static readonly Color MutedText = Color.FromArgb(105, 120, 145);
    public static readonly Color WhiteCard = Color.FromArgb(255, 255, 255);

    public static Font TitleFont(float size = 22f) => new("Segoe UI Semibold", size, FontStyle.Bold);
    public static Font SubtitleFont(float size = 10.5f) => new("Segoe UI", size, FontStyle.Regular);
    public static Font BodyFont(float size = 10f) => new("Segoe UI", size, FontStyle.Regular);
    public static Font ButtonFont(float size = 10.5f) => new("Segoe UI Semibold", size, FontStyle.Bold);

    public static Panel CreateCard(int x, int y, int width, int height, Color? backColor = null)
    {
        Panel card = new()
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = backColor ?? WhiteCard
        };

        card.Paint += (_, e) =>
        {
            using GraphicsPath path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 24);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using SolidBrush brush = new(card.BackColor);
            e.Graphics.FillPath(brush, path);
        };

        ApplyRoundedRegion(card, 24);
        return card;
    }

    public static Button CreatePrimaryButton(string text, int x, int y, int width = 160, int height = 44)
    {
        Button button = new()
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            FlatStyle = FlatStyle.Flat,
            BackColor = AccentBlue,
            ForeColor = Color.White,
            Font = ButtonFont(),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.Paint += (_, e) => DrawRoundedButton(button, e, AccentBlue);
        return button;
    }

    public static Button CreateSecondaryButton(string text, int x, int y, int width = 160, int height = 44)
    {
        Button button = new()
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            FlatStyle = FlatStyle.Flat,
            BackColor = SoftPink,
            ForeColor = DeepText,
            Font = ButtonFont(),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.Paint += (_, e) => DrawRoundedButton(button, e, SoftPink);
        return button;
    }

    public static TextBox CreateTextBox(string placeholder, int x, int y, int width, bool isPassword = false)
    {
        TextBox textBox = new()
        {
            Location = new Point(x, y),
            Size = new Size(width, 36),
            BorderStyle = BorderStyle.None,
            Font = BodyFont(11),
            BackColor = Color.White,
            ForeColor = MutedText,
            Text = placeholder,
            Tag = placeholder
        };

        if (isPassword)
        {
            textBox.Enter += (_, _) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = string.Empty;
                    textBox.ForeColor = DeepText;
                    textBox.UseSystemPasswordChar = true;
                }
            };
            textBox.Leave += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.UseSystemPasswordChar = false;
                    textBox.Text = placeholder;
                    textBox.ForeColor = MutedText;
                }
            };
        }
        else
        {
            textBox.Enter += (_, _) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = string.Empty;
                    textBox.ForeColor = DeepText;
                }
            };
            textBox.Leave += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = MutedText;
                }
            };
        }

        return textBox;
    }

    public static Panel WrapInput(TextBox textBox)
    {
        Panel wrapper = new()
        {
            Size = new Size(textBox.Width + 24, 48),
            Location = new Point(textBox.Left - 12, textBox.Top - 6),
            BackColor = Color.White
        };

        wrapper.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using GraphicsPath path = RoundedRect(new Rectangle(0, 0, wrapper.Width - 1, wrapper.Height - 1), 18);
            using SolidBrush brush = new(Color.White);
            using Pen pen = new(PanelBlue, 1.4f);
            e.Graphics.FillPath(brush, path);
            e.Graphics.DrawPath(pen, path);
        };

        ApplyRoundedRegion(wrapper, 18);
        textBox.Parent = wrapper;
        textBox.Location = new Point(12, 13);
        return wrapper;
    }

    public static void ApplyFormStyle(Form form, string title)
    {
        form.Text = title;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.BackColor = Background;
        form.Font = BodyFont();
        form.FormBorderStyle = FormBorderStyle.FixedSingle;
        form.MaximizeBox = false;
    }

    private static void DrawRoundedButton(Button button, PaintEventArgs e, Color fillColor)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = RoundedRect(new Rectangle(0, 0, button.Width - 1, button.Height - 1), 18);
        using SolidBrush brush = new(fillColor);
        e.Graphics.FillPath(brush, path);
        TextRenderer.DrawText(
            e.Graphics,
            button.Text,
            button.Font,
            button.ClientRectangle,
            button.ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static void ApplyRoundedRegion(Control control, int radius)
    {
        using GraphicsPath path = RoundedRect(new Rectangle(0, 0, control.Width, control.Height), radius);
        control.Region = new Region(path);
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        GraphicsPath path = new();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}