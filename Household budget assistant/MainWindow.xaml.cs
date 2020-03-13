using Microsoft.Win32;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Spire.Xls;
using System.Xml;


/* 
Известные баги/недоделки:
- сделать график отображения данных (нужен новый класс?)
- сделать hotkey удаления рядов (каким образом? Пока наиболее интересующий вопрос)
- сделать возможность распечатывать таблицу 
*/

namespace Personal_Budget_Assistant__Main_
{
    public partial class MainWindow : Window
    {
        private DataSourceTable dataSource;
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private Workbook book;
        private Workbook data_book;
        public string path;

        public MainWindow()
        {
            InitializeComponent();
            dataSource = new DataSourceTable(); 
            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();
            book = new Workbook();
            data_book = new Workbook();
            dataSource.FillDataGridView();
            DataGridView.ItemsSource = dataSource.getDataTable().AsDataView(); //для отображения таблицы в датагриде
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TTipAddRow(); //добавил программным методом tooltip, хотя лучше это сделать в xaml
        }

        private void BtnAddRow_Click(object sender, RoutedEventArgs e) //добавление рядов
        {
            string warningA = "Wrong input. Make sure you filled all fields with correct data!";
            string titleA = "Wrong data entered";
            string warningB = "The value is too big to compute!";
            string titleB = "Value Error"; /*насколько оправдано такое применение string для "читаемости" кода?
            сделал это только для этого, хотя плодить такие переменные, думаю, не лучшая практика*/
            try
            {
                DataRow nextRow = dataSource.getDataTable().NewRow();
                nextRow[0] = DatePicker.SelectedDate.Value.Date.ToShortDateString();
                nextRow[1] = CbbxType.SelectedValue;
                nextRow[2] = Convert.ToString(NameField1.Text);
                nextRow[3] = Convert.ToDecimal(ExpensesField1.Text);
                nextRow[4] = Convert.ToDecimal(IncomeField1.Text);
                nextRow[6] = Convert.ToDecimal(SavingsField1.Text);
                nextRow[7] = Convert.ToString("No comments yet");
                dataSource.getDataTable().Rows.Add(nextRow);
                DataGridView.ItemsSource = dataSource.getDataTable().AsDataView();
                UpdateTotal();
            }
            catch (ArgumentException) { MessageBox.Show(warningA, titleA); }
            catch (OverflowException) { MessageBox.Show(warningB, titleB); }
            catch (FormatException) { MessageBox.Show(warningA, titleA); }
        }
        private void TTipAddRow()
        {
            ToolTip toolTip = new ToolTip();
            StackPanel toolTipPanel = new StackPanel();
            toolTipPanel.Children.Add(new TextBlock { Text = "Adds a row to the current table" });
            toolTip.Content = toolTipPanel;
            BtnAddRow.ToolTip = toolTip;
        }

