using System;
using System.Data;
using System.Data.SqlClient;

namespace uploader
{
    class MSSERVERload
    {
        private string ConnectionString;
        private SqlConnection cn;
        private SqlCommand cmd = new SqlCommand();
        private SqlDataReader dr;
        public void saveData(string name, string email, object docs, string resultNum)
        {
            ConnectionString = @"Server=." + "\\" + "SQLTIN;Database=reodumm;uid=reallyty;pwd=esnoyy";
            cn = new SqlConnection(ConnectionString);
            try
            {
                string q = "select ID from workC";
                cmd.Connection = cn;
                cmd.CommandText = q;
                cn.Open();
                dr = cmd.ExecuteReader();
                int lastId = 0;
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        lastId = int.Parse(dr[0].ToString());
                    }
                }
                lastId += 1;
                dr.Close();
                cn.Close();

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"INSERT INTO workC(ID , Name, Email, Docs, idnumber) 
                    VALUES('" + lastId + "', '" + name + "', '" + email + "', '" + docs + "', '" + resultNum + "')";
                cmd.Parameters.AddWithValue("@ID", lastId);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Docs", docs);
                cmd.Parameters.AddWithValue("@idnumber", resultNum);
                cn.Open();
                cmd.ExecuteNonQuery();
                dr.Close();
                cn.Close();
                //System.Windows.Forms.MessageBox.Show(lastId+". An Item has been successfully added", "Caption", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            }
            catch (Exception e)
            {
                cn.Close();
                MessageBoxEx.Show(new MainWindow().Wind(), 
                    "MICROSOFT SQL SERVER ERROR: " + e.Message.ToString());
            }
        }
    }
}
