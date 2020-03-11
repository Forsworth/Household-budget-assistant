using System;
using System.Data;


namespace Personal_Budget_Assistant__Main_
{
    class DataSourceTable
    {
        private DataTable _dt = new DataTable("BudgetTable");

        private DataColumn _date = new DataColumn("Date", typeof(DateTime));
        private DataColumn _type = new DataColumn("Type", typeof(String));
        private DataColumn _name = new DataColumn("Name", typeof(String));
        private DataColumn _expenses = new DataColumn("Expenses", typeof(decimal));
        private DataColumn _income = new DataColumn("Income", typeof(decimal));
        private DataColumn _saldo = new DataColumn("Saldo", typeof(decimal), "Income - Expenses");
        private DataColumn _savings = new DataColumn("Savings", typeof(decimal));
        private DataColumn _comments = new DataColumn("Comments", typeof(String));

        public DataTable getDataTable()
        {
            return this._dt;
        }
        public void FillDataGridView()
        {
            _dt.Columns.Add(_date);
            _dt.Columns.Add(_type);
            _dt.Columns.Add(_name);
            _dt.Columns.Add(_expenses);
            _dt.Columns.Add(_income);
            _dt.Columns.Add(_saldo);
            _dt.Columns.Add(_savings);
            _dt.Columns.Add(_comments);
        }
    }
}
