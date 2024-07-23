using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisaGetDataCloud.Services
{
    public class Session
    {
        internal static string AppID = "8C3B3E41F3D34F639CEAF489FD62AD33"; //"3A136051-81E2-4372-BFC7-5C6C12B73159";

        internal static string Token = "";
        internal static bool IsConnectSQL = false;
        //internal static bool IsPushInvoice = true;
        /// <summary>
        /// mã số thuế kết nối
        /// </summary>
        internal static string TaxCode = "";
        internal static int CompanyID = -1;
        /// <summary>
        /// phương thức kết nối
        /// </summary>
        internal static string PTKN = "";
        /// <summary>
        /// kiểu kết nối
        /// </summary>
        internal static string ConnectType = "";
        /// <summary>
        /// máy chủ kết nối
        /// </summary>
        internal static string ServerDesktop = "";
        /// <summary>
        /// đường dẫn file kết nối
        /// </summary>
        internal static string PathMapping = "MappingColumn.xml";
        /// <summary>
        /// tài khoản kết nối
        /// </summary>
        internal static string UserName = "";
        /// <summary>
        /// mật khẩu kết nối
        /// </summary>
        internal static string Password = "";
        /// <summary>
        /// cờ ghi log file
        /// </summary>
        internal static bool LogFile = false;
    }
}
