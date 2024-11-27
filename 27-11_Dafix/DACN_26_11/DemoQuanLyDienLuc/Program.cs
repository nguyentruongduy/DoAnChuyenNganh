using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoQuanLyDienLuc
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DangNhap loginForm = new DangNhap();

            // Hiện dialog đăng nhập và kiểm tra kết quả
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // Giả sử bạn đã thiết lập để lấy tên khách hàng và tên nhân viên từ form đăng nhập
                string tenKhachHang = loginForm.Tag?.ToString(); // Lấy tên khách hàng từ Tag
                string tenNhanVien = loginForm.NhanVienTag?.ToString();
                string maNhanVien = loginForm.NhanVienTag?.ToString();
                string chucVu = loginForm.NhanVienTag?.ToString();

                // Nếu tên khách hàng hoặc tên nhân viên không rỗng, mở form TrangChu
                if (!string.IsNullOrEmpty(tenKhachHang))
                {
                    Application.Run(new TrangChu(tenKhachHang, null,null,chucVu)); // Mở form chính với tên khách hàng
                }
                else if (!string.IsNullOrEmpty(tenNhanVien))
                {
                    Application.Run(new TrangChu(null, tenNhanVien,maNhanVien,chucVu)); // Mở form chính với tên nhân viên
                }
                else
                {
                    MessageBox.Show("Không tìm thấy thông tin người dùng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Nếu người dùng hủy bỏ đăng nhập
                Application.Exit(); // Đóng ứng dụng
            }

        }

    }
}





//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace DemoQuanLyDienLuc
//{
//    static class Program
//    {
//        /// <summary>
//        /// The main entry point for the application.
//        /// </summary>
//        [STAThread]
//        static void Main()
//        {
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);
//            Application.Run(new GhiDien());
//        }

//    }
//}
