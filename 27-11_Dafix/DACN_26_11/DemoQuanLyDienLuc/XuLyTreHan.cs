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
using System.Net.Http;
using Newtonsoft.Json;

namespace DemoQuanLyDienLuc
{
    public partial class XuLyTreHan : Form
    {

        private DBConnect db = new DBConnect();
        private string maNhanVienHienTai;

        public XuLyTreHan(string maNhanVien)
        {
            InitializeComponent();
            this.maNhanVienHienTai = maNhanVien;
            LoadDataToComboBox();
            LoadDataHoaDon();
            dgvHoaDon.SelectionChanged += dgvHoaDon_SelectionChanged;
        }
        
        private void LoadDataToComboBox()
        {
            // ComboBox Trạng thái nhắc nhở
            cboTrangThaiNhac.Items.Add("Tất cả");
            cboTrangThaiNhac.Items.Add("Chưa đến hạn");
            cboTrangThaiNhac.Items.Add("Quá hạn 14 ngày - Cần nhắc");
            cboTrangThaiNhac.Items.Add("Đã nhắc - Chờ cắt điện");
            cboTrangThaiNhac.Items.Add("Chuẩn bị cắt điện");
            cboTrangThaiNhac.SelectedIndex = 0;
        }

        private void LoadDataHoaDon()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetDanhSachHoaDonTreHan", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaNhanVien", maNhanVienHienTai);
                    cmd.Parameters.AddWithValue("@TinhTrang", DBNull.Value);  // Lấy tất cả

                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    dgvHoaDon.DataSource = dt;
                    FormatDgv();
                }
            }
        }
        private void FormatDgv()
        {
            dgvHoaDon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Đặt tên cột
            var columnHeaders = new Dictionary<string, string>()
                {
                    { "MaHoaDon", "Mã hóa đơn" },
                    { "TenKhachHang", "Khách hàng" },
                    { "TongTien", "Tổng tiền" },
                    { "NgayGhiSo", "Ngày ghi sổ" },
                    { "SoNgayTre", "Số ngày trễ" },
                    { "TinhTrang", "Tình trạng" }
                };

            foreach (var col in columnHeaders)
            {
                dgvHoaDon.Columns[col.Key].HeaderText = col.Value;
            }

            // Format dữ liệu
            dgvHoaDon.Columns["TongTien"].DefaultCellStyle.Format = "N0";
            dgvHoaDon.Columns["NgayGhiSo"].DefaultCellStyle.Format = "dd/MM/yyyy";

            // Ẩn các cột không cần thiết
            string[] hiddenColumns = { "SoLanNhac", "NgayNhacGanNhat", "MaHeThong" };
            foreach (string colName in hiddenColumns)
            {
                dgvHoaDon.Columns[colName].Visible = false;
            }
        }

      
        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void dgvHoaDonTreHan_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvHoaDon.Rows[e.RowIndex];
                int soNgayTre = Convert.ToInt32(row.Cells["SoNgayTre"].Value);
                int soLanNhac = Convert.ToInt32(row.Cells["SoLanNhac"].Value);

                if (soNgayTre > 30)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                else if (soNgayTre > 20)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 200);
                else
                    row.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
            }
        }

       
        private void cboTrangThaiNhac_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tinhTrang = cboTrangThaiNhac.SelectedIndex == 0 ? null : cboTrangThaiNhac.Text;

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetDanhSachHoaDonTreHan", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaNhanVien", maNhanVienHienTai);
                    cmd.Parameters.AddWithValue("@TinhTrang", tinhTrang ?? (object)DBNull.Value);

                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    dgvHoaDon.DataSource = dt;
                }
            }
        }
        
     

        private void dgvHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string maHoaDon = dgvHoaDon.Rows[e.RowIndex].Cells["MaHoaDon"].Value.ToString();
                LoadChiTietHoaDon(maHoaDon);
            }
        }


        private void LoadChiTietHoaDon(string maHoaDon)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetChiTietHoaDon", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaHoaDon", maHoaDon);

                    DataSet ds = new DataSet();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);

                    if (ds.Tables[0].Rows.Count > 0)  // Thông tin hóa đơn
                    {
                        DataRow r = ds.Tables[0].Rows[0];
                        txtMaHoaDon.Text = maHoaDon;
                        txtKhachHang.Text = $"{r["TenKhachHang"]} ({r["MaKhachHang"]})";
                        txtSoDienThoai.Text = r["SoDienThoai"].ToString();
                        txtEmail.Text = r["Email"].ToString();
                        txtDiaChi.Text = r["DiaChiCuThe"].ToString();
                        // ... điền thêm các thông tin khác
                    }

                    if (ds.Tables[1].Rows.Count > 0)  // Lịch sử nhắc nợ
                    {
                        string lichSu = "LỊCH SỬ NHẮC NHỢ:\n";
                        foreach (DataRow r in ds.Tables[1].Rows)
                        {
                            lichSu += $"- {Convert.ToDateTime(r["NgayThongBao"]):dd/MM/yyyy HH:mm}\n";
                            lichSu += $"  Mức độ: Lần {r["MucDoNhacNho"]}\n";
                            lichSu += $"  Nội dung: {r["NoiDung"]}\n";
                            if (Convert.ToInt32(r["MucDoNhacNho"]) >= 2)
                            {
                                lichSu += $"  Hạn chót: {Convert.ToDateTime(r["NgayCatDien"]):dd/MM/yyyy HH:mm}\n";
                            }
                            lichSu += "-------------------\n";
                        }
                        txtLichSuNhacNo.Text = lichSu;
                    }
                }
            }
        }

        private void LoadLichSuNhacNho(string maHoaDon)
        {
            string sql = @"
        SELECT NgayThongBao, NoiDung 
        FROM PhieuNhacNho 
        WHERE MaHoaDon = @MaHoaDon 
        ORDER BY NgayThongBao DESC";

            SqlParameter[] parameters = {
        new SqlParameter("@MaHoaDon", maHoaDon)
    };

            DataTable dt = db.getDataTable(sql, parameters);

            if (dt.Rows.Count > 0)
            {
                string lichSuNhac = "";
                foreach (DataRow row in dt.Rows)
                {
                    lichSuNhac += $"- {row["NgayThongBao"].ToString()}: {row["NoiDung"].ToString()}\n";
                }
                txtLichSuNhacNo.Text = lichSuNhac;  
            }
            else
            {
                txtLichSuNhacNo.Text = "Chưa có lịch sử nhắc nhở";
            }
        }

        private void dgvHoaDon_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtTinhTrang_TextChanged(object sender, EventArgs e)
        {

        }


        // Này là dùng lấy mail xong dùng api
        private string GetEmailFromHoaDon(string maHoaDon)
        {
            string query = @"SELECT kh.Email 
                    FROM KhachHang kh
                    JOIN HeThongDien ht ON kh.MaKhachHang = ht.MaKhachHang
                    JOIN HoaDon hd ON ht.MaHeThong = hd.MaHeThong
                    WHERE hd.MaHoaDon = @MaHoaDon";

            SqlParameter[] param = { new SqlParameter("@MaHoaDon", maHoaDon) };
            return db.getScalar(query, param)?.ToString();
        }

        private void GuiEmailThongBao(string email, string maHoaDon, string noiDung)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("api-key", "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");

                    var content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            sender = new { name = "Điện lực Miền Nam", email = "hoangkhoitl2003@gmail.com" },
                            to = new[] { new { email = email } },
                            subject = $"Thông báo hóa đơn tiền điện - {maHoaDon}",
                            textContent = noiDung
                        }),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = client.PostAsync("https://api.sendinblue.com/v3/smtp/email", content).Result;

                    MessageBox.Show(response.IsSuccessStatusCode ?
                        "Gửi thông báo thành công!" :
                        $"Lỗi: {response.Content.ReadAsStringAsync().Result}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }


        private string GetThongTinChiTiet(string maHoaDon)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetThongTinHoaDon", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaHoaDon", maHoaDon);

                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow r = dt.Rows[0];

                                                return $@"THÔNG BÁO HÓA ĐƠN QUÁ HẠN
                        -------------------------------
                        Kính gửi quý khách hàng {r["TenKhachHang"]},

                        Điện lực {r["TenHuyen"]} trân trọng thông báo:
                        Hóa đơn tiền điện của quý khách đã quá hạn thanh toán.

                        CHI TIẾT HÓA ĐƠN:
                        - Mã hóa đơn: {r["MaHoaDon"]}
                        - Kỳ hóa đơn: {r["Thang"]}/{r["Nam"]}
                        - Số tiền: {string.Format("{0:N0}", r["TongTien"])} VNĐ

                        Để tránh bị cắt điện, quý khách vui lòng:
                        1. Thanh toán ngay số tiền trên
                        2. Giữ lại biên lai thanh toán
                        3. Bỏ qua thông báo nếu đã thanh toán

                        Thông tin liên hệ hỗ trợ:
                        - Hotline: 19009000
                        - Email: cskh@evnspc.vn

                        Trân trọng!";
                    }
                    return null;
                }
            }
        }


        private void btnGuiThongBao_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvHoaDon.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn hóa đơn cần gửi thông báo!");
                    return;
                }

                DataGridViewRow row = dgvHoaDon.SelectedRows[0];
                string maHoaDon = row.Cells["MaHoaDon"].Value.ToString();
                string tinhTrang = row.Cells["TinhTrang"].Value.ToString();
                string email = GetEmailFromHoaDon(maHoaDon);

                if (string.IsNullOrEmpty(email))
                {
                    MessageBox.Show("Không tìm thấy email của khách hàng!");
                    return;
                }

                // Lấy thông tin hóa đơn theo tình trạng
                using (SqlConnection conn = db.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetThongBaoTheoTinhTrang", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaHoaDon", maHoaDon);
                        cmd.Parameters.AddWithValue("@TinhTrang", tinhTrang);

                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            DataRow r = dt.Rows[0];
                            string tieuDe = "";
                            string noiDungCanhBao = "";

                           // Này là nội dung của mail
                            switch (tinhTrang)
                            {
                                case "Quá hạn 14 ngày - Cần nhắc":
                                    tieuDe = $"[Cảnh báo] Hóa đơn điện {r["Thang"]}/{r["Nam"]} quá hạn thanh toán";
                                    noiDungCanhBao = @"
                                        CẢNH BÁO LẦN 1:
                                        - Hóa đơn của quý khách đã quá hạn 14 ngày
                                        - Vui lòng thanh toán trong vòng 24 giờ tới
                                        - Sau thời hạn trên, nếu không thanh toán:
                                          + Phí phạt chậm trả: 98.000đ
                                          + Nguy cơ bị tạm ngưng cung cấp điện";
                                    break;

                                case "Đã nhắc - Chờ cắt điện":
                                    tieuDe = $"[KHẨN] Cảnh báo nguy cơ mất điện - HĐ {r["Thang"]}/{r["Nam"]}";
                                    noiDungCanhBao = $@"
                                        CẢNH BÁO LẦN 2:
                                        - Hóa đơn vẫn chưa được thanh toán sau lần nhắc thứ nhất
                                        - Phí phạt 98.000đ đã được cộng vào hóa đơn
                                        - Tổng số tiền phải thanh toán: {string.Format("{0:N0}", Convert.ToDecimal(r["TongTien"]) + 98000)}đ
                                        - Nếu không thanh toán trong vòng 24h tới:
                                          + Điện lực sẽ tạm ngưng cung cấp điện
                                          + Mất phí mở lại điện: 150.000đ";
                                    break;

                                case "Chuẩn bị cắt điện":
                                    tieuDe = $"[KHẨN CẤP] Thông báo cắt điện - HĐ {r["Thang"]}/{r["Nam"]}";
                                    noiDungCanhBao = $@"
                                        THÔNG BÁO CUỐI CÙNG:
                                        - Điện lực sẽ tiến hành TẠM NGƯNG CUNG CẤP ĐIỆN sau 24 giờ
                                        - Tổng nợ: {string.Format("{0:N0}", Convert.ToDecimal(r["TongTien"]) + 98000)}đ
                                          (Đã bao gồm phí phạt chậm trả 98.000đ)

                                        Để được khôi phục cung cấp điện, quý khách cần:
                                        1. Thanh toán toàn bộ số tiền nợ
                                        2. Nộp phí mở điện lại: 150.000đ
                                        3. Liên hệ tổng đài 19009000 sau khi thanh toán";
                                    break;

                                default:
                                    MessageBox.Show("Hóa đơn chưa đến hạn gửi thông báo!");
                                    return;
                            }

                            // Tạo nội dung email
                            string noiDung = $@"{tieuDe}
                                        ----------------------------------------

                                        Kính gửi: {r["TenKhachHang"]},

                                        {noiDungCanhBao}

                                        CHI TIẾT HÓA ĐƠN:
                                        - Mã khách hàng: {r["MaKhachHang"]}
                                        - Mã hóa đơn: {r["MaHoaDon"]} 
                                        - Kỳ hóa đơn: Tháng {r["Thang"]}/{r["Nam"]}
                                        - Chỉ số: {r["ChiSoCu"]} - {r["ChiSoMoi"]}
                                        - Tiền điện: {string.Format("{0:N0}", r["TongTien"])}đ

                                        Địa chỉ cung cấp:
                                        {r["DiaChiCuThe"]}, {r["TenXa"]}, {r["TenHuyen"]}, {r["TenTinh"]}

                                        THÔNG TIN THANH TOÁN:
                                        - STK: 09870153703
                                        - NH: TPBank
                                        - ĐV: Điện lực {r["TenHuyen"]}
                                        - Nội dung CK: {r["MaKhachHang"]} {r["MaHoaDon"]}

                                        Thông tin hỗ trợ:
                                        - Hotline: 19009000
                                        - Email: cskh@evnspc.vn

                                        Trân trọng!";

                            // Gửi email
                            GuiEmailThongBao(email, tieuDe, noiDung);

                            // Lưu lịch sử
                            using (SqlCommand cmdLog = new SqlCommand("sp_LuuPhieuNhacNo", conn))
                            {
                                cmdLog.CommandType = CommandType.StoredProcedure;
                                cmdLog.Parameters.AddWithValue("@MaHoaDon", maHoaDon);
                                cmdLog.Parameters.AddWithValue("@MaKhachHang", r["MaKhachHang"]);
                                cmdLog.Parameters.AddWithValue("@NoiDung", tieuDe);
                                cmdLog.Parameters.AddWithValue("@MucDoNhacNho",
                                    tinhTrang.Contains("Cần nhắc") ? 1 :
                                    tinhTrang.Contains("Chờ cắt") ? 2 : 3);
                                cmdLog.Parameters.AddWithValue("@MaNhanVien", maNhanVienHienTai);

                                conn.Open();
                                cmdLog.ExecuteNonQuery();
                                LoadDataHoaDon();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }

        }

        private void btnCatDien_Click(object sender, EventArgs e)
        {

        }

        private void dgvHoaDon_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void btnCatDien_Click_1(object sender, EventArgs e)
        {
            if (dgvHoaDon.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần cắt điện!");
                return;
            }

            DataGridViewRow row = dgvHoaDon.SelectedRows[0];
            string tinhTrang = row.Cells["TinhTrang"].Value.ToString();

            if (tinhTrang != "Chuẩn bị cắt điện")
            {
                MessageBox.Show("Chỉ được cắt điện với hóa đơn ở trạng thái Chuẩn bị cắt điện!");
                return;
            }

            if (MessageBox.Show("Xác nhận cắt điện cho khách hàng này?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                string maHeThong = row.Cells["MaHeThong"].Value.ToString();
                string sqlUpdate = @"
                    UPDATE HeThongDien 
                    SET TrangThai = N'Tạm ngưng cung cấp' 
                    WHERE MaHeThong = @MaHeThong";

                SqlParameter[] param = { new SqlParameter("@MaHeThong", maHeThong) };

                if (db.getNonQuery(sqlUpdate, param) > 0)
                {
                    MessageBox.Show("Đã cập nhật trạng thái tạm ngưng cung cấp điện!");
                    LoadDataHoaDon();
                }
            }

        }
    }
}
