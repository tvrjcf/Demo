using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using VS2008ToVS2005.Core;

namespace VS2008ToVS2005.UI
{
    public partial class frmMain : Form
    {
        private readonly RadioButton[] _rbtnsConvertType;

        /// <summary>
        /// 是否选中解决方案文件
        /// </summary>
        private DialogResult _isSelectSln = DialogResult.No;

        /// <summary>
        /// sln文件名
        /// </summary>
        private string _slnFileName = string.Empty;

        public frmMain()
        {
            InitializeComponent();

            _rbtnsConvertType = new[] {rbtnVS2005ToVS2008, rbtnVS2008ToVS2005};
        }

        /// <summary>
        /// 解决方案路径
        /// </summary>
        private string SlnPath
        {
            get { return _isSelectSln == DialogResult.OK ? Path.GetDirectoryName(_slnFileName) : string.Empty; }
        }

        /// <summary>
        /// 转换类型
        /// </summary>
        private ConvertType Type
        {
            get { return GetConvertType(_rbtnsConvertType); }
        }

        #region 获取转换类型

        /// <summary>
        /// 获取转换类型
        /// </summary>
        /// <param name="radioButtons"></param>
        /// <returns></returns>
        private static ConvertType GetConvertType(IEnumerable<RadioButton> radioButtons)
        {
            ConvertType type = ConvertType.None;

            foreach (RadioButton radioButton in radioButtons)
            {
                if (radioButton.Checked)
                {
                    type = (ConvertType)Enum.Parse(typeof(ConvertType), radioButton.Tag.ToString());

                    break;
                }
            }

            //IEnumerable<RadioButton> rbtns = from rbtn in radioButtons where rbtn.Checked select rbtn;

            //if (rbtns != null)
            //{
            //    RadioButton rbtnSelect = rbtns.First();

            //    type = (ConvertType) Enum.Parse(typeof (ConvertType), rbtnSelect.Tag.ToString());
            //}

            return type;
        }

        #endregion

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "解决方案文件(*.sln)|*.sln";

                ofd.Multiselect = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtSolutionFile.Text = ofd.FileName;
                    _slnFileName = txtSolutionFile.Text;
                    _isSelectSln = DialogResult.OK;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            string slnFileName = _slnFileName;

            if (slnFileName.Equals(string.Empty))
            {
                MessageBox.Show("请选择解决方案文件！");

                return;
            }

            Solution.ParseSolution(slnFileName, Type);

            MessageBox.Show("转换完毕！");
        }
    }
}