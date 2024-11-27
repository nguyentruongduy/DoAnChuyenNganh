using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoQuanLyDienLuc
{
    public partial class NhanVien : Form
    {
        private DBConnect db;
        public NhanVien()
        {
            InitializeComponent();
            db = new DBConnect();
            LoadProvinces();
            LoadNhanVien();
            cmbTinh.SelectedIndexChanged += cmbTinh_SelectedIndexChanged; // Gắn sự kiện chọn tỉnh
            cmbHuyen.SelectedIndexChanged += cmbHuyen_SelectedIndexChanged; // Gắn sự kiện chọn huyện
            dgvNhanVien.CellClick += new DataGridViewCellEventHandler(dgvNhanVien_CellClick);
            LoadTinh();
            cmbPCTinh.SelectedIndexChanged += cmbPCTinh_SelectedIndexChanged;
            cmbPCHuyen.SelectedIndexChanged += cmbPCHuyen_SelectedIndexChanged;
        }

        public class Ward
        {
            public int code { get; set; }
            public string name { get; set; }

            public int district_code { get; set; }
        }

        // Lớp đại diện cho quận/huyện
        public class District
        {
            public int code { get; set; }
            public string name { get; set; }
            public int province_code { get; set; }
            public List<Ward> wards { get; set; } // Thêm danh sách xã/phường
        }

        // Lớp đại diện cho tỉnh thành
        public class Province
        {
            public int code { get; set; }
            public string name { get; set; }
            public List<District> districts { get; set; }
        }
        public void SetMaTaiKhoan(string maTaiKhoan)
        {
            txtMaTaiKhoan.Text = maTaiKhoan;
        }

        private void LoadProvinces()
        {
            DBConnect db = new DBConnect();
            string query = "SELECT MaTinh, TenTinh FROM [QLDienMienNam].[dbo].[Tinh] WHERE TenTinh = N'Thành phố Hồ Chí Minh'";
            DataTable dtProvinces = db.getDataTable(query);

            if (dtProvinces.Rows.Count > 0)
            {
                cmbTinh.DataSource = dtProvinces;
                cmbTinh.DisplayMember = "TenTinh"; // Hiển thị tên tỉnh
                cmbTinh.ValueMember = "MaTinh";   // Mã tỉnh
                cmbTinh.SelectedIndex = 0;
            }
        }
        private void LoadTinh()
        {
            DBConnect db = new DBConnect();
            string query = "SELECT MaTinh, TenTinh FROM [QLDienMienNam].[dbo].[Tinh] WHERE TenTinh = N'Thành phố Hồ Chí Minh'";
            DataTable dtProvinces = db.getDataTable(query);

            if (dtProvinces.Rows.Count > 0)
            {
                cmbPCTinh.DataSource = dtProvinces;
                cmbPCTinh.DisplayMember = "TenTinh"; // Hiển thị tên tỉnh
                cmbPCTinh.ValueMember = "MaTinh";   // Mã tỉnh
                cmbPCTinh.SelectedIndex = 0;
            }
        }



        private void btnTaoTaiKhoan_Click(object sender, EventArgs e)
        {
            TaoTaiKhoan taoTaiKhoan = new TaoTaiKhoan(this);
            taoTaiKhoan.Show();
        }

        private  void cmbTinh_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTinh.SelectedValue != null && int.TryParse(cmbTinh.SelectedValue.ToString(), out int selectedProvinceCode))
            {
                LoadDistricts(selectedProvinceCode);
            }
        }

        private void LoadDistricts(int provinceCode)
        {
            DBConnect db = new DBConnect();
            string query = "SELECT MaHuyen, TenHuyen FROM [QLDienMienNam].[dbo].[Huyen] WHERE MaTinh = @MaTinh";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaTinh", provinceCode)
            };
            DataTable dtDistricts = db.getDataTable(query, parameters);

            if (dtDistricts.Rows.Count > 0)
            {
                cmbHuyen.DataSource = dtDistricts;
                cmbHuyen.DisplayMember = "TenHuyen"; // Hiển thị tên huyện
                cmbHuyen.ValueMember = "MaHuyen";   // Mã huyện
            }
        }

        private void LoadHuyen(int provinceCode)
        {
            DBConnect db = new DBConnect();
            string query = "SELECT MaHuyen, TenHuyen FROM [QLDienMienNam].[dbo].[Huyen] WHERE MaTinh = @MaTinh";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaTinh", provinceCode)
            };
            DataTable dtDistricts = db.getDataTable(query, parameters);

            if (dtDistricts.Rows.Count > 0)
            {
                cmbPCHuyen.DataSource = dtDistricts;
                cmbPCHuyen.DisplayMember = "TenHuyen"; // Hiển thị tên huyện
                cmbPCHuyen.ValueMember = "MaHuyen";   // Mã huyện
            }
        }

        private  void cmbHuyen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbHuyen.SelectedValue != null && int.TryParse(cmbHuyen.SelectedValue.ToString(), out int selectedDistrictCode))
            {
                LoadWards(selectedDistrictCode);
            }
        }

        private void LoadWards(int districtCode)
        {
            DBConnect db = new DBConnect();
            string query = "SELECT MaXa, TenXa FROM [QLDienMienNam].[dbo].[Xa] WHERE MaHuyen = @MaHuyen";
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@MaHuyen", districtCode)
            };
            DataTable dtWards = db.getDataTable(query, parameters);

            if (dtWards.Rows.Count > 0)
            {
                cmbXa.DataSource = dtWards;
                cmbXa.DisplayMember = "TenXa"; // Hiển thị tên xã
                cmbXa.ValueMember = "MaXa";   // Mã xã
            }
        }

        private void LoadXa(int districtCode)
        {
            DBConnect db = new DBConnect();
            string query = "SELECT MaXa, TenXa FROM [QLDienMienNam].[dbo].[Xa] WHERE MaHuyen = @MaHuyen";
            SqlParameter[] parameters = new SqlParameter[]
            {
                 new SqlParameter("@MaHuyen", districtCode)
            };
            DataTable dtWards = db.getDataTable(query, parameters);

            if (dtWards.Rows.Count > 0)
            {
                cmbPCXa.DataSource = dtWards;
                cmbPCXa.DisplayMember = "TenXa"; // Hiển thị tên xã
                cmbPCXa.ValueMember = "MaXa";   // Mã xã
            }
        }



        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Hàm kiểm tra định dạng số điện thoại (10 chữ số)
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^0\d{9}$");
        }

        

        private void LoadForm()
        {
            txtNhanVien.Clear();
            txtDiaChiCuThe.Clear();
            txtEmail.Clear();
            txtSoDienThoai.Clear();



        }
        private string maNhanVien;

        private void LoadNhanVien()
        {
            try
            {
                // Câu truy vấn để lấy thông tin khách hàng cùng với tên Tỉnh, Huyện, và Xã
                string query = @"
                    SELECT 
                        nv.MaNhanVien, 
                        nv.TenNhanVien, 
                        nv.SoDienThoai, 
                        nv.Email, 
                        nv.DiaChiCuThe, 
                        t.TenTinh AS Tinh, 
                        h.TenHuyen AS Huyen, 
                        x.TenXa AS Xa,
                        tk.ChucVu as TaiKhoan
                    FROM NhanVien nv
                    LEFT JOIN Tinh t ON nv.MaTinh = t.MaTinh
                    LEFT JOIN Huyen h ON nv.MaHuyen = h.MaHuyen
                    LEFT JOIN Xa x ON nv.MaXa = x.MaXa
                    LEFT JOIN TaiKhoan tk ON nv.MaTaiKhoan = tk.MaTaiKhoan";
            

                // Lấy dữ liệu từ cơ sở dữ liệu
                DataTable dt = db.getDataTable(query);

                // Gán dữ liệu vào DataGridView
                dgvNhanVien.DataSource = dt;

                // Tùy chọn: tự động điều chỉnh độ rộng của các cột theo dữ liệu
                dgvNhanVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Đặt tiêu đề cho các cột
                dgvNhanVien.Columns["MaNhanVien"].HeaderText = "Mã Nhân viên";
                dgvNhanVien.Columns["TenNhanVien"].HeaderText = "Tên Nhân Viên";
                dgvNhanVien.Columns["SoDienThoai"].HeaderText = "Số Điện Thoại";
                dgvNhanVien.Columns["Email"].HeaderText = "Email";
                dgvNhanVien.Columns["DiaChiCuThe"].HeaderText = "Địa Chỉ Cụ Thể";
                dgvNhanVien.Columns["Tinh"].HeaderText = "Tỉnh";
                dgvNhanVien.Columns["Huyen"].HeaderText = "Huyện";
                dgvNhanVien.Columns["Xa"].HeaderText = "Xã";
                dgvNhanVien.Columns["TaiKhoan"].HeaderText = "Chức vụ";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu khách hàng: " + ex.Message);
            }
        }

        private void dgvNhanVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Kiểm tra xem người dùng có click vào một dòng hợp lệ hay không
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvNhanVien.Rows[e.RowIndex];
                    if (row.Cells["MaNhanVien"].Value != null)
                    {
                        maNhanVien = row.Cells["MaNhanVien"].Value.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Mã nhân viên không hợp lệ.");
                        return;
                    }
                    // Lấy dòng hiện tại


                    // Lấy thông tin từ dòng được chọn và gán vào các điều khiển khác
                    txtNhanVien.Text = row.Cells["TenNhanVien"].Value.ToString();
                    txtSoDienThoai.Text = row.Cells["SoDienThoai"].Value.ToString();
                    txtEmail.Text = row.Cells["Email"].Value.ToString();

                    txtDiaChiCuThe.Text = row.Cells["DiaChiCuThe"].Value.ToString();

                    // Gán Tỉnh, Huyện, Xã vào ComboBox
                    cmbTinh.Text = row.Cells["Tinh"].Value.ToString();
                    cmbHuyen.Text = row.Cells["Huyen"].Value.ToString();
                    cmbXa.Text = row.Cells["Xa"].Value.ToString();
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi chọn dòng: " + ex.Message);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy thông tin từ các điều khiển
                string tennhanvien = txtNhanVien.Text;
                string soDienThoai = txtSoDienThoai.Text;
                string email = txtEmail.Text;
               
                string diaChiCuThe = txtDiaChiCuThe.Text;
                int maTinh = Convert.ToInt32(cmbTinh.SelectedValue);
                int maHuyen = Convert.ToInt32(cmbHuyen.SelectedValue);
                int maXa = Convert.ToInt32(cmbXa.SelectedValue);

                // Câu truy vấn SQL để cập nhật dữ liệu khách hàng
                string query = @"
                        UPDATE NhanVien
                        SET 
                            TenNhanVien = @TenNhanVien,
                            SoDienThoai = @SoDienThoai,
                            Email = @Email,
                            DiaChiCuThe = @DiaChiCuThe,
                            MaTinh = @MaTinh,
                            MaHuyen = @MaHuyen,
                            MaXa = @MaXa
                        WHERE MaNhanVien = @MaNhanVien";  // Sử dụng biến toàn cục trong WHERE

                // Tạo mảng tham số cho truy vấn
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaNhanVien", maNhanVien),  // Sử dụng biến toàn cục
                    new SqlParameter("@TenNhanVien", tennhanvien),
                    new SqlParameter("@SoDienThoai", soDienThoai),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@DiaChiCuThe", diaChiCuThe),
                    new SqlParameter("@MaTinh", maTinh),
                    new SqlParameter("@MaHuyen", maHuyen),
                    new SqlParameter("@MaXa", maXa)
                };

                // Thực thi câu truy vấn
                int rowsAffected = db.getNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Cập nhật thông tin nhân viên  thành công!");

                    // Tải lại danh sách khách hàng vào DataGridView
                    LoadNhanVien();
                    LoadForm();
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại, không có dòng nào bị ảnh hưởng.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật thông tin khách hàng: " + ex.Message);
            }
        }

        private void NhanVien_Load(object sender, EventArgs e)
        {

        }

       

        private void dgvNhanVien_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnThemNhanVien_Click(object sender, EventArgs e)
        {
            string tennhanVien = txtNhanVien.Text;
            string soDienThoai = txtSoDienThoai.Text;
            string email = txtEmail.Text;
            string maTaiKhoan = txtMaTaiKhoan.Text;

            int tinh = (int)cmbTinh.SelectedValue;
            int huyen = (int)cmbHuyen.SelectedValue;
            int xa = (int)cmbXa.SelectedValue;
            string diaChiCuThe = txtDiaChiCuThe.Text;
            int PCTinh = (int)cmbPCTinh.SelectedValue;
            int PCHuyen = (int)cmbPCHuyen.SelectedValue;
            int PCXa = (int)cmbPCXa.SelectedValue;

            if (string.IsNullOrEmpty(soDienThoai) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Số điện thoại và email không được để trống.");
                return;
            }

            if (!IsValidEmail(email) || !IsValidPhoneNumber(soDienThoai))
            {
                MessageBox.Show("Email hoặc số điện thoại không hợp lệ.");
                return;
            }

            using (SqlConnection conn = db.GetConnection())
            {
                try
                {
                    conn.Open();

                    // Bước 1: Thêm nhân viên vào bảng NhanVien
                    string queryInsertNhanVien = @"
                    INSERT INTO NhanVien (TenNhanVien, SoDienThoai, Email, MaTaiKhoan, DiaChiCuThe, MaTinh, MaHuyen, MaXa)
                    VALUES (@TenNhanVien, @SoDienThoai, @Email, @MaTaiKhoan, @DiaChiCuThe, @Tinh, @Huyen, @Xa)";

                    using (SqlCommand cmd = new SqlCommand(queryInsertNhanVien, conn))
                    {
                        cmd.Parameters.AddWithValue("@TenNhanVien", tennhanVien);
                        cmd.Parameters.AddWithValue("@SoDienThoai", soDienThoai);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                        cmd.Parameters.AddWithValue("@DiaChiCuThe", diaChiCuThe);
                        cmd.Parameters.AddWithValue("@Tinh", tinh);
                        cmd.Parameters.AddWithValue("@Huyen", huyen);
                        cmd.Parameters.AddWithValue("@Xa", xa);

                        cmd.ExecuteNonQuery(); // Thực hiện lệnh insert
                    }

                    // Bước 2: Lấy MaNhanVien của nhân viên vừa thêm
                    string queryGetMaNhanVien = @"
                        SELECT MaNhanVien
                        FROM NhanVien
                        WHERE MaTaiKhoan = @MaTaiKhoan";

                    string maNhanVien = "";
                    using (SqlCommand getCmd = new SqlCommand(queryGetMaNhanVien, conn))
                    {
                        getCmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);

                        // Thực thi câu lệnh và lấy MaNhanVien
                        object result = getCmd.ExecuteScalar();

                        if (result != DBNull.Value && result != null)
                        {
                            maNhanVien = result.ToString(); // Lấy giá trị MaNhanVien
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy mã nhân viên.");
                            return; // Dừng lại nếu không lấy được MaNhanVien
                        }
                    }

                    // Bước 3: Truy vấn lấy danh sách khách hàng theo khu vực
                    string queryGetKhachHang = @"
                        SELECT MaKhachHang
                        FROM KhachHang
                        WHERE MaTinh = @maTinh AND MaHuyen = @maHuyen AND MaXa = @maXa";

                    SqlParameter[] parameters = {
            new SqlParameter("@maTinh", PCTinh),
            new SqlParameter("@maHuyen", PCHuyen),
            new SqlParameter("@maXa", PCXa)
        };

                    DataTable khachHangTable = db.getDataTable(queryGetKhachHang, parameters);

                    if (khachHangTable.Rows.Count > 0)
                    {
                        // Phân công khách hàng cho nhân viên
                        foreach (DataRow row in khachHangTable.Rows)
                        {
                            string maKhachHang = row["MaKhachHang"].ToString();

                            string queryInsertQuanLyKhachHang = @"
                                INSERT INTO QuanLyKhachHang (MaKhachHang, MaNhanVien)
                                VALUES (@maKhachHang, @maNhanVien)";

                            using (SqlCommand insertCmd = new SqlCommand(queryInsertQuanLyKhachHang, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@maKhachHang", maKhachHang);
                                insertCmd.Parameters.AddWithValue("@maNhanVien", maNhanVien);

                                try
                                {
                                    insertCmd.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    // Xóa dữ liệu nếu lỗi và thử lại
                                    string queryDelete = "DELETE FROM QuanLyKhachHang WHERE MaNhanVien = @maNhanVien";
                                    using (SqlCommand deleteCmd = new SqlCommand(queryDelete, conn))
                                    {
                                        deleteCmd.Parameters.AddWithValue("@maNhanVien", maNhanVien);
                                        deleteCmd.ExecuteNonQuery();
                                    }
                                    // Thực hiện lại lệnh insert
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        MessageBox.Show("Phân công thành công!");
                    }
                    else
                    {
                        MessageBox.Show("Không có khách hàng trong khu vực được chọn.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
                }
            }


        }

        private void cmbPCTinh_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPCTinh.SelectedValue != null && int.TryParse(cmbPCTinh.SelectedValue.ToString(), out int selectedProvinceCode))
            {
                LoadHuyen(selectedProvinceCode);
            }
        
        }

        private void cmbPCHuyen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPCHuyen.SelectedValue != null && int.TryParse(cmbPCHuyen.SelectedValue.ToString(), out int selectedDistrictCode))
            {
                LoadXa(selectedDistrictCode);
            }
        }
    }
}

