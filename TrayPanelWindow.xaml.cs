using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace Rounder.Windows;

public partial class TrayPanelWindow : Window
{
    private readonly Action<Action<AppSettings>> applyChange;
    private readonly Action openSettings;
    private readonly Action quit;
    private AppSettings snapshot;
    private bool syncing;

    public TrayPanelWindow(
        AppSettings settings,
        Action<Action<AppSettings>> applyChange,
        Action openSettings,
        Action quit)
    {
        this.applyChange = applyChange;
        this.openSettings = openSettings;
        this.quit = quit;
        snapshot = settings.Clone();
        InitializeComponent();
        SyncControls();
    }

    public bool CloseWhenDeactivated { get; set; } = true;

    public void ReloadFrom(AppSettings settings)
    {
        snapshot = settings.Clone();
        SyncControls();
    }

    private void SyncControls()
    {
        syncing = true;
        EnabledBox.IsChecked = snapshot.IsEnabled;
        RadiusSlider.Value = Math.Clamp(snapshot.CornerRadius, 0, 40);
        RadiusValue.Text = $"{snapshot.CornerRadius} px";
        RoundedShapeButton.IsChecked = snapshot.CornerCutoutStyle == CornerCutoutStyle.Rounded;
        SquircleShapeButton.IsChecked = snapshot.CornerCutoutStyle == CornerCutoutStyle.Squircle;
        PolygonShapeButton.IsChecked = snapshot.CornerCutoutStyle == CornerCutoutStyle.Polygon;
        BlackSwatch.IsChecked = snapshot.CornerColor.ToArgb() == System.Drawing.Color.Black.ToArgb();
        WhiteSwatch.IsChecked = snapshot.CornerColor.ToArgb() == System.Drawing.Color.White.ToArgb();
        GraySwatch.IsChecked = snapshot.CornerColor.ToArgb() == System.Drawing.Color.Gray.ToArgb();
        TopLeftBox.IsChecked = snapshot.TopLeftEnabled;
        TopRightBox.IsChecked = snapshot.TopRightEnabled;
        BottomLeftBox.IsChecked = snapshot.BottomLeftEnabled;
        BottomRightBox.IsChecked = snapshot.BottomRightEnabled;
        GamingBox.IsChecked = snapshot.SuperGamingMode;
        syncing = false;
    }

    private void EnabledBox_Click(object sender, RoutedEventArgs e)
    {
        if (syncing) return;
        var value = EnabledBox.IsChecked == true;
        snapshot.IsEnabled = value;
        applyChange(settings => settings.IsEnabled = value);
    }

    private void RadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (RadiusValue is null) return;
        var value = (int)Math.Round(RadiusSlider.Value);
        RadiusValue.Text = $"{value} px";
        if (syncing) return;
        snapshot.CornerRadius = value;
        applyChange(settings => settings.CornerRadius = value);
    }

    private void ShapeButton_Click(object sender, RoutedEventArgs e)
    {
        if (syncing || sender is not System.Windows.Controls.RadioButton { Tag: string tag } || !int.TryParse(tag, out var index)) return;
        var style = index switch
        {
            1 => CornerCutoutStyle.Squircle,
            2 => CornerCutoutStyle.Polygon,
            _ => CornerCutoutStyle.Rounded
        };
        snapshot.CornerCutoutStyle = style;
        applyChange(settings => settings.CornerCutoutStyle = style);
    }

    private void Black_Click(object sender, RoutedEventArgs e) => ApplyColor(System.Drawing.Color.Black);
    private void White_Click(object sender, RoutedEventArgs e) => ApplyColor(System.Drawing.Color.White);
    private void Gray_Click(object sender, RoutedEventArgs e) => ApplyColor(System.Drawing.Color.Gray);

    private void ApplyColor(System.Drawing.Color color)
    {
        snapshot.CornerColor = color;
        BlackSwatch.IsChecked = color.ToArgb() == System.Drawing.Color.Black.ToArgb();
        WhiteSwatch.IsChecked = color.ToArgb() == System.Drawing.Color.White.ToArgb();
        GraySwatch.IsChecked = color.ToArgb() == System.Drawing.Color.Gray.ToArgb();
        applyChange(settings => settings.CornerColor = color);
    }

    private void MoreColors_Click(object sender, RoutedEventArgs e)
    {
        Close();
        openSettings();
    }

    private void CornerBox_Click(object sender, RoutedEventArgs e)
    {
        if (syncing) return;
        var topLeft = TopLeftBox.IsChecked == true;
        var topRight = TopRightBox.IsChecked == true;
        var bottomLeft = BottomLeftBox.IsChecked == true;
        var bottomRight = BottomRightBox.IsChecked == true;
        snapshot.TopLeftEnabled = topLeft;
        snapshot.TopRightEnabled = topRight;
        snapshot.BottomLeftEnabled = bottomLeft;
        snapshot.BottomRightEnabled = bottomRight;
        applyChange(settings =>
        {
            settings.TopLeftEnabled = topLeft;
            settings.TopRightEnabled = topRight;
            settings.BottomLeftEnabled = bottomLeft;
            settings.BottomRightEnabled = bottomRight;
        });
    }

    private void GamingBox_Click(object sender, RoutedEventArgs e)
    {
        if (syncing) return;
        var value = GamingBox.IsChecked == true;
        snapshot.SuperGamingMode = value;
        applyChange(settings => settings.SuperGamingMode = value);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        Close();
        openSettings();
    }

    private void Quit_Click(object sender, RoutedEventArgs e)
    {
        Close();
        quit();
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (CloseWhenDeactivated)
        {
            Close();
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        PositionNearCursor();
        Activate();
    }

    private void PositionNearCursor()
    {
        var cursor = System.Windows.Forms.Cursor.Position;
        var screen = System.Windows.Forms.Screen.FromPoint(cursor);
        var source = PresentationSource.FromVisual(this);
        var transform = source?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;

        var cursorDip = transform.Transform(new System.Windows.Point(cursor.X, cursor.Y));
        var workTopLeft = transform.Transform(new System.Windows.Point(screen.WorkingArea.Left, screen.WorkingArea.Top));
        var workBottomRight = transform.Transform(new System.Windows.Point(screen.WorkingArea.Right, screen.WorkingArea.Bottom));

        var workLeft = workTopLeft.X;
        var workTop = workTopLeft.Y;
        var workRight = workBottomRight.X;
        var workBottom = workBottomRight.Y;

        var desiredLeft = cursorDip.X - ActualWidth + 24;
        var desiredTop = cursorDip.Y - ActualHeight - 12;
        if (desiredTop < workTop + 8)
        {
            desiredTop = cursorDip.Y + 12;
        }

        Left = Math.Clamp(desiredLeft, workLeft + 8, Math.Max(workLeft + 8, workRight - ActualWidth - 8));
        Top = Math.Clamp(desiredTop, workTop + 8, Math.Max(workTop + 8, workBottom - ActualHeight - 8));
    }
}