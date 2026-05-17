namespace Rounder.Windows;

public sealed class SettingsForm : Form
{
    private readonly AppSettings settings;
    private readonly List<CornerPreset> presets;
    private AppTheme theme = AppTheme.Current();
    private readonly CheckBox enabledBox = new() { Text = "Enable rounded corners", AutoSize = true };
    private readonly NumericUpDown radiusInput = new() { Minimum = 0, Maximum = 40, Width = 76 };
    private readonly TrackBar radiusSlider = new() { Minimum = 0, Maximum = 40, TickFrequency = 5, LargeChange = 5, SmallChange = 1, Dock = DockStyle.Fill };
    private readonly Button colorButton = new() { Text = "Custom...", AutoSize = true };
    private readonly Panel colorPreview = new() { Width = 42, Height = 28, BorderStyle = BorderStyle.FixedSingle, Tag = "color-preview" };
    private readonly CheckBox topLeftBox = new() { Text = "Top left", AutoSize = true };
    private readonly CheckBox topRightBox = new() { Text = "Top right", AutoSize = true };
    private readonly CheckBox bottomLeftBox = new() { Text = "Bottom left", AutoSize = true };
    private readonly CheckBox bottomRightBox = new() { Text = "Bottom right", AutoSize = true };
    private readonly CheckBox gamingBox = new() { Text = "Super Duper Gaming Mode", AutoSize = true };
    private readonly NumericUpDown speedInput = new() { Minimum = 1, Maximum = 50, DecimalPlaces = 1, Increment = 1, Width = 76 };
    private readonly NumericUpDown glowInput = new() { Minimum = 1, Maximum = 30, DecimalPlaces = 1, Increment = 1, Width = 76 };
    private readonly CheckedListBox displayList = new() { CheckOnClick = true, Dock = DockStyle.Fill, IntegralHeight = false };
    private readonly ListBox presetList = new() { Dock = DockStyle.Fill, IntegralHeight = false };
    private readonly Label presetDetails = new() { AutoSize = false, Dock = DockStyle.Fill, ForeColor = SystemColors.GrayText };

    public SettingsForm(AppSettings settings, List<CornerPreset> presets)
    {
        this.settings = settings;
        this.presets = presets;

        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        Icon = AppAssets.AppIcon();
        Text = "Rounder Settings";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(720, 520);
        ClientSize = new Size(940, 720);

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = theme.Window
        };
        shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        shell.Controls.Add(CreateHeader(), 0, 0);
        shell.Controls.Add(CreateTabs(), 0, 1);
        shell.Controls.Add(CreateFooter(), 0, 2);
        Controls.Add(shell);