        private void BtnDeleteAll_Click(object sender, RoutedEventArgs e) //удаление всех рядов
        {
            string warning = "Are you sure you want to erase all data from the current table?";
            string title = "Delete all rows?";
            if (MessageBox.Show(warning, title, MessageBoxButton.YesNo, 
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
                dataSource.getDataTable().Rows.Clear();
            BalanceBox.SelectedText = Convert.ToString(0);//очевидный костыль
        }

        private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e) //выборочное удаление рядов
        {
            string warning = "Some computed fields " +
                    "contain null values! Please change the source file null values by 0.";
            while (DataGridView.SelectedItems.Count >= 1) /*При удалении рядов нужно обновлять колонку баланса.
                Для этого я написал элементарный алгоритм, использующий временные переменные. Вышло громоздко*/
            {
                decimal tmpInc; //доход
                decimal tmpExp; //расход
                decimal diff; //разница
                decimal result; //конечный результат
                try
                {
                    decimal balanceCol = Convert.ToDecimal(BalanceBox.SelectedText);

                
                DataRowView drv = (DataRowView)DataGridView.SelectedItem;
                tmpInc = Convert.ToDecimal(drv.Row.ItemArray.GetValue(3));
                tmpExp = Convert.ToDecimal(drv.Row.ItemArray.GetValue(4));
                diff = tmpInc + tmpExp;
                drv.Row.Delete();
                result = balanceCol - diff;
                if (dataSource.getDataTable().Rows.Count > 0)
                {
                    BalanceBox.SelectedText = result.ToString();
                    UpdateTotal(); //обновляем таблицу
                }
                else BalanceBox.SelectedText = Convert.ToString(0);
                UpdateTotal();
                }
                catch (FormatException) { return; }
                catch (InvalidCastException) { MessageBox.Show(warning); return; }
            }
        }

        private void BtnTotal_Click(object sender, RoutedEventArgs e) //кнопка подсчета общего баланса
        {
            decimal total;
            string warningA = "Not enough data to make the calculation!";
            string titleA = "Wrong data";
            string warningB = "The value is too big to compute!";
            string titleB = "Value Error";
            string title = "The overall balance is:";

            try
            {
                decimal sumIncome = Convert.ToDecimal
                    (dataSource.getDataTable().Compute("SUM(Income)", string.Empty));
                decimal sumExpense = Convert.ToDecimal
                    (dataSource.getDataTable().Compute("SUM(Expenses)", string.Empty));
                total = sumIncome - sumExpense;
                MessageBox.Show(total.ToString(), title);
            }
            catch (InvalidCastException) { MessageBox.Show(warningA, titleA); }
            catch (OverflowException) { MessageBox.Show(warningB, titleB); }
        }

        private void UpdateTotal() /*обновление показаний баланса, написал вручную. 
            (не стал имплементировать INotifyPropertyChanged интерфейс, хотя слышал, 
            что это, вроде бы, гораздо более адекватный способ регистрировать изменения)*/
        {
            decimal total;
            try
            {
                decimal sumIncome = Convert.ToDecimal
                    (dataSource.getDataTable().Compute("SUM(Income)", string.Empty));
                decimal sumExpense = Convert.ToDecimal
                    (dataSource.getDataTable().Compute("SUM(Expenses)", string.Empty));
                total = sumIncome - sumExpense;
                BalanceBox.SelectedText = total.ToString();
            }
            catch (InvalidCastException) { return; }
            catch (OverflowException) { return; }
        }

        private void BtnSavings_Click(object sender, RoutedEventArgs e) //кнопка подсчета накоплений
        {
            string warning = "Not enough data to make the calculation!";
            string warningTitle = "Wrong data";
            string title = "You have managed to save:";
            try
            {
                decimal savings = Convert.ToDecimal
                    (dataSource.getDataTable().Compute("SUM(Savings)", string.Empty));
                MessageBox.Show(savings.ToString(), title);

            }
            catch (InvalidCastException) { MessageBox.Show(warning, warningTitle); }
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e) //информационная сводка о создателе
        {
            String info = "This software is open-source and available for " +
            "everyone to change/use/modify etc. If you need the project files, " +
            "or any additional info, e-mail me at *E-MAIL ADDRESS*" +
            "\n\nDeveloped by 'Vekktrsz.', 2020";
            String title = "Household Budget Assistant";
            MessageBox.Show(info, title);
        }

        private void BtnSaveAs_Click(object sender, RoutedEventArgs e) // кнопка "сохранить как XML"
        {
            saveFileDialog.Filter = "XML-File | *.xml";
            if (saveFileDialog.ShowDialog() == true)
                dataSource.getDataTable().WriteXml(saveFileDialog.FileName);
            path = openFileDialog.FileName;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e) /* тут я пытался сделать так,
            чтобы программа, один раз получив путь сохранения файла в xml могла сохранять изменения при
            нажатии на эту кнопку без вывода диалогового окна. Для этого создал переменную path, поробовал
            добавить в settings, но пока так и не понял, как это правильно сделать. */

        {
            string warning = "Couldn't find saved path!";
            string title = "Unknown path error";
            try
            {
                dataSource.getDataTable().WriteXml(path);
            }
            catch (ArgumentException) { MessageBox.Show(warning, title); }
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e) // кнокп "открыть XML"
        {
            openFileDialog.Filter = "xml files (*.xml)|*.xml;|All files (*.*)|*.*";
            path = openFileDialog.FileName;
            if (openFileDialog.ShowDialog() == true)
                dataSource.getDataTable().Rows.Clear();
            try
            {
                dataSource.getDataTable().ReadXml(openFileDialog.FileName);
                UpdateTotal();
            }
            catch (ArgumentException) { return; }
            catch (XmlException) { return; }
        }

        private void BtnOpenExcel(object sender, RoutedEventArgs e)  // импорт из excel
            /* Важно: файл, созданный не программой, должен соответствовать колонкам таблицы */
        {
            string warning = "Your excel file contains duplicate column!";
            string title = "Duplicate Column";
            openFileDialog.Filter = "excel files (*.xlsx)|*xls;*.xlsx;|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                book.LoadFromFile(openFileDialog.FileName);
            }
            try
            {
            DataTable table = book.Worksheets[0].ExportDataTable(); 
            data_book.Worksheets[0].InsertDataTable(table, true, 1, 1);
            data_book.SaveToFile(openFileDialog.FileName, ExcelVersion.Version2016);
                foreach (DataRow dr in table.Rows)
                {
                dataSource.getDataTable().ImportRow(dr);
                }
                UpdateTotal();
            }
            catch (ArgumentNullException) { return;  }
            catch (ArgumentException) { return; }
            catch (DuplicateNameException) { MessageBox.Show(warning,title); }
        }

        private void BtnSaveToExcel(object sender, RoutedEventArgs e) //сохранение в excel
        {
            try
            {
                Workbook workbook = new Workbook();
                Worksheet sheet = workbook.Worksheets[0];
                saveFileDialog.Filter = "excel file | *.xlsx;*xls;";
                if (saveFileDialog.ShowDialog() == true)
                //Export datatable to excel
                sheet.InsertDataTable((DataTable)this.dataSource.getDataTable(), true, 1, 1, -1, -1);
                sheet.AllocatedRange.AutoFitColumns();
                sheet.AllocatedRange.AutoFitRows();
                //Save the file
                workbook.SaveToFile(saveFileDialog.FileName, ExcelVersion.Version2016);
            }
            catch (ArgumentOutOfRangeException) { return; }
        }


        /*Некоторые методы остались подвисшими. 
        Пока не разобрался, как их убрать отсюда так, чтобы программа не вылетала*/
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SavingsField1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void DataGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
