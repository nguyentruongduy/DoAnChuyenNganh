using Guna.UI2.WinForms;
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
    public partial class KhachHang : Form
    {
        private DBConnect db;
        public KhachHang()
        {
            InitializeComponent();
            db = new DBConnect();
            LoadProvinces();
            cmbTinh.SelectedIndexChanged += cmbTinh_SelectedIndexChanged; // Gắn sự kiện chọn tỉnh
            cmbHuyen.SelectedIndexChanged += cmbHuyen_SelectedIndexChanged; // Gắn sự kiện chọn huyện
            dgvKhachHang.CellClick += new DataGridViewCellEventHandler(dgvKhachHang_CellClick);

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


        // Sự kiện khi người dùng chọn tỉnh
        private void cmbTinh_SelectedIndexChanged(object sender, EventArgs e)
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

        // Sự kiện khi người dùng chọn quận/huyện
        private void cmbHuyen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbHuyen.SelectedValue != null && int.TryParse(cmbHuyen.SelectedValue.ToString(), out int selectedDistrictCode))
            {
                LoadWards(selectedDistrictCode);
            }
        }

        // Hàm để lấy danh sách xã/phường từ API dựa trên mã quận/huyện
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


        private void guna2Button1_Click(object sender, EventArgs e)
        {
            TaoTaiKhoan taoTaiKhoan = new TaoTaiKhoan(this);
            taoTaiKhoan.Show();

        }



        private void LoadKhachHang(string filter)
        {
            try
            {
                string query = @"
                    SELECT 
                        kh.MaKhachHang, 
                        kh.TenKhachHang, 
                        kh.SoDienThoai, 
                        kh.Email, 
                        kh.CCCD, 
                        kh.NgaySinh, 
                        kh.DiaChiCuThe, 
                        t.TenTinh AS Tinh, 
                        h.TenHuyen AS Huyen, 
                        x.TenXa AS Xa
                    FROM KhachHang kh
                    LEFT JOIN Tinh t ON kh.MaTinh = t.MaTinh
                    LEFT JOIN Huyen h ON kh.MaHuyen = h.MaHuyen
                    LEFT JOIN Xa x ON kh.MaXa = x.MaXa
                    LEFT JOIN HeThongDien hd ON kh.MaKhachHang = hd.MaKhachHang";

                // Thêm điều kiện lọc nếu chọn "Hôm nay"
                if (filter == "Hôm nay")
                {
                    query += " WHERE CONVERT(DATE, hd.NgayLap) = CONVERT(DATE, GETDATE())";
                }

                if (filter == "Tháng này")
                {
                    query += " WHERE MONTH(hd.NgayLap) = MONTH(GETDATE()) AND YEAR(hd.NgayLap) = YEAR(GETDATE())";
                }

                if (filter == "Năm nay")
                {
                    query += " WHERE YEAR(hd.NgayLap) = YEAR(GETDATE())";
                }


                DataTable dt = db.getDataTable(query);
                dgvKhachHang.DataSource = dt;
                dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Đặt tiêu đề cho các cột
                dgvKhachHang.Columns["MaKhachHang"].HeaderText = "Mã Khách Hàng";
                dgvKhachHang.Columns["TenKhachHang"].HeaderText = "Tên Khách Hàng";
                dgvKhachHang.Columns["SoDienThoai"].HeaderText = "Số Điện Thoại";
                dgvKhachHang.Columns["Email"].HeaderText = "Email";
                dgvKhachHang.Columns["CCCD"].HeaderText = "CCCD";
                dgvKhachHang.Columns["NgaySinh"].HeaderText = "Ngày Sinh";
                dgvKhachHang.Columns["DiaChiCuThe"].HeaderText = "Địa Chỉ Cụ Thể";
                dgvKhachHang.Columns["Tinh"].HeaderText = "Tỉnh";
                dgvKhachHang.Columns["Huyen"].HeaderText = "Huyện";
                dgvKhachHang.Columns["Xa"].HeaderText = "Xã";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu khách hàng: " + ex.Message);
            }
        }





        private void BtnThemKhachHang_Click(object sender, EventArgs e)
        {

            string tenKhachHang = txtTenKhachHang.Text;
            string soDienThoai = txtSoDienThoai.Text;
            string email = txtEmail.Text;
            string maTaiKhoan = txtMaTaiKhoan.Text;

            int tinh = (int)cmbTinh.SelectedValue;
            int huyen = (int)cmbHuyen.SelectedValue;
            int xa = (int)cmbXa.SelectedValue;
            string cccd = txtCCCD.Text;
            DateTime ngaySinh = DTPNgaySinh.Value;
            string diaChiCuThe = txtDiaChiCuThe.Text;
            if (string.IsNullOrEmpty(soDienThoai))
            {
                MessageBox.Show("Số điện thoại không được để trống.");
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Email không được để trống.");
                return;
            }
            if (string.IsNullOrEmpty(cccd))
            {
                MessageBox.Show("CCCD không được để trống.");
                return;
            }
            if (!IsValidCCCD(cccd))
            {
                MessageBox.Show("CCCD Phải có 12 số.");
                return;
            }

            // Kiểm tra định dạng email
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Email không hợp lệ.");
                return;
            }

            // Kiểm tra định dạng số điện thoại (Ví dụ: 10 chữ số)
            if (!IsValidPhoneNumber(soDienThoai))
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Số điện thoại phải có 10 chữ số.");
                return;
            }
            DateTime ngaySinh1 = DTPNgaySinh.Value;
            DateTime ngayHienTai = DateTime.Today;

            // Tính số năm cách nhau giữa ngày sinh và ngày hiện tại
            if (ngaySinh1 >= ngayHienTai)
            {
                MessageBox.Show("Ngày sinh phải nhỏ hơn ngày hiện tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tính số năm cách nhau giữa ngày sinh và ngày hiện tại
            int tuoi = ngayHienTai.Year - ngaySinh.Year;

            // Điều chỉnh nếu ngày sinh chưa đến trong năm hiện tại
            if (ngaySinh1 > ngayHienTai.AddYears(-tuoi))
            {
                tuoi--;
            }

            // Kiểm tra tuổi phải đủ 18
            if (tuoi < 18)
            {
                MessageBox.Show("Khách hàng phải trên 18 tuổi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string checkCCCDQuery = "SELECT COUNT(*) FROM KhachHang WHERE CCCD = @CCCD";
            SqlParameter checkCCCDParam = new SqlParameter("@CCCD", cccd);
            int cccdCount = (int)db.getScalar(checkCCCDQuery, new SqlParameter[] { checkCCCDParam });

            if (cccdCount > 0)
            {
                MessageBox.Show("CCCD đã tồn tại. Vui lòng nhập CCCD khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }



            // Kết nối tới cơ sở dữ liệu và thực hiện lệnh SQL
            using (SqlConnection conn = db.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"
                        DECLARE @NewMaKhachHang NVARCHAR(50);
        
                        INSERT INTO KhachHang (MaTaiKhoan, TenKhachHang, SoDienThoai, Email, NgaySinh, CCCD, MaTinh, MaHuyen, MaXa, DiaChiCuThe)
                        VALUES (@MaTaiKhoan, @TenKhachHang, @SoDienThoai, @Email, @NgaySinh, @CCCD, @MaTinh, @MaHuyen, @MaXa, @DiaChiCuThe);
        
                        SET @NewMaKhachHang = (SELECT TOP 1 MaKhachHang FROM KhachHang ORDER BY MaKhachHang DESC);
        
                        SELECT @NewMaKhachHang;"; // Trả về mã khách hàng vừa thêm

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                        cmd.Parameters.AddWithValue("@TenKhachHang", tenKhachHang);
                        cmd.Parameters.AddWithValue("@SoDienThoai", soDienThoai);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@NgaySinh", ngaySinh);
                        cmd.Parameters.AddWithValue("@CCCD", cccd);
                        cmd.Parameters.AddWithValue("@MaTinh", tinh);
                        cmd.Parameters.AddWithValue("@MaHuyen", huyen);
                        cmd.Parameters.AddWithValue("@MaXa", xa);
                        cmd.Parameters.AddWithValue("@DiaChiCuThe", diaChiCuThe);

                        // Lấy mã khách hàng mới được thêm
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            string newMaKhachHang = result.ToString();


                            try
                            {

                                DateTime ngayLap = DateTime.Now;

                                // Truy vấn để lấy thông tin địa chỉ đầy đủ (DiaChiCuThe + Xa + Huyen + Tinh)
                                string query2 = @"
                                    SELECT k.DiaChiCuThe, x.TenXa, h.TenHuyen, t.TenTinh
                                    FROM KhachHang k
                                    JOIN Xa x ON k.MaXa = x.MaXa
                                    JOIN Huyen h ON k.MaHuyen = h.MaHuyen
                                    JOIN Tinh t ON k.MaTinh = t.MaTinh
                                    WHERE k.MaKhachHang = @MaKhachHang";

                                // Tạo tham số cho câu lệnh SQL
                                SqlParameter[] param = new SqlParameter[]
                                {
                                     new SqlParameter("@MaKhachHang", newMaKhachHang)
                                };

                                // Lấy thông tin địa chỉ từ cơ sở dữ liệu
                                DataTable dt = db.getDataTable(query2, param); // Gọi phương thức getDataTable từ DBConnect
                                if (dt.Rows.Count > 0)
                                {
                                    // Lấy dữ liệu địa chỉ từ kết quả truy vấn
                                    string diaChiCuThe1 = dt.Rows[0]["DiaChiCuThe"].ToString();
                                    string tenXa = dt.Rows[0]["TenXa"].ToString();
                                    string tenHuyen = dt.Rows[0]["TenHuyen"].ToString();
                                    string tenTinh = dt.Rows[0]["TenTinh"].ToString();

                                    // Tạo vị trí lắp đặt đầy đủ
                                    string viTriLapDat = $"{diaChiCuThe1}, {tenXa}, {tenHuyen}, {tenTinh}";

                                    // Câu truy vấn để thêm mới vào bảng HeThongDien
                                    string insertQuery = @"
                                        INSERT INTO HeThongDien (MaKhachHang, NgayLap, ViTriLapDat,TrangThai) 
                                        VALUES (@MaKhachHang, @NgayLap, @ViTriLapDat,N'Chờ Duyệt')";

                                    // Tạo tham số cho câu lệnh INSERT
                                    SqlParameter[] insertParams = new SqlParameter[]
                                    {
                                        new SqlParameter("@MaKhachHang", newMaKhachHang),
                                        new SqlParameter("@NgayLap", ngayLap),
                                        new SqlParameter("@ViTriLapDat", viTriLapDat)
                                    };

                                    // Thực hiện câu lệnh thêm mới vào cơ sở dữ liệu
                                    int result1 = db.getNonQuery(insertQuery, insertParams);

                                    if (result1 > 0)
                                    {
                                        MessageBox.Show("Thêm mới khách hàng thành công!");
                                        LoadKhachHang("Hôm nay");

                                    }
                                    else
                                    {
                                        MessageBox.Show("Thêm mới thất bại.");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Không tìm thấy địa chỉ khách hàng.");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Lỗi: " + ex.Message);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Không thể lấy mã khách hàng mới.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
                }
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

        private bool IsValidCCCD(string cccd)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(cccd, @"^\d{12}$");
        }


        private void LoadForm()
        {
            txtCCCD.Clear();
            txtDiaChiCuThe.Clear();
            txtEmail.Clear();
            txtSoDienThoai.Clear();
            txtTenKhachHang.Clear();


        }

        private void KhachHang_Load(object sender, EventArgs e)
        {
            cmbFilter.Items.Add("Hôm nay");
            cmbFilter.Items.Add("Tất cả");
            cmbFilter.Items.Add("Tháng này");
            cmbFilter.Items.Add("Năm nay");
            cmbFilter.SelectedIndex = 0; 

            LoadKhachHang("Hôm nay");
        }

        private void btnTaiLai_Click(object sender, EventArgs e)
        {
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            string searchKeyword = txtTimKiem.Text;

            try
            {
                // Tạo câu truy vấn SQL để tìm kiếm theo CCCD và lấy thông tin từ các bảng liên quan
                string query = @"
                        SELECT 
                            kh.MaKhachHang, 
                            kh.TenKhachHang, 
                            kh.SoDienThoai, 
                            kh.Email, 
                            kh.CCCD, 
                            kh.NgaySinh, 
                            kh.DiaChiCuThe, 
                            t.TenTinh AS Tinh, 
                            h.TenHuyen AS Huyen, 
                            x.TenXa AS Xa
                        FROM KhachHang kh
                        LEFT JOIN Tinh t ON kh.MaTinh = t.MaTinh
                        LEFT JOIN Huyen h ON kh.MaHuyen = h.MaHuyen
                        LEFT JOIN Xa x ON kh.MaXa = x.MaXa
                        WHERE kh.CCCD LIKE @search";

                // Tạo mảng tham số cho truy vấn, sử dụng ký tự % để tìm kiếm tương đối
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@search", "%" + searchKeyword + "%")
                };

                // Lấy dữ liệu từ DB bằng phương thức getDataTable của lớp DBConnect
                DataTable dt = db.getDataTable(query, parameters);

                // Gán dữ liệu vào DataGridView để hiển thị
                dgvKhachHang.DataSource = dt;

                // Tùy chọn: tự động điều chỉnh độ rộng của các cột theo dữ liệu
                dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Đặt tiêu đề cho các cột
                dgvKhachHang.Columns["MaKhachHang"].HeaderText = "Mã Khách Hàng";
                dgvKhachHang.Columns["TenKhachHang"].HeaderText = "Tên Khách Hàng";
                dgvKhachHang.Columns["SoDienThoai"].HeaderText = "Số Điện Thoại";
                dgvKhachHang.Columns["Email"].HeaderText = "Email";
                dgvKhachHang.Columns["CCCD"].HeaderText = "CCCD";
                dgvKhachHang.Columns["NgaySinh"].HeaderText = "Ngày Sinh";
                dgvKhachHang.Columns["DiaChiCuThe"].HeaderText = "Địa Chỉ Cụ Thể";
                dgvKhachHang.Columns["Tinh"].HeaderText = "Tỉnh";
                dgvKhachHang.Columns["Huyen"].HeaderText = "Huyện";
                dgvKhachHang.Columns["Xa"].HeaderText = "Xã";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tìm kiếm dữ liệu khách hàng: " + ex.Message);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy thông tin từ các điều khiển
                string tenKhachHang = txtTenKhachHang.Text;
                string soDienThoai = txtSoDienThoai.Text;
                string email = txtEmail.Text;
                string cccd = txtCCCD.Text;
                DateTime ngaySinh = DTPNgaySinh.Value;
                DateTime ngayHienTai = DateTime.Today;
                string diaChiCuThe = txtDiaChiCuThe.Text;
                int maTinh = Convert.ToInt32(cmbTinh.SelectedValue);
                int maHuyen = Convert.ToInt32(cmbHuyen.SelectedValue);
                int maXa = Convert.ToInt32(cmbXa.SelectedValue);

                // Kiểm tra ngày sinh hợp lệ
                if (ngaySinh >= ngayHienTai)
                {
                    MessageBox.Show("Ngày sinh phải nhỏ hơn ngày hiện tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int tuoi = ngayHienTai.Year - ngaySinh.Year;
                if (ngaySinh > ngayHienTai.AddYears(-tuoi)) tuoi--;

                if (tuoi < 18)
                {
                    MessageBox.Show("Khách hàng phải trên 18 tuổi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra CCCD đã tồn tại
                string checkCCCDQuery = "SELECT COUNT(*) FROM KhachHang WHERE CCCD = @CCCD AND MaKhachHang <> @MaKhachHang";
                SqlParameter[] checkCCCDParams = new SqlParameter[]
                {
                    new SqlParameter("@CCCD", cccd),
                    new SqlParameter("@MaKhachHang", maKhachHangSelected)
                };
                int cccdCount = (int)db.getScalar(checkCCCDQuery, checkCCCDParams);

                if (cccdCount > 0)
                {
                    MessageBox.Show("CCCD đã tồn tại. Vui lòng nhập CCCD khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra các điều kiện khác
                if (string.IsNullOrEmpty(soDienThoai) || !IsValidPhoneNumber(soDienThoai))
                {
                    MessageBox.Show("Số điện thoại không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
                {
                    MessageBox.Show("Email không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(cccd) || !IsValidCCCD(cccd))
                {
                    MessageBox.Show("CCCD phải có 12 số.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Cập nhật dữ liệu khách hàng
                string query = @"
                    UPDATE KhachHang
                    SET 
                        TenKhachHang = @TenKhachHang,
                        SoDienThoai = @SoDienThoai,
                        Email = @Email,
                        CCCD = @CCCD,
                        NgaySinh = @NgaySinh,
                        DiaChiCuThe = @DiaChiCuThe,
                        MaTinh = @MaTinh,
                        MaHuyen = @MaHuyen,
                        MaXa = @MaXa
                    WHERE MaKhachHang = @MaKhachHang";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaKhachHang", maKhachHangSelected),
                    new SqlParameter("@TenKhachHang", tenKhachHang),
                    new SqlParameter("@SoDienThoai", soDienThoai),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@CCCD", cccd),
                    new SqlParameter("@NgaySinh", ngaySinh),
                    new SqlParameter("@DiaChiCuThe", diaChiCuThe),
                    new SqlParameter("@MaTinh", maTinh),
                    new SqlParameter("@MaHuyen", maHuyen),
                    new SqlParameter("@MaXa", maXa)
                };

                int rowsAffected = db.getNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    // Lấy thông tin địa chỉ đầy đủ để cập nhật vị trí lắp đặt trong bảng HeThongDien
                    string query2 = @"
                        SELECT k.DiaChiCuThe, x.TenXa, h.TenHuyen, t.TenTinh
                        FROM KhachHang k
                        JOIN Xa x ON k.MaXa = x.MaXa
                        JOIN Huyen h ON k.MaHuyen = h.MaHuyen
                        JOIN Tinh t ON k.MaTinh = t.MaTinh
                        WHERE k.MaKhachHang = @MaKhachHang";

                    SqlParameter[] param = new SqlParameter[]
                    {
                        new SqlParameter("@MaKhachHang", maKhachHangSelected)
                    };

                    DataTable dt = db.getDataTable(query2, param);
                    if (dt.Rows.Count > 0)
                    {
                        string diaChiCuThe1 = dt.Rows[0]["DiaChiCuThe"].ToString();
                        string tenXa = dt.Rows[0]["TenXa"].ToString();
                        string tenHuyen = dt.Rows[0]["TenHuyen"].ToString();
                        string tenTinh = dt.Rows[0]["TenTinh"].ToString();
                        string viTriLapDat = $"{diaChiCuThe1}, {tenXa}, {tenHuyen}, {tenTinh}";

                        // Cập nhật vị trí lắp đặt trong bảng HeThongDien
                        string updateQuery = @"
                        UPDATE HeThongDien
                        SET ViTriLapDat = @ViTriLapDat, TrangThai = N'Chờ Duyệt'
                        WHERE MaKhachHang = @MaKhachHang";

                        SqlParameter[] updateParams = new SqlParameter[]
                        {
                            new SqlParameter("@MaKhachHang", maKhachHangSelected),
                            new SqlParameter("@ViTriLapDat", viTriLapDat)
                        };

                        int result1 = db.getNonQuery(updateQuery, updateParams);

                        if (result1 > 0)
                        {
                            MessageBox.Show("Cập nhật thông tin khách hàng và hệ thống điện thành công!");
                            LoadKhachHang("Hôm nay"); // Tải lại danh sách khách hàng
                            LoadForm(); // Reset form
                        }
                        else
                        {
                            MessageBox.Show("Cập nhật hệ thống điện thất bại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy địa chỉ khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại, không có dòng nào bị ảnh hưởng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật thông tin khách hàng: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private string maKhachHangSelected;
        private void dgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Kiểm tra xem người dùng có click vào một dòng hợp lệ hay không
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvKhachHang.Rows[e.RowIndex];
                    if (row.Cells["MaKhachHang"].Value != null)
                    {
                        maKhachHangSelected = row.Cells["MaKhachHang"].Value.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Mã khách hàng không hợp lệ.");
                        return;
                    }
                    // Lấy dòng hiện tại


                    // Lấy thông tin từ dòng được chọn và gán vào các điều khiển khác
                    txtTenKhachHang.Text = row.Cells["TenKhachHang"].Value.ToString();
                    txtSoDienThoai.Text = row.Cells["SoDienThoai"].Value.ToString();
                    txtEmail.Text = row.Cells["Email"].Value.ToString();
                    txtCCCD.Text = row.Cells["CCCD"].Value.ToString();
                    DTPNgaySinh.Value = Convert.ToDateTime(row.Cells["NgaySinh"].Value);
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

        private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadKhachHang(cmbFilter.SelectedItem.ToString());
        }

        private void cmbTinh_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
    }
}

