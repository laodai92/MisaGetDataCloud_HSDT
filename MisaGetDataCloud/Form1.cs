using CQDT.CloudClient;
using Microsoft.Win32;
using MISA.MeInvoice.DS.Contract.Entity;
using MISA.MeInvoice.DS.Lib;
using MisaGetDataCloud.Models;
using MisaGetDataCloud.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Invoice = MISA.MeInvoice.DS.Lib.Invoice;
using InvoiceDetail = MISA.MeInvoice.DS.Lib.InvoiceDetail;

namespace MisaGetDataCloud
{
    public partial class MisaGetData : Form
    {
        readonly RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        private readonly string strValidate = "<{0}> không được bổ trống.";
        private const string mscServerDesktopName = "ServerDesktopName";
        private readonly List<string> DoiTuongXuLys = new List<string>();
        private int Selected = 0;

        /// <summary>
        /// Khởi tạo
        /// </summary>
        public MisaGetData()
        {
            InitializeComponent();
            RegisterInStartup(cbStartUp.Checked);
            CheckStartup();

            DoiTuongXuLys = new List<string>()
            {
                "Gộp theo BNCCT và viện phí nhân dân",
                "Gộp theo đối tượng thu",
                "Gộp theo đối tượng thu + BNCCT",
                "Gộp theo đối tượng thu + BNCCT - Không phụ thu",
                "Gộp theo đối tượng thu + BNCCT - Chi tiết khám bệnh",
                "Gộp theo đối tượng thu + BNCCT + Detail BNTT + NotPT",
                "Không gộp",
            };

            cbbDoiTuongXL.DataSource = DoiTuongXuLys;

            if (registryKey.GetValue("MisaGetData") == null)
            {
                cbStartUp.Checked = false;
            }
            else
            {
                cbStartUp.Checked = true;
            }
        }

        /// <summary>
        /// Get data HIS
        /// </summary>
        /// <returns></returns>
        public async Task GetDataHIS()
        {
            try
            {
                var checkinternet = Dns.GetHostEntry("https:");

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                await PubSubHelper.Initialize(AppSetting.GetAppSettingWithDefaultValue("Token"));

                PubSubHelper.Subcribe($"phieuthu/pending",
                       async (sub, msgToken) =>
                       {
                           Root root = await $"misa-hdvlist?msgToken={msgToken}".GetAsJson<Root>(AppSetting.GetAppSettingWithDefaultValue("Token"));

                           if (root != null && root.result.Count > 0)
                           {
                               File.AppendAllText(txtLinkSaveFile.Text + "\\result_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt", SerializeUtil.SerializeObject(root));
                               Writelog.WriteLogInfo("file log:" + txtLinkSaveFile.Text + "\\result_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt");
                               ProcessingData(root);
                           }

                           return true;
                       },
                       (subErr) =>
                       {
                           return true;
                       });


            }
            catch (UnauthorizedAccessException)
            {
                Writelog.WriteLogError("Không có quyền truy cập hoặc không đúng giấy phép");
            }
            catch (HttpRequestException httpex)
            {
                Writelog.WriteLogError(httpex.Message);
            }
        }

