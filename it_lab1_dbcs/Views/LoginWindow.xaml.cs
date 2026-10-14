using it_lab1_dbcs.Storage;
using System.Windows;
using System.Windows.Media;

namespace it_lab1_dbcs.Views
{
    public partial class LoginWindow : Window
    {
        private readonly UserManager _userManager;
        public LoginWindow()
        {
            InitializeComponent();
            _userManager = new UserManager();
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtLoginUsername.Text;
            string password = txtLoginPassword.Password;
            var (success, message) = _userManager.Login(username, password);

            if (success)
            {
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ShowMessage(message, isError: true);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtRegisterUsername.Text;
            string password = txtRegisterPassword.Password;
            string confirmPassword = txtRegisterPasswordConfirm.Password;

            if (password != confirmPassword)
            {
                ShowMessage("Паролі не збігаються!", isError: true);
                return;
            }

            var (success, message) = _userManager.Register(username, password);

            if (success)
            {
                ShowMessage(message, isError: false);
                txtRegisterUsername.Clear();
                txtRegisterPassword.Clear();
                txtRegisterPasswordConfirm.Clear();
            }
            else
            {
                ShowMessage(message, isError: true);
            }
        }

        private void ShowMessage(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.Foreground = isError
                ? new SolidColorBrush(Colors.Red)
                : new SolidColorBrush(Colors.Green);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (UserManager.CurrentUser == null && Application.Current.Windows.Count == 0)
            {
                Application.Current.Shutdown();
            }
        }
    }
}