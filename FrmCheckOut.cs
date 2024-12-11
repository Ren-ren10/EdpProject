using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HotelReservationProject
{
    public partial class FrmCheckOut : Form
    {
        private string connectionString = "Data Source=DESKTOP-C41CMDV\\SQLEXPRESS;Initial Catalog=myEdpDB;Integrated Security=True;";
        private string nameFilter;

        private delegate DataTable ExecuteQueryDelegate(string query, SqlParameter[] parameters = null);
        public FrmCheckOut()
        {
            InitializeComponent();
            LoadCheckedInCustomers();
        }

        private void LoadCheckedInCustomers(string nameFilter = "")
        {
            string query = @"
                SELECT Customers.CustomerID, Customers.FullName, Customers.RoomID, Rooms.RoomNumber
                FROM Customers
                INNER JOIN Rooms ON Customers.RoomID = Rooms.RoomID
                WHERE Customers.CheckOut IS NULL";

            if (!string.IsNullOrEmpty(nameFilter))
            {
                query += " AND Customers.FullName LIKE @NameFilter";
            }

            ExecuteQueryDelegate executeQuery = ExecuteQuery;
            SqlParameter[] parameters = !string.IsNullOrEmpty(nameFilter)
                ? new[] { new SqlParameter("@NameFilter", "%" + nameFilter + "%") }
                : null;

            try
            {
                DataTable dt = executeQuery(query, parameters);
                dataGridViewCheckOut.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customer data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }


        private void ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }



        private void btnSearch_Click(object sender, EventArgs e)
        {
            nameFilter = txtSearchName.Text.Trim();

            if (string.IsNullOrEmpty(nameFilter))
            {
                LoadCheckedInCustomers();
            }
            else
            {
                LoadCheckedInCustomers(nameFilter);
            }
        }


        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            if (dataGridViewCheckOut.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a customer to check out.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int customerId = Convert.ToInt32(dataGridViewCheckOut.SelectedRows[0].Cells["CustomerID"].Value);
                int roomId = Convert.ToInt32(dataGridViewCheckOut.SelectedRows[0].Cells["RoomID"].Value);

                
                Action<string, SqlParameter[]> executeNonQuery = ExecuteNonQuery;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string updateCustomerQuery = "UPDATE Customers SET CheckOut = @CheckOut WHERE CustomerID = @CustomerID";
                    executeNonQuery(updateCustomerQuery, new[]
                    {
                        new SqlParameter("@CheckOut", DateTime.Now),
                        new SqlParameter("@CustomerID", customerId)
                    });

                    string updateRoomQuery = "UPDATE Rooms SET Status = 'Available' WHERE RoomID = @RoomID";
                    executeNonQuery(updateRoomQuery, new[]
                    {
                        new SqlParameter("@RoomID", roomId)
                    });

                    MessageBox.Show("Customer checked out successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadCheckedInCustomers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during checkout: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        


        private void FrmCheckOut_Load(object sender, EventArgs e)
        {
            LoadCheckedInCustomers();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {

        }

       
    }
}
