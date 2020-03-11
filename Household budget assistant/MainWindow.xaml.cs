using Microsoft.Win32;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Spire.Xls;
using System.Xml;


/* 
Известные баги/недоделки:
- запилить график отображения данных
- сделать hotkey удаления рядов
- сделать возможность распечатывать таблицу
*/

namespace Personal_Budget_Assistant__Main_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private DataSourceTable items = new DataSourceTable();
        private OpenFileDialog openFileDialog = new OpenFileDialog();
        private SaveFileDialog saveFileDialog = new SaveFileDialog();
        private Workbook book = new Workbook();
        private Workbook data_book = new Workbook();
        public String path;

        public MainWindow()
        {
            InitializeComponent();
            items.FillDataGridView();
            DataGridView.ItemsSource = items.getDataTable().AsDataView();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TTipAddRow(); //programatically adding tooltip (just 4 fun)
        }

        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            string warningA = "Wrong input. Make sure you filled all fields with correct data!";
            string titleA = "Wrong data entered";
            string warningB = "The value is too big to compute!";
            string titleB = "Value Error";
            try
            {
                DataRow nextRow = items.getDataTable().NewRow();
                nextRow[0] = DateTime.Now.ToString();
                nextRow[1] = CbbxType.SelectedValue;
                nextRow[2] = Convert.ToString(NameField1.Text);
                nextRow[3] = Convert.ToDecimal(ExpensesField1.Text);
                nextRow[4] = Convert.ToDecimal(IncomeField1.Text);
                nextRow[6] = Convert.ToDecimal(SavingsField1.Text);
                nextRow[7] = Convert.ToString("No comments yet");
                items.getDataTable().Rows.Add(nextRow);
                DataGridView.ItemsSource = items.getDataTable().AsDataView();
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

        private void BtnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to erase all data from the current table?", "Delete all rows?",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                items.getDataTable().Rows.Clear();
            BalanceBox.SelectedText = Convert.ToString(0);//seriously?
        }

        private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            while (DataGridView.SelectedItems.Count >= 1)
            {
                decimal tmpInc;
                decimal tmpExp;
                decimal diff;
                try
                {
                    decimal balanceCol = Convert.ToDecimal(BalanceBox.SelectedText);

                decimal result;
                DataRowView drv = (DataRowView)DataGridView.SelectedItem;
                tmpInc = Convert.ToDecimal(drv.Row.ItemArray.GetValue(3));
                tmpExp = Convert.ToDecimal(drv.Row.ItemArray.GetValue(4));
                diff = tmpInc + tmpExp;
                drv.Row.Delete();
                UpdateTotal();
                result = balanceCol - diff;
                if (items.getDataTable().Rows.Count > 0)
                {
                    BalanceBox.SelectedText = result.ToString();
                    UpdateTotal();
                }
                else BalanceBox.SelectedText = Convert.ToString(0);
                UpdateTotal();
                }
                catch (FormatException) { return; }
                catch (InvalidCastException) { MessageBox.Show("Some computed fields contain null values! Please change the source file null values by 0."); return; }
            }
        }

        private void BtnTotal_Click(object sender, RoutedEventArgs e)
        {
            decimal total;
            string warningA = "Not enough data to make the calculation!";
            string titleA = "Wrong data";
            string warningB = "The value is too big to compute!";
            string titleB = "Value Error";
            string title = "The overall balance is:";

            try
            {
                decimal sumIncome = Convert.ToDecimal(items.getDataTable().Compute("SUM(Income)", string.Empty));
                decimal sumExpense = Convert.ToDecimal(items.getDataTable().Compute("SUM(Expenses)", string.Empty));
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
                decimal sumIncome = Convert.ToDecimal(items.getDataTable().Compute("SUM(Income)", string.Empty));
                decimal sumExpense = Convert.ToDecimal(items.getDataTable().Compute("SUM(Expenses)", string.Empty));
                total = sumIncome - sumExpense;
                BalanceBox.SelectedText = total.ToString();
            }
            catch (InvalidCastException) { return; }
            catch (OverflowException) { return; }
        }

        private void BtnSavings_Click(object sender, RoutedEventArgs e)
        {
            string warning = "Not enough data to make the calculation!";
            string warningTitle = "Wrong data";
            string title = "You have managed to save:";
            try
            {
                decimal savings = Convert.ToDecimal(items.getDataTable().Compute("SUM(Savings)", string.Empty));
                MessageBox.Show(savings.ToString(), title);

            }
            catch (InvalidCastException) { MessageBox.Show(warning, warningTitle); }
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            String info = "This software is open-source and available for " +
            "everyone to change/use/modify etc. If you need the project files, " +
            "or any additional info, e-mail me @ Vekktrsz@gmail.com" +
            "\n\nDeveloped by 'Vekktrsz.', 2020";
            String title = "Personal Budget Assistant";
            MessageBox.Show(info, title);
        }

        private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            saveFileDialog.Filter = "XML-File | *.xml";
            if (saveFileDialog.ShowDialog() == true)
                items.getDataTable().WriteXml(saveFileDialog.FileName);
            path = openFileDialog.FileName;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e) // doesn't work they way it should
            //need to find the way to store path variable after the app shuts down
        {
            try
            {
                items.getDataTable().WriteXml(path);
            }
            catch (ArgumentException) { MessageBox.Show("Couldn't find saved path!", "Unknown path error"); }
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = "xml files (*.xml)|*.xml;|All files (*.*)|*.*";
            path = openFileDialog.FileName;
            if (openFileDialog.ShowDialog() == true)
                items.getDataTable().Rows.Clear();
            try
            {
                items.getDataTable().ReadXml(openFileDialog.FileName);
                UpdateTotal();
            }
            catch (ArgumentException) { return; }
            catch (XmlException) { return; }
        }

        private void BtnImportFromExcel(object sender, RoutedEventArgs e) 
        {
            openFileDialog.Filter = "excel files (*.xlsx)|*xls;*.xlsx;|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                book.LoadFromFile(openFileDialog.FileName);
            }
            try
            {
            DataTable table = book.Worksheets[0].ExportDataTable(); //may throw duplicate name exception 
            data_book.Worksheets[0].InsertDataTable(table, true, 1, 1);
            data_book.SaveToFile(openFileDialog.FileName, ExcelVersion.Version2016);
                foreach (DataRow dr in table.Rows)
                {
                items.getDataTable().ImportRow(dr);
                }
                UpdateTotal();
            }
            catch (ArgumentNullException) { return;  }
            catch (ArgumentException) { return; }
            catch (DuplicateNameException) { MessageBox.Show("Your excel file contains duplicate column!","Duplicate Column"); }
        }

        private void BtnSaveToExcel(object sender, RoutedEventArgs e)
        {
            try
            {
                Workbook workbook = new Workbook();
                Worksheet sheet = workbook.Worksheets[0];
                saveFileDialog.Filter = "excel file | *.xlsx;*xls;";
                if (saveFileDialog.ShowDialog() == true)
                //Export datatable to excel
                sheet.InsertDataTable((DataTable)this.items.getDataTable(), true, 1, 1, -1, -1);
                sheet.AllocatedRange.AutoFitColumns();
                sheet.AllocatedRange.AutoFitRows();
                //Save the file
                workbook.SaveToFile(saveFileDialog.FileName, ExcelVersion.Version2013);
            }
            catch (System.ArgumentOutOfRangeException) { return; }
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
