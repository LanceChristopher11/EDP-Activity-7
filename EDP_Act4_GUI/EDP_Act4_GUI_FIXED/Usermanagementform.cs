namespace ConcertEventSystemUI;

public class UserManagementForm : ShellForm
{
    private DataGridView _grid        = null!;
    private TextBox      _searchBox   = null!;
    private Label        _statusLabel = null!;

    // Filter tab buttons
    private Button _tabAll      = null!;
    private Button _tabActive   = null!;
    private Button _tabInactive = null!;
    private bool?  _activeFilter = null;

    // Right-side add/edit panel
    private Panel    _editPanel      = null!;
    private Label    _editTitle      = null!;
    private TextBox  _tbUsername     = null!;
    private TextBox  _tbFullName     = null!;
    private TextBox  _tbEmail        = null!;
    private TextBox  _tbPassword     = null!;
    private TextBox  _tbConfirmPass  = null!;
    private Label    _confirmPassLbl = null!;
    private ComboBox _cbRole         = null!;
    private Label    _editError      = null!;

    private int  _editingId = 0;
    private bool _isEditing = false;

    public UserManagementForm() : base("Lance Fest | User Management", "User Management")
    {
        BuildUi();
        LoadUsers();
    }

    // UI
    private void BuildUi()
    {
        // Page header
        ContentPanel.Controls.Add(new Label
        {
            Text      = "User Management",
            Font      = Theme.TitleFont(28),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(36, 28)
        });
        ContentPanel.Controls.Add(new Label
        {
            Text      = "Add, update, activate, or deactivate system accounts.",
            Font      = Theme.SubtitleFont(11),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(40, 72)
        });

        // Filter tabs (All / Active / Inactive) ─────────────────────────────
        _tabAll      = MakeTabButton("All",      40,  108);
        _tabActive   = MakeTabButton("Active",   130, 108);
        _tabInactive = MakeTabButton("Inactive", 220, 108);

        _tabAll.Click      += (_, _) => { _activeFilter = null;  ApplyTabStyle(); LoadUsers(); };
        _tabActive.Click   += (_, _) => { _activeFilter = true;  ApplyTabStyle(); LoadUsers(); };
        _tabInactive.Click += (_, _) => { _activeFilter = false; ApplyTabStyle(); LoadUsers(); };

        ContentPanel.Controls.Add(_tabAll);
        ContentPanel.Controls.Add(_tabActive);
        ContentPanel.Controls.Add(_tabInactive);
        ApplyTabStyle();

        // Search + Add toolbar
        _searchBox = Theme.CreateTextBox("Search by name, username, or email…", 40, 160, 280);
        Panel searchWrap = Theme.WrapInput(_searchBox);
        searchWrap.Location = new Point(40, 154);
        ContentPanel.Controls.Add(searchWrap);

        Button searchBtn = Theme.CreateSecondaryButton("Search", 338, 160, 100, 44);
        searchBtn.Click += (_, _) => LoadUsers(_searchBox.Text);
        ContentPanel.Controls.Add(searchBtn);

        Button addBtn = Theme.CreatePrimaryButton("+ Add User", 452, 160, 130, 44);
        addBtn.Click += (_, _) => OpenEditPanel(null);
        ContentPanel.Controls.Add(addBtn);

        _statusLabel = new Label
        {
            Text      = "",
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(600, 172)
        };
        ContentPanel.Controls.Add(_statusLabel);

        // User grid
        _grid = new DataGridView
        {
            Location              = new Point(40, 214),
            Size                  = new Size(600, 454),
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

        _grid.ColumnHeadersDefaultCellStyle.BackColor           = Theme.PanelBlue;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor           = Theme.DeepText;
        _grid.ColumnHeadersDefaultCellStyle.Font                = Theme.ButtonFont(9.5f);
        _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor  = Theme.PanelBlue;
        _grid.ColumnHeadersHeight       = 40;
        _grid.EnableHeadersVisualStyles  = false;
        _grid.DefaultCellStyle.SelectionBackColor = Theme.AccentPink;
        _grid.DefaultCellStyle.SelectionForeColor = Theme.DeepText;
        _grid.RowTemplate.Height = 36;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",      HeaderText = "ID",       FillWeight = 28  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUsername", HeaderText = "Username", FillWeight = 90  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFullName", HeaderText = "Full Name",FillWeight = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmail",    HeaderText = "Email",   FillWeight = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRole",     HeaderText = "Role",    FillWeight = 55  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus",   HeaderText = "Status",  FillWeight = 60  });

