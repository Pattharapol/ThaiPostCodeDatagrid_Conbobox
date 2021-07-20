using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PostCodeDataGrid
{
    public partial class frmPostCodeDataGrid : Form
    {
        // nugat system.data.oledb
        private OleDbConnection Conn;

        private OleDbCommand Cmd;
        private OleDbDataReader DR;
        private OleDbDataAdapter DA;
        private string sql = string.Empty;
        private DataGridViewComboBoxColumn ColProvice = new DataGridViewComboBoxColumn();
        private DataGridViewComboBoxColumn ColAmphur = new DataGridViewComboBoxColumn();
        private DataGridViewComboBoxColumn ColTumbon = new DataGridViewComboBoxColumn();
        private DataGridViewTextBoxColumn ColPostCode = new DataGridViewTextBoxColumn();

        public frmPostCodeDataGrid()
        {
            InitializeComponent();
        }

        private void frmPostCodeDataGrid_Load(object sender, EventArgs e)
        {
            // กำหนด Path ของไฟล์ฐานข้อมูล
            string strPath = Application.StartupPath;
            strPath = strPath.ToLower();
            //strPath = strPath.Replace(@"\net5.0-windows\", "");
            strPath = strPath.Replace(@"\bin\debug", @"\");

            string strConn = string.Format($"Provider=Microsoft.Jet.OLEDB.4.0;Data Source ={strPath}Data\\PostCode.mdb;Persist Security Info=False;");

            try
            {
                // เปิดการเชื่อมต่อไฟล์ฐานข้อมูล
                Conn = new OleDbConnection(strConn);
                Conn.Open();
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.MultiSelect = false;

                // จัดการ จังหวัด
                ColProvice.Name = "Province";
                ColProvice.HeaderText = "จังหวัด";
                ColProvice.ReadOnly = false;
                ColProvice.MaxDropDownItems = 10;
                dataGridView1.Columns.Add(ColProvice);
                LoadProvince();

                // จัดการ อำเภอ
                ColAmphur.Name = "Amphur";
                ColAmphur.HeaderText = "เขต/อำเภอ";
                ColAmphur.ReadOnly = false;
                ColAmphur.MaxDropDownItems = 10;
                dataGridView1.Columns.Add(ColAmphur);

                // จัดการ ตำบล
                ColTumbon.Name = "Tumbon";
                ColTumbon.HeaderText = "ตำบล";
                ColTumbon.ReadOnly = false;
                ColTumbon.MaxDropDownItems = 10;
                dataGridView1.Columns.Add(ColTumbon);

                // จัดการ รหัสไปรษณีย์
                ColPostCode.Name = "PostCode";
                ColPostCode.HeaderText = "รหัสไปรษณีย์";
                ColPostCode.ReadOnly = false;
                dataGridView1.Columns.Add(ColPostCode);

                // Add New Row
                dataGridView1.Rows.Add("", "", "", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Exit();
            }
        }

        /// <summary>
        /// โหลดรายชื่อจังหวัดเข้าสู่ ComboBox
        /// </summary>
        private void LoadProvince()
        {
            // DISTINCT คือ หากชื่อรายการมันซ้ำ ต้องตัดให้เหลือเพียงรายการเดียว
            sql = "SELECT DISTINCT PostCode.Province From PostCode ORDER BY PostCode.Province";
            Cmd = new OleDbCommand(sql, Conn);
            DR = Cmd.ExecuteReader();
            while (DR.Read())
            {
                ColProvice.Items.Add(DR["Province"].ToString());
            }
            Conn.Close();
        }

        /// <summary>
        /// โหลดรายชื่ออำเภอ หลังจากเลือกจังหวัด
        /// </summary>
        /// <param name="Province"></param>
        /// <param name="row"></param>
        private void LoadAmphur(string Province, int row)
        {
            sql = string.Format(@"SELECT DISTINCT PostCode.Amphur, PostCode.Province From PostCode WHERE Province = '{0}' ORDER BY PostCode.Amphur", Province);

            try
            {
                DataGridViewRow gridRow = dataGridView1.Rows[row];
                DataGridViewComboBoxCell MyCell = new DataGridViewComboBoxCell();
                MyCell = (DataGridViewComboBoxCell)gridRow.Cells["Amphur"];
                if (Conn.State == ConnectionState.Closed) Conn.Open();
                DA = new OleDbDataAdapter(sql, Conn);
                DataTable dt = new DataTable();
                DA.Fill(dt);
                MyCell.Items.Clear();
                foreach (DataRow dataRow in dt.Rows)
                {
                    MyCell.Items.Add(dataRow["Amphur"].ToString());
                }
                dt.Dispose();
                DA.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// โหลดรายชื่อตำบล หลังจากเลือกจังหวัดและอำเภอ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadTumbon(string Province, string Amphur, int row)
        {
            sql = string.Format(@"SELECT PostCode.Amphur, PostCode.Province, PostCode.Tumbon From PostCode WHERE Province = '{0}' AND Amphur = '{1}' ORDER BY PostCode.Tumbon", Province, Amphur);

            try
            {
                DataGridViewRow gridRow = dataGridView1.Rows[row];
                DataGridViewComboBoxCell MyCell = new DataGridViewComboBoxCell();
                MyCell = (DataGridViewComboBoxCell)gridRow.Cells["Tumbon"];
                if (Conn.State == ConnectionState.Closed) Conn.Open();
                DA = new OleDbDataAdapter(sql, Conn);
                DataTable dt = new DataTable();
                DA.Fill(dt);
                MyCell.Items.Clear();
                foreach (DataRow dataRow in dt.Rows)
                {
                    MyCell.Items.Add(dataRow["Tumbon"].ToString());
                }
                dt.Dispose();
                DA.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// สุดท้าย โหลด รหัสไปรษณีย์
        /// </summary>
        /// <param name="province"></param>
        /// <param name="amphur"></param>
        /// <param name="tumbon"></param>
        /// <param name="rowIndex"></param>
        private void LoadPostCode(string province, string amphur, string tumbon, int rowIndex)
        {
            sql = string.Format(@"SELECT PostCode.Province, PostCode.Amphur, PostCode.Tumbon, PostCode.PostCode, PostCode.Remark From PostCode WHERE Province = '{0}' AND Amphur = '{1}' AND Tumbon = '{2}' ORDER BY PostCode.Tumbon", province, amphur, tumbon);

            try
            {
                if (Conn.State == ConnectionState.Closed) Conn.Open();
                Cmd = new OleDbCommand(sql, Conn);
                DR = Cmd.ExecuteReader();
                while (DR.Read())
                {
                    dataGridView1.Rows[rowIndex].Cells[3].Value = DR["PostCode"].ToString();
                }
                DR.Close();
                Cmd.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string Province = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
            string Amphur = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
            string Tumbon = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();

            switch (e.ColumnIndex)
            {
                case 0:
                    //dataGridView1[1, e.RowIndex].Value.ToString();
                    LoadAmphur(Province, e.RowIndex);
                    break;

                case 1:
                    if (Province != "" && Amphur != "")
                    {
                        LoadTumbon(Province, Amphur, e.RowIndex);
                    }
                    break;

                case 2:
                    if (Province != "" && Amphur != "" && Tumbon != "")
                    {
                        LoadPostCode(Province, Amphur, Tumbon, e.RowIndex);
                    }
                    break;
            }
        }

        private void btnAddRow_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add("", "", "", "");
        }

        private void btnRemoveROw_Click(object sender, EventArgs e)
        {
            if (dataGridView1.RowCount == 0)
            {
                return;
            }
            else
            {
                dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
            }
        }
    }
}