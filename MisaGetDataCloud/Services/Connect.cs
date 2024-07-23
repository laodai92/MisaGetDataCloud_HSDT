using MisaGetDataCloud.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Windows.Forms;

namespace MisaGetDataCloud.Services
{
    public class Connect
    {
        private string RootPathSql = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)?.Replace("file:\\", "") + "\\SqlInfo.txt";

        /// <summary>
        /// Kiểm tra kết nối sql
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public Boolean CheckConnectSql(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Kết nối meinvoice server
        /// </summary>
        /// <param name="ServerDesktop"></param>
        /// <returns></returns>
        public bool ConnectServerMeinvoice(string ServerDesktop)
        {
            string sPort = "52024;52025;52026;52027;52028";
            string[] portList = sPort.Split(';');
            foreach (string port in portList)
            {
                string url = string.Format("http://{0}:{1}/api", ServerDesktop, port);

                string result = CommonFunction.CheckAPI(url);
                if (result != string.Empty)
                {
                    AppSetting.SaveAppSetting("SeverDesktop", url);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra table
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private bool CheckExistsTable(string connectionString, string tableName)
        {
            bool exists = false;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmdUser = new SqlCommand();
                {
                    cmdUser.Connection = con;
                    cmdUser.CommandType = CommandType.Text;
                    // lấy ra dữ liệu bán hàng , dòng 1 là lấy dữ liệu bán hàng , dòng 3 là lấy dữ liệu hóa đơn và dịch vụ khác , dòng 5 là lấy dữ liệu hàng trả lại  , 7 là hóa đơn giảm giá bán hàng , 9 là hóa đơn giảm giá dịch vụ khác
                    cmdUser.CommandText = "select case when exists((select* from information_schema.tables where table_name = '" + tableName + "')) then 1 else 0 end";
                    exists = (int)cmdUser.ExecuteScalar() == 1;

                }
                con.Close();
                con.Dispose();
            }
            return exists;
        }

        /// <summary>
        /// Thiết lập cơ sở dữ liệu
        /// </summary>
        /// <returns></returns>
        public bool ConnectSql(out SqlConnectionStringBuilder oBuilder, string ptknSQL, string serverName, string dbName, string userName, string passWord, string masterTable, string detailTable, bool showMessage = false)
        {
            bool success = false;
            oBuilder = new SqlConnectionStringBuilder();
            if (!String.IsNullOrEmpty(ptknSQL) && !String.IsNullOrEmpty(dbName) && !String.IsNullOrEmpty(serverName))
            {
                if (ptknSQL == "Sql Server Authentication")
                {
                    if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(passWord))
                    {
                        oBuilder.UserID = userName.Trim();
                        oBuilder.Password = passWord.Trim();
                        oBuilder.DataSource = serverName.Trim();
                        oBuilder.InitialCatalog = dbName.Trim();
                        oBuilder.MultipleActiveResultSets = true;
                        oBuilder.AsynchronousProcessing = true;
                        success = true;
                    }
                    else
                    {
                        MessageBox.Show("");
                    }
                }
                else if (ptknSQL == "Windows Authentication")
                {

                    oBuilder.IntegratedSecurity = true;
                    oBuilder.DataSource = serverName.Trim();
                    oBuilder.InitialCatalog = dbName.Trim();
                    //.ConnectTimeout = iTimeOut
                    oBuilder.MultipleActiveResultSets = true;
                    oBuilder.AsynchronousProcessing = true;
                    success = true;

                }

                if (success == true && CheckConnectSql(oBuilder.ConnectionString))
                {
                    if (CheckExistsTable(oBuilder.ConnectionString, masterTable.Trim()) && CheckExistsTable(oBuilder.ConnectionString, detailTable.Trim()))
                    {
                        string ConnectSqlInfo = Newtonsoft.Json.JsonConvert.SerializeObject(oBuilder);
                        AppSetting.SaveAppSetting("InforTableDB", masterTable.Trim() + ";" + detailTable.Trim());
                        if (File.Exists(RootPathSql))
                        {
                            File.Delete(RootPathSql);

                        }
                        Session.IsConnectSQL = true;
                        File.WriteAllText(RootPathSql, ConnectSqlInfo);
                        if (showMessage == true)
                        {
                            MessageBox.Show("");
                        }
                    }
                    else
                    {
                        success = false;
                    }

                }
            }
            else
            {
                MessageBox.Show("");
            }
            return success;
        }
    }
}
