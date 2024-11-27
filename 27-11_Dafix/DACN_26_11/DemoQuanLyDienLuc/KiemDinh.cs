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
    public partial class KiemDinh : Form
    {
        private DBConnect db;
        private string maNhanVien;
        private byte[] bienBanData;
        public KiemDinh(string maNhanVien)
        {
           
            db = new DBConnect();
            InitializeComponent();
            LoadHeThongDien(maNhanVien);
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
                    htd.MaNhanVien,
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
                        dgvHeThong.Columns["MaNhanVien"].HeaderText = "Mã Nhân viên";
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
                    lblMaNhanVien.Text = row.Cells["MaNhanVien"].Value.ToString();
                    lblMaDongHo.Text = row.Cells["MaDongHo"].Value.ToString();
                    lblNgayLapDat.Text = row.Cells["NgayLapDat"].Value != null ? Convert.ToDateTime(row.Cells["NgayLapDat"].Value).ToString("dd/MM/yyyy") : string.Empty;
                    //DTPNgayLapDat.Value = Convert.ToDateTime(row.Cells["NgayLapDat"].Value);

                    string maKhachHang = row.Cells["MaKhachHang"].Value != DBNull.Value ? row.Cells["MaKhachHang"].Value.ToString() : string.Empty;

                    lblMaKhachHang.Text = row.Cells["MaKhachHang"].Value.ToString();

                    string query = "SELECT TenKhachHang FROM KhachHang WHERE MaKhachHang = @MaKhachHang";
                    SqlParameter[] parameters = { new SqlParameter("@MaKhachHang", maKhachHang) };
                    string tenKhachHang = (string)db.getScalar(query, parameters);
                    lblTenKhachHang.Text = tenKhachHang ?? string.Empty; // Gán trắng nếu không có tên khách hàng

                    string maNhanVien = row.Cells["MaNhanVien"].Value != DBNull.Value ? row.Cells["MaNhanVien"].Value.ToString() : string.Empty;

                    lblTenNhanVien.Text = row.Cells["MaNhanVien"].Value.ToString();

                    string query1 = "SELECT TenNhanVien FROM NhanVien WHERE MaNhanVien = @MaNhanVien";
                    SqlParameter[] parameters1 = { new SqlParameter("@MaNhanVien", maNhanVien) };
                    string tenNhanVien = (string)db.getScalar(query1, parameters1);
                    lblTenNhanVien.Text = tenNhanVien ?? string.Empty; // Gán trắng nếu không có tên khách hàng


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi chọn dòng: " + ex.Message);
            }
        }



      
        private byte[] GetBienBan(string maHeThong)
        {
            string query = @"
                SELECT BienBan 
                FROM HeThongDien 
                WHERE MaHeThong = @MaHeThong";

            using (SqlConnection conn = new SqlConnection(db.strConn1))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaHeThong", maHeThong);
                    object result = cmd.ExecuteScalar(); // Lấy giá trị đầu tiên của trường BienBan

                    return result as byte[]; // Chuyển đổi về mảng byte
                }
            }
        }

        private void SaveBienBanToFile(string maHeThong)
        {
            byte[] bienBanData = GetBienBan(maHeThong);

            if (bienBanData != null)
            {
                string tempFilePath = Path.Combine(Path.GetTempPath(), "BienBan_" + maHeThong + ".png"); // Đặt tên cho tệp

                File.WriteAllBytes(tempFilePath, bienBanData); // Ghi mảng byte ra tệp


                // Mở tệp
                OpenFile(tempFilePath);
            }
            else
            {
                MessageBox.Show("Không tìm thấy biên bản cho mã hệ thống này.");
            }
        }




        private void OpenFile(string filePath)
        {
            try
            {
                System.Diagnostics.Process.Start(filePath); // Mở tệp với ứng dụng mặc định
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi mở tệp: " + ex.Message);
            }
        }



        private void UpdateBienBan(string maHeThong, byte[] bienBanData)
        {
            string hienTrang = txtHienTrang.Text;
            string query = @"
                UPDATE HeThongDien
                SET BienBan = @BienBan,
                    HienTrang = @HienTrang,
                    TrangThai = N'Đang hoạt động'
                WHERE MaHeThong = @MaHeThong";

            // Sử dụng DBConnect để thực thi câu lệnh
            using (SqlConnection conn = new SqlConnection(db.strConn1)) // Giả sử db.ConnectionString là thuộc tính chứa chuỗi kết nối
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BienBan", bienBanData);
                    cmd.Parameters.AddWithValue("@HienTrang", hienTrang);
                    cmd.Parameters.AddWithValue("@MaHeThong", maHeThong);

                    // Sử dụng phương thức ExecuteNonQuery từ DBConnect
                    int rowsAffected = db.ExecuteNonQuery(cmd);
                    MessageBox.Show(rowsAffected + " row(s) updated.");
                }
            }
        }

        private void btnHoanThanh_Click(object sender, EventArgs e)
        {
            if (bienBanData != null)
            {
                string maHeThong = maHTselected;

                try
                {
                    // Lưu biên bản vào cơ sở dữ liệu
                    UpdateBienBan(maHeThong, bienBanData);
                    LoadHeThongDien(maNhanVien);

                    MessageBox.Show("Lưu biên bản thành công.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu biên bản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một tệp ảnh trước khi hoàn thành.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }



        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadHeThongDien(maNhanVien);
        }

        private void btnChonAnh_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Chọn tệp Biên Bản";
                openFileDialog.Filter = "Tất cả các tệp (*.*)|*.*|Hình ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Đọc nội dung tệp và lưu trữ vào biến toàn cục bienBanData
                    bienBanData = File.ReadAllBytes(openFileDialog.FileName);

                    // Hiển thị tên tệp được chọn trong tiêu đề
                    lblBienBan.Text = $"Đã chọn tệp Biên Bản: {Path.GetFileName(openFileDialog.FileName)}";

                    // Kiểm tra và hiển thị ảnh trong PictureBox nếu là tệp ảnh
                    string fileExtension = Path.GetExtension(openFileDialog.FileName).ToLower();
                    if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".bmp")
                    {
                        ptbBienBan.Image = Image.FromFile(openFileDialog.FileName);
                        ptbBienBan.SizeMode = PictureBoxSizeMode.Zoom; // Thiết lập chế độ hiển thị ảnh
                    }
                    else
                    {
                        MessageBox.Show("Tệp đã chọn không phải là hình ảnh. Không thể hiển thị trong PictureBox.",
                                        "Thông Báo",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                        bienBanData = null; // Đặt lại bienBanData nếu không phải là tệp ảnh
                    }
                }
            }
        }
    }





}
