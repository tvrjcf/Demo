using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Platform.Domain.Validation;
using Platform.Utils;
using System.Globalization;

namespace Demo_DateCode
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.textBox1.Text = "2017-09-28T16:19:21+08:00";
            this.textBox2.Text = "yyyy-MM-dd'T'HH:mm:sszzz";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                //this.textBox2.Text = GetDateCode(this.textBox1.Text);


                //string text = "11/23/2011 23:59:59 UTC +0800";
                //string pattern = "MM/dd/yyyy HH:mm:ss 'UTC' zzz";

                var text = this.textBox1.Text;
                var pattern = this.textBox2.Text;

                DateTime dto = DateTime.ParseExact(text, pattern, CultureInfo.InvariantCulture);

                MessageBox.Show(dto.ToString("yyyy-mm-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        #region 转换datecode
        /// <summary>
        /// 转换datecode为每周一的日期
        /// </summary>
        /// <param name="dateCode"></param>
        public virtual string GetDateCode(string dateCode)
        {
            if (dateCode.IsNullOrEmpty())
                throw new ValidationException("日期码为空".FormatArgs(dateCode));

            //去除常见前缀\后缀
            //string[] strReplace = new string[] { "9D", "10D", "12D", "XD", "9DB" };
            var dateCodePrefix = ConfigurationHelper.GetAppSettingOrDefault("DateCodePrefix", "");
            var dateCodeSuffix = ConfigurationHelper.GetAppSettingOrDefault("DateCodeSuffix", "");
            string[] strReplace = (dateCodePrefix + "," + dateCodeSuffix).Split(',');
            foreach (var str in strReplace)
            {
                if (str.IsNotEmpty() && dateCode.StartsWith(str))
                    dateCode = dateCode.Replace(str, "");
            }

            //8位日期码,直接返回原字符串
            if (dateCode.Length == 8 && dateCode.StartsWith("20"))
                return dateCode;

            //4位日期码前面用20补位
            if (dateCode.Length != 4 && dateCode.Length != 6)
                throw new ValidationException("日期码 [{0}] 无法识别".FormatArgs(dateCode));
            if (dateCode.Length == 4)
                dateCode = "20" + dateCode;//默认补齐20

            var year = Int32.Parse(dateCode.Substring(0, 4));
            var wk = Int32.Parse(dateCode.Substring(4, dateCode.Length - 4));
            var fdate = new DateTime(year, 1, 1);
            var tdate = fdate.AddDays((wk - 1) * 7 + (1 - (int)(fdate.DayOfWeek)));
            return tdate.ToString("yyyyMMdd");
        }
        #endregion
    }
}
