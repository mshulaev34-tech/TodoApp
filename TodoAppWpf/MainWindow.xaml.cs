using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _dbService = new DatabaseService();

        private const int MaxTitleLength = 100;
        private const int MinTitleLength = 2;

        private string _currentFilter = "All";
        private string _currentSearch = "";
        private string _currentSort = "DateDesc";

        public MainWindow()
        {
            InitializeComponent();

            // Загружаем задачи после полной инициализации
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }

        private void LoadTasks()
        {
            try
            {
                // Проверяем, что все элементы существуют
                if (LvTasks == null || _dbService == null)
                {
                    return;
                }

                var tasks = _dbService.GetTasks(_currentFilter, _currentSearch, _currentSort);

                if (tasks != null)
                {
                    LvTasks.ItemsSource = tasks;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач:\n{ex.Message}\n\nПроверьте:\n1. Подключение к SQL Server\n2. Строку подключения в DatabaseService.cs\n3.是否存在 таблица Tasks в базе данных TodoDb",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (RbAll != null && RbAll.IsChecked == true) _currentFilter = "All";
            else if (RbActive != null && RbActive.IsChecked == true) _currentFilter = "Active";
            else if (RbCompleted != null && RbCompleted.IsChecked == true) _currentFilter = "Completed";

            LoadTasks();
        }

        private void Search_Changed(object sender, TextChangedEventArgs e)
        {
            if (TxtSearch != null)
            {
                _currentSearch = TxtSearch.Text;
                LoadTasks();
            }
        }

        private void Sort_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (CbSort != null && CbSort.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                _currentSort = item.Tag.ToString();
                LoadTasks();
            }
        }

        private string ValidateTaskTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "Название задачи не может быть пустым";

            if (title.Length < MinTitleLength)
                return $"Минимальная длина: {MinTitleLength} символа";

            if (title.Length > MaxTitleLength)
                return $"Максимальная длина: {MaxTitleLength} символов";

            if (_dbService.TaskExists(title))
                return "Такая задача уже существует";

            return null;
        }

        private void ShowError(string errorMessage)
        {
            if (TxtError != null && InputBorder != null)
            {
                TxtError.Text = errorMessage;
                TxtError.Visibility = Visibility.Visible;
                InputBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                InputBorder.BorderThickness = new Thickness(2);
            }
        }

        private void ClearError()
        {
            if (TxtError != null && InputBorder != null)
            {
                TxtError.Text = "";
                TxtError.Visibility = Visibility.Collapsed;
                InputBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
                InputBorder.BorderThickness = new Thickness(1);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (TxtNewTask == null) return;

            string title = TxtNewTask.Text.Trim();

            if (title == "Что нужно сделать?")
            {
                ShowError("Введите название задачи");
                return;
            }

            string validationError = ValidateTaskTitle(title);
            if (validationError != null)
            {
                ShowError(validationError);
                return;
            }

            try
            {
                _dbService.AddTask(title);
                TxtNewTask.Text = "Что нужно сделать?";
                TxtNewTask.Foreground = new SolidColorBrush(Colors.Gray);
                ClearError();

                if (RbAll != null) RbAll.IsChecked = true;
                if (TxtSearch != null) TxtSearch.Text = "";

                LoadTasks();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private void ChkCompleted_Checked(object sender, RoutedEventArgs e)
        {
            UpdateTaskStatus(sender, true);
        }

        private void ChkCompleted_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateTaskStatus(sender, false);
        }

        private void UpdateTaskStatus(object sender, bool isCompleted)
        {
            if (sender is CheckBox chk && chk.Tag is int id)
            {
                try
                {
                    _dbService.ToggleTaskCompletion(id, isCompleted);
                    LoadTasks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления: {ex.Message}");
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                if (MessageBox.Show("Удалить задачу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _dbService.DeleteTask(id);
                        LoadTasks();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                }
            }
        }

        private void TxtNewTask_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtNewTask != null && TxtNewTask.Text == "Что нужно сделать?")
            {
                TxtNewTask.Text = "";
                TxtNewTask.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void TxtNewTask_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TxtNewTask != null && string.IsNullOrWhiteSpace(TxtNewTask.Text))
            {
                TxtNewTask.Text = "Что нужно сделать?";
                TxtNewTask.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void TxtNewTask_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnAdd_Click(sender, new RoutedEventArgs());
            }
        }

        private void TxtNewTask_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtError != null && TxtError.Visibility == Visibility.Visible)
            {
                ClearError();
            }
        }
    }
}