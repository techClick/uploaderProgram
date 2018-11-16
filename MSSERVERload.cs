using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace uploader
{
    class MSSERVERload
    {
        private String ConnectionString;
        private SqlConnection cn;
        private SqlCommand cmd = new SqlCommand();
        private SqlDataReader dr;
        public void saveData(string name, string email, object docs, string resultNum)
        {
            ConnectionString = @"Server=." + "\\" + "SQLEXPRESS;Database=workCH;uid=debuchy8;pwd=delabEGO234";
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
                System.Windows.MessageBox.Show("MICROSOFT SQL SERVER ERROR: " + e.Message.ToString());
            }
        }
    }
}
