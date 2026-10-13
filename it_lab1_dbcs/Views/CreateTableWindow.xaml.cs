using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using it_lab1_dbcs.Models;

namespace it_lab1_dbcs.Views
{
    public partial class CreateTableWindow : Window
    {
        /// <summary>
        /// Створена таблиця, яку зможе отримати MainForm після успішного закриття вікна
        /// </summary>
        public Table? CreatedTable { get; private set; }

        private readonly ObservableCollection<FieldSchemeViewModel> _fields = new ObservableCollection<FieldSchemeViewModel>();

        public CreateTableWindow()
        {
            InitializeComponent();
            cmbFieldType.ItemsSource = Enum.GetValues(typeof(FieldType));
            cmbFieldType.SelectedIndex = 0;
            dgFields.ItemsSource = _fields;
        }

        private void CmbFieldType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFieldType.SelectedItem is FieldType selectedType)
            {
                txtEnumValues.IsEnabled = (selectedType == FieldType.Enum);
                if (!txtEnumValues.IsEnabled)
                {
                    txtEnumValues.Clear();
                }
            }
        }

        private void BtnAddField_Click(object sender, RoutedEventArgs e)
        {
            string fieldName = txtFieldName.Text.Trim();

            if (string.IsNullOrWhiteSpace(fieldName))
            {
                MessageBox.Show("Введіть назву поля.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_fields.Any(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Поле з такою назвою вже додано у список.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FieldType selectedType = (FieldType)cmbFieldType.SelectedItem;
            List<string> allowedEnumValues = new List<string>();

            if (selectedType == FieldType.Enum)
            {
                string rawEnumValues = txtEnumValues.Text.Trim();
                if (string.IsNullOrWhiteSpace(rawEnumValues))
                {
                    MessageBox.Show("Для типу Enum необхідно вказати хоча б одне значення через кому.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                allowedEnumValues = rawEnumValues.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(v => v.Trim())
                                                .Where(v => !string.IsNullOrEmpty(v))
                                                .Distinct()
                                                .ToList();

                if (allowedEnumValues.Count == 0)
                {
                    MessageBox.Show("Введіть хоча б одне коректне значення для Enum.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            _fields.Add(new FieldSchemeViewModel
            {
                Name = fieldName,
                Type = selectedType,
                AllowedEnumValues = allowedEnumValues
            });
            txtFieldName.Clear();
            txtEnumValues.Clear();
            txtFieldName.Focus();
        }

        private void BtnRemoveField_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is FieldSchemeViewModel itemToRemove)
            {
                _fields.Remove(itemToRemove);
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            string tableName = txtTableName.Text.Trim();

            if (string.IsNullOrWhiteSpace(tableName))
            {
                MessageBox.Show("Введіть назву таблиці.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_fields.Count == 0)
            {
                MessageBox.Show("Таблиця повинна містити хоча б одну колонку.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var table = new Table(tableName);
            foreach (var f in _fields)
            {
                table.Schema.AddField(new FieldScheme(f.Name, f.Type, f.AllowedEnumValues));
            }

            CreatedTable = table;
            DialogResult = true;
            Close();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// Вспоміжний клас для зручного відображення списку полів у DataGrid
    /// </summary>
    public class FieldSchemeViewModel
    {
        public string Name { get; set; } = string.Empty;
        public FieldType Type { get; set; }
        public List<string> AllowedEnumValues { get; set; } = new List<string>();

        public string AllowedEnumValuesString => AllowedEnumValues.Count > 0
            ? string.Join(", ", AllowedEnumValues)
            : "—";
    }
}