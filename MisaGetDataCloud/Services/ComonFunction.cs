using MISA.MeInvoice.DC.Library;
using MISA.MeInvoice.DS.Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MisaGetDataCloud.Services
{
    public class CommonFunction
    {
        private const string mscPOST = "POST";
        private const string mscDataService = "dal";

        /// <summary>
        /// Lấy ra 1 dataset từ list dữ liệu
        /// Dùng khi nào, ở đâu: 
        /// Tham số đầu vào: 
        /// Kết quả: 
        /// Ai là người review/duyệt: 
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="entityName">Tên lớp dữ liệu</param>
        /// <param name="tType">Type of the t.</param>
        /// <returns></returns><author>
        /// nnanh 23/02/2017
        /// </author>
        public static DataTable ConvertListToDataTable(IList list, string entityName, Type tType, bool isAdded = false)
        {
            DataTable table = new DataTable(entityName);
            PropertyInfo[] fields = tType.GetProperties();
            foreach (PropertyInfo field in fields)
            {
                if (!table.Columns.Contains(field.Name))
                {
                    DataColumn dcColumn = new DataColumn
                    {
                        ColumnName = field.Name,
                        AllowDBNull = true,
                        DataType = field.PropertyType.GetNonNullableType()
                    };
                    table.Columns.Add(dcColumn);
                }
            }
            foreach (object item in list)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo field in fields)
                {
                    object oData = field.GetValue(item, null);
                    if (oData == null)
                    {
                        oData = DBNull.Value;
                    }
                    row[field.Name] = oData;
                }
                if (row.Table.Columns.Contains("InvtypeID"))
                {
                    //dhthinh 04-07-2019 (PBI: 326755): lấy lên tất cả các phiếu xuất kho kiêm vận chuyển nội bộ và phiếu xuất kho hàng gửi bán đại lý
                    //if (row["InvtypeID"].ToString().Trim() != "4" && row["InvtypeID"].ToString().Trim() != "5")
                    //{
                    table.Rows.Add(row);
                    if (!isAdded)
                    {
                        row.AcceptChanges();
                    }
                    //}
                }
                else
                {
                    table.Rows.Add(row);
                    if (!isAdded)
                    {
                        row.AcceptChanges();
                    }
                }

            }
            return table;
        }

        /// <summary>
        /// Check Exist API
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public static string CheckAPI(string apiurl)
        {
            bool oResult = CallApiFunctionTest(apiurl, mscPOST, mscDataService, "CheckExistAPI");
            if (oResult)
            {
                return apiurl;

            }
            return "";
        }

        /// <summary>
        /// Thực hiện gọi 1 hàm trên API Meinvoice
        /// </summary>
        /// <returns></returns>
        public static ServiceResult ExecuteApiFunction(string method, string apiURL, Dictionary<string, string> headers = null, object parameter = null)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                result = CallWebRequest<ServiceResult>(method, apiURL, headers, parameter);
            }
            catch (Exception ex)
            {
                //throw ex;
                result.Success = false;
                result.ErrorCode = ex.Message;
                result.Errors.Add(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Gọi API
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="api"></param>
        /// <param name="headers"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static T CallWebRequest<T>(string method, string api, Dictionary<string, string> headers, object parameter)
        {
            T result = default(T);
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)(System.Net.WebRequest.Create(api));
            request.Method = method;
            request.KeepAlive = true;
            request.Timeout = 30000;
            request.ContentType = "application/json; charset=utf-8";
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }
            if (method.ToLower() != System.Net.WebRequestMethods.Http.Get.ToLower() && parameter != null)
            {
                string strParam = SerializeUtil.SerializeObject(parameter);
                byte[] byteArray = (new System.Text.UTF8Encoding()).GetBytes(strParam);
                request.ContentLength = byteArray.Length;
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
            }

            using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)(request.GetResponse()))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        string resultstring = sr.ReadToEnd();
                        if (!(string.IsNullOrWhiteSpace(resultstring)))
                        {
                            result = SerializeUtil.DeserializeObject<T>(resultstring);
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Gọi API
        /// </summary>
        /// <param name="method"></param>
        /// <param name="serviceName"></param>
        /// <param name="functionName"></param>
        /// <param name="parameter"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static bool CallApiFunctionTest(string apiurl, string method, string serviceName, string functionName, Dictionary<string, string> headers = null)
        {
            ServiceResult result = new ServiceResult();
            try
            {

                if (!string.IsNullOrEmpty(apiurl))
                {
                    while (apiurl.EndsWith("/"))
                    {
                        apiurl = apiurl.Substring(0, apiurl.Length - 1);
                    }
                    System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)(System.Net.WebRequest.Create(string.Format("{0}/{1}/{2}", apiurl, serviceName, functionName)));
                    request.Method = method;
                    request.ContentType = "application/json; charset=utf-8";
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            request.Headers.Add(item.Key, item.Value);
                        }
                    }
                    //Add thêm TaxCode
                    if (method != "GET")
                    {
                        string strParam = "";
                        //   SetGlobalAPIParamInfo(parameter);

                        System.Text.UTF8Encoding encode = new System.Text.UTF8Encoding();
                        byte[] byteArray = encode.GetBytes(strParam);
                        request.ContentLength = byteArray.Length;

                        using (Stream dataStream = request.GetRequestStream())
                        {
                            dataStream.Write(byteArray, 0, byteArray.Length);
                        }
                    }

                    using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)(request.GetResponse()))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            StreamReader sr = new StreamReader(response.GetResponseStream());
                            string resultstring = sr.ReadToEnd();
                            if (!(string.IsNullOrWhiteSpace(resultstring)))
                            {
                                result = SerializeUtil.DeserializeObject<ServiceResult>(resultstring);
                                if (result.ErrorCode == "INVALID_TAXCODE" || result.ErrorCode == "NOT_SERVER_SETUP")
                                {
                                    //string sErrorCode = result.ErrorCode;
                                    result.Success = false;
                                    result.ErrorCode = result.ErrorCode;
                                    result.Errors.Add(response.StatusDescription);
                                }
                            }
                        }
                        else
                        {
                            result.Success = false;
                            result.ErrorCode = response.StatusDescription;
                            result.Errors.Add(response.StatusDescription);
                        }
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Lấy dữ liệu EInvoice theo RefID
        /// </summary>
        /// <param name="refId"></param>
        /// <param name="companyTaxCode"></param>
        /// <returns></returns>
        public static DataTable GetDataByRefIDNotPublish(string refId, string companyTaxCode)
        {
            MeInvoiceServerDatabase oDatabase = new MeInvoiceServerDatabase(companyTaxCode);
            var result = oDatabase.ExecuteDataSet(String.Format("select refid from Einvoice where RefID='{0}'and PublishStatus <> 0 ", refId));
            return result != null ? result.Tables[0] : null;
        }
    }
}
