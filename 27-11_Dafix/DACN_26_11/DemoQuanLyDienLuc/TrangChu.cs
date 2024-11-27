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
    public partial class TrangChu : Form
    {
        private DBConnect db;
        private string tenKhachHang;
        private string tenNhanVien;
        private string maNhanVien;
        private string chucVu;
        public TrangChu(string tenKhachHang, string tenNhanVien, string maNhanVien, string chucVu)
        {
            InitializeComponent();
            db = new DBConnect();

            // Gán giá trị cho các biến thành viên
            this.tenKhachHang = tenKhachHang;
            this.tenNhanVien = tenNhanVien;
            this.maNhanVien = maNhanVien;
            this.chucVu = chucVu;
            if (!string.IsNullOrEmpty(tenKhachHang))
            {
                lblTenKhachHang.Text = tenKhachHang; // Giả sử bạn có label để hiển thị tên khách hàng
            }

            if (!string.IsNullOrEmpty(tenNhanVien))
            {
                lblTenNhanVien.Text = tenNhanVien; // Giả sử bạn có label để hiển thị tên nhân viên
            }

            SetAccessRights();
            container(new Dashboard());
            CenterToScreen();
        }

        private void SetAccessRights()
        {
            switch (chucVu)
            {
                case "Quản lý":
                    // Admin có quyền truy cập tất cả
                    btnKhachHang.Enabled = true;
                    btnNhanVien.Enabled = true;
                    //btnHeThongDien.Enabled = true;
                    btnPhanCong.Enabled = true;
                    btnPhanCong.Enabled = true;
                    btn_GhiDien.Enabled = true;
                    btnHoaDon.Enabled = true;
                    btnKiemDinh.Enabled = true;
                    //btnKiemDinh.Enabled=true;
                    break;
                case "Quản lý kỹ thuật":
                    btnNhanVien.Enabled = true;
                    btnPhanCong.Enabled = true;
                    btn_GhiDien.Enabled = true;
                    btnKhachHang.Enabled = false;
                    btnHeThongDien.Enabled = false;
                    btnHoaDon.Enabled = false;
                    break;
                case "Nhân viên tiếp nhận":
                    // Admin có quyền truy cập tất cả
                    btnKhachHang.Enabled = true;
                    btnNhanVien.Enabled = false;
                    btnHeThongDien.Enabled = true;
                    btnPhanCong.Enabled = false;
                    btn_GhiDien.Enabled = false;
                    btnHoaDon.Enabled = false;

                    break;

                case "Nhân viên ghi điện":

                    btnKhachHang.Enabled = false;
                    btnNhanVien.Enabled = false;
                    btnHeThongDien.Enabled = false;
                    btnPhanCong.Enabled = false;
                    btn_GhiDien.Enabled = true;
                    btnHoaDon.Enabled = false;
                    btnKiemDinh.Enabled = false;

                    break;


                default:
                    // Mặc định không cho phép
                    btnKhachHang.Enabled = false;
                    btnNhanVien.Enabled = false;
                    btnHeThongDien.Enabled = false;
                    btnPhanCong.Enabled = true;
                    btn_GhiDien.Enabled = false;
                    break;
            }
        }


        private void btnTrangChu_Click(object sender, EventArgs e)
        {
            container(new Dashboard());
        }



        private void container(object _form)
        {
            if (guna2Panel1_container.Controls.Count > 0) guna2Panel1_container.Controls.Clear();

            Form fm = _form as Form;
            fm.TopLevel = false;
            fm.FormBorderStyle = FormBorderStyle.None;
            fm.Dock = DockStyle.Fill;
            guna2Panel1_container.Controls.Add(fm);
            guna2Panel1_container.Tag = fm;
            fm.Show();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // Nếu người dùng chọn Yes, thực hiện đăng xuất
            if (result == DialogResult.Yes)
            {
                // Ẩn form hiện tại
                this.Hide();

                // Mở lại form đăng nhập
                DangNhap loginForm = new DangNhap();
                loginForm.ShowDialog(); // Sử dụng ShowDialog để dừng luồng đến khi form đăng nhập đóng

            }
        }

        private void btnKhachHang_Click(object sender, EventArgs e)
        {
            container(new KhachHang());
        }

        private void TrangChu_Load(object sender, EventArgs e)
        {
            lblTenKhachHang.Text = "Xin chào, " + tenKhachHang + "!";
            lblTenNhanVien.Text = "Xin chào," + tenNhanVien + "!";

            timer1.Start();
            timer1.Tick += Timer1_Tick;

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            lblClock.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
        }

        private void btn_GhiDien_Click(object sender, EventArgs e)
        {
            container(new GhiDien(maNhanVien, tenNhanVien));
        }

        private void btnHeThongDien_Click(object sender, EventArgs e)
        {
            container(new HeThongDien());
        }

        private void guna2Panel1_container_Paint(object sender, PaintEventArgs e)
        {

        }



        private void btnNhanVien_Click(object sender, EventArgs e)
        {
            container(new NhanVien());
        }

        private void btnPhanCong_Click(object sender, EventArgs e)
        {
            container(new PhanCong(maNhanVien));
        }

        private void btnXuLyTreHan_Click(object sender, EventArgs e)
        {
            container(new XuLyTreHan(maNhanVien));
        }

        private void btnXuLySuCo_Click(object sender, EventArgs e)
        {
            container(new XuLySuCo());
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {

                Application.Exit();
            }
        }

        private void btnKiemDinh_Click(object sender, EventArgs e)
        {
            container(new KiemDinh(maNhanVien));
        }

        private void btnHoaDon_Click(object sender, EventArgs e)
        {
            container(new HoaDon(maNhanVien));
        }
    }


}
