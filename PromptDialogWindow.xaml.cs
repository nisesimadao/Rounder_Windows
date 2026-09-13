using System.Windows;

namespace Rounder.Windows;

public partial class PromptDialogWindow : Window
{
    private PromptDialogWindow(string title, string label, string initialValue)
    {
        InitializeComponent();
        Title = title;
        PromptLabel.Text = label;
        ValueBox.Text = initialValue;
        Loaded += (_, _) =>
        {
            ValueBox.Focus();
            ValueBox.SelectAll();
        };
    }

    public string Value => ValueBox.Text.Trim();

    public static string? Show(Window owner, string title, string label, string initialValue = "")
    {
        var dialog = new PromptDialogWindow(title, label, initialValue) { Owner = owner };
        return dialog.ShowDialog() == true ? dialog.Value : null;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
