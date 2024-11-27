using Guna.UI2.WinForms.Suite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoQuanLyDienLuc
{
    public partial class DangNhap : Form
    {
        DBConnect dbConnect = new DBConnect();
        public DangNhap()
        {
            InitializeComponent();
        }
        PrivateFontCollection pfc;

        public string NhanVienTag { get; internal set; }

        private void btnLogin_Click_1(object sender, EventArgs e)
        {
            string username = txtUser.Text;
            string password = txtPassword.Text;

            string sql = "SELECT ChucVu FROM TaiKhoan WHERE TenDangNhap = @username AND MatKhau = @password";

            using (SqlCommand cmd = new SqlCommand(sql, dbConnect.Conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                dbConnect.open();
                object result = cmd.ExecuteScalar();
                dbConnect.close();

                if (result != null)
                {
                    string role = result.ToString(); // Lấy chức vụ

                    if (role == "Khách Hàng")
                    {
                        string tenKhachHang = LayTenKhachHang(username);

                        if (!string.IsNullOrEmpty(tenKhachHang))
                        {
                            // Open TrangChu for customers
                            TrangChu trangChuForm = new TrangChu(tenKhachHang, null, null, role); // Truyền chức vụ
                            trangChuForm.FormClosed += (s, args) => this.Show();
                            this.Hide();
                            trangChuForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy thông tin khách hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        var (tenNhanVien, maNhanVien) = LayTenNhanVien(username);

                        if (!string.IsNullOrEmpty(tenNhanVien) && !string.IsNullOrEmpty(maNhanVien))
                        {
                            // Open TrangChu for employees
                            TrangChu trangChuForm = new TrangChu(null, tenNhanVien, maNhanVien, role); // Truyền chức vụ
                            trangChuForm.FormClosed += (s, args) => this.Show();
                            this.Hide();
                            trangChuForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy thông tin nhân viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private (string tenNhanVien, string maNhanVien) LayTenNhanVien(string tenTaiKhoan)
        {
            string tenNhanVien = null;
            string maNhanVien = null; // Khởi tạo mã nhân viên là null
            DBConnect db = new DBConnect();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = @"SELECT nv.TenNhanVien, nv.MaNhanVien 
                         FROM NhanVien nv 
                         INNER JOIN TaiKhoan tk ON nv.MaTaiKhoan = tk.MaTaiKhoan 
                         WHERE tk.TenDangNhap = @TenDangNhap";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenDangNhap", tenTaiKhoan);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // Kiểm tra nếu có kết quả
                        {
                            tenNhanVien = reader["TenNhanVien"].ToString(); // Lấy tên nhân viên
                            maNhanVien = reader["MaNhanVien"].ToString(); // Lấy mã nhân viên
                        }
                    }
                }
            }

            return (tenNhanVien, maNhanVien); // Trả về tuple chứa tên và mã nhân viên
        }


        private string LayTenKhachHang(string tenTaiKhoan)
        {
            string tenKhachHang = "";
            DBConnect db = new DBConnect();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = @"SELECT kh.TenKhachHang 
                         FROM KhachHang kh 
                         INNER JOIN TaiKhoan tk ON kh.MaTaiKhoan = tk.MaTaiKhoan 
                         WHERE tk.TenDangNhap = @TenDangNhap";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenDangNhap", tenTaiKhoan);
                    tenKhachHang = (string)cmd.ExecuteScalar();
                }
            }
            return tenKhachHang;
        }

        private void btnExit_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {

                Application.Exit();
            }
        }

       

        private void ChkPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !ChkPassword.Checked;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.PasswordChar = '\0'; 

        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick(); // Gọi sự kiện Click của btnLogin
            }
        }

        private void txtUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtPassword.Focus();
            }
        }

        private async Task<bool> CheckIfTableHasDataAsync(string tableName)
        {
            using (SqlConnection conn = new SqlConnection(dbConnect.strConn1))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand($"SELECT COUNT(*) FROM {tableName}", conn))
                {
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        // Gọi API để lấy dữ liệu
        private async Task<JArray> GetProvincesDataFromApiAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://provinces.open-api.vn/api/?depth=3");
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JArray.Parse(jsonResponse);
            }
        }

        // Thêm dữ liệu vào bảng Tinh
        private async Task InsertDataToTableTinhAsync(JArray provincesData)
        {
            using (SqlConnection conn = new SqlConnection(dbConnect.strConn1))
            {
                await conn.OpenAsync();
                foreach (var province in provincesData)
                {
                    string maTinh = province["code"].ToString();
                    string tenTinh = province["name"].ToString();

                    string query = "INSERT INTO Tinh (MaTinh, TenTinh) VALUES (@MaTinh, @TenTinh)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaTinh", maTinh);
                        cmd.Parameters.AddWithValue("@TenTinh", tenTinh);
                        await cmd.ExecuteNonQueryAsync();

                        // Thêm dữ liệu huyện liên quan
                        await InsertDataToTableHuyenAsync(province["districts"] as JArray, maTinh, conn);
                    }
                }
            }
        }

        // Thêm dữ liệu vào bảng Huyen
        private async Task InsertDataToTableHuyenAsync(JArray districtsData, string maTinh, SqlConnection conn)
        {
            foreach (var district in districtsData)
            {
                string maHuyen = district["code"].ToString();
                string tenHuyen = district["name"].ToString();

                string query = "INSERT INTO Huyen (MaHuyen, TenHuyen, MaTinh) VALUES (@MaHuyen, @TenHuyen, @MaTinh)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaHuyen", maHuyen);
                    cmd.Parameters.AddWithValue("@TenHuyen", tenHuyen);
                    cmd.Parameters.AddWithValue("@MaTinh", maTinh);
                    await cmd.ExecuteNonQueryAsync();

                    // Thêm dữ liệu xã liên quan
                    await InsertDataToTableXaAsync(district["wards"] as JArray, maHuyen, conn);
                }
            }
        }

        // Thêm dữ liệu vào bảng Xa
        private async Task InsertDataToTableXaAsync(JArray wardsData, string maHuyen, SqlConnection conn)
        {
            foreach (var ward in wardsData)
            {
                string maXa = ward["code"].ToString();
                string tenXa = ward["name"].ToString();

                string query = "INSERT INTO Xa (MaXa, TenXa, MaHuyen) VALUES (@MaXa, @TenXa, @MaHuyen)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaXa", maXa);
                    cmd.Parameters.AddWithValue("@TenXa", tenXa);
                    cmd.Parameters.AddWithValue("@MaHuyen", maHuyen);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task InitializeDataAsync()
        {
            // Kiểm tra và thêm dữ liệu cho bảng Tinh, Huyen, Xa
            if (!await CheckIfTableHasDataAsync("Tinh"))
            {
                var provincesData = await GetProvincesDataFromApiAsync();
                await InsertDataToTableTinhAsync(provincesData);
                MessageBox.Show("Dữ liệu đã được thêm vào bảng Tinh, Huyen, và Xa.");
            }
          
        }
    }
}