        LoadSettingsIntoControls();
        RefreshPresetList();
        ApplyTheme();
        Microsoft.Win32.SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
    }

    public event EventHandler<AppSettings>? SettingsApplied;
    public event EventHandler? PresetsChanged;

    private Control CreateHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            Padding = new Padding(18, 14, 18, 12),
            BackColor = theme.SurfaceAlt
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var icon = new PictureBox
        {
            Image = AppAssets.HeaderImage(),
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(44, 44),
            Margin = new Padding(0, 0, 12, 0)
        };
        var title = new Label
        {
            Text = "Rounder",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 18F, FontStyle.Bold),
            Margin = new Padding(0)
        };
        var subtitle = new Label
        {
            Text = "Screen corner overlays tuned for multi-monitor Windows desktops.",
            AutoSize = true,
            ForeColor = theme.MutedText,
            Tag = "muted",
            Margin = new Padding(0, 4, 0, 0)
        };
        var titleStack = Stack(title, subtitle);
        titleStack.Margin = new Padding(0);

        header.Controls.Add(icon, 0, 0);
        header.Controls.Add(titleStack, 1, 0);
        return header;
    }

    private TabControl CreateTabs()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Padding = new Point(18, 6),
            Margin = new Padding(0),
            ImageList = AppAssets.TabImages()
        };
        var settingsPage = CreateSettingsTab();
        settingsPage.ImageKey = "settings";
        var presetsPage = CreatePresetsTab();
        presetsPage.ImageKey = "presets";
        var permissionsPage = CreatePermissionsTab();
        permissionsPage.ImageKey = "permissions";
        var creditsPage = CreateCreditsTab();
        creditsPage.ImageKey = "credits";
        tabs.TabPages.Add(settingsPage);
        tabs.TabPages.Add(presetsPage);
        tabs.TabPages.Add(permissionsPage);
        tabs.TabPages.Add(creditsPage);
        return tabs;
    }

    private Control CreateFooter()
    {
        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(18, 12, 18, 12),
            BackColor = theme.SurfaceAlt
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var hint = new Label
        {
            Text = "Apply updates the live overlays immediately.",
            AutoSize = true,
            ForeColor = theme.MutedText,
            Tag = "muted",
            Anchor = AnchorStyles.Left
        };
        var buttons = Row(
            FooterButton("Cancel", Close),
            FooterButton("Apply", Apply),
            FooterButton("OK", () =>
            {
                Apply();
                Close();
            }, prominent: true));

        footer.Controls.Add(hint, 0, 0);
        footer.Controls.Add(buttons, 1, 0);
        return footer;
    }

    private TabPage CreateSettingsTab()
    {
        var page = new TabPage("Settings") { BackColor = theme.Window };
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(18) };
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 4
        };

        ConfigureRadiusControls();
        ConfigureColorPicker();

        root.Controls.Add(Section("General", "Turn the corner overlays on or off.", enabledBox));
        root.Controls.Add(Section("Monitor Selection", "Choose the displays where Rounder should draw overlays.", CreateMonitorPanel()));
        root.Controls.Add(Section("Appearance", "Fine tune radius, color, and individual corner visibility.", CreateAppearancePanel()));
        root.Controls.Add(Section("Super Duper Gaming Mode", "Animated rainbow color and glow for the selected corners.", CreateGamingPanel()));

        scroll.Controls.Add(root);
        page.Controls.Add(scroll);
        return page;
    }

    private TabPage CreatePresetsTab()
    {
        var page = new TabPage("Presets") { BackColor = theme.Window };
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(18)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));

        presetList.SelectedIndexChanged += (_, _) => UpdatePresetDetails();

        var buttons = Stack(
            SidebarButton("Apply", ApplySelectedPreset, true),
            SidebarButton("Save Current...", AddPresetFromCurrentSettings),
            SidebarButton("Edit...", EditSelectedPreset),
            SidebarButton("Delete", DeleteSelectedPreset),
            SidebarButton("Import...", ImportPresets),
            SidebarButton("Export...", ExportPresets));

        presetDetails.Padding = new Padding(4, 10, 4, 0);
        root.Controls.Add(presetList, 0, 0);
        root.Controls.Add(buttons, 1, 0);
        root.Controls.Add(presetDetails, 0, 1);
        root.SetColumnSpan(presetDetails, 2);

        page.Controls.Add(root);
        return page;
    }

    private static TabPage CreatePermissionsTab()
    {
        var page = new TabPage("Permissions") { BackColor = AppTheme.Current().Window };
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            Padding = new Padding(24)
        };
        root.Controls.Add(Section("Windows permissions", "Rounder for Windows does not require Accessibility, Screen Recording, or Automation permissions.", Stack(
            PermissionRow("Overlay windows", "Uses standard topmost click-through windows on the current desktop.", "Granted"),
            PermissionRow("Display changes", "Listens for Windows display-setting changes and regenerates overlays.", "Granted"),
            PermissionRow("Startup automation", "Not configured automatically. Add Rounder to Startup manually if desired.", "Optional"))));
        page.Controls.Add(root);
        return page;
    }

    private static TabPage CreateCreditsTab()
    {
        var page = new TabPage("Credits") { BackColor = AppTheme.Current().Window };
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            Padding = new Padding(24)
        };
        var text = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            Text = $"""
Rounder for Windows

Original macOS app by Nisesimadao.
Windows port built with .NET 8, WinForms, GDI+, and small Win32 window-style bridges.

Settings are stored in %AppData%\Rounder.
Current settings folder:
{JsonStore.ConfigDirectory}

Windows notes:
- Secure desktop, lock screen, and exclusive fullscreen apps can appear above the overlay.
- Display changes are handled by regenerating overlays instead of restarting the app.
- The app is PerMonitorV2 DPI aware, so the settings UI and overlay placement adapt when monitor scale changes.
""",
            ForeColor = AppTheme.Current().Text
        };
        root.Controls.Add(text);
        page.Controls.Add(root);
        return page;
    }

    private void ConfigureRadiusControls()
    {
        radiusInput.ValueChanged += (_, _) =>
        {
            if (radiusSlider.Value != (int)radiusInput.Value)
            {
                radiusSlider.Value = (int)radiusInput.Value;
            }
        };
        radiusSlider.ValueChanged += (_, _) =>
        {
            if (radiusInput.Value != radiusSlider.Value)
            {
                radiusInput.Value = radiusSlider.Value;
            }
        };
    }

    private void ConfigureColorPicker()
    {
        colorButton.Click += (_, _) =>
        {
            using var dialog = new ColorDialog { Color = colorPreview.BackColor, FullOpen = true };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                colorPreview.BackColor = dialog.Color;
            }
        };
    }

    private Control CreateAppearancePanel()
    {
        var table = FormGrid();
        table.Controls.Add(FieldLabel("Corner radius"), 0, 0);
        table.Controls.Add(Row(radiusInput, new Label { Text = "px", AutoSize = true, Anchor = AnchorStyles.Left }, radiusSlider), 1, 0);

        table.Controls.Add(FieldLabel("Corner color"), 0, 1);
        table.Controls.Add(Row(colorPreview, Swatch(Color.Black), Swatch(Color.White), Swatch(Color.Gray), colorButton), 1, 1);

        table.Controls.Add(FieldLabel("Visible corners"), 0, 2);
        table.Controls.Add(CreateCornerGrid(), 1, 2);
        return table;
    }

    private Control CreateCornerGrid()
    {
        var grid = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, RowCount = 2 };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.Controls.Add(topLeftBox, 0, 0);
        grid.Controls.Add(topRightBox, 1, 0);
        grid.Controls.Add(bottomLeftBox, 0, 1);
        grid.Controls.Add(bottomRightBox, 1, 1);
        return grid;
    }

    private Control CreateGamingPanel()
    {
        var table = FormGrid();
        table.Controls.Add(FieldLabel("Mode"), 0, 0);
        table.Controls.Add(gamingBox, 1, 0);
        table.Controls.Add(FieldLabel("Speed"), 0, 1);
        table.Controls.Add(Row(speedInput, new Label { Text = "x", AutoSize = true }), 1, 1);
        table.Controls.Add(FieldLabel("Glow intensity"), 0, 2);
        table.Controls.Add(Row(glowInput, new Label { Text = "x", AutoSize = true }), 1, 2);
        return table;
    }

    private Control CreateMonitorPanel()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 2 };
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var refreshButton = new Button { Text = "Refresh monitors", AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
        refreshButton.Click += (_, _) => LoadDisplays();

        table.Controls.Add(displayList, 0, 0);
        table.Controls.Add(refreshButton, 0, 1);
        return table;
    }

    private void LoadSettingsIntoControls()
    {
        enabledBox.Checked = settings.IsEnabled;
        radiusInput.Value = Math.Clamp(settings.CornerRadius, 0, 40);
        radiusSlider.Value = (int)radiusInput.Value;
        colorPreview.BackColor = settings.CornerColor;
        topLeftBox.Checked = settings.TopLeftEnabled;
        topRightBox.Checked = settings.TopRightEnabled;
        bottomLeftBox.Checked = settings.BottomLeftEnabled;
        bottomRightBox.Checked = settings.BottomRightEnabled;
        gamingBox.Checked = settings.SuperGamingMode;
        speedInput.Value = Math.Clamp(settings.GamingSpeed * 10m, 1m, 50m);
        glowInput.Value = Math.Clamp(settings.GlowIntensity * 10m, 1m, 30m);
        LoadDisplays();
    }

    private void LoadDisplays()
    {
        displayList.Items.Clear();
        var selected = settings.SelectedDisplays.Count == 0
            ? Screen.AllScreens.Select(screen => screen.DeviceName).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : settings.SelectedDisplays.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var screen in Screen.AllScreens)
        {
            var scale = DeviceDpi > 0 ? $"{DeviceDpi / 96.0:0.##}x" : "system";
            var label = $"{screen.DeviceName}  {screen.Bounds.Width}x{screen.Bounds.Height}  scale {scale}" + (screen.Primary ? "  Primary" : "");
            var item = new DisplayItem(screen.DeviceName, label);
            displayList.Items.Add(item, selected.Contains(screen.DeviceName));
        }
    }

    private void Apply()
    {
        ApplyControlsToSettingsOnly();
        settings.SelectedDisplays = displayList.CheckedItems
            .Cast<DisplayItem>()
            .Select(item => item.DeviceName)
            .ToList();

        if (settings.SelectedDisplays.Count == 0)
        {
            MessageBox.Show(this, "Select at least one monitor.", "Rounder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SettingsApplied?.Invoke(this, settings.Clone());
    }

    private void AddPresetFromCurrentSettings()
    {
        ApplyControlsToSettingsOnly();
        var name = PromptDialog.Show("New Preset", "Preset name:");
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        presets.Add(CornerPreset.FromSettings(name, settings));
        RefreshPresetList();
        PresetsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplySelectedPreset()
    {
        if (SelectedPreset() is not { } preset)
        {
            return;
        }

        preset.ApplyTo(settings);
        LoadSettingsIntoControls();
        Apply();
    }

    private void EditSelectedPreset()
    {
        if (SelectedPreset() is not { } preset)
        {
            return;
        }

        using var editor = new PresetEditorForm(preset);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            RefreshPresetList();
            PresetsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void DeleteSelectedPreset()
    {
        if (SelectedPreset() is not { } preset)
        {
            return;
        }

        if (MessageBox.Show(this, $"Delete '{preset.Name}'?", "Rounder", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            presets.Remove(preset);
            RefreshPresetList();
            PresetsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ImportPresets()
    {
        using var dialog = new OpenFileDialog { Filter = "Rounder presets (*.json)|*.json|JSON files (*.json)|*.json|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var imported = JsonStore.ReadPresetFile(dialog.FileName);
            var added = 0;
            foreach (var preset in imported)
            {
                if (presets.All(existing => !string.Equals(existing.Name, preset.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    presets.Add(preset);
                    added++;
                }
            }

            RefreshPresetList();
            PresetsChanged?.Invoke(this, EventArgs.Empty);
            MessageBox.Show(this, $"Imported {added} preset(s).", "Rounder");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportPresets()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Rounder presets (*.json)|*.json|JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = "rounder_presets.json"
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            JsonStore.WritePresetFile(dialog.FileName, presets);
        }
    }

    private void ApplyControlsToSettingsOnly()
    {
        settings.IsEnabled = enabledBox.Checked;
        settings.CornerRadius = (int)radiusInput.Value;
        settings.CornerColor = colorPreview.BackColor;
        settings.TopLeftEnabled = topLeftBox.Checked;
        settings.TopRightEnabled = topRightBox.Checked;
        settings.BottomLeftEnabled = bottomLeftBox.Checked;
        settings.BottomRightEnabled = bottomRightBox.Checked;
        settings.SuperGamingMode = gamingBox.Checked;
        settings.GamingSpeed = speedInput.Value / 10m;
        settings.GlowIntensity = glowInput.Value / 10m;
    }

    private CornerPreset? SelectedPreset()
    {
        return presetList.SelectedItem as CornerPreset;
    }

    private void RefreshPresetList()
    {
        var selectedId = SelectedPreset()?.Id;
        presetList.Items.Clear();
        foreach (var preset in presets)
        {
            presetList.Items.Add(preset);
            if (preset.Id == selectedId)
            {
                presetList.SelectedItem = preset;
            }
        }

        if (presetList.SelectedIndex < 0 && presetList.Items.Count > 0)
        {
            presetList.SelectedIndex = 0;
        }

        UpdatePresetDetails();
    }

    private void UpdatePresetDetails()
    {
        if (SelectedPreset() is not { } preset)
        {
            presetDetails.Text = "No preset selected.";
            return;
        }

        var corners = string.Join(", ", new[]
        {
            preset.TopLeftEnabled ? "top left" : null,
            preset.TopRightEnabled ? "top right" : null,
            preset.BottomLeftEnabled ? "bottom left" : null,
            preset.BottomRightEnabled ? "bottom right" : null
        }.Where(value => value is not null));

        presetDetails.Text = $"{preset.Name}: {preset.CornerRadius}px, {ColorTranslator.ToHtml(preset.CornerColor)}, corners: {(string.IsNullOrEmpty(corners) ? "none" : corners)}";
    }

    private Button Swatch(Color color)
    {
        var button = new Button
        {
            BackColor = color,
            Width = 30,
            Height = 28,
            FlatStyle = FlatStyle.Flat,
            AutoSize = false,
            Tag = "color-preview",
            Margin = new Padding(0, 0, 8, 0)
        };
        button.FlatAppearance.BorderColor = SystemColors.ControlDark;
        button.Click += (_, _) => colorPreview.BackColor = color;
        return button;
    }

    private static TableLayoutPanel Section(string title, string subtitle, Control content)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 16),
            BackColor = AppTheme.Current().Surface,
            Tag = "surface",
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        panel.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
            Margin = new Padding(0)
        }, 0, 0);
        panel.Controls.Add(new Label
        {
            Text = subtitle,
            AutoSize = true,
            ForeColor = AppTheme.Current().MutedText,
            Tag = "muted",
            Margin = new Padding(0, 4, 0, 12)
        }, 0, 1);
        panel.Controls.Add(content, 0, 2);
        return panel;
    }

    private static TableLayoutPanel FormGrid()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 4
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return table;
    }

    private static Label FieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = AppTheme.Current().MutedText,
            Tag = "muted",
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 12, 8)
        };
    }

    private static FlowLayoutPanel Stack(params Control[] controls)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        panel.Controls.AddRange(controls);
        return panel;
    }

    private static Control PermissionRow(string title, string description, string status)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            Margin = new Padding(0, 0, 0, 12)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var text = Stack(
            new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point) },
            new Label { Text = description, AutoSize = true, ForeColor = SystemColors.GrayText });
        var badge = new Label
        {
            Text = status,
            AutoSize = true,
            ForeColor = status == "Optional" ? Color.FromArgb(138, 89, 0) : Color.FromArgb(20, 118, 70),
            Anchor = AnchorStyles.Right | AnchorStyles.Top,
            Margin = new Padding(16, 2, 0, 0)
        };
        row.Controls.Add(text, 0, 0);
        row.Controls.Add(badge, 1, 0);
        return row;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        AppWindowEffects.Apply(this, theme);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Microsoft.Win32.SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;
        }

        base.Dispose(disposing);
    }

    private void HandleUserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
    {
        if (e.Category is Microsoft.Win32.UserPreferenceCategory.General or Microsoft.Win32.UserPreferenceCategory.Color)
        {
            BeginInvoke(ApplyTheme);
        }
    }

    private void ApplyTheme()
    {
        theme = AppTheme.Current();
        BackColor = theme.Window;
        ForeColor = theme.Text;
        AppTheme.ApplyTo(this, theme);
        if (IsHandleCreated)
        {
            AppWindowEffects.Apply(this, theme);
        }
    }

    private static FlowLayoutPanel Row(params Control[] controls)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(0, 4, 0, 4)
        };
        panel.Controls.AddRange(controls);
        return panel;
    }

    private static Button SidebarButton(string text, Action action, bool prominent = false)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(150, 36),
            Margin = new Padding(0, 0, 0, 8),
            Tag = prominent ? "accent" : null
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static Button FooterButton(string text, Action action, bool prominent = false)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(96, 34),
            Margin = new Padding(8, 0, 0, 0)
        };
        if (prominent)
        {
            button.Font = new Font(button.Font, FontStyle.Bold);
            button.Tag = "accent";
        }

        button.Click += (_, _) => action();
        return button;
    }

    private sealed record DisplayItem(string DeviceName, string Label)
    {
        public override string ToString() => Label;
    }
}