        /// <summary>
        /// Xử lý dữ liệu
        /// </summary>
        /// <param name="rs"></param>
        private void ProcessingData(Root rs)
        {
            try
            {
                string sSeverDesktop = AppSetting.GetAppSettingWithDefaultValue("SeverDesktop");
                var lstID = new List<string>();
                List<Invoice> lstInvoice = new List<Invoice>();
                List<InvoiceDetail> lstDetail = new List<InvoiceDetail>();
                if (rs != null && rs.result != null)
                {
                    if (rs.result.Count > 0)
                    {
                        foreach (var item in rs.result)
                        {
                            var data = CommonFunction.GetDataByRefIDNotPublish(item.Id, txtMST.Text);
                            if (data != null && data.Rows != null && data.Rows.Count > 0)
                            {
                                Writelog.WriteLogInfo("Mã BN đã được kéo về Meinvoice: " + item.MaKhachHang);
                                continue;
                            }

                            // id của phiếu thu
                            lstID.Add(item.Id);
                            Invoice oInvoice = new Invoice();
                            oInvoice.RefID = item.Id;
                            oInvoice.IsInvoiceWithCode = false;
                            CultureInfo provider = CultureInfo.InvariantCulture;
                            string invDate = item.NgayHoaDon;
                            DateTime dateTime = DateTime.ParseExact(invDate, "dd/MM/yyyy", provider);
                            string dateFormat = dateTime.ToString("yyyy-MM-dd");
                            oInvoice.InvDate = DateTime.ParseExact(dateFormat, "yyyy-MM-dd", provider);
                            oInvoice.RefType = 1;
                            oInvoice.AccountObjectAddress = item.DiaChi;
                            oInvoice.ContactName = item.Hoten;
                            oInvoice.AccountObjectName = item.TenDonVi;
                            oInvoice.AccountObjectTaxCode = item.MST;
                            oInvoice.AccountObjectCode = item.MaKhachHang;
                            Writelog.WriteLogInfo("thực hiện đẩy Mã khách hàng : " + oInvoice.AccountObjectCode);
                            // Hình thức thanh toán
                            if (item.HinhThucTToan == 0)
                            {
                                oInvoice.PaymentMethod = "Tiền mặt";
                            }
                            else
                            {
                                oInvoice.PaymentMethod = "Chuyển khoản";
                            }

                            if (item.LoaiPhieuThu == 1)
                            {
                                oInvoice.CustomInfo5 = "Nội trú";
                            }
                            else if (item.LoaiPhieuThu == 0)
                            {
                                oInvoice.CustomInfo5 = "Ngoại trú";
                            }

                            oInvoice.CurrencyID = "VND";
                            oInvoice.ExchangeRate = 1.0;
                            oInvoice.CreatedDate = DateTime.Now;

                            var hanghoadichvus = item.HangHoaDichVu;
                            double totalAmount = 0;

                            // kiểm tra theo đơn vị BV Chương Mỹ
                            // lấy thông tin để tạo ra 2 dòng hàng hóa gồm 1. Viện phí nhân dân, 2. Bệnh nhân cùng chi trả.
                            // Xử lý hàng hóa, dịch vụ gộp theo BH cùng chi trả và viện phí nhân dân
                            if (Selected == 0)
                            {
                                // đọc thông tin hàng hóa dựa vào số tiền bn cùng chi trả và viện phí nhân dân
                                double TotalTienBNTT = 0;
                                double TotalTienBNCCT = 0;
                                if (hanghoadichvus != null && hanghoadichvus.Count > 0)
                                {
                                   
                                    foreach (var dv in hanghoadichvus)
                                    {
                                        TotalTienBNTT = TotalTienBNTT + (dv.TienBNTT * dv.SoLuong) ;
                                        TotalTienBNCCT = TotalTienBNCCT + (dv.TienBNCCT * dv.SoLuong);
                                    }
                                    // kiểm tra thêm dòng viện phí nhân dân
                                    if (TotalTienBNTT > 0)
                                    {
                                        InvoiceDetail invoiceDetail = new InvoiceDetail();
                                        invoiceDetail.RefDetailID = Guid.NewGuid().ToString();
                                        invoiceDetail.RefID = item.Id;
                                        invoiceDetail.SortOrder = 1;
                                        invoiceDetail.Description = "Viện phí nhân dân";
                                        invoiceDetail.UnitName = "Lần";
                                        invoiceDetail.UnitPrice = TotalTienBNTT;
                                        invoiceDetail.Quantity = 1;
                                        invoiceDetail.Amount = TotalTienBNTT;
                                        invoiceDetail.AmountOC = TotalTienBNTT;
                                        lstDetail.Add(invoiceDetail);
                                    }
                                    // kiểm tra thêm dòng bệnh nhân cùng chi trả
                                    if (TotalTienBNCCT > 0)
                                    {
                                        var invoiceDetail = new InvoiceDetail();
                                        invoiceDetail.RefDetailID = Guid.NewGuid().ToString();
                                        invoiceDetail.RefID = item.Id;
                                        if (TotalTienBNTT > 0)
                                        {
                                            invoiceDetail.SortOrder = 2;
                                        }
                                        else
                                        {
                                            invoiceDetail.SortOrder = 1;
                                        }
                                        invoiceDetail.SortOrder = 1;
                                        invoiceDetail.Description = "Bệnh nhân cùng chi trả: " + (100 - item.PhanTramBaoHiemThanhToan).ToString() + "%";
                                        invoiceDetail.UnitName = "Lần";
                                        invoiceDetail.UnitPrice = TotalTienBNCCT;
                                        invoiceDetail.Quantity = 1;
                                        invoiceDetail.Amount = TotalTienBNCCT;
                                        invoiceDetail.AmountOC = TotalTienBNCCT;
                                        lstDetail.Add(invoiceDetail);
                                    }
                                    // thêm tổng tiền ở các hàng detail để check ở cuối khi thêm vào
                                    totalAmount = TotalTienBNTT + TotalTienBNCCT;
                                    // thêm các trường mở rộng của đơn vị
                                    oInvoice.CustomInfo6 = item.TenPhongKham;
                                    oInvoice.CustomInfo4 = item.GioiTinh;
                                    oInvoice.CustomInfo7 = item.Tuoi;
                                    oInvoice.CustomInfo3 = item.TenPhongKham;
                                    oInvoice.CustomInfo1 = item.TenNguoiThu;

                                    //Writelog.WriteLogInfo("Tên Khoa Khám : " + item.TenPhongKham);
                                }
                            }
                            // Xử lý hàng hóa, dịch vụ cho BV bằng đối tượng thu - HSDT
                            else if (Selected == 1)
                            {
                                var dtDetailDoiTuongThu = new List<InvoiceDetail>();
                                // đọc thông tin hàng hóa dịch vụ dựa vào đối tượng thu mà his đẩy qua
                                foreach (var dv in hanghoadichvus)
                                {
                                    if (dtDetailDoiTuongThu.Count > 0)
                                    {
                                        bool checklist = true;
                                        foreach (var dt in dtDetailDoiTuongThu)
                                        {
                                            // kiểm tra đã tồn tại loại đối tượng trước đó
                                            if (dv.TenLoaiDoiTuongThu.Equals(dt.Description))
                                            {
                                                dt.UnitPrice += dv.ThanhTien;
                                                dt.Amount += dv.ThanhTien;
                                                dt.AmountOC += dv.ThanhTien;
                                                checklist = false;
                                            }
                                        }
                                        if (checklist)
                                        {
                                            var invoiceDetail = new InvoiceDetail();
                                            invoiceDetail.RefDetailID = Guid.NewGuid().ToString();
                                            invoiceDetail.RefID = item.Id;
                                            invoiceDetail.SortOrder = dtDetailDoiTuongThu.Count;
                                            invoiceDetail.Description = dv.TenLoaiDoiTuongThu;
                                            invoiceDetail.UnitName = "Lần";
                                            invoiceDetail.UnitPrice = dv.ThanhTien;
                                            invoiceDetail.Quantity = 1;
                                            invoiceDetail.Amount = dv.ThanhTien;
                                            invoiceDetail.AmountOC = dv.ThanhTien;
                                            dtDetailDoiTuongThu.Add(invoiceDetail);
                                        }
                                    }
                                    else
                                    {
                                        var invoiceDetail = new InvoiceDetail();
                                        invoiceDetail.RefDetailID = Guid.NewGuid().ToString();
                                        invoiceDetail.RefID = item.Id;
                                        invoiceDetail.SortOrder = dtDetailDoiTuongThu.Count;
                                        invoiceDetail.Description = dv.TenLoaiDoiTuongThu;
                                        invoiceDetail.UnitName = "Lần";
                                        invoiceDetail.UnitPrice = dv.ThanhTien;
                                        invoiceDetail.Quantity = 1;
                                        invoiceDetail.Amount = dv.ThanhTien;
                                        invoiceDetail.AmountOC = dv.ThanhTien;
                                        dtDetailDoiTuongThu.Add(invoiceDetail);
                                    }
                                }

                                foreach (var dt in dtDetailDoiTuongThu)
                                {
                                    var invoiceDetail = new InvoiceDetail();
                                    invoiceDetail.RefDetailID = dt.RefDetailID;
                                    invoiceDetail.RefID = dt.RefID;
                                    invoiceDetail.SortOrder = dt.SortOrder;
                                    invoiceDetail.Description = dt.Description;
                                    invoiceDetail.UnitName = dt.UnitName;
                                    invoiceDetail.UnitPrice = dt.UnitPrice;
                                    invoiceDetail.Quantity = dt.Quantity;
                                    invoiceDetail.Amount = dt.Amount;
                                    invoiceDetail.AmountOC = dt.AmountOC;
                                    lstDetail.Add(invoiceDetail);
                                    totalAmount += dt.AmountOC;
                                }

                                // thêm các trường mở rộng của đơn vị
                                oInvoice.CustomInfo6 = item.TenPhongKham;
                                oInvoice.CustomInfo4 = item.GioiTinh;
                                oInvoice.CustomInfo7 = item.Tuoi;
                                oInvoice.CustomInfo3 = item.TenPhongKham;
                                oInvoice.CustomInfo1 = item.TenNguoiThu;

                                //Writelog.WriteLogInfo("Tên Khoa Khám : " + item.TenPhongKham);
                            }
                            // Xử lý hàng hóa, dịch vụ cho BV bằng đối tượng thu + BNCCT - HSDT
                            else if (Selected == 2)
                            {
                                //đọc thông tin hàng hóa dịch vụ dựa vào đối tượng thu mà his đẩy qua
                                double TotalTienBNCCT = 0;
                                var dtDetailDoiTuongThu = new List<InvoiceDetail>();
                                foreach (var dv in hanghoadichvus)
                                {
                                    if (dv.TienBNCCT > 0)
                                    {
                                        TotalTienBNCCT = TotalTienBNCCT + (dv.TienBNCCT * dv.SoLuong);

                                        if (dv.TienBNTT > 0)
                                        {
                                            if (dtDetailDoiTuongThu.Count > 0)
                                            {
                                                bool Checklist = true;
                                                foreach (var dt in dtDetailDoiTuongThu)
                                                {
                                                    if (dv.TenLoaiDoiTuongThu.Equals(dt.Description))
                                                    {
                                                        dt.UnitPrice += (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                        dt.Amount += (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                        dt.AmountOC += (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                        Checklist = false;
                                                    }
                                                }
                                                if (Checklist)
                                                {
                                                    var detail = new InvoiceDetail();
                                                    detail.RefDetailID = Guid.NewGuid().ToString();
                                                    detail.RefID = item.Id;
                                                    detail.SortOrder = dtDetailDoiTuongThu.Count;
                                                    detail.Description = dv.TenLoaiDoiTuongThu;
                                                    detail.UnitName = "Lần";
                                                    detail.UnitPrice = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                    detail.Quantity = 1;
                                                    detail.Amount = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                    detail.AmountOC = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                    dtDetailDoiTuongThu.Add(detail);
                                                }
                                            }
                                            else
                                            {
                                                var detail = new InvoiceDetail();
                                                detail.RefDetailID = Guid.NewGuid().ToString();
                                                detail.RefID = item.Id;
                                                detail.SortOrder = 1;
                                                detail.Description = dv.TenLoaiDoiTuongThu;
                                                detail.UnitName = "Lần";
                                                detail.UnitPrice = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                detail.Quantity = 1;
                                                detail.Amount = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                detail.AmountOC = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                dtDetailDoiTuongThu.Add(detail);
                                            }
                                        }
                                        else if (dv.PhuThu > 0)
                                        {
                                            if (dtDetailDoiTuongThu.Count > 0)
                                            {
                                                bool Checklist = true;
                                                foreach (var dt in dtDetailDoiTuongThu)
                                                {
                                                    if (dv.TenLoaiDoiTuongThu.Equals(dt.Description))
                                                    {
                                                        dt.UnitPrice += (dv.PhuThu * dv.SoLuong);
                                                        dt.Amount += (dv.PhuThu * dv.SoLuong);
                                                        dt.AmountOC += (dv.PhuThu * dv.SoLuong);
                                                        Checklist = false;
                                                    }
                                                }
                                                if (Checklist)
                                                {
                                                    var detail = new InvoiceDetail();
                                                    detail.RefDetailID = Guid.NewGuid().ToString();
                                                    detail.RefID = item.Id;
                                                    detail.SortOrder = dtDetailDoiTuongThu.Count;
                                                    detail.Description = dv.TenLoaiDoiTuongThu;
                                                    detail.UnitName = "Lần";
                                                    detail.UnitPrice = (dv.PhuThu * dv.SoLuong);
                                                    detail.Quantity = 1;
                                                    detail.Amount = (dv.PhuThu * dv.SoLuong);
                                                    detail.AmountOC = (dv.PhuThu * dv.SoLuong);
                                                    dtDetailDoiTuongThu.Add(detail);
                                                }
                                            }
                                            else
                                            {
                                                var detail = new InvoiceDetail();
                                                detail.RefDetailID = Guid.NewGuid().ToString();
                                                detail.RefID = item.Id;
                                                detail.SortOrder = 1;
                                                detail.Description = dv.TenLoaiDoiTuongThu;
                                                detail.UnitName = "Lần";
                                                detail.UnitPrice = (dv.PhuThu * dv.SoLuong);
                                                detail.Quantity = 1;
                                                detail.Amount = (dv.PhuThu * dv.SoLuong);
                                                detail.AmountOC = (dv.PhuThu * dv.SoLuong);
                                                dtDetailDoiTuongThu.Add(detail);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (dtDetailDoiTuongThu.Count > 0)
                                        {
                                            bool Checklist = true;
                                            foreach (var dt in dtDetailDoiTuongThu)
                                            {
                                                if (dv.TenLoaiDoiTuongThu.Equals(dt.Description))
                                                {
                                                    dt.UnitPrice += dv.ThanhTien;
                                                    dt.Amount += dv.ThanhTien;
                                                    dt.AmountOC += dv.ThanhTien;
                                                    Checklist = false;
                                                }
                                            }
                                            if (Checklist)
                                            {
                                                var dichvu = new InvoiceDetail();
                                                dichvu.RefDetailID = Guid.NewGuid().ToString();
                                                dichvu.RefID = item.Id;
                                                dichvu.SortOrder = dtDetailDoiTuongThu.Count;
                                                dichvu.Description = dv.TenLoaiDoiTuongThu;
                                                dichvu.UnitName = "Lần";
                                                dichvu.UnitPrice = dv.ThanhTien;
                                                dichvu.Quantity = 1;
                                                dichvu.Amount = dv.ThanhTien;
                                                dichvu.AmountOC = dv.ThanhTien;
                                                dtDetailDoiTuongThu.Add(dichvu);
                                            }
                                        }
                                        else
                                        {
                                            var detail = new InvoiceDetail();
                                            detail.RefDetailID = Guid.NewGuid().ToString();
                                            detail.RefID = item.Id;
                                            detail.SortOrder = 1;
                                            detail.Description = dv.TenLoaiDoiTuongThu;
                                            detail.UnitName = "Lần";
                                            detail.UnitPrice = dv.ThanhTien;
                                            detail.Quantity = 1;
                                            detail.Amount = dv.ThanhTien;
                                            detail.AmountOC = dv.ThanhTien;
                                            dtDetailDoiTuongThu.Add(detail);
                                        }
                                    }
                                }

                                if (TotalTienBNCCT > 0)
                                {
                                    var detail = new InvoiceDetail();
                                    detail.RefDetailID = Guid.NewGuid().ToString();
                                    detail.RefID = item.Id;
                                    detail.SortOrder = 1;
                                    detail.Description = "Bệnh nhân cùng chi trả: " + (100 - item.PhanTramBaoHiemThanhToan).ToString() + "%";
                                    detail.UnitName = "Lần";
                                    detail.UnitPrice = TotalTienBNCCT;
                                    detail.Quantity = 1;
                                    detail.Amount = TotalTienBNCCT;
                                    detail.AmountOC = TotalTienBNCCT;
                                    totalAmount += TotalTienBNCCT;
                                    lstDetail.Add(detail);
                                }

                                foreach (var dt in dtDetailDoiTuongThu)
                                {
                                    var detail = new InvoiceDetail();
                                    detail.RefDetailID = dt.RefDetailID;
                                    detail.RefID = dt.RefID;
                                    detail.SortOrder = dt.SortOrder;
                                    detail.Description = dt.Description;
                                    detail.UnitName = dt.UnitName;
                                    detail.UnitPrice = dt.UnitPrice;
                                    detail.Quantity = dt.Quantity;
                                    detail.Amount = dt.Amount;
                                    detail.AmountOC = dt.AmountOC;
                                    lstDetail.Add(detail);
                                    totalAmount += dt.AmountOC;
                                }

                                // thêm các trường mở rộng của đơn vị
                                oInvoice.CustomInfo6 = item.TenPhongKham;
                                oInvoice.CustomInfo4 = item.GioiTinh;
                                oInvoice.CustomInfo3 = item.Tuoi;
                                oInvoice.CustomInfo2 = item.TenPhongKham;
                                oInvoice.CustomInfo1 = item.TenNguoiThu;

                                //Writelog.WriteLogInfo("Tên Khoa Khám : " + item.TenPhongKham);
                            }
                            // Xử lý hàng hóa, dịch vụ cho BV bằng đối tượng thu + BNCCT - HSDT - không phụ thu
                            else if (Selected == 3)
                            {
                                // đọc thông tin hàng hóa dịch vụ dựa vào đối tượng thu mà his đẩy qua
                                double TotalTienBNCCT = 0;
                                var dtDetailDoiTuongThu = new List<InvoiceDetail>();
                                foreach (var dv in hanghoadichvus)
                                {
                                    if (dv.TienBNCCT > 0)
                                    {
                                        TotalTienBNCCT = TotalTienBNCCT + (dv.TienBNCCT * dv.SoLuong);
                                        if (dv.TienBNTT > 0)
                                        {
                                            if (dtDetailDoiTuongThu.Count > 0)
                                            {
                                                bool Checklist = true;
                                                foreach (var dt in dtDetailDoiTuongThu)
                                                {
                                                    if (dv.TenLoaiDoiTuongThu.Equals(dt.Description))
                                                    {
                                                        dt.UnitPrice += (dv.TienBNTT * dv.SoLuong);
                                                        dt.Amount += (dv.TienBNTT * dv.SoLuong);
                                                        dt.AmountOC += (dv.TienBNTT * dv.SoLuong);
                                                        Checklist = false;
                                                    }
                                                }
                                                if (Checklist)
                                                {
                                                    var detail = new InvoiceDetail();
                                                    detail.RefDetailID = Guid.NewGuid().ToString();
                                                    detail.RefID = item.Id;
                                                    detail.SortOrder = dtDetailDoiTuongThu.Count;
                                                    detail.Description = dv.TenLoaiDoiTuongThu;
                                                    detail.UnitName = "Lần";
                                                    detail.UnitPrice = (dv.TienBNTT * dv.SoLuong);
                                                    detail.Quantity = 1;
                                                    detail.Amount = (dv.TienBNTT * dv.SoLuong);
                                                    detail.AmountOC = (dv.TienBNTT * dv.SoLuong);
                                                    dtDetailDoiTuongThu.Add(detail);
                                                }
                                            }
                                            else
                                            {
                                                var detail = new InvoiceDetail();
                                                detail.RefDetailID = Guid.NewGuid().ToString();
                                                detail.RefID = item.Id;
                                                detail.SortOrder = 1;
                                                detail.Description = dv.TenLoaiDoiTuongThu;
                                                detail.UnitName = "Lần";
                                                detail.UnitPrice = (dv.TienBNTT * dv.SoLuong);
                                                detail.Quantity = 1;
                                                detail.Amount = (dv.TienBNTT * dv.SoLuong);
                                                detail.AmountOC = (dv.TienBNTT * dv.SoLuong);
                                                dtDetailDoiTuongThu.Add(detail);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (dtDetailDoiTuongThu.Count > 0)
                                        {
                                            bool Checklist = true;
                                            foreach (var dt in dtDetailDoiTuongThu)
                                            {
                                                if (dv.TenLoaiDoiTuongThu.Equals(dt.Description))
                                                {
                                                    dt.UnitPrice += dv.ThanhTien;
                                                    dt.Amount += dv.ThanhTien;
                                                    dt.AmountOC += dv.ThanhTien;
                                                    Checklist = false;
                                                }
                                            }
                                            if (Checklist)
                                            {
                                                var detail = new InvoiceDetail();
                                                detail.RefDetailID = Guid.NewGuid().ToString();
                                                detail.RefID = item.Id;
                                                detail.SortOrder = dtDetailDoiTuongThu.Count;
                                                detail.Description = dv.TenLoaiDoiTuongThu;
                                                detail.UnitName = "Lần";
                                                detail.UnitPrice = dv.ThanhTien;
                                                detail.Quantity = 1;
                                                detail.Amount = dv.ThanhTien;
                                                detail.AmountOC = dv.ThanhTien;
                                                dtDetailDoiTuongThu.Add(detail);
                                            }
                                        }
                                        else
                                        {
                                            var dichvu = new InvoiceDetail();
                                            dichvu.RefDetailID = Guid.NewGuid().ToString();
                                            dichvu.RefID = item.Id;
                                            dichvu.SortOrder = 1;
                                            dichvu.Description = dv.TenLoaiDoiTuongThu;
                                            dichvu.UnitName = "Lần";
                                            dichvu.UnitPrice = dv.ThanhTien;
                                            dichvu.Quantity = 1;
                                            dichvu.Amount = dv.ThanhTien;
                                            dichvu.AmountOC = dv.ThanhTien;
                                            dtDetailDoiTuongThu.Add(dichvu);
                                        }
                                    }
                                }

                                if (TotalTienBNCCT > 0)
                                {
                                    var dichvu = new InvoiceDetail();
                                    dichvu.RefDetailID = Guid.NewGuid().ToString();
                                    dichvu.RefID = item.Id;
                                    dichvu.SortOrder = 1;
                                    dichvu.Description = "Bệnh nhân cùng chi trả: " + (100 - item.PhanTramBaoHiemThanhToan).ToString() + "%";
                                    dichvu.UnitName = "Lần";
                                    dichvu.UnitPrice = TotalTienBNCCT;
                                    dichvu.Quantity = 1;
                                    dichvu.Amount = TotalTienBNCCT;
                                    dichvu.AmountOC = TotalTienBNCCT;
                                    totalAmount += TotalTienBNCCT;
                                    lstDetail.Add(dichvu);
                                }

                                foreach (var dt in dtDetailDoiTuongThu)
                                {
                                    var dichvu = new InvoiceDetail();
                                    dichvu.RefDetailID = dt.RefDetailID;
                                    dichvu.RefID = dt.RefID;
                                    dichvu.SortOrder = dt.SortOrder;
                                    dichvu.Description = dt.Description;
                                    dichvu.UnitName = dt.UnitName;
                                    dichvu.UnitPrice = dt.UnitPrice;
                                    dichvu.Quantity = dt.Quantity;
                                    dichvu.Amount = dt.Amount;
                                    dichvu.AmountOC = dt.AmountOC;
                                    lstDetail.Add(dichvu);
                                    totalAmount += dt.AmountOC;
                                }

                                // thêm các trường mở rộng của đơn vị
                                oInvoice.CustomInfo6 = item.TenPhongKham;
                                oInvoice.CustomInfo4 = item.GioiTinh;
                                oInvoice.CustomInfo3 = item.Tuoi;
                                oInvoice.CustomInfo2 = item.TenPhongKham;
                                oInvoice.CustomInfo1 = item.TenNguoiThu;

                                //Writelog.WriteLogInfo("Tên Khoa Khám : " + item.TenPhongKham);
                            }
                            // Xử lý hàng hóa, dịch vụ cho BV bằng đối tượng thu + BNCCT - HSDT - DetailKhamBenh
                            else if (Selected == 4)
                            {
                                //  đọc thông tin hàng hóa dịch vụ dựa vào đối tượng thu mà his đẩy qua 
                                double TotalTienBNCCT = 0;
                                var dtDetailDoiTuongThu = new List<InvoiceDetail>();
                                foreach (var dv in hanghoadichvus)
                                {
                                    if (dv.TenLoaiDoiTuongThu.Equals("khám bệnh"))
                                    {
                                        var detail = new InvoiceDetail();
                                        detail.RefDetailID = Guid.NewGuid().ToString();
                                        detail.RefID = item.Id;
                                        detail.SortOrder = 1;
                                        detail.Description = dv.TenDichVu;
                                        detail.UnitName = "Lần";
                                        detail.UnitPrice = dv.ThanhTien;
                                        detail.Quantity = 1;
                                        detail.Amount = dv.ThanhTien;
                                        detail.AmountOC = dv.ThanhTien;
                                        totalAmount += dv.ThanhTien;
                                        lstDetail.Add(detail);
                                    }
                                    else if (dv.TienBNCCT > 0)
                                    {
                                        TotalTienBNCCT = TotalTienBNCCT + (dv.TienBNCCT * dv.SoLuong);
                                        if (dv.TienBNTT > 0)
                                        {
                                            if (dtDetailDoiTuongThu.Count > 0)
                                            {
                                                bool Checklist = true;
                                                foreach (var dt in dtDetailDoiTuongThu)
                                                {
                                                    dt.UnitPrice += (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                    dt.Amount += (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                    dt.AmountOC += (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                    Checklist = false;
                                                }
                                                if (Checklist)
                                                {
                                                    var detail = new InvoiceDetail();
                                                    detail.RefDetailID = Guid.NewGuid().ToString();
                                                    detail.RefID = item.Id;
                                                    detail.SortOrder = dtDetailDoiTuongThu.Count;
                                                    detail.Description = dv.TenLoaiDoiTuongThu;
                                                    detail.UnitName = "Lần";
                                                    detail.UnitPrice = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                    detail.Quantity = 1;
                                                    detail.Amount = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                    detail.AmountOC = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                    dtDetailDoiTuongThu.Add(detail);
                                                }
                                            }
                                            else
                                            {
                                                var dichvu = new InvoiceDetail();
                                                dichvu.RefDetailID = Guid.NewGuid().ToString();
                                                dichvu.RefID = item.Id;
                                                dichvu.SortOrder = 1;
                                                dichvu.Description = dv.TenLoaiDoiTuongThu;
                                                dichvu.UnitName = "Lần";
                                                dichvu.UnitPrice = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                dichvu.Quantity = 1;
                                                dichvu.Amount = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                dichvu.AmountOC = (dv.TienBNTT * dv.SoLuong) + (dv.PhuThu * dv.SoLuong);
                                                dtDetailDoiTuongThu.Add(dichvu);
                                            }
                                        }
                                    }
                                    else if (dv.PhuThu > 0)
                                    {
                                        if (dtDetailDoiTuongThu.Count > 0)
                                        {
                                            bool Checklist = true;
                                            foreach (var dt in dtDetailDoiTuongThu)
                                            {
                                                dt.UnitPrice += (dv.PhuThu * dv.SoLuong);
                                                dt.Amount += (dv.PhuThu * dv.SoLuong);
                                                dt.AmountOC += (dv.PhuThu * dv.SoLuong);
                                                Checklist = false;
                                            }
                                            if (Checklist)
                                            {
                                                var detail = new InvoiceDetail();
                                                detail.RefDetailID = Guid.NewGuid().ToString();
                                                detail.RefID = item.Id;
                                                detail.SortOrder = dtDetailDoiTuongThu.Count;
                                                detail.Description = dv.TenLoaiDoiTuongThu;
                                                detail.UnitName = "Lần";
                                                detail.UnitPrice = (dv.PhuThu * dv.SoLuong);
                                                detail.Quantity = 1;
                                                detail.Amount = (dv.PhuThu * dv.SoLuong);
                                                detail.AmountOC = (dv.PhuThu * dv.SoLuong);
                                                dtDetailDoiTuongThu.Add(detail);
                                            }
                                        }
                                        else
                                        {
                                            var dichvu = new InvoiceDetail();
                                            dichvu.RefDetailID = Guid.NewGuid().ToString();
                                            dichvu.RefID = item.Id;
                                            dichvu.SortOrder = 1;
                                            dichvu.Description = dv.TenLoaiDoiTuongThu;
                                            dichvu.UnitName = "Lần";
                                            dichvu.UnitPrice = (dv.PhuThu * dv.SoLuong);
                                            dichvu.Quantity = 1;
                                            dichvu.Amount = (dv.PhuThu * dv.SoLuong);
                                            dichvu.AmountOC = (dv.PhuThu * dv.SoLuong);
                                            dtDetailDoiTuongThu.Add(dichvu);
                                        }
                                    }
                                    else
                                    {
                                        if (dtDetailDoiTuongThu.Count > 0)
                                        {
                                            bool Checklist = true;
                                            foreach (var dt in dtDetailDoiTuongThu)
                                            {
                                                dt.UnitPrice += dv.ThanhTien;
                                                dt.Amount += dv.ThanhTien;
                                                dt.AmountOC += dv.ThanhTien;
                                                Checklist = false;
                                            }
                                            if (Checklist)
                                            {
                                                var detail = new InvoiceDetail();
                                                detail.RefDetailID = Guid.NewGuid().ToString();
                                                detail.RefID = item.Id;
                                                detail.SortOrder = dtDetailDoiTuongThu.Count;
                                                detail.Description = dv.TenLoaiDoiTuongThu;
                                                detail.UnitName = "Lần";
                                                detail.UnitPrice = dv.ThanhTien;
                                                detail.Quantity = 1;
                                                detail.Amount = dv.ThanhTien;
                                                detail.AmountOC = dv.ThanhTien;
                                                dtDetailDoiTuongThu.Add(detail);
                                            }
                                        }
                                        else
                                        {
                                            var detail = new InvoiceDetail();
                                            detail.RefDetailID = Guid.NewGuid().ToString();
                                            detail.RefID = item.Id;
                                            detail.SortOrder = 1;
                                            detail.Description = dv.TenLoaiDoiTuongThu;
                                            detail.UnitName = "Lần";
                                            detail.UnitPrice = dv.ThanhTien;
                                            detail.Quantity = 1;
                                            detail.Amount = dv.ThanhTien;
                                            detail.AmountOC = dv.ThanhTien;
                                            dtDetailDoiTuongThu.Add(detail);
                                        }
                                    }
                                }

                                if (TotalTienBNCCT > 0)
                                {
                                    var detail = new InvoiceDetail();
                                    detail.RefDetailID = Guid.NewGuid().ToString();
                                    detail.RefID = item.Id;
                                    detail.SortOrder = 1;
                                    detail.Description = "Bệnh nhân cùng chi trả: " + (100 - item.PhanTramBaoHiemThanhToan).ToString() + "%";
                                    detail.UnitName = "Lần";
                                    detail.UnitPrice = TotalTienBNCCT;
                                    detail.Quantity = 1;
                                    detail.Amount = TotalTienBNCCT;
                                    detail.AmountOC = TotalTienBNCCT;
                                    totalAmount += TotalTienBNCCT;
                                    lstDetail.Add(detail);
                                }

                                foreach (var dt in dtDetailDoiTuongThu)
                                {
                                    var detail = new InvoiceDetail();
                                    detail.RefDetailID = dt.RefDetailID;
                                    detail.RefID = dt.RefID;
                                    detail.SortOrder = dt.SortOrder;
                                    detail.Description = dt.Description;
                                    detail.UnitName = dt.UnitName;
                                    detail.UnitPrice = dt.UnitPrice;
                                    detail.Quantity = dt.Quantity;
                                    detail.Amount = dt.Amount;
                                    detail.AmountOC = dt.AmountOC;
                                    lstDetail.Add(detail);
                                    totalAmount += dt.AmountOC;
                                }

                                // thêm các trường mở rộng của đơn vị
                                oInvoice.CustomInfo6 = item.TenPhongKham;
                                oInvoice.CustomInfo4 = item.GioiTinh;
                                oInvoice.CustomInfo3 = item.Tuoi;
                                oInvoice.CustomInfo2 = item.TenPhongKham;
                                oInvoice.CustomInfo1 = item.TenNguoiThu;

                                //Writelog.WriteLogInfo("Tên Khoa Khám : " + item.TenPhongKham);
                            }
                            //Gộp theo đối tượng thu + BNCCT + Detail BNTT + Not PT
                            else if (Selected == 5)
                            {
                                // đọc thông tin hàng hóa dịch vụ dựa vào đối tượng thu mà his đẩy qua
                                double TotalTienBNCCT = 0;
                                var dtDetailDoiTuongThu = new List<InvoiceDetail>();
                                foreach (var dv in hanghoadichvus)
                                {
                                    if (dv.TienBNCCT > 0)
                                    {
                                        TotalTienBNCCT = TotalTienBNCCT + (dv.TienBNCCT * dv.SoLuong);

                                        if (dv.TienBNTT > 0)
                                        {
                                            if (dtDetailDoiTuongThu.Count > 0)
                                            {
                                                bool Checklist = true;
                                                foreach (var dt in dtDetailDoiTuongThu)
                                                {
                                                    if (dv.TenLoaiDoiTuongThu.Equals(dt.Description))
                                                    {
                                                        dt.UnitPrice += (dv.TienBNTT * dv.SoLuong);
                                                        dt.Amount += (dv.TienBNTT * dv.SoLuong);
                                                        dt.AmountOC += (dv.TienBNTT * dv.SoLuong);
                                                        Checklist = false;
                                                    }
                                                    if (dv.TenDichVu.Equals(dt.Description))
                                                    {
                                                        dt.Quantity += dv.SoLuong;
                                                        dt.Amount += (dv.TienBNTT * dv.SoLuong);
                                                        dt.AmountOC += (dv.TienBNTT * dv.SoLuong);
                                                        Checklist = false;
                                                    }
                                                }
                                                if (Checklist)
                                                {
                                                    var detail = new InvoiceDetail();
                                                    detail.RefDetailID = Guid.NewGuid().ToString();
                                                    detail.RefID = item.Id;
                                                    detail.SortOrder = dtDetailDoiTuongThu.Count;
                                                    if (dv.TenLoaiDoiTuongThu == "Vật tư" || dv.TenLoaiDoiTuongThu == "Thuốc")
                                                    {
                                                        detail.Description = dv.TenLoaiDoiTuongThu;
                                                        detail.UnitPrice = (dv.TienBNTT * dv.SoLuong);
                                                        detail.Quantity = 1;
                                                    }
                                                    else
                                                    {
                                                        detail.Description = dv.TenDichVu;
                                                        detail.UnitPrice = dv.TienBNTT;
                                                        detail.Quantity = dv.SoLuong;
                                                    }
                                                    detail.UnitName = "Lần";
                                                    detail.Amount = (dv.TienBNTT * dv.SoLuong);
                                                    detail.AmountOC = (dv.TienBNTT * dv.SoLuong);
                                                    dtDetailDoiTuongThu.Add(detail);
                                                }
                                            }
                                            else
                                            {
                                                var detail = new InvoiceDetail();
                                                detail.RefDetailID = Guid.NewGuid().ToString();
                                                detail.RefID = item.Id;
                                                detail.SortOrder = 1;
                                                if (dv.TenLoaiDoiTuongThu == "Vật tư" || dv.TenLoaiDoiTuongThu == "Thuốc")
                                                {
                                                    detail.Description = dv.TenLoaiDoiTuongThu;
                                                    detail.UnitPrice = (dv.TienBNTT * dv.SoLuong);
                                                    detail.Quantity = 1;
                                                }
                                                else
                                                {
                                                    detail.Description = dv.TenDichVu;
                                                    detail.UnitPrice = dv.TienBNTT;
                                                    detail.Quantity = dv.SoLuong;
                                                }
                                                detail.UnitName = "Lần";
                                                detail.Amount = (dv.TienBNTT * dv.SoLuong);
                                                detail.AmountOC = (dv.TienBNTT * dv.SoLuong);
                                                dtDetailDoiTuongThu.Add(detail);
                                            }
                                        }
                                    }
                                    else if (dv.ThanhTien > 0)
                                    {
                                        if (dtDetailDoiTuongThu.Count > 0)
                                        {
                                            bool Checklist = true;
                                            foreach (var dt in dtDetailDoiTuongThu)
                                            {
                                                if (dv.TenLoaiDoiTuongThu.Equals(dt.Description))
                                                {
                                                    dt.UnitPrice += dv.ThanhTien;
                                                    dt.Amount += dv.ThanhTien;
                                                    dt.AmountOC += dv.ThanhTien;
                                                    Checklist = false;
                                                }
                                                if (dv.TenDichVu.Equals(dt.Description))
                                                {
                                                    dt.Quantity += dv.SoLuong;
                                                    dt.Amount += (dv.TienBNTT * dv.SoLuong);
                                                    dt.AmountOC += (dv.TienBNTT * dv.SoLuong);
                                                    Checklist = false;
                                                }
                                            }
                                            if (Checklist)
                                            {
                                                var detail = new InvoiceDetail();
                                                detail.RefDetailID = Guid.NewGuid().ToString();
                                                detail.RefID = item.Id;
                                                detail.SortOrder = dtDetailDoiTuongThu.Count;
                                                if (dv.TenLoaiDoiTuongThu == "Vật tư" || dv.TenLoaiDoiTuongThu == "Thuốc")
                                                {
                                                    detail.Description = dv.TenLoaiDoiTuongThu;
                                                    detail.UnitPrice = (dv.TienBNTT * dv.SoLuong);
                                                    detail.Quantity = 1;
                                                }
                                                else
                                                {
                                                    detail.Description = dv.TenDichVu;
                                                    detail.UnitPrice = dv.TienBNTT;
                                                    detail.Quantity = dv.SoLuong;
                                                }
                                                detail.UnitName = "Lần";
                                                detail.Amount = dv.ThanhTien;
                                                detail.AmountOC = dv.ThanhTien;
                                                dtDetailDoiTuongThu.Add(detail);
                                            }
                                        }
                                        else
                                        {
                                            var dichvu = new InvoiceDetail();
                                            dichvu.RefDetailID = Guid.NewGuid().ToString();
                                            dichvu.RefID = item.Id;
                                            dichvu.SortOrder = 1;
                                            if (dv.TenLoaiDoiTuongThu == "Vật tư" || dv.TenLoaiDoiTuongThu == "Thuốc")
                                            {
                                                dichvu.Description = dv.TenLoaiDoiTuongThu;
                                            }
                                            else
                                            {
                                                dichvu.Description = dv.TenDichVu;
                                            }
                                            dichvu.UnitName = "Lần";
                                            dichvu.UnitPrice = dv.TienBNTT;
                                            dichvu.Quantity = dv.SoLuong;
                                            dichvu.Amount = dv.ThanhTien;
                                            dichvu.AmountOC = dv.ThanhTien;
                                            dtDetailDoiTuongThu.Add(dichvu);
                                        }
                                    }
                                }

                                if (TotalTienBNCCT > 0)
                                {
                                    var dichvu = new InvoiceDetail();
                                    dichvu.RefDetailID = Guid.NewGuid().ToString();
                                    dichvu.RefID = item.Id;
                                    dichvu.SortOrder = 1;
                                    dichvu.Description = "Bệnh nhân cùng chi trả: " + (100 - item.PhanTramBaoHiemThanhToan).ToString() + "%";
                                    dichvu.UnitName = "Lần";
                                    dichvu.UnitPrice = TotalTienBNCCT;
                                    dichvu.Quantity = 1;
                                    dichvu.Amount = TotalTienBNCCT;
                                    dichvu.AmountOC = TotalTienBNCCT;
                                    totalAmount += TotalTienBNCCT;
                                    lstDetail.Add(dichvu);
                                }

                                foreach (var dt in dtDetailDoiTuongThu)
                                {
                                    var dichvu = new InvoiceDetail();
                                    dichvu.RefDetailID = dt.RefDetailID;
                                    dichvu.RefID = dt.RefID;
                                    dichvu.SortOrder = dt.SortOrder;
                                    dichvu.Description = dt.Description;
                                    dichvu.UnitName = dt.UnitName;
                                    dichvu.UnitPrice = dt.UnitPrice;
                                    dichvu.Quantity = dt.Quantity;
                                    dichvu.Amount = dt.Amount;
                                    dichvu.AmountOC = dt.AmountOC;
                                    lstDetail.Add(dichvu);
                                    totalAmount += dt.AmountOC;
                                }

                                // thêm các trường mở rộng của đơn vị
                                oInvoice.CustomInfo6 = item.TenPhongKham;
                                oInvoice.CustomInfo4 = item.GioiTinh;
                                oInvoice.CustomInfo3 = item.Tuoi;
                                oInvoice.CustomInfo2 = item.TenPhongKham;
                                oInvoice.CustomInfo1 = item.TenNguoiThu;

                                //Writelog.WriteLogInfo("Tên Khoa Khám : " + item.TenPhongKham);
                            }
                            // Xử lý hàng hóa, dịch vụ lấy bảng kê của HIS qua
                            else
                            {
                                // đọc thông tin hàng hóa dịch vụ lấy nguyên bảng kê của his
                                foreach (var dv in hanghoadichvus)
                                {
                                    var detail = new InvoiceDetail();
                                    detail.RefDetailID = Guid.NewGuid().ToString();
                                    detail.RefID = item.Id;
                                    detail.SortOrder = dv.STT >= 0 ? (long)dv.STT : 0;
                                    detail.Description = dv.TenDichVu;
                                    detail.UnitName = dv.DonVi;
                                    detail.UnitPrice = dv.DonGiaHoaDon;
                                    detail.Quantity = dv.SoLuong;
                                    detail.Amount = dv.ThanhTien;
                                    detail.AmountOC = dv.ThanhTien;
                                    detail.DiscountAmountOC = 0;
                                    detail.DiscountAmount = 0;
                                    lstDetail.Add(detail);
                                    totalAmount += dv.ThanhTien;
                                }

                                // thêm các trường mở rộng của đơn vị
                                oInvoice.CustomInfo6 = item.TenPhongKham;
                                oInvoice.CustomInfo4 = item.GioiTinh;
                                oInvoice.CustomInfo3 = item.Tuoi;
                                oInvoice.CustomInfo2 = item.TenPhongKham;
                                oInvoice.CustomInfo1 = item.TenNguoiThu;
                            }

                            // thông tin tổng tiền
                            oInvoice.TotalSaleAmount = (decimal)item.SoTien;
                            oInvoice.TotalSaleAmountOC = (decimal)item.SoTien;
                            oInvoice.TotalAmount = item.SoTien;
                            oInvoice.TotalAmountOC = (decimal)item.SoTien;

                            // thông tin VAT và chiết khấu ở viện không áp dụng nên sẽ gán 0
                            oInvoice.TotalDiscountAmount = 0;
                            oInvoice.TotalDiscountAmountOC = 0;
                            oInvoice.TotalVATAmount = 0;
                            oInvoice.TotalVATAmountOC = 0;
                            oInvoice.CustomInfo10 = "1";

                            // Tắt thuế VAT = 8%
                            oInvoice.IsTaxReduction43 = false;

                            //Writelog.WriteLogError("Kiểm tra lệch tiền hay không");
                            // check lệch tiền hay không
                            if (Math.Abs(totalAmount) != Math.Abs(item.SoTien))
                            {
                                double ChenhLech = Math.Abs(totalAmount) - Math.Abs(item.SoTien);
                                if (Math.Abs(ChenhLech) < 100)
                                {
                                    if (lstDetail != null && lstDetail.Count > 0)
                                    {
                                        oInvoice.InvoiceDetail = lstDetail;
                                    }
                                    lstInvoice.Add(oInvoice);
                                }
                                else
                                {
                                    Writelog.WriteLogInfo("Lỗi lệch tiền: " + oInvoice.AccountObjectCode + " Lệch: " + ChenhLech.ToString());
                                }
                            }
                            else
                            {
                                if (lstDetail != null && lstDetail.Count > 0)
                                {
                                    oInvoice.InvoiceDetail = lstDetail;
                                }
                                lstInvoice.Add(oInvoice);
                            }
                            // kiểm tra nếu không có hàng detail thì xóa master đi
                            if (lstInvoice != null && lstInvoice.Count > 0)
                            {
                                foreach (var itemValDetail in lstInvoice)
                                {
                                    // kiểm tra detail có tồn tại
                                    if (lstDetail != null && lstDetail.Count > 0)
                                    {
                                        var details = lstDetail.Select(x => x.RefID.Equals(itemValDetail.RefID)).ToList();
                                        if (details == null || details.Count < 1)
                                        {
                                            lstInvoice.Remove(itemValDetail);
                                        }
                                    }
                                }
                            }
                            // Call API create EInvoice
                            if (lstInvoice != null && lstInvoice.Count > 0)
                            {
                                MeInvoiceRequest oMeInvoiceRequest = new MeInvoiceRequest(sSeverDesktop);
                                try
                                {
                                    var oResult = oMeInvoiceRequest.CreateInvoice(lstInvoice);

                                    if (oResult.Success && string.IsNullOrEmpty(oResult.ErrorCode))
                                    {
                                        Writelog.WriteLogInfo("đẩy thành công MBN: " + oInvoice.AccountObjectCode);
                                    }
                                    else
                                    {
                                        Writelog.WriteLogInfo("Đẩy lỗi MBN:" + oInvoice.AccountObjectCode + " Error code: " + oResult.ErrorCode);
                                    }
                                }
                                finally
                                {

                                }
                            }
                            else
                            {
                                Writelog.WriteLogError("Phiếu thu lỗi !!!");
                            }
                            lstInvoice.Clear();
                            lstDetail.Clear();
                        }






                    }
                }
            }
            catch (Exception ex)
            {
                Writelog.WriteLogError(ex.Message);
            }
        }

        /// <summary>
        /// Đăng ký khởi động cùng windows
        /// </summary>
        /// <param name="isChecked"></param>
        private void RegisterInStartup(bool isChecked)
        {
            if (isChecked)
            {
                // Đăng ký stratup cùng Windows
                registryKey.SetValue("MisaGetData", Directory.GetCurrentDirectory() + "\\MisaGetDataCloud.exe");
            }
        }

        /// <summary>
        /// kiểm tra kết nối với máy chủ Desktop
        /// </summary>
        /// <returns></returns>
        public bool ValiDateConnectServerMeinvoice()
        {
            Connect conn = new Connect();
            if (ValidateDesktop(txtMayChu.Text, "Máy chủ") && conn.ConnectServerMeinvoice(txtMayChu.Text.Trim()))
            {
                AppSetting.SaveAppSetting(mscServerDesktopName, txtMayChu.Text);
            }
            else
                return false;
            return true;
        }

        /// <summary>
        /// validate các control desktop
        /// </summary>
        /// <returns></returns>
        public bool ValidateDesktop(string text, string message)
        {
            if (string.IsNullOrEmpty(text))
            {
                MISAMessageBox.ShowExclamationMessage(string.Format(strValidate, message));
                txtMayChu.Focus();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Chạy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtMST.Text))
                {
                    MISAMessageBox.ShowExclamationMessage("Invalid MST !!!");
                }
                else if (string.IsNullOrEmpty(txtMayChu.Text))
                {
                    MISAMessageBox.ShowExclamationMessage("Invalid Server Name !!!");
                }
                else if (string.IsNullOrEmpty(txtLinkSaveFile.Text))
                {
                    MISAMessageBox.ShowExclamationMessage("Invalid Link File !!!");
                }
                else if (string.IsNullOrEmpty(txtToken.Text))
                {
                    MISAMessageBox.ShowExclamationMessage("Invalid Token !!!");
                }
                else
                {
                    if (ValiDateConnectServerMeinvoice())
                    {
                        btnRun.Enabled = false;
                        btnStop.Enabled = true;
                        notifyIcon.Visible = true;
                        this.Hide();
                        this.ShowInTaskbar = false;
                        WindowState = FormWindowState.Minimized;
                        AppSetting.SaveAppSetting(mscServerDesktopName, txtMayChu.Text);
                        AppSetting.SaveAppSetting("CompanyTaxCode", txtMST.Text);
                        AppSetting.SaveAppSetting("FilePath", txtLinkSaveFile.Text);
                        AppSetting.SaveAppSetting("DoiTuongXuLy", cbbDoiTuongXL.SelectedIndex.ToString());
                        gbName.Text = "Kết nối thành công";

                        // get data HIS
                        AppSetting.SaveAppSetting("Token", txtToken.Text);
                        timer1.Enabled = true;
                        //_ = GetDataHIS();
                    }
                    else
                    {
                        MISAMessageBox.ShowExclamationMessage("Connect Failed !!!");
                        Writelog.WriteLogInfo("Connect Failed !!!");
                    }
                }
            }
            catch (Exception ex)
            {
                MISAMessageBox.ShowExclamationMessage(ex.Message.ToString());
                Writelog.WriteLogError(ex.Message);
            }
        }

        /// <summary>
        /// Đóng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Clicked(object sender, EventArgs e)
        {
            //Writelog.WriteLogInfo("Đóng ứng dụng");
            if (cbStartUp.Checked)
            {
                registryKey.SetValue("MisaGetData", Application.ExecutablePath.ToString());
            }
            else
            {
                registryKey.DeleteValue("MisaGetData", false);
            }
            AppSetting.SaveAppSetting("CompanyTaxCode", txtMST.Text);
            AppSetting.SaveAppSetting("ServerDesktopName", txtMayChu.Text);
            AppSetting.SaveAppSetting("FilePath", txtLinkSaveFile.Text);
            AppSetting.SaveAppSetting("Token", txtToken.Text);
            AppSetting.SaveAppSetting("DoiTuongXuLy", Selected.ToString());
            if (cbAutoRun.Checked)
            {
                AppSetting.SaveAppSetting("AutoRun", "True");
            }
            else
            {
                AppSetting.SaveAppSetting("AutoRun", "False");
            }
            notifyIcon.Visible = true;
            notifyIcon.Visible = true;
            this.Hide();
            this.ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;

        }

        /// <summary>
        /// Chọn file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChooseFile_Clicked(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.txtLinkSaveFile.Text = dlg.SelectedPath;
                    AppSetting.SaveAppSetting("FilePath", dlg.SelectedPath.ToString());
                }
            }
            catch (Exception ex)
            {
                MISAMessageBox.ShowExclamationMessage(ex.ToString());
            }
        }

        /// <summary>
        /// Khởi động cùng windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartupWithWindows(object sender, EventArgs e)
        {
            RegisterInStartup(cbStartUp.Checked);
        }

        /// <summary>
        /// Kiểm tra khởi động cùng windows hay không ?
        /// </summary>
        private void CheckStartup()
        {
            // Nếu key này tồn tại thì việc đăng ký là thành công
            var rg = registryKey.GetValue("MisaGetDataCloud");
            if (rg != null)
                cbStartUp.Checked = true;
            else
                cbStartUp.Checked = false;
        }

        /// <summary>
        /// Click biểu tượng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_DoubleClicked(object sender, MouseEventArgs e)
        {
            notifyIcon.Visible = false;
            this.Show();
            this.ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// Load dữ liệu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void From_Loaded(object sender, EventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(AppSetting.GetAppSettingWithDefaultValue("CompanyTaxCode")))
                {
                    txtMST.Text = AppSetting.GetAppSettingWithDefaultValue("CompanyTaxCode");
                }

                if (!String.IsNullOrEmpty(AppSetting.GetAppSettingWithDefaultValue("ServerDesktopName")))
                {
                    txtMayChu.Text = AppSetting.GetAppSettingWithDefaultValue("ServerDesktopName");
                }

                if (!String.IsNullOrEmpty(AppSetting.GetAppSettingWithDefaultValue("Token")))
                {
                    txtToken.Text = AppSetting.GetAppSettingWithDefaultValue("Token");
                }

                if (!String.IsNullOrEmpty(AppSetting.GetAppSettingWithDefaultValue("DoiTuongXuLy")))
                {
                    cbbDoiTuongXL.SelectedIndex = Convert.ToInt16(AppSetting.GetAppSettingWithDefaultValue("DoiTuongXuLy"));
                }

                if (!String.IsNullOrEmpty(AppSetting.GetAppSettingWithDefaultValue("FilePath")))
                {
                    txtLinkSaveFile.Text = AppSetting.GetAppSettingWithDefaultValue("FilePath");
                }

                if (!String.IsNullOrEmpty(AppSetting.GetAppSettingWithDefaultValue("AutoRun")))
                {
                    cbAutoRun.Checked = Convert.ToBoolean(AppSetting.GetAppSettingWithDefaultValue("AutoRun"));
                }

                if (!String.IsNullOrEmpty(AppSetting.GetAppSettingWithDefaultValue("LogFile")))
                {
                    Session.LogFile = Convert.ToBoolean(AppSetting.GetAppSettingWithDefaultValue("LogFile"));
                }

                if (cbAutoRun.Checked && txtMST.Text != "" && txtToken.Text != "")
                {
                    btnRun.PerformClick();
                }
            }
            catch (Exception ex)
            {
                Writelog.WriteLogError(ex.Message);
            }
        }

        /// <summary>
        /// Đóng dữ liệu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if (cbStartUp.Checked)
            {
                registryKey.SetValue("MisaGetData", Application.ExecutablePath.ToString());
            }
            else
            {
                registryKey.DeleteValue("MisaGetData", false);
            }
            AppSetting.SaveAppSetting("CompanyTaxCode", txtMST.Text);
            AppSetting.SaveAppSetting("ServerDesktopName", txtMayChu.Text);
            AppSetting.SaveAppSetting("FilePath", txtLinkSaveFile.Text);
            AppSetting.SaveAppSetting("Token", txtToken.Text);
            AppSetting.SaveAppSetting("DoiTuongXuLy", Selected.ToString());
            if (cbAutoRun.Checked)
            {
                AppSetting.SaveAppSetting("AutoRun", "True");
            }
            else
            {
                AppSetting.SaveAppSetting("AutoRun", "False");
            }
        }

        /// <summary>
        /// Dừng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Clicked(object sender, EventArgs e)
        {
            btnRun.Enabled = true;
            btnStop.Enabled = false;
            gbName.Text = "Thông tin kết nối";
            //Writelog.WriteLogInfo("Tool dừng hoạt động !!!");
        }

        /// <summary>
        /// Chọn item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbbDoiTuong_SelectChanged(object sender, EventArgs e)
        {
            Selected = cbbDoiTuongXL.SelectedIndex;
        }

        /// <summary>
        /// Đẩy lại
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRetryPush_Clicked(object sender, EventArgs e)
        {
            try
            {
                AppSetting.SaveAppSetting(mscServerDesktopName, txtMayChu.Text);
                AppSetting.SaveAppSetting("CompanyTaxCode", txtMST.Text);
                AppSetting.SaveAppSetting("FilePath", txtLinkSaveFile.Text);
                AppSetting.SaveAppSetting("DoiTuongXuLy", cbbDoiTuongXL.SelectedIndex.ToString());
                AppSetting.SaveAppSetting("Token", txtToken.Text);

                if (!string.IsNullOrEmpty(txtFileDayLai.Text))
                {
                    if (File.Exists(txtFileDayLai.Text))
                    {
                        File.SetAttributes(txtFileDayLai.Text, FileAttributes.Normal);
                    }
                    var sQuerySql = File.ReadAllText(txtFileDayLai.Text);
                    ProcessingData(SerializeUtil.DeserializeObject<Root>(sQuerySql));
                    //MISAMessageBox.ShowInfoMessage("Xử lý dữ liệu thành công !!!");
                }
            }
            catch (Exception ex)
            {
                Writelog.WriteLogError(ex.Message);
            }

        }

        /// <summary>
        /// Chọn file đẩy lại
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChoseFileRetryPush_Clicked(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.txtFileDayLai.Text = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                MISAMessageBox.ShowExclamationMessage(ex.ToString());
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (CheckInterNet())
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    PubSubHelper.Initialize(AppSetting.GetAppSettingWithDefaultValue("Token"));

                    PubSubHelper.Once($"phieuthu/pending",
                           async (sub, msgToken) =>
                           {
                               Root root = await $"misa-hdvlist?msgToken={msgToken}".GetAsJson<Root>(AppSetting.GetAppSettingWithDefaultValue("Token"));

                               if (root != null && root.result.Count > 0)
                               {
                                   if (Session.LogFile)
                                   {
                                       File.AppendAllText(txtLinkSaveFile.Text + "\\result_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt", SerializeUtil.SerializeObject(root));
                                   }
                                   Writelog.WriteLogInfo("file log:" + txtLinkSaveFile.Text + "\\result_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt");
                                   ProcessingData(root);
                               }

                               return true;
                           },
                           (subErr) =>
                           {
                               return true;
                           });
                }
            }
            catch (UnauthorizedAccessException)
            {
                Writelog.WriteLogError("Không có quyền truy cập hoặc không đúng giấy phép");
            }
            catch (HttpRequestException httpex)
            {
                Writelog.WriteLogError(httpex.Message);
            }
        }

        private bool CheckInterNet()
        {
            try
            {
                Ping myPing = new Ping();
                String host = "status.cloud.google.com";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                return (reply.Status == IPStatus.Success);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
