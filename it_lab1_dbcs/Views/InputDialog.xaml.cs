using System.Windows;

namespace it_lab1_dbcs.Views
{
    public partial class InputDialog : Window
    {
        public string InputText { get; private set; } = string.Empty;

        public InputDialog(string title, string prompt, string defaultText = "")
        {
            InitializeComponent();
            Title = title;
            lblPrompt.Text = prompt;
            txtInput.Text = defaultText;
            txtInput.SelectAll();
            txtInput.Focus();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show("Значення не може бути порожнім.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            InputText = txtInput.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}