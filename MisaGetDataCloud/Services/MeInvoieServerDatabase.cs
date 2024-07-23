using MISA.MeInvoice.DataContract;
using MISA.MeInvoice.DC.Library;
using MISA.MeInvoice.DC.Library.Parameter;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisaGetDataCloud.Services
{
    public class MeInvoiceServerDatabase : MeInvoiceDatabase
    {
        #region "Declare"
        private const string mscPOST = "POST";
        private const string mscGET = "GET";
        private const string mscDataService = "dal";
        private static string _companyTaxCode;
        #endregion
        #region "Property"

        /// <summary>
        /// Database dùng MSSQL Server
        /// </summary>
        public static string CompanyTaxCode
        {
            get
            {

                return _companyTaxCode;
            }
            set
            {
                _companyTaxCode = value;
            }
        }

        #endregion

        public MeInvoiceServerDatabase(string companyTaxCode)
        {
            CompanyTaxCode = companyTaxCode;
        }

        /// <summary>
        /// Hàm ExecuteNonQuery với mảng tham số truyền vào
        /// </summary>
        /// <param name="storedProcedure">Tên Store</param>
        /// <param name="obj">Object truyền vào</param>
        /// <param name="ts">Transaction</param>
        /// <returns></returns>
        public override int ExecuteNonQuery(string storedProcedure, params object[] parameterValues)
        {
            //return DB.ExecuteNonQuery(ts, storedProcedure, parameterValues);
            int iResult = 0;
            ServiceResult oResult = CallApiFunction(mscPOST, mscDataService, "ExecuteNonQuery", new ExecuteNonQueryParameter() { storeProcedureName = storedProcedure, parameter = parameterValues });
            if (oResult.Success)
            {
                int.TryParse(oResult.Data.ToString(), out iResult);
            }
            return iResult;
        }

        /// <summary>
        /// ExecuteDataSet command text
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public override DataSet ExecuteDataSet(string commandText)
        {
            ServiceResult oResult = CallApiFunction(mscPOST, mscDataService, "ExecuteDataSetCommandText", new ExecuteDataSetCommandTextParameter() { commandText = commandText });
            if (oResult.Success)
            {
                return GetServiceDataSet(oResult);

            }
            return null;
        }

        /// <summary>
        /// Chuyen doi du lieu tra ve thanh DataSet
        /// </summary>
        /// <param name="oResult"></param>
        /// <returns></returns>
        private static DataSet GetServiceDataSet(ServiceResult oResult)
        {
            if (oResult.Data != null)
            {
                ServiceDataSet oData = SerializeUtil.DeserializeObject<ServiceDataSet>(oResult.Data.ToString());
                DataSet ds = new DataSet();
                byte[] schema = Convert.FromBase64String(oData.Schema);
                MemoryStream oStream = new MemoryStream(schema);
                ds.ReadXmlSchema(oStream);
                foreach (DataTable dt in oData.Data.Tables)
                {
                    // ddkhanh1 20/04/2019 Khi lấy dữ liệu từ SQLServer thì dữ liệu đó đã có trên DB rồi nên không có trạng thái là Added mà do Convert từ ServiceResult sang nên có trạng thái Added
                    // Ta AcceptChanges các trạng thái đó để dữ liệu không bị thay đổi
                    dt.AcceptChanges();
                    foreach (DataRow dr in dt.Rows)
                    {
                        ds.Tables[dt.TableName].ImportRow(dr);
                    }
                }
                return ds;
            }
            return null;

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
        public static ServiceResult CallApiFunction(string method, string serviceName, string functionName, object parameter, Dictionary<string, string> headers = null)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                string apiurl = System.Configuration.ConfigurationManager.AppSettings["SeverDesktop"];
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
                    request.Headers.Add("TaxCode", CompanyTaxCode);
                    if (method != "GET")
                    {
                        string strParam = "";
                        //   SetGlobalAPIParamInfo(parameter);
                        if (parameter != null)
                        {

                            strParam = SerializeUtil.SerializeObject(parameter);
                        }
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
                                    string sErrorCode = result.ErrorCode;
                                    Writelog.WriteLogError(sErrorCode);
                                }
                            }
                        }
                        else
                        {
                            result.Success = false;
                            result.ErrorCode = response.StatusDescription;
                            result.Errors.Add(response.StatusDescription);
                            Writelog.WriteLogError(result.ErrorCode);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorCode = "Exception";
                result.Errors.Add(ex.Message);
                Writelog.WriteLogError(ex.Message);
            }
            return result;
        }
    }

    public class ExecuteDataSetCommandTextParameter
    {

        /// <summary>
        /// commandText
        /// </summary>
        public string commandText { get; set; }
    }

    /// <summary>
    /// Object chứa DataSet
    /// </summary>
    public class ServiceDataSet
    {
        /// <summary>
        /// schema
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// data
        /// </summary>
        public System.Data.DataSet Data { get; set; }
    }
}
