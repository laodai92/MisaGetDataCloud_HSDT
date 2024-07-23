using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MisaGetDataCloud.Services
{
    public class MISAMessageBox
    {
        private static string strProductName = Application.ProductName;

        public static void ShowExclamationMessage(string sMessage)
        {
            //MessageBox.Show(SME2014Event.MainForm, sMessage, strProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            MessageBox.Show(sMessage, strProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Thông báo hỏi người dùng
        /// </summary>
        /// <param name="strMessage"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// Edit by: gtluc 14/08/2009: Biểu tượng là nút chấm than
        /// vttien sửa bổ sung dấu ?  khi cần thiết tránh tình trạng mỗi người viết 1 hàm lung tung ở các nơi khác nhau.
        /// vttien bổ sung 24/09/2014: Bổ sung trường hợp có YesNoCancel (áp dụng khi đóng chương trình cần confirm có sao lưu dữ liệu trước khi xóa)
        public static DialogResult ShowQuestionMessage(string strMessage, bool IsExclamation = true, bool IsShowYesNoCancel = false)
        {
            if (IsExclamation)
            {
                return MessageBox.Show(strMessage, strProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            }
            else if (IsShowYesNoCancel)
            {
                return MessageBox.Show(strMessage, strProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            }
            else
            {
                return MessageBox.Show(strMessage, strProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
        }

        /// <summary>
        /// Thông báo hỏi người dùng
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// Edit by: gtluc 14/08/2009: Biểu tượng là nút chấm than 
        /// Edit by: LDNGOC 21.11.2014: Bổ sung tùy chọn mặc định Focus vào nút nào
        public static DialogResult ShowQuestionMessage(string strMessage, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            return MessageBox.Show(strMessage, strProductName, buttons, MessageBoxIcon.Exclamation, defaultButton);
        }

        /// <summary>
        /// Show thông báo
        /// </summary>
        /// <param name="strMessage"></param>
        /// <remarks></remarks>
        public static void ShowInfoMessage(string strMessage)
        {
            MessageBox.Show(strMessage, strProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
