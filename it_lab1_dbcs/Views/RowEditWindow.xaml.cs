using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using it_lab1_dbcs.Models;
using it_lab1_dbcs.Validation;

namespace it_lab1_dbcs.Views
{
    public partial class RowEditWindow : Window
    {
        private readonly TableSchema _schema;
        private readonly TableRow? _existingRow;
        private readonly ValidationService _validationService;

        /// <summary>
        /// Словник, що пов'язує назву поля зі створеним елементом керування (TextBox або ComboBox)
        /// </summary>
        private readonly Dictionary<string, Control> _controlsMap = new Dictionary<string, Control>();

        /// <summary>
        /// Результат роботи вікна — сформований та валідований рядок
        /// </summary>
        public TableRow? ResultRow { get; private set; }

        public RowEditWindow(TableSchema schema, TableRow? existingRow = null)
        {
            InitializeComponent();

            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _existingRow = existingRow;
            _validationService = new ValidationService();

            if (_existingRow != null)
            {
                lblTitle.Text = "Редагування запису";
            }

            BuildDynamicForm();
        }

        /// <summary>
        /// Динамічно будує UI полів на основі схеми таблиці
        /// </summary>
        private void BuildDynamicForm()
        {
            pnlFieldsContainer.Children.Clear();
            _controlsMap.Clear();

            foreach (var field in _schema.Fields)
            {
                var fieldPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
                var label = new TextBlock
                {
                    Text = $"{field.Name} ({field.Type}):",
                    FontWeight = FontWeights.Medium,
                    Foreground = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                    Margin = new Thickness(0, 0, 0, 4)
                };
                fieldPanel.Children.Add(label);

                Control inputControl;

                if (field.Type == FieldType.Enum)
                {
                    var comboBox = new ComboBox
                    {
                        Height = 30,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        ItemsSource = field.AllowedEnumValues
                    };
                    if (_existingRow != null && !string.IsNullOrEmpty(_existingRow[field.Name]))
                    {
                        comboBox.SelectedItem = _existingRow[field.Name];
                    }
                    else if (field.AllowedEnumValues.Count > 0)
                    {
                        comboBox.SelectedIndex = 0;
                    }

                    inputControl = comboBox;
                }
                else
                {
                    var textBox = new TextBox
                    {
                        Height = 30,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(5)
                    };

                    if (_existingRow != null)
                    {
                        textBox.Text = _existingRow[field.Name] ?? string.Empty;
                    }

                    inputControl = textBox;
                }

                fieldPanel.Children.Add(inputControl);
                pnlFieldsContainer.Children.Add(fieldPanel);

                _controlsMap[field.Name] = inputControl;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            lblValidationError.Visibility = Visibility.Collapsed;
            var newRow = new TableRow();
            if (_existingRow != null)
            {
                newRow.Id = _existingRow.Id;
            }
            foreach (var field in _schema.Fields)
            {
                if (_controlsMap.TryGetValue(field.Name, out Control? control))
                {
                    string? value = null;

                    if (control is TextBox textBox)
                    {
                        value = textBox.Text.Trim();
                    }
                    else if (control is ComboBox comboBox)
                    {
                        value = comboBox.SelectedItem?.ToString();
                    }

                    newRow[field.Name] = value;
                }
            }
            var validationResult = _validationService.ValidateRow(_schema, newRow);

            if (!validationResult.IsValid)
            {
                lblValidationError.Text = validationResult.ErrorMessage;
                lblValidationError.Visibility = Visibility.Visible;
                return;
            }
            ResultRow = newRow;
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