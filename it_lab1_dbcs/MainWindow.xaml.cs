using it_lab1_dbcs.Models;
using it_lab1_dbcs.Storage;
using it_lab1_dbcs.Views;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace it_lab1_dbcs
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseManager _dbManager;
        private Database? _currentDatabase;
        private Table? _currentTable;
        private bool _isModified = false;
        public MainWindow()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserManager.CurrentUser == null)
            {
                var loginWin = new LoginWindow();
                loginWin.Show();
                this.Close();
                return;
            }

            lblCurrentUser.Text = UserManager.CurrentUser.Username;
            CreateNewDatabase("DefaultDB");
        }
        #region Управління станом незбережених змін

        /// <summary>
        /// Позначає базу даних як змінену або збережену та оновлює індикатор у заголовоку
        /// </summary>
        private void MarkAsModified(bool isModified = true)
        {
            _isModified = isModified;
            if (_currentDatabase != null)
            {
                lblDbName.Text = _isModified ? $"{_currentDatabase.Name} *" : _currentDatabase.Name;
            }
        }

        /// <summary>
        /// Перевіряє наявність незбережених змін та запитує підтвердження користувача
        /// </summary>
        /// <returns>True - можна продовжувати дію; False - дію скасовано</returns>
        private bool PromptSaveIfModified()
        {
            if (!_isModified || _currentDatabase == null)
                return true;

            var result = MessageBox.Show(
                $"База даних '{_currentDatabase.Name}' має незбережені зміни. Зберегти перед продовженням?",
                "Незбережені зміни",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                return SaveDatabase();
            }
            return result == MessageBoxResult.No;
        }

        /// <summary>
        /// Перехоплює закриття вікна (хрестик або Alt+F4)
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            var prmt = PromptSaveIfModified();
            if (!prmt)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Application.Current.Windows.Count == 0)
            {
                Application.Current.Shutdown();
            }
        }

        #endregion
        #region Робота з Базою Даних

        private void CreateNewDatabase(string dbName)
        {
            if (UserManager.CurrentUser == null) return;

            _currentDatabase = new Database(dbName, UserManager.CurrentUser.Username);
            MarkAsModified(false);
            UpdateDatabaseUI();
            SetStatus($"Створено нову базу даних: '{dbName}'");
        }

        private void UpdateDatabaseUI()
        {
            if (_currentDatabase == null) return;

            lblDbName.Text = _isModified ? $"{_currentDatabase.Name} *" : _currentDatabase.Name;
            lstTables.ItemsSource = _currentDatabase.Tables;

            if (_currentDatabase.Tables.Count > 0)
            {
                lstTables.SelectedIndex = 0;
            }
            else
            {
                _currentTable = null;
                UpdateTableUI();
            }
        }

        private void MenuNewDb_Click(object sender, RoutedEventArgs e)
        {
            var prmt = PromptSaveIfModified();
            if (!prmt) return;
            CreateNewDatabase($"NewDB_{DateTime.Now:HHmmss}");
        }

        private void MenuSaveDb_Click(object sender, RoutedEventArgs e)
        {
            SaveDatabase();
        }
        private bool SaveDatabase()
        {
            if (_currentDatabase == null) return false;

            try
            {
                _dbManager.SaveDatabase(_currentDatabase);
                MarkAsModified(false);
                MessageBox.Show($"Базу даних '{_currentDatabase.Name}' успішно збережено!",
                                "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                SetStatus("БД успішно збережено на диск.");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        private void MenuOpenDb_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Databases")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var loadedDb = _dbManager.LoadDatabase(openFileDialog.FileName);
                    if (UserManager.CurrentUser != null &&
                        !string.Equals(loadedDb.OwnerUsername, UserManager.CurrentUser.Username, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("У вас немає прав для відкриття цієї бази даних (вона належить іншому користувачу).",
                                        "Доступ заборонено", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                    _currentDatabase = loadedDb;
                    MarkAsModified(false);
                    UpdateDatabaseUI();
                    SetStatus($"Завантажено БД '{_currentDatabase.Name}'");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка відкриття файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            var prmt = PromptSaveIfModified();
            if (!prmt) return;
            _isModified = false;
            var loginWin = new LoginWindow();
            Application.Current.MainWindow = loginWin;
            loginWin.Show();
            UserManager.Logout();
            this.Close();
        }
        #endregion

        #region Робота з Таблицями

        private void LstTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentTable = lstTables.SelectedItem as Table;
            UpdateTableUI();
        }

        private void UpdateTableUI()
        {
            dgRecords.Columns.Clear();

            if (_currentTable == null)
            {
                lblTableName.Text = "Не обрано";
                dgRecords.ItemsSource = null;
                return;
            }

            lblTableName.Text = _currentTable.Name;
            foreach (var field in _currentTable.Schema.Fields)
            {
                var column = new DataGridTextColumn
                {
                    Header = $"{field.Name} ({field.Type})",
                    Binding = new Binding($"Values[{field.Name}]"), 
                    SortMemberPath = field.Name
                };
                dgRecords.Columns.Add(column);
            }

            dgRecords.ItemsSource = _currentTable.Rows;
            SetStatus($"Завантажено таблицю '{_currentTable.Name}'. Записів: {_currentTable.Rows.Count}");
        }

        private void BtnCreateTable_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDatabase == null) return;

            var createWin = new CreateTableWindow { Owner = this };
            if (createWin.ShowDialog() == true && createWin.CreatedTable != null)
            {
                try
                {
                    _currentDatabase.AddTable(createWin.CreatedTable);
                    lstTables.SelectedItem = createWin.CreatedTable;
                    MarkAsModified(true);
                    SetStatus($"Створено таблицю '{createWin.CreatedTable.Name}'");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDeleteTable_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDatabase == null || _currentTable == null) return;

            var result = MessageBox.Show($"Ви дійсно бажаєте видалити таблицю '{_currentTable.Name}'?",
                                         "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string name = _currentTable.Name;
                _currentDatabase.RemoveTable(name);
                MarkAsModified(true);
                UpdateDatabaseUI();
                SetStatus($"Видалено таблицю '{name}'");
            }
        }

        #endregion

        #region Робота з Рядками (CRUD & Validation)

        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTable == null)
            {
                MessageBox.Show("Спочатку оберіть або створіть таблицю.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var editWin = new RowEditWindow(_currentTable.Schema) { Owner = this };
            if (editWin.ShowDialog() == true && editWin.ResultRow != null)
            {
                _currentTable.Rows.Add(editWin.ResultRow);
                MarkAsModified(true);
                SetStatus("Успішно додано новий запис.");
            }
        }

        private void BtnEditRow_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTable == null || dgRecords.SelectedItem is not TableRow selectedRow)
            {
                MessageBox.Show("Оберіть рядок для редагування.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWin = new RowEditWindow(_currentTable.Schema, selectedRow) { Owner = this };
            if (editWin.ShowDialog() == true && editWin.ResultRow != null)
            {
                foreach (var kvp in editWin.ResultRow.Values)
                {
                    selectedRow[kvp.Key] = kvp.Value;
                }

                dgRecords.Items.Refresh();
                MarkAsModified(true);
                SetStatus("Запис успішно відредаговано.");
            }
        }

        private void BtnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTable == null || dgRecords.SelectedItem is not TableRow selectedRow)
            {
                MessageBox.Show("Оберіть рядок для видалення.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentTable.Rows.Remove(selectedRow);
            MarkAsModified(true);
            SetStatus("Запис видалено.");
        }

        #endregion
        #region Перейменування БД та Таблиць

        private void MenuRenameDb_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDatabase == null) return;

            string oldName = _currentDatabase.Name;

            var dialog = new InputDialog("Перейменування БД", "Введіть нову назву для бази даних:", oldName)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                string newName = dialog.InputText;

                try
                {
                    _dbManager.RenameDatabase(_currentDatabase, oldName, newName);

                    lblDbName.Text = newName;
                    MarkAsModified(false); 
                    SetStatus($"Базу даних '{oldName}' успішно перейменовано на '{newName}'.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка перейменування: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRenameTable_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDatabase == null || _currentTable == null)
            {
                MessageBox.Show("Спочатку оберіть таблицю для перейменування.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string oldTableName = _currentTable.Name;

            var dialog = new InputDialog("Перейменування таблиці", "Введіть нову назву для таблиці:", oldTableName)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                string newTableName = dialog.InputText;

                try
                {
                    _currentDatabase.RenameTable(oldTableName, newTableName);
                    MarkAsModified(true);
                    lstTables.Items.Refresh();
                    lblTableName.Text = newTableName;

                    SetStatus($"Таблицю '{oldTableName}' перейменовано на '{newTableName}'.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Помилка перейменування", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Сортування

        private void DgRecords_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            if (_currentTable == null) return;

            string fieldName = e.Column.SortMemberPath;
            ListSortDirection newDirection = (e.Column.SortDirection != ListSortDirection.Ascending)
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            e.Column.SortDirection = newDirection;
            _currentTable.SortByField(fieldName, newDirection == ListSortDirection.Ascending);

            SetStatus($"Таблицю відсортовано за полем '{fieldName}' ({newDirection}).");
        }

        #endregion

        private void SetStatus(string message)
        {
            lblStatus.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
        }
    }
}