        _grid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "colEdit", HeaderText = "", Text = "Edit",
            UseColumnTextForButtonValue = true, FillWeight = 42
        });
        _grid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "colToggle", HeaderText = "", Text = "Toggle",
            UseColumnTextForButtonValue = true, FillWeight = 72
        });

        _grid.CellClick      += Grid_CellClick;
        _grid.CellFormatting += Grid_CellFormatting;
        ContentPanel.Controls.Add(_grid);

        // Add/Edit panel
        _editPanel = Theme.CreateCard(658, 154, 350, 514);
        _editPanel.Visible = false;

        _editTitle = new Label
        {
            Text      = "Add New User",
            Font      = Theme.TitleFont(16),
            ForeColor = Theme.DeepText,
            AutoSize  = true,
            Location  = new Point(26, 22)
        };
        _editPanel.Controls.Add(_editTitle);

        int ey = 62;
        AddFormRow("Username",    ref ey, out _tbUsername,    false);
        AddFormRow("Full Name",   ref ey, out _tbFullName,    false);
        AddFormRow("Email",       ref ey, out _tbEmail,       false);
        AddFormRow("Password",    ref ey, out _tbPassword,    true);

        // Confirm password row
        _confirmPassLbl = new Label
        {
            Text      = "Confirm Password",
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(26, ey)
        };
        _editPanel.Controls.Add(_confirmPassLbl);
        ey += 20;
        _tbConfirmPass = Theme.CreateTextBox("Confirm Password", 26, ey, 298, true);
        Panel confirmWrap = Theme.WrapInput(_tbConfirmPass);
        confirmWrap.Location = new Point(26, ey - 4);
        _editPanel.Controls.Add(confirmWrap);
        ey += 56;

        // Role
        _editPanel.Controls.Add(new Label
        {
            Text      = "Role",
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(26, ey)
        });
        ey += 20;

        _cbRole = new ComboBox
        {
            Location      = new Point(26, ey),
            Size          = new Size(298, 30),
            FlatStyle     = FlatStyle.Flat,
            BackColor     = Color.White,
            ForeColor     = Theme.DeepText,
            Font          = Theme.BodyFont(10),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cbRole.Items.AddRange(["admin", "staff"]);
        _cbRole.SelectedIndex = 1;
        _editPanel.Controls.Add(_cbRole);
        ey += 46;

        // Inline error label
        _editError = new Label
        {
            Text      = "",
            Font      = Theme.SubtitleFont(9f),
            ForeColor = Color.FromArgb(200, 60, 80),
            AutoSize  = false,
            Size      = new Size(298, 32),
            Location  = new Point(26, ey),
            Visible   = false
        };
        _editPanel.Controls.Add(_editError);
        ey += 36;

        Button saveBtn   = Theme.CreatePrimaryButton("Save",   26,  ey, 150, 44);
        Button cancelBtn = Theme.CreateSecondaryButton("Cancel", 190, ey, 120, 44);
        saveBtn.Click   += BtnSave_Click;
        cancelBtn.Click += (_, _) => { _editPanel.Visible = false; _editError.Visible = false; };

        _editPanel.Controls.Add(saveBtn);
        _editPanel.Controls.Add(cancelBtn);
        ContentPanel.Controls.Add(_editPanel);
    }

    // Tab styling
    private Button MakeTabButton(string text, int x, int y)
    {
        var btn = new Button
        {
            Text      = text,
            Location  = new Point(x, y),
            Size      = new Size(84, 32),
            FlatStyle = FlatStyle.Flat,
            Font      = Theme.ButtonFont(9.5f),
            Cursor    = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private void ApplyTabStyle()
    {
        SetTab(_tabAll,      _activeFilter == null);
        SetTab(_tabActive,   _activeFilter == true);
        SetTab(_tabInactive, _activeFilter == false);
    }

    private static void SetTab(Button btn, bool active)
    {
        btn.BackColor = active ? Theme.AccentBlue : Color.FromArgb(230, 240, 255);
        btn.ForeColor = active ? Color.White : Theme.DeepText;
    }

    // Data loading
    private void LoadUsers(string search = "")
    {
        // Ignore placeholder text
        if (search == "Search by name, username, or email…") search = "";

        _grid.Rows.Clear();
        try
        {
            var users = DatabaseConnection.GetAllUsers(search, _activeFilter);
            foreach (var u in users)
            {
                _grid.Rows.Add(
                    u.Id,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.IsActive ? "Active" : "Inactive",
                    "Edit",
                    u.IsActive ? "Deactivate" : "Activate"
                );
            }

            string tabLabel = _activeFilter switch
            {
                true  => "active",
                false => "inactive",
                null  => "total"
            };
            SetStatus($"{users.Count} {tabLabel} user(s) found.", false);
        }
        catch (Exception ex)
        {
            SetStatus($"DB error: {ex.Message}", true);
        }
    }

    // Grid events
    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var row = _grid.Rows[e.RowIndex];

        int    id       = Convert.ToInt32(row.Cells["colId"].Value);
        bool   isActive = row.Cells["colStatus"].Value?.ToString() == "Active";
        string username = row.Cells["colUsername"].Value?.ToString() ?? "";
        string fullName = row.Cells["colFullName"].Value?.ToString() ?? "";
        string email    = row.Cells["colEmail"].Value?.ToString()    ?? "";
        string role     = row.Cells["colRole"].Value?.ToString()     ?? "staff";

        if (e.ColumnIndex == _grid.Columns["colEdit"]!.Index)
        {
            OpenEditPanel(new UserRecord(id, username, email, fullName, role, isActive));
        }
        else if (e.ColumnIndex == _grid.Columns["colToggle"]!.Index)
        {
            string action  = isActive ? "deactivate" : "activate";
            var    confirm = MessageBox.Show(
                $"Are you sure you want to {action} \"{username}\"?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                DatabaseConnection.SetUserActive(id, !isActive);
                LoadUsers(_searchBox.Text);
                SetStatus(isActive ? "Account deactivated." : "Account activated.", false);
            }
            catch (Exception ex) { SetStatus($"Error: {ex.Message}", true); }
        }
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
        string col = _grid.Columns[e.ColumnIndex].Name;

        if (col == "colStatus" && e.Value is not null)
        {
            e.CellStyle.ForeColor = e.Value.ToString() == "Active"
                ? Color.FromArgb(50, 160, 110)
                : Color.FromArgb(200, 60, 80);
            e.CellStyle.Font = Theme.ButtonFont(9.5f);
        }
        if (col is "colEdit" or "colToggle")
        {
            e.CellStyle.BackColor          = Theme.PanelBlue;
            e.CellStyle.ForeColor          = Theme.DeepText;
            e.CellStyle.SelectionBackColor = Theme.AccentBlue;
            e.CellStyle.SelectionForeColor = Color.White;
            e.CellStyle.Font               = Theme.ButtonFont(9);
        }
    }

    // Edit / Add panel
    private void OpenEditPanel(UserRecord? user)
    {
        _isEditing = user is not null;
        _editingId = user?.Id ?? 0;

        _editTitle.Text      = _isEditing ? "Edit User" : "Add New User";
        _tbUsername.ReadOnly = _isEditing;

        PopulateField(_tbUsername,    user?.Username ?? "");
        PopulateField(_tbFullName,    user?.FullName ?? "");
        PopulateField(_tbEmail,       user?.Email    ?? "");
        PopulateField(_tbPassword,    "");
        PopulateField(_tbConfirmPass, "");

        // On edit, password fields are optional; on add they are required
        string passHint = _isEditing ? "New password (leave blank to keep)" : "Password";
        _tbPassword.Tag = passHint;       // update placeholder tag
        _tbPassword.Text      = passHint;
        _tbPassword.ForeColor = Theme.MutedText;

        _confirmPassLbl.Text = _isEditing ? "Confirm new password (if changing)" : "Confirm Password";

        _cbRole.SelectedItem = user?.Role ?? "staff";
        _editError.Visible   = false;
        _editPanel.Visible   = true;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        _editError.Visible = false;

        string username = _tbUsername.Text.Trim();
        string fullName = _tbFullName.Text.Trim();
        string email    = _tbEmail.Text.Trim();
        string role     = _cbRole.SelectedItem?.ToString() ?? "staff";

        // Get password values
        string passHint    = _isEditing ? "New password (leave blank to keep)" : "Password";
        string confirmHint = _isEditing ? "Confirm new password (if changing)" : "Confirm Password";

        string password = _tbPassword.Text    == passHint    ? "" : _tbPassword.Text;
        string confirm  = _tbConfirmPass.Text == confirmHint ? "" : _tbConfirmPass.Text;

        // Validation
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(fullName)  ||
            string.IsNullOrWhiteSpace(email))
        {
            ShowEditError("Username, Full Name, and Email are required.");
            return;
        }

        if (!email.Contains('@') || !email.Contains('.'))
        {
            ShowEditError("Please enter a valid email address.");
            return;
        }

        if (!_isEditing && string.IsNullOrWhiteSpace(password))
        {
            ShowEditError("A password is required for new accounts.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            if (password.Length < 6)
            {
                ShowEditError("Password must be at least 6 characters.");
                return;
            }
            if (password != confirm)
            {
                ShowEditError("Passwords do not match.");
                return;
            }
        }

        // Save to Database
        try
        {
            if (_isEditing)
            {
                DatabaseConnection.UpdateUser(_editingId, email, fullName, role, password);
                SetStatus("User updated successfully.", false);
            }
            else
            {
                DatabaseConnection.AddUser(username, email, fullName, role, password);
                SetStatus("User added successfully.", false);
            }

            _editPanel.Visible = false;
            LoadUsers();
        }
        catch (Exception ex)
        {
            ShowEditError($"DB error: {ex.Message}");
        }
    }

    private void AddFormRow(string label, ref int y, out TextBox tb, bool isPassword)
    {
        _editPanel.Controls.Add(new Label
        {
            Text      = label,
            Font      = Theme.SubtitleFont(9.5f),
            ForeColor = Theme.MutedText,
            AutoSize  = true,
            Location  = new Point(26, y)
        });
        y += 20;

        tb = Theme.CreateTextBox(label, 26, y, 298, isPassword);
        Panel wrap = Theme.WrapInput(tb);
        wrap.Location = new Point(26, y - 4);
        _editPanel.Controls.Add(wrap);
        y += 56;
    }

    private static void PopulateField(TextBox tb, string value)
    {
        tb.Text      = value;
        tb.ForeColor = string.IsNullOrEmpty(value) ? Theme.MutedText : Theme.DeepText;
    }

    private void ShowEditError(string msg)
    {
        _editError.Text    = msg;
        _editError.Visible = true;
    }

    private void SetStatus(string msg, bool isError)
    {
        _statusLabel.ForeColor = isError ? Color.FromArgb(200, 60, 80) : Theme.MutedText;
        _statusLabel.Text      = msg;
    }
}