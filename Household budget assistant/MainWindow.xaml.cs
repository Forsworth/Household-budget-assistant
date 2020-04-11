using Microsoft.Win32;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Spire.Xls;
using System.Xml;
using System.Windows.Input;
using Household_budget_assistant.Properties;


/* 
Известные баги/недоделки:
- сделать график отображения данных 
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
        private string pathXml = Settings.Default.pathXml;
        private string pathExcel = Settings.Default.pathExcel;
        private bool isXmlPath;

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
            TTipAddRow(); 
        }

        private void AddRow()
        {
            string warningA = "Wrong input. Make sure you filled all fields with correct data!";
            string titleA = "Wrong data entered";
            string warningB = "The value is too big to compute!";
            string titleB = "Value Error"; 
            try
            {
                DataRow nextRow = dataSource.getDataTable().NewRow();
                nextRow[0] = DatePicker.SelectedDate.Value.Date.ToShortDateString();
                nextRow[1] = CbbxType.SelectedValue;
                nextRow[2] = Convert.ToString(NameField1.Text);
                nextRow[3] = Convert.ToDecimal(ExpensesField1.Text);
                nextRow[4] = Convert.ToDecimal(IncomeField1.Text);
                nextRow[6] = Convert.ToDecimal(SavingsField1.Text);
                nextRow[7] = Convert.ToString(CommentsField.Text);
                dataSource.getDataTable().Rows.Add(nextRow);
                DataGridView.ItemsSource = dataSource.getDataTable().AsDataView();
                UpdateTotal();
            }
            catch (ArgumentException) { MessageBox.Show(warningA, titleA); }
            catch (OverflowException) { MessageBox.Show(warningB, titleB); }
            catch (FormatException) { MessageBox.Show(warningA, titleA); }
            catch (InvalidOperationException) { MessageBox.Show(warningA, titleA); }
        }
        private void BtnAddRow_Click(object sender, RoutedEventArgs e) //добавление рядов
        {
            AddRow();
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
            BalanceBox.SelectedText = Convert.ToString(0);
        }

        private void DeleteSelectedRow()
        {
            string warning = "Some computed fields " +
        "contain null values! Please change the source file null values by 0.";
            while (DataGridView.SelectedItems.Count >= 1) 
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

        private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e) //выборочное удаление рядов
        {
            DeleteSelectedRow();
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

        private void UpdateTotal() 
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

        private void BtnAbout_Click(object sender, RoutedEventArgs e) //информационная сводка о создателе + горячие клавиши
        {
            String info = "This software is open-source and available for " +
            "everyone to change/use/modify etc. If you need the project files, " +
            "or any additional info, e-mail me at *E-MAIL ADDRESS*" +
            "\n\nHotkeys: Ctrl+R - adds row, Ctrl+D - deletes selected row, " +
            "Ctrl+S - saves as xml, Ctrl+E - saves as xlsx." +
            "\n\nDeveloped by 'Vekktrsz.', 2020";
            String title = "Household Budget Assistant";
            MessageBox.Show(info, title);
        }

        private void SaveAsXml()
        {
            saveFileDialog.Filter = "XML-File | *.xml";
            if (saveFileDialog.ShowDialog() == true)
                dataSource.getDataTable().WriteXml(saveFileDialog.FileName);
            pathXml = this.saveFileDialog.FileName;
            isXmlPath = true;
            Settings.Default.pathXml = pathXml;
            Settings.Default.Save();
        }

        private void BtnSaveAsXml_Click(object sender, RoutedEventArgs e) // кнопка "сохранить как XML"
        {
            SaveAsXml();
        }

        private void SaveAsExcel()
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
                pathExcel = this.saveFileDialog.FileName;
                isXmlPath = false;
                Settings.Default.pathExcel = pathExcel;
                Settings.Default.Save();
            }
            catch (ArgumentOutOfRangeException) { return; }
        }

        private void BtnSaveToExcel(object sender, RoutedEventArgs e) //сохранение в excel
        {
            SaveAsExcel();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e) 
        {
            string warning = "Couldn't find saved path!";
            string title = "Unknown path error";
            try
            {
                if (isXmlPath) //doesn't save path
                {
                    dataSource.getDataTable().WriteXml(pathXml);
                }
                else if (!isXmlPath) //works
                {
                    Workbook workbook = new Workbook();
                    Worksheet sheet = workbook.Worksheets[0];
                    sheet.InsertDataTable((DataTable)this.dataSource.getDataTable(), true, 1, 1, -1, -1);
                    sheet.AllocatedRange.AutoFitColumns();
                    sheet.AllocatedRange.AutoFitRows();
                    workbook.SaveToFile(pathExcel, ExcelVersion.Version2016);
                }
            }          
            catch (ArgumentException) { MessageBox.Show(warning, title); }
        }

        private void BtnOpenXML_Click(object sender, RoutedEventArgs e) // кнопка "открыть XML"
        {
            openFileDialog.Filter = "xml files (*.xml)|*.xml;|All files (*.*)|*.*";
            pathXml = openFileDialog.FileName; 
            if (openFileDialog.ShowDialog() == true)
                dataSource.getDataTable().Rows.Clear();
            pathXml = openFileDialog.FileName;
            isXmlPath = true;
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
                pathExcel = openFileDialog.FileName;
                isXmlPath = false;
            }
            catch (ArgumentNullException) { return; }
            catch (ArgumentException) { return; }
            catch (DuplicateNameException) { MessageBox.Show(warning, title); }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.R))
            {
                AddRow();
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.D))
            {
                DeleteSelectedRow(); 
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S))
            {
                SaveAsXml();      
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.E))
            {
                SaveAsExcel();
            }
        }


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
