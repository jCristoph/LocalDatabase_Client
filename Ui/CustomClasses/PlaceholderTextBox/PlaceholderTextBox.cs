using System.Windows.Controls;

namespace LocalDatabase_Client.Ui.CustomClasses.PlaceholderTextBox
{
    public static class PlaceholderTextBox
    {
        public static void Placeholder(this TextBox textBox, string placeholder)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
                textBox.Text = placeholder;
            textBox.SetResourceReference(Control.ForegroundProperty,
                textBox.Text != placeholder
                    ? "SystemControlForegroundBaseHighBrush"
                    : "SystemControlForegroundBaseMediumBrush");
            var ignoreSelectionChanged = false;
            textBox.SelectionChanged += (sender, args) =>
            {
                if (ignoreSelectionChanged) { ignoreSelectionChanged = false; return; }
                if (textBox.Text != placeholder) return;
                ignoreSelectionChanged = true;
                textBox.Select(0, 0);
            };
            var lastText = textBox.Text;
            var ignoreTextChanged = false;
            textBox.TextChanged += (sender, args) =>
            {
                if (ignoreTextChanged) { ignoreTextChanged = false; return; }
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    ignoreTextChanged = true;
                    textBox.Text = placeholder;
                    textBox.Select(0, 0);
                }
                else if (lastText == placeholder)
                {
                    ignoreTextChanged = true;
                    textBox.Text = textBox.Text.Substring(0, 1);
                    textBox.Select(1, 0);
                }

                textBox.SetResourceReference(Control.ForegroundProperty,
                    textBox.Text != placeholder
                        ? "SystemControlForegroundBaseHighBrush"
                        : "SystemControlForegroundBaseMediumBrush");
                lastText = textBox.Text;
            };
        }
    }
}

