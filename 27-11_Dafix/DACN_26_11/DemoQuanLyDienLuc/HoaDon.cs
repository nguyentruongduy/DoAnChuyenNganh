using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoQuanLyDienLuc
{
    public partial class HoaDon : Form
    {

        private DBConnect db = new DBConnect();
        private string maNhanVienHienTai;

        public HoaDon(string maNV)
        {
            InitializeComponent();
            this.maNhanVienHienTai = maNV;
            LoadCboTrangThai();
            LoadDanhSachHoaDon();
        }

        private void LoadCboTrangThai()
        {
            cboTrangThai.Items.Add("Tất cả");
            cboTrangThai.Items.Add("Chưa thanh toán");
            cboTrangThai.Items.Add("Đã thanh toán");
            cboTrangThai.SelectedIndex = 0;
        }

        private void LoadDanhSachHoaDon()
        {
            string sql = @"
            SELECT 
                h.MaHoaDon,
                k.TenKhachHang,
                h.ChiSoCu,
                h.ChiSoMoi,
                h.SoDienTieuThu,
                h.TongTien,
                h.TrangThaiThanhToan,
                h.Thang,
                h.Nam
            FROM HoaDon h
            JOIN HeThongDien ht ON h.MaHeThong = ht.MaHeThong
            JOIN KhachHang k ON ht.MaKhachHang = k.MaKhachHang
            WHERE MONTH(h.NgayGhiSo) = MONTH(GETDATE()) 
            AND YEAR(h.NgayGhiSo) = YEAR(GETDATE())";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(txtMaHoaDon.Text.Trim()))
            {
                sql += " AND h.MaHoaDon LIKE @MaHD";
                parameters.Add(new SqlParameter("@MaHD", "%" + txtMaHoaDon.Text.Trim() + "%"));
            }

            if (cboTrangThai.SelectedIndex > 0)
            {
                sql += " AND h.TrangThaiThanhToan = @TrangThai";
                parameters.Add(new SqlParameter("@TrangThai", cboTrangThai.Text));
            }

            sql += " ORDER BY h.NgayGhiSo DESC";

            dgvHoaDon.DataSource = db.getDataTable(sql, parameters.ToArray());
            FormatDGV();
        }

        private void FormatDGV()
        {
            dgvHoaDon.Columns["MaHoaDon"].HeaderText = "Mã hóa đơn";
            dgvHoaDon.Columns["TenKhachHang"].HeaderText = "Khách hàng";
            dgvHoaDon.Columns["ChiSoCu"].HeaderText = "Chỉ số cũ";
            dgvHoaDon.Columns["ChiSoMoi"].HeaderText = "Chỉ số mới";
            dgvHoaDon.Columns["SoDienTieuThu"].HeaderText = "Tiêu thụ";
            dgvHoaDon.Columns["TongTien"].HeaderText = "Tổng tiền";
            dgvHoaDon.Columns["TrangThaiThanhToan"].HeaderText = "Trạng thái";

            dgvHoaDon.Columns["TongTien"].DefaultCellStyle.Format = "N0";
            dgvHoaDon.Columns["Thang"].Visible = false;
            dgvHoaDon.Columns["Nam"].Visible = false;
        }

        private void LoadChiTietHoaDon(string maHD)
        {
            // Load thông tin hóa đơn
            string sqlHoaDon = @"
            SELECT 
                h.MaHoaDon,
                k.TenKhachHang,
                k.SoDienThoai,
                k.DiaChiCuThe + ', ' + x.TenXa + ', ' + huyen.TenHuyen + ', ' + t.TenTinh as DiaChi,
                h.ChiSoCu,
                h.ChiSoMoi,
                h.SoDienTieuThu,
                h.TongTien
            FROM HoaDon h
            JOIN HeThongDien ht ON h.MaHeThong = ht.MaHeThong
            JOIN KhachHang k ON ht.MaKhachHang = k.MaKhachHang
            JOIN Xa x ON k.MaXa = x.MaXa
            JOIN Huyen huyen ON x.MaHuyen = huyen.MaHuyen
            JOIN Tinh t ON huyen.MaTinh = t.MaTinh
            WHERE h.MaHoaDon = @MaHD";

            SqlParameter[] parameters = { new SqlParameter("@MaHD", maHD) };
            DataTable dtHoaDon = db.getDataTable(sqlHoaDon, parameters);

            if (dtHoaDon.Rows.Count > 0)
            {
                DataRow dr = dtHoaDon.Rows[0];
                lblMaHoaDon.Text = dr["MaHoaDon"].ToString();
                lblKhachHang.Text = dr["TenKhachHang"].ToString();
                lblDiaChi.Text = dr["DiaChi"].ToString();
                lblSoDienThoai.Text = dr["SoDienThoai"].ToString();
                lblChiSoCu.Text = dr["ChiSoCu"].ToString();
                lblChiSoMoi.Text = dr["ChiSoMoi"].ToString();
                lblTongTien.Text = string.Format("{0:N0}", dr["TongTien"]);

                // Load chi tiết các bậc điện
                string sqlChiTiet = @"
                SELECT 
                    BacDien,
                    SoKwhTieuThu,
                    DonGia,
                    ThanhTien
                FROM ChiTietHoaDon
                WHERE MaHoaDon = @MaHD
                ORDER BY BacDien";

                DataTable dtChiTiet = db.getDataTable(sqlChiTiet, parameters);


                foreach (DataRow row in dtChiTiet.Rows)
                {
                    int bacDien = Convert.ToInt32(row["BacDien"]);
                    decimal thanhTien = Convert.ToDecimal(row["ThanhTien"]);

                    switch (bacDien)
                    {
                        case 1:
                            lblBac1.Text = string.Format("{0:N0}", thanhTien);
                            break;
                        case 2:
                            lblBac2.Text = string.Format("{0:N0}", thanhTien);
                            break;

                    }
                }
            }
        }


        private void btnLoc_Click(object sender, EventArgs e)
        {
            LoadDanhSachHoaDon();
        }

        private void dgvHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string maHD = dgvHoaDon.Rows[e.RowIndex].Cells["MaHoaDon"].Value.ToString();
                LoadChiTietHoaDon(maHD);
            }
        }

        private void btnThanhToanTienMat_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(lblMaHoaDon.Text))
                {
                    MessageBox.Show("Vui lòng chọn hóa đơn cần thanh toán!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra trạng thái thanh toán
                string sqlCheck = "SELECT TrangThaiThanhToan FROM HoaDon WHERE MaHoaDon = @MaHD";
                SqlParameter[] param = { new SqlParameter("@MaHD", lblMaHoaDon.Text) };
                DataTable dt = db.getDataTable(sqlCheck, param);

                if (dt.Rows.Count > 0)
                {
                    string trangThai = dt.Rows[0]["TrangThaiThanhToan"].ToString();
                    if (trangThai == "Đã thanh toán")
                    {
                        MessageBox.Show("Hóa đơn này đã được thanh toán!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                ThanhToanHoaDon("Tiền mặt");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thanh toán: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ThanhToanHoaDon(string phuongThucThanhToan)
        {
            try
            {
                // 1. Cập nhật trạng thái hóa đơn
                string sqlUpdateHoaDon = @"
                    UPDATE HoaDon 
                    SET TrangThaiThanhToan = N'Đã thanh toán'
                    WHERE MaHoaDon = @MaHD";

                SqlParameter[] paramsHoaDon = {
            new SqlParameter("@MaHD", lblMaHoaDon.Text)
        };

                int kq1 = db.getNonQuery(sqlUpdateHoaDon, paramsHoaDon);

                // 2. Thêm giao dịch thanh toán
                string sqlInsertGiaoDich = @"
                    INSERT INTO GiaoDichThanhToan (MaHoaDon, NgayThanhToan, SoTienThanhToan, PhuongThucThanhToan)
                    VALUES (@MaHD, GETDATE(), @SoTien, @PhuongThuc)";

                decimal soTien = decimal.Parse(lblTongTien.Text.Replace(",", ""));
                SqlParameter[] paramsGiaoDich = {
            new SqlParameter("@MaHD", lblMaHoaDon.Text),
            new SqlParameter("@SoTien", soTien),
            new SqlParameter("@PhuongThuc", phuongThucThanhToan)
        };

                int kq2 = db.getNonQuery(sqlInsertGiaoDich, paramsGiaoDich);

                if (kq1 > 0 && kq2 > 0)
                {
                    MessageBox.Show($"Thanh toán thành công qua {phuongThucThanhToan}!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh lại danh sách hóa đơn
                    LoadDanhSachHoaDon();

                    // Clear thông tin chi tiết
                    ClearChiTietHoaDon();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi thanh toán!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thanh toán: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearChiTietHoaDon()
        {
            lblMaHoaDon.Text = "";
            lblKhachHang.Text = "";
            lblDiaChi.Text = "";
            lblSoDienThoai.Text = "";
            lblChiSoCu.Text = "";
            lblChiSoMoi.Text = "";
            lblBac1.Text = "";
            lblBac2.Text = "";
            lblTongTien.Text = "";
        }

        private async void btnMOMO_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(lblMaHoaDon.Text))
                {
                    MessageBox.Show("Vui lòng chọn hóa đơn cần thanh toán!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra trạng thái thanh toán
                string sqlCheck = "SELECT TrangThaiThanhToan FROM HoaDon WHERE MaHoaDon = @MaHD";
                SqlParameter[] param = { new SqlParameter("@MaHD", lblMaHoaDon.Text) };
                DataTable dt = db.getDataTable(sqlCheck, param);

                if (dt.Rows.Count > 0)
                {
                    string trangThai = dt.Rows[0]["TrangThaiThanhToan"].ToString();
                    if (trangThai == "Đã thanh toán")
                    {
                        MessageBox.Show("Hóa đơn này đã được thanh toán!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                var momoTest = new MomoTest();
                string orderId = $"{lblMaHoaDon.Text}_{DateTime.Now.Ticks}";

                string rawAmount = lblTongTien.Text.Replace(",", "").Replace(".", "").Trim();
                if (!long.TryParse(rawAmount, out long amount))
                {
                    MessageBox.Show("Số tiền không hợp lệ!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string orderInfo = $"Thanh toan hoa don {lblMaHoaDon.Text}";
                string payUrl = await momoTest.CreateTestPayment(orderId, amount, orderInfo);
                System.Diagnostics.Process.Start(payUrl);

                if (MessageBox.Show("Đã hoàn tất thanh toán qua MOMO?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ThanhToanHoaDon("MOMO");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thanh toán MOMO: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnNganHang_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(lblMaHoaDon.Text))
                {
                    MessageBox.Show("Vui lòng chọn hóa đơn cần thanh toán!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var vnPayTest = new VNPayTest();
                string orderId = lblMaHoaDon.Text;

                // Xử lý số tiền
                string rawAmount = lblTongTien.Text.Replace(",", "").Replace(".", "").Trim();
                if (!long.TryParse(rawAmount, out long amount))
                {
                    MessageBox.Show("Số tiền không hợp lệ!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string orderInfo = $"Thanh toan hoa don {orderId}";
                string paymentUrl = vnPayTest.CreatePaymentUrl(orderId, amount, orderInfo);

                // Mở URL trong trình duyệt mặc định
                System.Diagnostics.Process.Start(paymentUrl);

                // Sau khi thanh toán thành công
                if (MessageBox.Show("Đã hoàn tất thanh toán qua VNPay?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ThanhToanHoaDon("VNPay");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thanh toán VNPay: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
