using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisaGetDataCloud.Models
{
    public class Root
    {
        public List<Result> result { get; set; }
    }

    public class HangHoaDichVu
    {
        public string IdChiDinh { get; set; }
        public string IdDonThuoc { get; set; }
        public double STT { get; set; }
        public string TenDichVu { get; set; }
        public string TenLoaiDoiTuongThu { get; set; }
        public string DonVi { get; set; }
        public double SoLuong { get; set; }
        public double DonGiaHoaDon { get; set; }
        public double ThanhTien { get; set; }
        public double TienBNTT { get; set; }
        public double TienBNCCT { get; set; }
        public double PhuThu { get; set; }
    }

    public class Result
    {
        public string Id { get; set; }
        public int status { get; set; }
        public string Hoten { get; set; }
        public string DiaChi { get; set; }
        public string MST { get; set; }
        public string NgayHoaDon { get; set; }
        public string MaKhachHang { get; set; }
        public int HinhThucTToan { get; set; }
        public string SoTienBangChu { get; set; }
        public string NguoiNopTien { get; set; }
        public string TenDonVi { get; set; }
        public string GioiTinh { get; set; }
        public string Tuoi { get; set; }
        public string TenKhoaKham { get; set; }
        public string TaiKhoanThu { get; set; }
        public string TenNguoiThu { get; set; }
        public string TenPhongKham { get; set; }
        public List<HangHoaDichVu> HangHoaDichVu { get; set; }
        public int LoaiPhieuThu { get; set; }
        public int NgayTao { get; set; }
        public double SoTien { get; set; }
        public int PhanTramBaoHiemThanhToan { get; set; }
    }

    public class UpdateStatusHSDT
    {
        public List<string> id { get; set; }
        public int status { get; set; }
    }

    public class SendInvoice
    {
        public string id { get; set; }
        public SendInvoiceDetail result { get; set; }
    }

    public class SendInvoiceDetail
    {
        public string fileUrl { get; set; }
        public string sohoadon { get; set; }
    }
}
