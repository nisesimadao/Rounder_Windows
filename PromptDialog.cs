namespace Rounder.Windows;

public static class PromptDialog
{
    public static string? Show(string title, string label, string initialValue = "")
    {
        using var form = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ClientSize = new Size(380, 130)
        };

        var labelControl = new Label { Text = label, AutoSize = true, Left = 12, Top = 16 };
        var textBox = new TextBox { Left = 12, Top = 42, Width = 356, Text = initialValue };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 212, Width = 75, Top = 88 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 293, Width = 75, Top = 88 };

        form.Controls.AddRange([labelControl, textBox, ok, cancel]);
        form.AcceptButton = ok;
        form.CancelButton = cancel;

        return form.ShowDialog() == DialogResult.OK
            ? textBox.Text.Trim()
            : null;
    }
}
