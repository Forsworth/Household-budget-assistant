using System;
using System.Data;


namespace Personal_Budget_Assistant__Main_
{
    class DataSourceTable
    {
        private DataTable dt = new DataTable("BudgetTable");

        private DataColumn date = new DataColumn("Date", typeof(DateTime));
        private DataColumn type = new DataColumn("Type", typeof(String));
        private DataColumn name = new DataColumn("Name", typeof(String));
        private DataColumn expenses = new DataColumn("Expenses", typeof(decimal));
        private DataColumn income = new DataColumn("Income", typeof(decimal));
        private DataColumn saldo = new DataColumn("Saldo", typeof(decimal), "Income - Expenses");
        private DataColumn savings = new DataColumn("Savings", typeof(decimal));
        private DataColumn comments = new DataColumn("Comments", typeof(String));

        public DataTable getDataTable()
        {
            return this.dt;
        }
        public void FillDataGridView()
        {
            dt.Columns.Add(date);
            dt.Columns.Add(type);
            dt.Columns.Add(name);
            dt.Columns.Add(expenses);
            dt.Columns.Add(income);
            dt.Columns.Add(saldo);
            dt.Columns.Add(savings);
            dt.Columns.Add(comments);
        }
    }
}
