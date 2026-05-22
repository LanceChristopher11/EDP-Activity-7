namespace ConcertEventSystemUI;

public class PasswordRecoveryForm : ShellForm
{
    public PasswordRecoveryForm() : base("Lance Fest | Password Recovery", "Password Recovery")
    {
        BuildUi();
    }

    private void BuildUi()
    {
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Password Recovery",
            Font      = Theme.TitleFont(28),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 28)
        });
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Enter your registered email to look up your account, then set a new password.",
            Font      = Theme.SubtitleFont(11),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(40, 72)
        });

        Panel step1Card = Theme.CreateCard(40, 120, 560, 200);

        step1Card.Controls.Add(new Label
        {
            Text      = "Step 1 — Find your account",
            Font      = Theme.TitleFont(14),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(28, 22)
        });
        step1Card.Controls.Add(new Label
        {
            Text      = "Email address",
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(30, 62)
        });

        TextBox emailBox  = Theme.CreateTextBox("your@email.com", 32, 82, 330);
        Panel   emailWrap = Theme.WrapInput(emailBox);
        step1Card.Controls.Add(emailWrap);

        Label step1Feedback = new()
        {
            Text      = "",
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(30, 144),
            Visible   = false
        };
        step1Card.Controls.Add(step1Feedback);

        Button findBtn = Theme.CreatePrimaryButton("Find Account", 380, 88, 148, 44);
        step1Card.Controls.Add(findBtn);

        ContentPanel.Controls.Add(step1Card);

        Panel step2Card = Theme.CreateCard(40, 340, 560, 280);
        step2Card.Visible = false;

        Label step2Title = new()
        {
            Text      = "Step 2 — Set a new password",
            Font      = Theme.TitleFont(14),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(28, 22)
        };
        step2Card.Controls.Add(step2Title);

        Label accountBanner = new()
        {
            Text      = "",
            Font      = Theme.SubtitleFont(10),
            ForeColor = Color.FromArgb(50, 160, 110),
            AutoSize  = true,
            Location  = new Point(30, 58)
        };
        step2Card.Controls.Add(accountBanner);

        step2Card.Controls.Add(new Label
        {
            Text      = "New password",
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(30, 86)
        });
        TextBox newPassBox  = Theme.CreateTextBox("New password", 32, 106, 330, true);
        Panel   newPassWrap = Theme.WrapInput(newPassBox);
        step2Card.Controls.Add(newPassWrap);

        step2Card.Controls.Add(new Label
        {
            Text      = "Confirm new password",
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(30, 162)
        });
        TextBox confirmBox  = Theme.CreateTextBox("Confirm password", 32, 182, 330, true);
        Panel   confirmWrap = Theme.WrapInput(confirmBox);
        step2Card.Controls.Add(confirmWrap);

        Label step2Feedback = new()
        {
            Text      = "",
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(30, 238),
            Visible   = false
        };
        step2Card.Controls.Add(step2Feedback);

        Button resetBtn = Theme.CreatePrimaryButton("Save New Password", 380, 188, 162, 44);
        step2Card.Controls.Add(resetBtn);

        ContentPanel.Controls.Add(step2Card);

        Panel tips = Theme.CreateCard(630, 120, 310, 220, Theme.SoftPink);
        tips.Controls.Add(new Label
        {
            Text      = "Recovery Tips",
            Font      = Theme.TitleFont(14),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(24, 22)
        });
        tips.Controls.Add(new Label
        {
            Text =
                "• Use the email you registered with\n\n" +
                "• Password must be at least\n  6 characters\n\n" +
                "• Contact admin if you no longer\n  have access to your email",
            Font      = Theme.BodyFont(10.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(24, 64)
        });
        ContentPanel.Controls.Add(tips);

        Button backBtn = Theme.CreateSecondaryButton("Back to Login", 630, 360, 180, 44);
        backBtn.Click += (_, _) => Navigation.Open(this, new LoginForm());
        ContentPanel.Controls.Add(backBtn);

        int foundUserId = 0;

        findBtn.Click += (_, _) =>
        {
            string email = emailBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || email == "your@email.com")
            {
                step1Feedback.ForeColor = Color.FromArgb(200, 60, 80);
                step1Feedback.Text      = "Please enter your email address.";
                step1Feedback.Visible   = true;
                step2Card.Visible       = false;
                return;
            }

            try
            {
                UserRecord? found = DatabaseConnection.FindAccountByEmail(email);

                if (found is null)
                {
                    step1Feedback.ForeColor = Color.FromArgb(200, 60, 80);
                    step1Feedback.Text      = "No account found with that email address.";
                    step1Feedback.Visible   = true;
                    step2Card.Visible       = false;
                    foundUserId             = 0;
                }
                else
                {
                    foundUserId             = found.Id;
                    step1Feedback.ForeColor = Color.FromArgb(50, 160, 110);
                    step1Feedback.Text      = $"Account found: {found.Username}";
                    step1Feedback.Visible   = true;

                    accountBanner.Text = $"Resetting password for:  {found.Username}  ({found.Email})";

                    newPassBox.Text  = "New password";
                    newPassBox.ForeColor = Theme.MutedText;
                    confirmBox.Text  = "Confirm password";
                    confirmBox.ForeColor = Theme.MutedText;
                    step2Feedback.Visible = false;

                    step2Card.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        resetBtn.Click += (_, _) =>
        {
            if (foundUserId == 0)
            {
                step2Feedback.ForeColor = Color.FromArgb(200, 60, 80);
                step2Feedback.Text      = "Please complete Step 1 first.";
                step2Feedback.Visible   = true;
                return;
            }

            string newPass  = newPassBox.Text;
            string confirm  = confirmBox.Text;

            bool newIsPlaceholder     = newPass  == "New password"      || string.IsNullOrWhiteSpace(newPass);
            bool confirmIsPlaceholder = confirm  == "Confirm password"  || string.IsNullOrWhiteSpace(confirm);

            if (newIsPlaceholder || confirmIsPlaceholder)
            {
                step2Feedback.ForeColor = Color.FromArgb(200, 60, 80);
                step2Feedback.Text      = "Please fill in both password fields.";
                step2Feedback.Visible   = true;
                return;
            }

            if (newPass.Length < 6)
            {
                step2Feedback.ForeColor = Color.FromArgb(200, 60, 80);
                step2Feedback.Text      = "Password must be at least 6 characters.";
                step2Feedback.Visible   = true;
                return;
            }

            if (newPass != confirm)
            {
                step2Feedback.ForeColor = Color.FromArgb(200, 60, 80);
                step2Feedback.Text      = "Passwords do not match.";
                step2Feedback.Visible   = true;
                return;
            }

            try
            {
                bool ok = DatabaseConnection.ResetPassword(foundUserId, newPass);

                if (ok)
                {
                    step2Feedback.ForeColor = Color.FromArgb(50, 160, 110);
                    step2Feedback.Text      = "Password updated successfully!";
                    step2Feedback.Visible   = true;

                    var go = MessageBox.Show(
                        "Your password has been updated.\nGo back to the login screen?",
                        "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (go == DialogResult.Yes)
                        Navigation.Open(this, new LoginForm());
                }
                else
                {
                    step2Feedback.ForeColor = Color.FromArgb(200, 60, 80);
                    step2Feedback.Text      = "Update failed — please try again.";
                    step2Feedback.Visible   = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
    }
}