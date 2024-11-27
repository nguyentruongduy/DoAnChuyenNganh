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
    public partial class XuLySuCo : Form
    {
        private DBConnect db = new DBConnect();
        private byte[] fileData = null;
        private string fileName = "";

        public XuLySuCo()
        {
            InitializeComponent();
            LoadDataToGrid();
            LoadLoaiSuCo();  
            LoadTrangThai();

        }

        private void LoadLoaiSuCo()
        {
            // Thêm các loại sự cố vào ComboBox
            cboLoaiSuCo.Items.Clear();
            cboLoaiSuCo.Items.AddRange(new string[] {
                "Mất điện",
                "Điện yếu",
                "Sự cố đường dây",
                "Sự cố trạm biến áp",
                "Khác"
            });
        }

        private void LoadTrangThai()
        {
            // Thêm các trạng thái vào ComboBox
            cboTrangThai.Items.Clear();
            cboTrangThai.Items.AddRange(new string[] {
                "Chờ xử lý",
                "Đang xử lý",
                "Đã xử lý"
             });
        }

        private void LoadDataToGrid()
        {
            try
            {
                string sql = @"SELECT xs.MaSuCo, xs.MaKhachHang, kh.TenKhachHang, 
                    xs.LoaiSuCo, xs.NgayBaoCao, xs.NgayXuLy, xs.TrangThai
                    FROM XuLySuCo xs
                    JOIN KhachHang kh ON xs.MaKhachHang = kh.MaKhachHang
                    ORDER BY xs.NgayBaoCao DESC";

                DataTable dt = db.getDataTable(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    dgvDanhSach.DataSource = dt;
                    // Format lại tên cột hiển thị
                    dgvDanhSach.Columns["MaSuCo"].HeaderText = "Mã sự cố";
                    dgvDanhSach.Columns["MaKhachHang"].HeaderText = "Mã KH";
                    dgvDanhSach.Columns["TenKhachHang"].HeaderText = "Tên khách hàng";
                    dgvDanhSach.Columns["LoaiSuCo"].HeaderText = "Loại sự cố";
                    dgvDanhSach.Columns["NgayBaoCao"].HeaderText = "Ngày báo cáo";
                    dgvDanhSach.Columns["NgayXuLy"].HeaderText = "Ngày xử lý";
                    dgvDanhSach.Columns["TrangThai"].HeaderText = "Trạng thái";
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu sự cố!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnLoc_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTimKiem.Text)) return;

            string sql = @"SELECT xs.MaSuCo, xs.MaKhachHang, kh.TenKhachHang, 
                          xs.LoaiSuCo, xs.NgayBaoCao, xs.NgayXuLy, xs.TrangThai,
                          CASE WHEN xs.FileDinhKem IS NULL THEN N'Chưa có' ELSE N'Đã đính kèm' END as TrangThaiFile
                          FROM XuLySuCo xs
                          JOIN KhachHang kh ON xs.MaKhachHang = kh.MaKhachHang
                          WHERE kh.TenKhachHang LIKE @Search OR kh.MaKhachHang LIKE @Search";

            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Search", "%" + txtTimKiem.Text + "%")
            };

            dgvDanhSach.DataSource = db.getDataTable(sql, parameters);

        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaKH.Text) || cboLoaiSuCo.SelectedIndex == -1 || cboTrangThai.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Sửa lại câu SQL, bỏ FileDinhKem và TenFile
            string sql = @"INSERT INTO XuLySuCo (MaKhachHang, LoaiSuCo, NgayBaoCao, TrangThai) 
                  VALUES (@MaKH, @LoaiSuCo, @NgayBao, @TrangThai)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaKH", txtMaKH.Text),
                new SqlParameter("@LoaiSuCo", cboLoaiSuCo.Text),
                new SqlParameter("@NgayBao", DateTime.Now),
                new SqlParameter("@TrangThai", cboTrangThai.Text)
            };

            try
            {
                int result = db.getNonQuery(sql, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Thêm sự cố thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDataToGrid();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            if (dgvDanhSach.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn sự cố cần cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE XuLySuCo 
                  SET TrangThai = @TrangThai,
                      NgayXuLy = CASE WHEN @TrangThai = N'Đã xử lý' THEN GETDATE() ELSE NgayXuLy END
                  WHERE MaSuCo = @MaSuCo";

            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@TrangThai", cboTrangThai.Text),
        new SqlParameter("@MaSuCo", dgvDanhSach.SelectedRows[0].Cells["MaSuCo"].Value)
            };

            try
            {
                int result = db.getNonQuery(sql, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDataToGrid();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvDanhSach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDanhSach.Rows[e.RowIndex];
                txtMaKH.Text = row.Cells["MaKhachHang"].Value.ToString();
                LoadTenKhachHang(txtMaKH.Text);
                cboLoaiSuCo.Text = row.Cells["LoaiSuCo"].Value.ToString();
                cboTrangThai.Text = row.Cells["TrangThai"].Value.ToString();
            }
        }

        private void LoadTenKhachHang(string maKH)
        {
            string sql = "SELECT TenKhachHang FROM KhachHang WHERE MaKhachHang = @MaKH";
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@MaKH", maKH)
            };

            object result = db.getScalar(sql, parameters);
            if (result != null)
                txtTenKH.Text = result.ToString();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.jpg, *.png)|*.jpg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Hiển thị ảnh lên PictureBox
                    picHinhAnh.Image = Image.FromFile(ofd.FileName);
                    picHinhAnh.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        private void ClearForm()
        {
            txtMaKH.Clear();
            txtTenKH.Clear();
            cboLoaiSuCo.SelectedIndex = -1;
            cboTrangThai.SelectedIndex = -1;
            fileData = null;
            fileName = "";

            lblFileName.Text = "";
        }
    }
}
