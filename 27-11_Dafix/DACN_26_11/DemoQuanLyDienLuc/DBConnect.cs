using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoQuanLyDienLuc
{
    class DBConnect
    {
        SqlConnection conn;


        public SqlConnection Conn
        {
            get { return conn; }
            set { conn = value; }
        }

        public string strConn1 = @"Data Source=TRUONGDUY\SQLEXPRESS;Initial Catalog=QLDienMienNam;Integrated Security=True;";


        public static string strConn = @"Data Source=TRUONGDUY\SQLEXPRESS;Initial Catalog=QLDienMienNam;Integrated Security=True;";
        public DBConnect()
        {
            conn = new SqlConnection(strConn);
        }

        public int ExecuteNonQuery(SqlCommand cmd)
        {
            return cmd.ExecuteNonQuery();
        }



        public SqlConnection GetConnection()
        {
            return new SqlConnection(strConn);
        }
        public void open()
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        public DataTable getDataTable(string sql, SqlParameter[] parameters = null)
        {
            //Mới sửa lại chỗ này
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(strConn1))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                       
                        cmd.Parameters.Clear();
                        foreach (SqlParameter param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.ParameterName, param.Value);
                        }
                    }

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public void close()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }

        public int getNonQuery(string sql, SqlParameter[] parameters)
        {
            open();
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Thêm các tham số vào câu lệnh SQL
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            int kq = cmd.ExecuteNonQuery();
            close();
            return kq;
        }
        //public object getScalar(string sql, SqlParameter[] parameters)
        //{
        //    open();
        //    SqlCommand cmd = new SqlCommand(sql, conn);
        //    object kq = cmd.ExecuteScalar();
        //    close();
        //    return kq;

        //    //using (SqlConnection connection = new SqlConnection(strConn))
        //    //{
        //    //    using (SqlCommand command = new SqlCommand(sql, connection))
        //    //    {
        //    //        if (parameters != null)
        //    //        {
        //    //            command.Parameters.AddRange(parameters);
        //    //        }
        //    //        connection.Open();
        //    //        return command.ExecuteScalar();
        //    //    }
        //    //}
        //}

        public object getScalar(string sql, SqlParameter[] parameters)
        {
            open();
            SqlCommand cmd = new SqlCommand(sql, conn);

            // Thêm parameters vào cmd nếu có
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            object kq = cmd.ExecuteScalar();
            close();
            return kq;
        }



        public DataTable getDataTable(string sql)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            da.Fill(dt);
            return dt;
        }

        public SqlDataReader getDataReader(string sql)
        {
            open();
            SqlCommand cmd = new SqlCommand(sql, conn);
            SqlDataReader dr = cmd.ExecuteReader();
            close();
            return dr;
        }
        public int updateData(DataTable dt, string sql)
        {
            SqlDataAdapter adapt = new SqlDataAdapter(sql, conn);
            SqlCommandBuilder cmd = new SqlCommandBuilder(adapt);
            int kq = adapt.Update(dt);
            return kq;
        }
    }
}
