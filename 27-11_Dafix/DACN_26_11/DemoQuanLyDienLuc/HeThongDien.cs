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

namespace DemoQuanLyDienLuc
{
    public partial class HeThongDien : Form
    {
        private DBConnect db;
        private string maHeThong;
        public HeThongDien()
        {
            InitializeComponent();
            db = new DBConnect();
            
            dgvHeThong.CellClick += dgvHeThong_CellClick; // Đăng ký sự kiện
            LoadComboBoxTrangThai();
            cmbFilter.SelectedIndex = 0;


        }

        private void LoadComboBoxTrangThai()
        {
            try
            {
                // Câu truy vấn để lấy các giá trị độc nhất từ cột TrangThai
                string query = "SELECT DISTINCT TrangThai FROM HeThongDien WHERE TrangThai IS NOT NULL";

                // Lấy dữ liệu từ cơ sở dữ liệu
                DataTable dt = db.getDataTable(query);

                // Xóa tất cả các mục hiện có trong ComboBox
                cmbFilter.Items.Clear();

                // Thêm các giá trị từ DataTable vào ComboBox
                foreach (DataRow row in dt.Rows)
                {
                    cmbFilter.Items.Add(row["TrangThai"].ToString());
                }

                // Nếu ComboBox có giá trị, chọn mục đầu tiên
                if (cmbFilter.Items.Count > 0)
                {
                    cmbFilter.SelectedIndex = 0; // Mặc định chọn giá trị đầu tiên
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách trạng thái: " + ex.Message);
            }
        }

        private void SearchKhachHangByCCCD(string cccd)
        {
            try
            {
                // Câu truy vấn lấy thông tin khách hàng dựa trên CCCD
                string query = @"
            SELECT MaKhachHang, TenKhachHang, CCCD 
            FROM KhachHang 
            WHERE CCCD = @CCCD";

                using (SqlConnection connection = db.GetConnection())
                {
                    connection.Open(); // Đảm bảo kết nối được mở trước khi thực thi truy vấn
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@CCCD", cccd);

                        // Thực thi truy vấn và lấy dữ liệu
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            // Nếu tìm thấy khách hàng, đọc dữ liệu và hiển thị thông báo
                            reader.Read();
                            string maKhachHang = reader["MaKhachHang"].ToString();
                            string tenKhachHang = reader["TenKhachHang"].ToString();

                            MessageBox.Show($"Mã Khách Hàng: {maKhachHang}\nTên Khách Hàng: {tenKhachHang}",
                                            "Thông Tin Khách Hàng",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                        }
                        else
                        {
                            // Nếu không tìm thấy khách hàng
                            MessageBox.Show("Không tìm thấy khách hàng với CCCD này.",
                                            "Thông Báo",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tìm kiếm khách hàng: " + ex.Message,
                                "Lỗi",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }


        private void LoadHeThongDien(string trangThaiFilter = null)
        {

            try
            {
                // Khởi tạo câu truy vấn
                string query = @"
                        SELECT TOP (1000) 
                            MaHeThong, 
                            MaKhachHang, 
                            MaDongHo, 
                            MaNhanVien, 
                            NgayLap, 
                            NgayLapDat, 
                            HienTrang, 
                            ViTriLapDat, 
                            TrangThai,
                            BienBan
                        FROM HeThongDien";

                // Nếu có điều kiện lọc, thêm điều kiện vào truy vấn
                if (!string.IsNullOrEmpty(trangThaiFilter))
                {
                    query += " WHERE TrangThai = @TrangThai";
                }

                // Tạo đối tượng Command để thực hiện truy vấn
                using (SqlCommand cmd = new SqlCommand(query, db.GetConnection()))
                {
                    // Nếu có điều kiện lọc, thêm tham số vào command
                    if (!string.IsNullOrEmpty(trangThaiFilter))
                    {
                        cmd.Parameters.AddWithValue("@TrangThai", trangThaiFilter);
                    }

                    // Thực thi truy vấn và lấy dữ liệu từ cơ sở dữ liệu
                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    dgvHeThong.DataSource = dt;

                    dgvHeThong.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvHeThong.AutoGenerateColumns = true;
                    dgvHeThong.Columns["MaHeThong"].HeaderText = "Mã Hệ Thống";
                    dgvHeThong.Columns["MaKhachHang"].HeaderText = "Mã Khách Hàng";
                    dgvHeThong.Columns["MaDongHo"].HeaderText = "Mã Đồng Hồ";
                    dgvHeThong.Columns["MaNhanVien"].HeaderText = "Mã Nhân Viên";
                    dgvHeThong.Columns["NgayLap"].HeaderText = "Ngày Lập";
                    dgvHeThong.Columns["NgayLapDat"].HeaderText = "Ngày Lắp Đặt";
                    dgvHeThong.Columns["HienTrang"].HeaderText = "Hiện Trạng";
                    dgvHeThong.Columns["ViTriLapDat"].HeaderText = "Địa chỉ";
                    dgvHeThong.Columns["TrangThai"].HeaderText = "Trạng Thái";
                    dgvHeThong.Columns["BienBan"].Visible = false; // Ẩn cột BienBan nếu không cần hiển thị

                    if (dgvHeThong.Rows.Count > 0)
                    {
                        dgvHeThong.Rows[0].Selected = true; // Chọn dòng đầu tiên
                        dgvHeThong.CurrentCell = dgvHeThong.Rows[0].Cells[0]; // Thiết lập ô hiện tại là ô đầu tiên

                        // Gọi sự kiện CellClick cho dòng đầu tiên
                        dgvHeThong_CellClick(dgvHeThong, new DataGridViewCellEventArgs(0, 0));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }




        private void HeThongDien_Load(object sender, EventArgs e)
        {
            
        }

        private void dgvHeThong_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Kiểm tra nếu dòng được chọn hợp lệ
                if (e.RowIndex >= 0)
                {
                    // Lấy dữ liệu từ dòng đã chọn
                    DataGridViewRow row = dgvHeThong.Rows[e.RowIndex];

                   
            
                    // Lấy và kiểm tra các giá trị
                    string maHeThong = row.Cells["MaHeThong"].Value != DBNull.Value ? row.Cells["MaHeThong"].Value.ToString() : string.Empty;
                    string maKhachHang = row.Cells["MaKhachHang"].Value != DBNull.Value ? row.Cells["MaKhachHang"].Value.ToString() : string.Empty;
                    DateTime? ngayLap = row.Cells["NgayLap"].Value != DBNull.Value ? (DateTime?)Convert.ToDateTime(row.Cells["NgayLap"].Value) : null;
                    DateTime? ngayLapDat = row.Cells["NgayLapDat"].Value != DBNull.Value ? (DateTime?)Convert.ToDateTime(row.Cells["NgayLapDat"].Value) : null;
                    string viTriLapDat = row.Cells["ViTriLapDat"].Value != DBNull.Value ? row.Cells["ViTriLapDat"].Value.ToString() : string.Empty;
                    string trangThai = row.Cells["TrangThai"].Value != DBNull.Value ? row.Cells["TrangThai"].Value.ToString() : string.Empty;
                    string maDongHo = row.Cells["MaDongHo"].Value != DBNull.Value ? row.Cells["MaDongHo"].Value.ToString() : string.Empty;
                    string hienTrang = row.Cells["HienTrang"].Value != DBNull.Value ? row.Cells["HienTrang"].Value.ToString() : string.Empty;
                    byte[] bienBan = row.Cells["BienBan"].Value != DBNull.Value ? (byte[])row.Cells["BienBan"].Value : null;

                    if (trangThai == "Chờ Duyệt")
                    {
                        btnDuyet.Visible = true; // Hiện nút Duyệt nếu trạng thái là Chờ Duyệt
                    }
                    else
                    {
                        btnDuyet.Visible = false; // Ẩn nút Duyệt nếu trạng thái không phải Chờ Duyệt
                    }

                    // Hiển thị các giá trị lên các Label
                    lblMaHeThong.Text = maHeThong;
                    lblMaKhachHang.Text = maKhachHang;
                    lblNgayLap.Text = ngayLap.HasValue ? ngayLap.Value.ToString("dd/MM/yyyy") : string.Empty;
                    lblMaDongHo.Text = maDongHo;
                    lblHienTrang.Text = hienTrang;
                    lblNgayLapDat.Text = ngayLapDat.HasValue ? ngayLapDat.Value.ToString("dd/MM/yyyy") : string.Empty;
                    lblViTri.Text = viTriLapDat;
                    lblTrangThai.Text = trangThai;

                    // Lấy tên khách hàng từ bảng KhachHang
                    string query = "SELECT TenKhachHang FROM KhachHang WHERE MaKhachHang = @MaKhachHang";
                    SqlParameter[] parameters = { new SqlParameter("@MaKhachHang", maKhachHang) };
                    string tenKhachHang = (string)db.getScalar(query, parameters);
                    lblTenKhachHang.Text = tenKhachHang ?? string.Empty; // Gán trắng nếu không có tên khách hàng

                    // Hiển thị hình ảnh biên bản lên PictureBox
                    if (bienBan != null)
                    {
                        using (MemoryStream ms = new MemoryStream(bienBan))
                        {
                            pibBienBan.Image = Image.FromStream(ms);
                        }
                        // Thiết lập chế độ hiển thị hình ảnh
                        pibBienBan.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    else
                    {
                        pibBienBan.Image = new Bitmap(pibBienBan.Width, pibBienBan.Height); // Hình ảnh trắng nếu không có biên bản
                    }
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Lỗi khi lấy thông tin: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
            }
        }

        private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTrangThai = cmbFilter.SelectedItem?.ToString();
            LoadHeThongDien(selectedTrangThai);
          
        }

        private void btnDuyet_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(maHeThong))
            {
                // Tạo kết nối tới database thông qua lớp DBConnect
                DBConnect db = new DBConnect();

                // Câu lệnh SQL để cập nhật trạng thái
                string query = "UPDATE HeThongDien SET TrangThai = N'Chờ lắp đặt' WHERE MaHeThong = @MaHeThong";

                // Tạo tham số cho câu lệnh SQL
                SqlParameter[] parameters = new SqlParameter[]
              {
                    new SqlParameter("@MaHeThong", maHeThong)
              };

                try
                {
                    // Thực hiện câu lệnh cập nhật
                    int rowsAffected = db.getNonQuery(query, parameters);

                    // Kiểm tra kết quả
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Duyệt Thành Công!");
                        LoadComboBoxTrangThai();
                        
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy hệ thống với mã này.");
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi
                    MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một hệ thống.");
            }
        }

        private void dgvHeThong_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvHeThong.SelectedRows.Count > 0)
            {
                // Lấy mã hệ thống từ cột đầu tiên của hàng được chọn
                maHeThong = dgvHeThong.SelectedRows[0].Cells["MaHeThong"].Value.ToString();
            }
        }

        private void lblMaDongHo_Click(object sender, EventArgs e)
        {

        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            string cccd = txtTimKiem.Text.Trim();

            if (!string.IsNullOrEmpty(cccd))
            {
                SearchKhachHangByCCCD(cccd);
            }
            else
            {
                MessageBox.Show("Vui lòng nhập CCCD để tìm kiếm.",
                                "Thông Báo",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }
    }
}
