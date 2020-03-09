using System;
using System.Data;


namespace Personal_Budget_Assistant__Main_
{
    class DataSourceTable
    {
        private DataTable _dt = new DataTable("BudgetTable");

        public DataTable getDataTable()
        {
            return this._dt;
        }
        public void FillDataGridView()
        {
            DataColumn date = new DataColumn("Date", typeof(DateTime));
            DataColumn type = new DataColumn("Type", typeof(String));
            DataColumn name = new DataColumn("Name", typeof(String));
            DataColumn expenses = new DataColumn("Expenses", typeof(decimal));
            DataColumn income = new DataColumn("Income", typeof(decimal));
            DataColumn saldo = new DataColumn("Saldo", typeof(decimal), "Income - Expenses");
            DataColumn savings = new DataColumn("Savings", typeof(decimal));
            DataColumn comments = new DataColumn("Comments", typeof(String));
            _dt.Columns.Add(date);
            _dt.Columns.Add(type);
            _dt.Columns.Add(name);
            _dt.Columns.Add(expenses);
            _dt.Columns.Add(income);
            _dt.Columns.Add(saldo);
            _dt.Columns.Add(savings);
            _dt.Columns.Add(comments);
        }
    }
}
