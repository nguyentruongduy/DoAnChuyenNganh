using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;

namespace DemoQuanLyDienLuc
{

    public partial class PhanCong : Form
    {
        private DBConnect db;
        private string maNhanVien;
        public PhanCong(string maNhanVien)
        {
            InitializeComponent();
            db = new DBConnect();
            LoadHeThongDien(maNhanVien);
            LoadManhanVien();
            LoadMaDH();
            this.maNhanVien = maNhanVien;

        }

        private void LoadHeThongDien(string maNhanVien)
        {
            string query = @"
                SELECT TOP (1000) 
                    htd.MaHeThong, 
                    htd.MaKhachHang, 
                    htd.MaDongHo, 
                    htd.NgayLap, 
                    htd.NgayLapDat, 
                    htd.HienTrang, 
                    htd.ViTriLapDat, 
                    htd.TrangThai,
                    htd.BienBan
                FROM HeThongDien htd
                JOIN QuanLyKhachHang ql ON htd.MaKhachHang = ql.MaKhachHang
                WHERE htd.TrangThai = N'Chờ lắp đặt' AND ql.MaNhanVien = @maNhanVien";

            if (dgvHeThong.Rows.Count > 0)
            {
                dgvHeThong.CurrentCell = dgvHeThong.Rows[0].Cells[0];
                dgvHeThong.Rows[0].Selected = true;
            }
            // Ensure the connection is opened before executing the command
            using (SqlConnection connection = db.GetConnection())
            {
                connection.Open(); // Open the connection here


                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@maNhanVien", maNhanVien);


                    // Execute the command and fill the DataTable
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt); // Fill the DataTable with the results

                        // Bind the DataTable to the DataGridView
                        dgvHeThong.DataSource = dt;
                        dgvHeThong.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                        // Đặt tiêu đề cho các cột
                        dgvHeThong.Columns["MaHeThong"].HeaderText = "Mã Hệ Thống";
                        dgvHeThong.Columns["MaKhachHang"].HeaderText = "Mã Khách Hàng";
                        dgvHeThong.Columns["MaDongHo"].HeaderText = "Mã Đồng hồ";
                        dgvHeThong.Columns["NgayLap"].HeaderText = "Ngày Lập";
                        dgvHeThong.Columns["NgayLapDat"].HeaderText = "Ngày Lắp Đặt";
                        dgvHeThong.Columns["HienTrang"].HeaderText = "Hiện Trạng";
                        dgvHeThong.Columns["ViTriLapDat"].HeaderText = "Địa chỉ";
                        dgvHeThong.Columns["TrangThai"].HeaderText = "Trạng Thái";
                        dgvHeThong.Columns["BienBan"].HeaderText = "Biên Bản"; // Don't forget to set the header for BienBan if needed
                    }
                }
            }
        }

        private string maHTselected;
        private void dgvHeThong_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Kiểm tra xem người dùng có click vào một dòng hợp lệ hay không
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvHeThong.Rows[e.RowIndex];
                    if (row.Cells["MaHeThong"].Value != null)
                    {
                        maHTselected = row.Cells["MaHeThong"].Value.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Mã khách hàng không hợp lệ.");
                        return;
                    }
                    // Lấy dòng hiện tại


                    // Lấy thông tin từ dòng được chọn và gán vào các điều khiển khác
                    lblMaKhachHang.Text = row.Cells["MaKhachHang"].Value.ToString();
                    lblDiaChi.Text = row.Cells["ViTriLapDat"].Value.ToString();
                    lblTrangThai.Text = row.Cells["TrangThai"].Value.ToString();
                    lblNgayLap.Text = row.Cells["NgayLap"].Value != null ? Convert.ToDateTime(row.Cells["NgayLap"].Value).ToString("dd/MM/yyyy") : string.Empty;

                    //DTPNgayLapDat.Value = Convert.ToDateTime(row.Cells["NgayLapDat"].Value);

                    string maKhachHang = row.Cells["MaKhachHang"].Value != DBNull.Value ? row.Cells["MaKhachHang"].Value.ToString() : string.Empty;

                    lblMaKhachHang.Text = row.Cells["MaKhachHang"].Value.ToString();

                    string query = "SELECT TenKhachHang FROM KhachHang WHERE MaKhachHang = @MaKhachHang";
                    SqlParameter[] parameters = { new SqlParameter("@MaKhachHang", maKhachHang) };
                    string tenKhachHang = (string)db.getScalar(query, parameters);
                    lblTenKhachHang.Text = tenKhachHang ?? string.Empty; // Gán trắng nếu không có tên khách hàng

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi chọn dòng: " + ex.Message);
            }
        }

        private void PhanCong_Load(object sender, EventArgs e)
        {

        }
        private void LoadManhanVien()
        {
            try
            {
                // Câu lệnh SQL có điều kiện lọc theo chức vụ là "Nhân Viên Kỹ Thuật"
                string query = @"
                    SELECT NV.MaNhanVien 
                    FROM NhanVien NV
                    JOIN TaiKhoan TK ON NV.MaTaiKhoan = TK.MaTaiKhoan
                    WHERE TK.ChucVu = N'Nhân viên kỹ thuật'";

                DataTable dt = db.getDataTable(query); // Sử dụng DBConnect để lấy dữ liệu

                // Gán dữ liệu cho ComboBox
                cmbMaNhanVien.DataSource = dt;
                cmbMaNhanVien.DisplayMember = "MaNhanVien"; // Cột hiển thị
                cmbMaNhanVien.ValueMember = "MaNhanVien";   // Cột giá trị thực tế
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi load dữ liệu: " + ex.Message);
            }
        }

        private void LoadMaDH()
        {
            try
            {
                // Câu lệnh SQL có điều kiện lọc theo chức vụ là "Nhân Viên Kỹ Thuật"
                string query = @"
                    SELECT DH.MaDongHo 
                    FROM DongHoDien DH
                    WHERE NOT EXISTS (
                        SELECT 1 
                        FROM HeThongDien HTD 
                        WHERE HTD.MaDongHo = DH.MaDongHo
                    )";

                DataTable dt = db.getDataTable(query); // Sử dụng DBConnect để lấy dữ liệu

                // Gán dữ liệu cho ComboBox
                cmbMaDongHo.DataSource = dt;
                cmbMaDongHo.DisplayMember = "MaDongHo"; // Cột hiển thị
                cmbMaDongHo.ValueMember = "MaDongHo";   // Cột giá trị thực tế
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi load dữ liệu: " + ex.Message);
            }
        }

       


        private void btnPhanCong_Click(object sender, EventArgs e)
        {
            try
            {

                string maKhachHang = lblMaKhachHang.Text;
                DateTime ngayLapDat = DTPNgayLapDat.Value;
                string maDongHo = Convert.ToString(cmbMaDongHo.SelectedValue);
                string maNhanVien = Convert.ToString(cmbMaNhanVien.SelectedValue);



                string query = @"
                        UPDATE HeThongDien
                        SET 
                            NgayLapDat = @NgayLapDat,
                            MaDongHo = @MaDongHo,
                            MaNhanVien = @MaNhanVien
                        WHERE MaHeThong = @MaHeThong";  // Sử dụng biến toàn cục trong WHERE

                // Tạo mảng tham số cho truy vấn
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaHeThong", maHTselected),  // Sử dụng biến toàn cục
                    new SqlParameter("@NgayLapDat", ngayLapDat),
                    new SqlParameter("@MaDongHo", maDongHo),
                    new SqlParameter("@MaNhanVien", maNhanVien),

                };

                // Thực thi câu truy vấn
                int rowsAffected = db.getNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Cập nhật thông tin khách hàng thành công!");

                    // Tải lại danh sách khách hàng vào DataGridView
                    LoadHeThongDien(maNhanVien);
                    LoadMaDH();



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

        //private void brnXemBienBan_Click(object sender, EventArgs e)
        //{
        //    string maHeThong = maHTselected; // Thay đổi theo mã hệ thống bạn muốn mở
        //    SaveBienBanToFile(maHeThong);
        //}

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadHeThongDien(maNhanVien);
        }

        private void lblDiaChi_Click(object sender, EventArgs e)
        {

        }

        private void cmbMaNhanVien_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra nếu chưa chọn gì trong ComboBox
                if (cmbMaNhanVien.SelectedValue == null) return;

                // Lấy MaNhanVien được chọn từ ComboBox
                string maNhanVien = cmbMaNhanVien.SelectedValue.ToString();

                // Câu lệnh SQL để lấy tên nhân viên từ MaNhanVien
                string query = "SELECT TenNhanVien FROM NhanVien WHERE MaNhanVien = @MaNhanVien";

                // Khởi tạo tham số cho câu lệnh SQL
                SqlParameter[] parameters = new SqlParameter[]
                {
                 new SqlParameter("@MaNhanVien", SqlDbType.NVarChar) { Value = maNhanVien }
                };

                // Gọi phương thức getDataTable với tham số
                DataTable dt = db.getDataTable(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    // Gán tên nhân viên vào lblTenNhanVien
                    lblTenNhanVien.Text = dt.Rows[0]["TenNhanVien"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi lấy thông tin nhân viên: " + ex.Message);
            }
        }

        private void GuiEmailThongBao(string maKhachHang, string tenKhachHang, string email, DateTime ngayLapDat, string diaChi)
        {
            try
            {
                string noiDung = $@"Kính gửi {tenKhachHang},

                    Công ty Điện lực Miền Nam xin thông báo về lịch lắp đặt điện của quý khách như sau:

                    Thông tin khách hàng:
                    - Mã khách hàng: {maKhachHang}
                    - Tên khách hàng: {tenKhachHang}
                    - Địa chỉ lắp đặt: {diaChi}

                    Thời gian lắp đặt dự kiến: {ngayLapDat:dd/MM/yyyy}

                    Quý khách vui lòng có mặt tại địa điểm lắp đặt vào ngày giờ trên để phối hợp với nhân viên kỹ thuật.

                    Nếu có thay đổi về lịch lắp đặt, chúng tôi sẽ thông báo đến quý khách trong thời gian sớm nhất.

                    Thông tin liên hệ:
                    - Hotline: 19009000
                    - Email: cskh@evnspc.vn

                    Trân trọng!";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("api-key", "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");

                    var content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            sender = new { name = "Điện lực Miền Nam", email = "hoangkhoitl2003@gmail.com" },
                            to = new[] { new { email = email, name = tenKhachHang } },
                            subject = "Thông báo lịch lắp đặt điện",
                            textContent = noiDung
                        }),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = client.PostAsync("https://api.sendinblue.com/v3/smtp/email", content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Đã gửi thông báo lịch lắp đặt đến khách hàng!");
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi khi gửi email: {response.Content.ReadAsStringAsync().Result}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }
        private string GetEmailKhachHang(string maKhachHang)
        {
            string query = "SELECT Email FROM KhachHang WHERE MaKhachHang = @MaKhachHang";
            SqlParameter[] param = { new SqlParameter("@MaKhachHang", maKhachHang) };
            return db.getScalar(query, param)?.ToString();
        }


        private void btnGuiThongBao_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(maHTselected))
            {
                MessageBox.Show("Vui lòng chọn hệ thống điện cần gửi thông báo!");
                return;
            }

            string maKhachHang = lblMaKhachHang.Text;
            string tenKhachHang = lblTenKhachHang.Text;
            string diaChi = lblDiaChi.Text;

            string email = GetEmailKhachHang(maKhachHang);
            if (!string.IsNullOrEmpty(email))
            {
                GuiEmailThongBao(maKhachHang, tenKhachHang, email, DTPNgayLapDat.Value, diaChi);
            }
            else
            {
                MessageBox.Show("Không tìm thấy email khách hàng!");
            }

        }
    }
   
}
