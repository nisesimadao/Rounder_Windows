using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Rounder.Windows;

public sealed record AppTheme(
    bool IsDark,
    Color Window,
    Color Surface,
    Color SurfaceAlt,
    Color Text,
    Color MutedText,
    Color Border,
    Color Accent,
    Color AccentText)
{
    public static AppTheme Current()
    {
        var isDark = IsWindowsAppDarkMode();
        return isDark
            ? new AppTheme(
                true,
                Color.FromArgb(32, 32, 32),
                Color.FromArgb(39, 39, 39),
                Color.FromArgb(46, 46, 46),
                Color.FromArgb(243, 243, 243),
                Color.FromArgb(174, 174, 174),
                Color.FromArgb(75, 75, 75),
                Color.FromArgb(96, 205, 255),
                Color.FromArgb(0, 35, 52))
            : new AppTheme(
                false,
                Color.FromArgb(249, 249, 249),
                Color.White,
                Color.FromArgb(243, 246, 250),
                Color.FromArgb(32, 32, 32),
                Color.FromArgb(96, 96, 96),
                Color.FromArgb(222, 226, 232),
                Color.FromArgb(0, 103, 192),
                Color.White);
    }

    public static void ApplyTo(Control control, AppTheme theme)
    {
        control.BackColor = control is TextBoxBase or ListBox or CheckedListBox
            ? theme.Surface
            : theme.Window;
        control.ForeColor = theme.Text;

        foreach (Control child in control.Controls)
        {
            if (child.Tag as string == "color-preview")
            {
                continue;
            }

            switch (child)
            {
                case Button button:
                    StyleButton(button, theme, button.Tag as string == "accent");
                    break;
                case TabControl tabControl:
                    tabControl.BackColor = theme.Window;
                    tabControl.ForeColor = theme.Text;
                    ApplyTo(tabControl, theme);
                    break;
                case TabPage tabPage:
                    tabPage.BackColor = theme.Window;
                    tabPage.ForeColor = theme.Text;
                    ApplyTo(tabPage, theme);
                    break;
                case TextBoxBase textBox:
                    textBox.BackColor = theme.Surface;
                    textBox.ForeColor = theme.Text;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case CheckedListBox checkedListBox:
                    checkedListBox.BackColor = theme.Surface;
                    checkedListBox.ForeColor = theme.Text;
                    break;
                case ListBox listBox:
                    listBox.BackColor = theme.Surface;
                    listBox.ForeColor = theme.Text;
                    break;
                case NumericUpDown numeric:
                    numeric.BackColor = theme.Surface;
                    numeric.ForeColor = theme.Text;
                    break;
                case Label label when label.Tag as string == "muted":
                    label.ForeColor = theme.MutedText;
                    break;
                case TableLayoutPanel table when table.Tag as string == "surface":
                    table.BackColor = theme.Surface;
                    ApplyTo(table, theme);
                    break;
                default:
                    child.BackColor = child.Tag as string == "surface-alt" ? theme.SurfaceAlt : theme.Window;
                    child.ForeColor = theme.Text;
                    ApplyTo(child, theme);
                    break;
            }
        }
    }

    public static void StyleButton(Button button, AppTheme theme, bool accent = false)
    {
        button.AutoSize = true;
        button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button.MinimumSize = new Size(96, 34);
        button.Padding = new Padding(14, 5, 14, 5);
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        button.BackColor = accent ? theme.Accent : theme.SurfaceAlt;
        button.ForeColor = accent ? theme.AccentText : theme.Text;
        button.FlatAppearance.BorderColor = accent ? theme.Accent : theme.Border;
        button.FlatAppearance.BorderSize = 1;
    }

    private static bool IsWindowsAppDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
        catch
        {
            return false;
        }
    }
}

public static class AppWindowEffects
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmwcpRound = 2;
    private const int DwmSystemBackdropMainWindow = 2;

    public static void Apply(Form form, AppTheme theme)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            return;
        }

        var dark = theme.IsDark ? 1 : 0;
        _ = DwmSetWindowAttribute(form.Handle, DwmwaUseImmersiveDarkMode, ref dark, sizeof(int));

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            var rounded = DwmwcpRound;
            _ = DwmSetWindowAttribute(form.Handle, DwmwaWindowCornerPreference, ref rounded, sizeof(int));
        }

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621))
        {
            var backdrop = DwmSystemBackdropMainWindow;
            _ = DwmSetWindowAttribute(form.Handle, DwmwaSystemBackdropType, ref backdrop, sizeof(int));
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);
}
