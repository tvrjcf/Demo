using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo_NPOI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private IWorkbook workbook = null;
        private ISheet sheet = null;
        private FileStream fs = null;

        private void btn_Ok_Click(object sender, EventArgs e)
        {
            string fileName = tb_File.Text; //文件名;
            DataTable data = new DataTable();

            int count = 0;

            try
            {
                fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);

                if (workbook != null)
                {
                    cbbSheets.Items.Clear();
                    for (int i = 0, j = workbook.NumberOfSheets; i < j; i++)
                    {
                        var name = workbook.GetSheetName(i);
                        cbbSheets.Items.Add(name);
                    }
                    if (cbbSheets.Items.Count > 0)
                        cbbSheets.SelectedIndex = 0;
                    //sheet = workbook.CreateSheet("X-RP");
                    GetValue();

                    ICell cell = sheet.GetRow(14).GetCell(14);
                }
                else
                {
                    //return -1;
                }


                //workbook.Write(fs); //写入到excel
                //return count;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //return -1;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }

        private void btn_Update_Click(object sender, EventArgs e)
        {
            double[] arr = new double[] { 22.41, 22.38, 22.59, 22.19 };
            if (workbook != null)
            {
                sheet = workbook.GetSheet("X-R");
                if (sheet != null)
                {
                    ICell cell = sheet.GetRow(11).GetCell(3);

                    int i = new Random().Next(0, 4);
                    btn_Update.Text = string.Format("修改值：{0}", arr[i]);
                    cell.SetCellValue(arr[i]);

                    //sheet.ForceFormulaRecalculation = true;

                    GetValue();


                }
            }
        }

        private void GetValue()
        {
            sheet = workbook.GetSheet("X-RP");
            var eval = new XSSFFormulaEvaluator(workbook);
            if (sheet != null)
            {
                ICell cell = sheet.GetRow(12).GetCell(8);
                eval.EvaluateFormulaCell(cell);
                tb_EV.Text = cell.NumericCellValue.ToString();

                cell = sheet.GetRow(17).GetCell(8);
                eval.EvaluateFormulaCell(cell);
                tb_AV.Text = cell.NumericCellValue.ToString();

                cell = sheet.GetRow(22).GetCell(8);
                eval.EvaluateFormulaCell(cell);
                tb_RR.Text = cell.NumericCellValue.ToString();

                cell = sheet.GetRow(30).GetCell(8);
                eval.EvaluateFormulaCell(cell);
                tb_PV.Text = cell.NumericCellValue.ToString();
            }
        }

        private static object CheckVlaue(ICell cell)
        {
            object returnValue = "";
            if (cell == null)
            {
                returnValue = "";
            }
            else
            {
                //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)***
                switch (cell.CellType)
                {
                    case CellType.Blank: //BLANK:
                        returnValue = null;
                        break;
                    case CellType.Boolean: //BOOLEAN:
                        returnValue = cell.BooleanCellValue;
                        break;
                    case CellType.Numeric: //NUMERIC:
                        returnValue = cell.NumericCellValue;
                        break;
                    case CellType.String: //STRING:
                        returnValue = cell.StringCellValue;
                        break;
                    case CellType.Error: //ERROR:
                        returnValue = cell.ErrorCellValue;
                        break;
                    case CellType.Formula: //FORMULA:
                        returnValue = "(" + cell.NumericCellValue.ToString() + ")=" + cell.CellFormula;
                        break;
                    default:
                        return null;
                        //case CellType.Blank:
                        //    returnValue = "";
                        //    break;
                        //case CellType.Numeric:
                        //    short format = cell.CellStyle.DataFormat;
                        //    //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理***
                        //    if (format == 14 || format == 31 || format == 57 || format == 58 || format == 20)
                        //        returnValue = cell.DateCellValue.ToString(" HH:mm");
                        //    else
                        //        returnValue = cell.NumericCellValue;
                        //    break;
                        //case CellType.String:
                        //    returnValue = cell.StringCellValue;
                        //    break;
                }
            }
            return returnValue;
        }


        /// <summary>
        /// 递归获取目录（包含子目录）所有xml文件
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        private List<FileInfo> GetFiles(string dirPath, List<FileInfo> list)
        {
            if (list == null) list = new List<FileInfo>();
            var dirInfo = new DirectoryInfo(dirPath);
            if (dirInfo != null)
            {
                var files = dirInfo.GetFiles();
                foreach (var file in files)
                {
                    if (file.Extension.ToLower().Contains("xml"))
                        list.Add(file);
                }
            }
            var dirs = Directory.GetDirectories(dirPath);
            if (dirs.Length > 0)
            {
                foreach (var dir in dirs)
                {
                    GetFiles(dir, list);
                }
            }
            return list.OrderBy(p => p.CreationTime).ToList();
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            try
            {
                //var files = GetFiles(@"C:\TestFolder\MS", null);
                //tb_Result.Text = string.Format("{0}", files.Count);
                //return;

                int i = Convert.ToInt32(tb_Row.Text);
                int j = Convert.ToInt32(tb_Column.Text);

                sheet = workbook.GetSheet(cbbSheets.Text);
                ICell cell = sheet.GetRow(i).GetCell(j);

                tb_Result.Text = string.Format("{0}", CheckVlaue(cell));
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message); ;
            }
        }
    }
}
