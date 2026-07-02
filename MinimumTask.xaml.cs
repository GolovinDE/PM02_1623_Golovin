using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace Minimum
{
    public partial class MinimumTask : Window
    {
        private int suppliersCount;
        private int consumersCount;
        private double[,] costs;
        private double[] supplies;
        private double[] demands;
        private double[,] allocation;
        private double totalCost;

        public MinimumTask()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateTable();
        }

        private void GenerateTable()
        {
            if (MainTableGrid == null) return;

            MainTableGrid.Children.Clear();
            MainTableGrid.RowDefinitions.Clear();
            MainTableGrid.ColumnDefinitions.Clear();

            suppliersCount = int.Parse(((ComboBoxItem)SuppliersComboBox.SelectedItem).Content.ToString());
            consumersCount = int.Parse(((ComboBoxItem)ConsumersComboBox.SelectedItem).Content.ToString());

            int totalRows = suppliersCount + 4;
            int totalCols = consumersCount + 2;

            for (int i = 0; i < totalRows; i++)
                MainTableGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            for (int i = 0; i < totalCols; i++)
                MainTableGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star), MinWidth = 70 });

            for (int row = 0; row < totalRows; row++)
            {
                for (int col = 0; col < totalCols; col++)
                {
                    if (row == totalRows - 1 && col == totalCols - 1)
                        continue;

                    Border border = new Border()
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(1),
                        Padding = new Thickness(5),
                        Background = Brushes.White,
                        MinWidth = 70,
                        MinHeight = 40
                    };

                    FrameworkElement content = null;

                    if (row == 0 && col == 0)
                    {
                        content = new TextBlock()
                        {
                            Text = "Поставщик",
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetRowSpan(border, 2);
                    }
                    else if (row == 0 && col == totalCols - 1)
                    {
                        content = new TextBlock()
                        {
                            Text = "Запас",
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetRowSpan(border, 2);
                    }
                    else if (row == 0 && col == 1)
                    {
                        border.Background = Brushes.LightGray;
                        content = new TextBlock()
                        {
                            Text = "Потребитель",
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetColumnSpan(border, consumersCount);
                    }
                    else if (row == 1 && col > 0 && col <= consumersCount)
                    {
                        border.Background = Brushes.LightGray;
                        content = new TextBlock()
                        {
                            Text = $"B{col}",
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    }
                    else if (col == 0 && row >= 2 && row <= suppliersCount + 1)
                    {
                        border.Background = Brushes.LightGray;
                        int supplierNumber = row - 1;
                        content = new TextBlock()
                        {
                            Text = $"A{supplierNumber}",
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    }
                    else if (row == suppliersCount + 2 && col == 0)
                    {
                        border.BorderThickness = new Thickness(0);
                        border.Background = Brushes.Transparent;
                        border.Margin = new Thickness(0, 15, 0, 5);
                        content = new TextBlock()
                        {
                            Text = "Потребитель:",
                            FontWeight = FontWeights.Bold,
                            FontSize = 14,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetColumnSpan(border, totalCols);
                    }
                    else if (row >= 2 && row <= suppliersCount + 1 && col > 0 && col <= consumersCount)
                    {
                        var textBox = new TextBox()
                        {
                            Width = 70,
                            Height = 35,
                            Text = "0",
                            TextAlignment = TextAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center
                        };
                        textBox.TextChanged += (s, e) => ValidateNumber(textBox);
                        content = textBox;
                    }
                    else if (row >= 2 && row <= suppliersCount + 1 && col == totalCols - 1)
                    {
                        var textBox = new TextBox()
                        {
                            Width = 70,
                            Height = 35,
                            Text = "0",
                            TextAlignment = TextAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center
                        };
                        textBox.TextChanged += (s, e) => ValidateNumber(textBox);
                        content = textBox;
                    }
                    else if (row == totalRows - 1 && col > 0 && col <= consumersCount)
                    {
                        var textBox = new TextBox()
                        {
                            Width = 70,
                            Height = 35,
                            Text = "0",
                            TextAlignment = TextAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center
                        };
                        textBox.TextChanged += (s, e) => ValidateNumber(textBox);
                        content = textBox;
                    }

                    if (content != null)
                    {
                        border.Child = content;
                        Grid.SetRow(border, row);
                        Grid.SetColumn(border, col);
                        MainTableGrid.Children.Add(border);
                    }
                }
            }

            costs = new double[suppliersCount, consumersCount];
            supplies = new double[suppliersCount];
            demands = new double[consumersCount];
            allocation = new double[suppliersCount, consumersCount];
            totalCost = 0;
            TotalCostTextBlock.Text = "";
            ResultGrid.Children.Clear();
            ResultGrid.RowDefinitions.Clear();
            ResultGrid.ColumnDefinitions.Clear();
        }

        private void ValidateNumber(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
                return;

            if (!double.TryParse(textBox.Text, out _))
            {
                textBox.Background = Brushes.LightPink;
            }
            else
            {
                textBox.Background = Brushes.White;
            }
        }

        private bool ReadTableData()
        {
            try
            {
                Array.Clear(costs, 0, costs.Length);
                Array.Clear(supplies, 0, supplies.Length);
                Array.Clear(demands, 0, demands.Length);

                bool allFilled = true;

                foreach (var child in MainTableGrid.Children)
                {
                    if (child is Border border && border.Child is TextBox textBox)
                    {
                        int row = Grid.GetRow(border);
                        int col = Grid.GetColumn(border);
                        string text = textBox.Text.Trim();

                        if (string.IsNullOrEmpty(text) || text == "0")
                        {
                            allFilled = false;
                            continue;
                        }

                        if (!double.TryParse(text, out double value) || value < 0)
                        {
                            MessageBox.Show($"Некорректное значение: '{text}'",
                                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            textBox.Focus();
                            textBox.SelectAll();
                            return false;
                        }

                        if (row >= 2 && row <= suppliersCount + 1)
                        {
                            if (col >= 1 && col <= consumersCount)
                            {
                                int supplierIndex = row - 2;
                                int consumerIndex = col - 1;
                                costs[supplierIndex, consumerIndex] = value;
                            }
                            else if (col == consumersCount + 1)
                            {
                                int supplierIndex = row - 2;
                                supplies[supplierIndex] = value;
                            }
                        }
                        else if (row == suppliersCount + 3 && col >= 1 && col <= consumersCount)
                        {
                            int consumerIndex = col - 1;
                            demands[consumerIndex] = value;
                        }
                    }
                }

                if (!allFilled)
                {
                    MessageBox.Show("Заполните все ячейки (значения не должны быть 0)!",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении данных: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void Calculate()
        {
            if (!ReadTableData())
                return;

            double totalSupply = supplies.Sum();
            double totalDemand = demands.Sum();

            if (Math.Abs(totalSupply - totalDemand) > 0.001)
            {
                MessageBox.Show($"Задача не сбалансирована!\nЗапасы: {totalSupply:F2}\nПотребности: {totalDemand:F2}\n" +
                                $"Разница: {Math.Abs(totalSupply - totalDemand):F2}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            allocation = new double[suppliersCount, consumersCount];
            totalCost = MinimumCostMethod();

            ShowResult();
        }

        private double MinimumCostMethod()
        {
            double[] supply = (double[])supplies.Clone();
            double[] demand = (double[])demands.Clone();
            double totalCost = 0;

            while (true)
            {
                double minCost = double.MaxValue;
                int minI = -1, minJ = -1;

                for (int i = 0; i < suppliersCount; i++)
                {
                    if (supply[i] > 0.0001)
                    {
                        for (int j = 0; j < consumersCount; j++)
                        {
                            if (demand[j] > 0.0001 && costs[i, j] < minCost)
                            {
                                minCost = costs[i, j];
                                minI = i;
                                minJ = j;
                            }
                        }
                    }
                }

                if (minI == -1) break;

                double amount = Math.Min(supply[minI], demand[minJ]);
                allocation[minI, minJ] = amount;
                totalCost += amount * costs[minI, minJ];
                supply[minI] -= amount;
                demand[minJ] -= amount;
            }

            return totalCost;
        }

        private void ShowResult()
        {
            if (ResultGrid == null) return;

            ResultGrid.Children.Clear();
            ResultGrid.RowDefinitions.Clear();
            ResultGrid.ColumnDefinitions.Clear();

            int totalRows = suppliersCount + 4;
            int totalCols = consumersCount + 2;

            for (int i = 0; i < totalRows; i++)
                ResultGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            for (int i = 0; i < totalCols; i++)
                ResultGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star), MinWidth = 70 });

            for (int row = 0; row < totalRows; row++)
            {
                for (int col = 0; col < totalCols; col++)
                {
                    if (row == totalRows - 1 && col == totalCols - 1)
                        continue;

                    Border border = new Border()
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(1),
                        Padding = new Thickness(5),
                        Background = Brushes.White,
                        MinWidth = 70,
                        MinHeight = 40
                    };

                    string text = "";
                    bool isHeader = false;

                    if (row == 0 && col == 0)
                    {
                        text = "Поставщик";
                        isHeader = true;
                        Grid.SetRowSpan(border, 2);
                        border.Background = Brushes.LightGray;
                    }
                    else if (row == 0 && col == totalCols - 1)
                    {
                        text = "Запас";
                        isHeader = true;
                        Grid.SetRowSpan(border, 2);
                        border.Background = Brushes.LightGray;
                    }
                    else if (row == 0 && col == 1)
                    {
                        text = "Потребитель";
                        isHeader = true;
                        Grid.SetColumnSpan(border, consumersCount);
                        border.Background = Brushes.LightGray;
                    }
                    else if (row == 1 && col > 0 && col <= consumersCount)
                    {
                        text = $"B{col}";
                        isHeader = true;
                        border.Background = Brushes.LightGray;
                    }
                    else if (col == 0 && row >= 2 && row <= suppliersCount + 1)
                    {
                        int supplierNumber = row - 1;
                        text = $"A{supplierNumber}";
                        isHeader = true;
                        border.Background = Brushes.LightGray;
                    }
                    else if (row == suppliersCount + 2 && col == 0)
                    {
                        border.BorderThickness = new Thickness(0);
                        border.Background = Brushes.Transparent;
                        border.Margin = new Thickness(0, 15, 0, 5);
                        text = "Потребитель:";
                        isHeader = true;
                        Grid.SetColumnSpan(border, totalCols);
                    }
                    else if (row >= 2 && row <= suppliersCount + 1 && col == totalCols - 1)
                    {
                        text = supplies[row - 2].ToString("F2");
                        border.Background = Brushes.LightGray;
                    }
                    else if (row == totalRows - 1 && col > 0 && col <= consumersCount)
                    {
                        text = demands[col - 1].ToString("F2");
                        border.Background = Brushes.LightGray;
                    }
                    else if (row >= 2 && row <= suppliersCount + 1 && col > 0 && col <= consumersCount)
                    {
                        double value = allocation[row - 2, col - 1];
                        text = value > 0 ? value.ToString("F2") : "0";
                        if (value > 0)
                        {
                            border.Background = Brushes.LightGreen;
                            border.BorderBrush = Brushes.Green;
                            border.BorderThickness = new Thickness(2);
                        }
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        TextBlock textBlock = new TextBlock()
                        {
                            Text = text,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Center
                        };

                        if (isHeader)
                        {
                            textBlock.FontWeight = FontWeights.Bold;
                        }

                        border.Child = textBlock;
                        Grid.SetRow(border, row);
                        Grid.SetColumn(border, col);
                        ResultGrid.Children.Add(border);
                    }
                }
            }

            TotalCostTextBlock.Text = $"ОБЩАЯ СТОИМОСТЬ: {totalCost:F2}";
            TotalCostTextBlock.Foreground = Brushes.DarkGreen;
        }

        // ЗАГРУЗКА ИЗ ФАЙЛА
        private void LoadFromFile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                // Фильтруем строки: убираем комментарии и пустые строки
                var validLines = lines
                    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("//"))
                    .Select(line => line.Trim())
                    .ToList();

                if (validLines.Count < 3)
                {
                    MessageBox.Show("Файл содержит недостаточно данных!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Определяем размеры из первой строки
                string[] firstLine = validLines[0].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int rows = firstLine.Length;
                int cols = firstLine.Length;

                // Проверяем, что размеры не превышают 6
                if (rows > 6 || cols > 6)
                {
                    MessageBox.Show("Максимальный размер матрицы - 6x6!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Устанавливаем размеры
                suppliersCount = rows;
                consumersCount = cols;

                // Обновляем ComboBox'ы
                SuppliersComboBox.SelectedItem = SuppliersComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => int.Parse(item.Content.ToString()) == suppliersCount);
                ConsumersComboBox.SelectedItem = ConsumersComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => int.Parse(item.Content.ToString()) == consumersCount);

                // Перегенерируем таблицу с новыми размерами
                GenerateTable();

                // Заполняем матрицу стоимостей (первые rows строк)
                int lineIndex = 0;
                for (int i = 0; i < rows; i++)
                {
                    string[] numbers = validLines[lineIndex++].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < cols && j < numbers.Length; j++)
                    {
                        costs[i, j] = double.Parse(numbers[j]);
                    }
                }

                // Читаем запасы (следующая строка)
                if (lineIndex < validLines.Count)
                {
                    string[] supplyValues = validLines[lineIndex++].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < rows && i < supplyValues.Length; i++)
                    {
                        supplies[i] = double.Parse(supplyValues[i]);
                    }
                }

                // Читаем потребности (следующая строка)
                if (lineIndex < validLines.Count)
                {
                    string[] demandValues = validLines[lineIndex++].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < cols && i < demandValues.Length; i++)
                    {
                        demands[i] = double.Parse(demandValues[i]);
                    }
                }

                // Обновляем таблицу с данными
                UpdateTableWithData();
                MessageBox.Show("Данные успешно загружены!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файла:\n{ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTableWithData()
        {
            foreach (var child in MainTableGrid.Children)
            {
                if (child is Border border && border.Child is TextBox textBox)
                {
                    int row = Grid.GetRow(border);
                    int col = Grid.GetColumn(border);

                    if (row >= 2 && row <= suppliersCount + 1 && col >= 1 && col <= consumersCount)
                    {
                        textBox.Text = costs[row - 2, col - 1].ToString();
                    }
                    else if (row >= 2 && row <= suppliersCount + 1 && col == consumersCount + 1)
                    {
                        textBox.Text = supplies[row - 2].ToString();
                    }
                    else if (row == suppliersCount + 3 && col >= 1 && col <= consumersCount)
                    {
                        textBox.Text = demands[col - 1].ToString();
                    }
                }
            }
        }

        // СОХРАНЕНИЕ РЕЗУЛЬТАТА В ФАЙЛ
        private void SaveResultToFile(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("=== РЕЗУЛЬТАТ РАСПРЕДЕЛЕНИЯ ===");
                    writer.WriteLine($"Метод: Минимальных элементов");
                    writer.WriteLine($"Поставщиков: {suppliersCount}, Потребителей: {consumersCount}");
                    writer.WriteLine();
                    writer.WriteLine("Матрица распределения:");
                    writer.WriteLine();

                    for (int i = 0; i < suppliersCount; i++)
                    {
                        string line = "";
                        for (int j = 0; j < consumersCount; j++)
                        {
                            line += allocation[i, j].ToString("F2").PadLeft(10);
                        }
                        writer.WriteLine(line);
                    }

                    writer.WriteLine();
                    writer.WriteLine($"Общая стоимость: {totalCost:F2}");
                    writer.WriteLine();
                    writer.WriteLine("=== КОНЕЦ РЕЗУЛЬТАТА ===");
                }

                MessageBox.Show("Результат успешно сохранен!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ОБРАБОТЧИКИ СОБЫТИЙ
        private void SuppliersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateTable();
        }

        private void ConsumersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateTable();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateTable();
            MessageBox.Show("Таблица очищена!", "Информация",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.Title = "Выберите файл с данными";

            if (openFileDialog.ShowDialog() == true)
            {
                LoadFromFile(openFileDialog.FileName);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (allocation == null || totalCost == 0)
            {
                MessageBox.Show("Сначала выполните расчет!", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.Title = "Сохраните результат";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.FileName = "result_minimum";

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveResultToFile(saveFileDialog.FileName);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
