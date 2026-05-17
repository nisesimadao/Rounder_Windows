namespace Rounder.Windows;

public sealed class PresetEditorForm : Form
{
    private readonly CornerPreset preset;
    private readonly AppTheme theme = AppTheme.Current();
    private readonly TextBox nameBox = new() { Dock = DockStyle.Fill };
    private readonly NumericUpDown radiusInput = new() { Minimum = 0, Maximum = 40, Width = 76 };
    private readonly Panel colorPreview = new() { Width = 42, Height = 28, BorderStyle = BorderStyle.FixedSingle, Tag = "color-preview" };
    private readonly CheckBox topLeftBox = new() { Text = "Top left", AutoSize = true };
    private readonly CheckBox topRightBox = new() { Text = "Top right", AutoSize = true };
    private readonly CheckBox bottomLeftBox = new() { Text = "Bottom left", AutoSize = true };
    private readonly CheckBox bottomRightBox = new() { Text = "Bottom right", AutoSize = true };
    private readonly CheckBox gamingBox = new() { Text = "Super Duper Gaming Mode", AutoSize = true };
    private readonly NumericUpDown speedInput = new() { Minimum = 1, Maximum = 50, DecimalPlaces = 1, Increment = 1, Width = 76 };
    private readonly NumericUpDown glowInput = new() { Minimum = 1, Maximum = 30, DecimalPlaces = 1, Increment = 1, Width = 76 };

    public PresetEditorForm(CornerPreset preset)
    {
        this.preset = preset;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        Icon = AppAssets.AppIcon();
        Text = "Edit Preset";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        MinimumSize = new Size(520, 420);
        ClientSize = new Size(560, 430);

        LoadPreset();

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = SystemColors.Window
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        shell.Controls.Add(CreateBody(), 0, 0);
        shell.Controls.Add(CreateFooter(), 0, 1);
        Controls.Add(shell);
        AppTheme.ApplyTo(this, theme);
    }

    private void LoadPreset()
    {
        nameBox.Text = preset.Name;
        radiusInput.Value = Math.Clamp(preset.CornerRadius, 0, 40);
        colorPreview.BackColor = preset.CornerColor;
        topLeftBox.Checked = preset.TopLeftEnabled;
        topRightBox.Checked = preset.TopRightEnabled;
        bottomLeftBox.Checked = preset.BottomLeftEnabled;
        bottomRightBox.Checked = preset.BottomRightEnabled;
        gamingBox.Checked = preset.SuperGamingMode;
        speedInput.Value = Math.Clamp(preset.GamingSpeed * 10m, 1m, 50m);
        glowInput.Value = Math.Clamp(preset.GlowIntensity * 10m, 1m, 30m);
    }

    private Control CreateBody()
    {
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(20)
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var chooseColor = new Button { Text = "Choose Color", AutoSize = true };
        chooseColor.Click += (_, _) =>
        {
            using var dialog = new ColorDialog { Color = colorPreview.BackColor, FullOpen = true };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                colorPreview.BackColor = dialog.Color;
            }
        };

        body.Controls.Add(FieldLabel("Name"), 0, 0);
        body.Controls.Add(nameBox, 1, 0);
        body.Controls.Add(FieldLabel("Radius"), 0, 1);
        body.Controls.Add(Row(radiusInput, new Label { Text = "px", AutoSize = true }), 1, 1);
        body.Controls.Add(FieldLabel("Color"), 0, 2);
        body.Controls.Add(Row(colorPreview, chooseColor), 1, 2);
        body.Controls.Add(FieldLabel("Corners"), 0, 3);
        body.Controls.Add(CreateCornerGrid(), 1, 3);
        body.Controls.Add(FieldLabel("Mode"), 0, 4);
        body.Controls.Add(gamingBox, 1, 4);
        body.Controls.Add(FieldLabel("Speed"), 0, 5);
        body.Controls.Add(Row(speedInput, new Label { Text = "x", AutoSize = true }), 1, 5);
        body.Controls.Add(FieldLabel("Glow"), 0, 6);
        body.Controls.Add(Row(glowInput, new Label { Text = "x", AutoSize = true }), 1, 6);

        return body;
    }

    private Control CreateCornerGrid()
    {
        var grid = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, RowCount = 2 };
        grid.Controls.Add(topLeftBox, 0, 0);
        grid.Controls.Add(topRightBox, 1, 0);
        grid.Controls.Add(bottomLeftBox, 0, 1);
        grid.Controls.Add(bottomRightBox, 1, 1);
        return grid;
    }

    private Control CreateFooter()
    {
        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12),
            BackColor = theme.SurfaceAlt
        };
        var save = new Button { Text = "Save", DialogResult = DialogResult.OK, Tag = "accent" };
        save.Click += (_, _) => Save();
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
        footer.Controls.AddRange([save, cancel]);
        AcceptButton = save;
        CancelButton = cancel;
        return footer;
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(nameBox.Text))
        {
            DialogResult = DialogResult.None;
            MessageBox.Show(this, "Preset name is required.", "Rounder");
            return;
        }

        preset.Name = nameBox.Text.Trim();
        preset.CornerRadius = (int)radiusInput.Value;
        preset.CornerColor = colorPreview.BackColor;
        preset.TopLeftEnabled = topLeftBox.Checked;
        preset.TopRightEnabled = topRightBox.Checked;
        preset.BottomLeftEnabled = bottomLeftBox.Checked;
        preset.BottomRightEnabled = bottomRightBox.Checked;
        preset.SuperGamingMode = gamingBox.Checked;
        preset.GamingSpeed = speedInput.Value / 10m;
        preset.GlowIntensity = glowInput.Value / 10m;
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

    private static FlowLayoutPanel Row(params Control[] controls)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 4, 0, 4)
        };
        panel.Controls.AddRange(controls);
        return panel;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        AppWindowEffects.Apply(this, theme);
    }
}
