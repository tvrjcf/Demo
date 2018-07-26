using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VS2008ToVS2005.Core
{
    public sealed class Solution
    {
        #region 获取csproj文件路径

        /// <summary>
        /// 获取csproj文件路径
        /// </summary>
        /// <param name="slnFileName">sln文件名</param>
        /// <param name="slnContextLines">sln文件内容</param>
        /// <returns>csproj文件路径</returns>
        public static List<string> GetCSProjFileName(string slnFileName, string[] slnContextLines)
        {
            var projectsPath = new List<string>();


            if (slnContextLines != null)
            {
                for (int i = 0; i < slnContextLines.Length; i++)
                {
                    if (slnContextLines[i].Trim().ToLower().StartsWith("project"))
                    {
                        string[] csprojPath = slnContextLines[i].Split(',');

                        projectsPath.Add(string.Format("{0}\\{1}", System.IO.Path.GetDirectoryName(slnFileName), csprojPath[1].Replace("\"", "").Trim()));

                        continue;
                    }
                }
            }

            return projectsPath;
        }

        #endregion

        #region 读取sln文件

        /// <summary>
        ///  读取sln文件
        /// </summary>
        /// <param name="slnFileName">sln文件名</param>
        /// <returns>sln内容</returns>
        public static string[] ReadSlnFile(string slnFileName)
        {
            string[] slnContextLines = null;

            if (File.Exists(slnFileName))
            {
                slnContextLines = File.ReadAllLines(slnFileName, Encoding.UTF8);
            }

            return slnContextLines;
        }

        #endregion

        #region 修改Sln文件

        /// <summary>
        /// 修改Sln文件
        /// </summary>
        /// <param name="slnFileName">Sln文件名</param>
        /// <param name="slnContextLines">sln文件内容</param>
        /// <param name="convertType">转换类型</param>
        /// <returns></returns>
        public static void ModifySln(string slnFileName, string[] slnContextLines, ConvertType convertType)
        {
            //写回sln文件
            if (slnContextLines != null)
            {
                switch (convertType)
                {
                    case ConvertType.VS2008ToVS2005:
                        {
                            for (int i = 0; i < slnContextLines.Length; i++)
                            {
                                if (slnContextLines[i].Trim().ToLower().StartsWith("microsoft"))
                                {
                                    slnContextLines[i] = "Microsoft Visual Studio Solution File, Format Version 9.00";

                                    continue;
                                }

                                if (slnContextLines[i].Trim().StartsWith("#"))
                                {
                                    slnContextLines[i] = "# Visual Studio 2005";

                                    break;
                                }
                            }

                            break;
                        }
                    case ConvertType.VS2005ToVS2008:
                        {
                            for (int i = 0; i < slnContextLines.Length; i++)
                            {
                                if (slnContextLines[i].Trim().ToLower().StartsWith("microsoft"))
                                {
                                    slnContextLines[i] = "Microsoft Visual Studio Solution File, Format Version 10.00";

                                    continue;
                                }

                                if (slnContextLines[i].Trim().StartsWith("#"))
                                {
                                    slnContextLines[i] = "# Visual Studio 2008";

                                    break;
                                }
                            }

                            break;
                        }
                    default:
                        {
                            break;
                        }
                }


                File.WriteAllLines(slnFileName, slnContextLines, Encoding.UTF8);
            }
        }

        #endregion

        #region 修改csproj文件

        /// <summary>
        /// 修改csproj文件
        /// </summary>
        /// <param name="csprojs">csproj文件名</param>
        /// <param name="convertType">转换类型</param>
        public static void ModifyCSProjFile(IEnumerable<string> csprojs, ConvertType convertType)
        {
            switch (convertType)
            {
                case ConvertType.VS2008ToVS2005:
                    {
                        //遍历csproj文件
                        foreach (string csproj in csprojs)
                        {
                            if (File.Exists(csproj))
                            {
                                string txt = File.ReadAllText(csproj, Encoding.UTF8);

                                txt = txt.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n", "");
                                txt =
                                    txt.Replace(
                                        "<Project ToolsVersion=\"3.5\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">",
                                        "<Project DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
                                txt = txt.Replace(
                                    "<Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />",
                                    "<Import Project=\"$(MSBuildBinPath)\\Microsoft.CSharp.targets\" />");

                                //写回csproj文件
                                File.WriteAllText(csproj, txt, Encoding.UTF8);
                            }
                        }

                        break;
                    }
                case ConvertType.VS2005ToVS2008:
                    {
                        //遍历csproj文件
                        foreach (string csproj in csprojs)
                        {
                            if (File.Exists(csproj))
                            {
                                string txt = File.ReadAllText(csproj, Encoding.UTF8);

                                txt =
                                    txt.Replace(
                                        "<Project DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">",
                                        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Project ToolsVersion=\"3.5\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
                                txt = txt.Replace("<Import Project=\"$(MSBuildBinPath)\\Microsoft.CSharp.targets\" />",
                                                  "<Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />");

                                //写回csproj文件
                                File.WriteAllText(csproj, txt, Encoding.UTF8);
                            }
                        }

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion

        #region 转换Solution

        /// <summary>
        /// 转换Solution
        /// </summary>
        /// <param name="slnFileName">sln文件</param>
        /// <param name="convertType">转换类型</param>
        public static void ParseSolution(string slnFileName, ConvertType convertType)
        {
            //读取sln文件内容
            string[] slnContextLines = ReadSlnFile(slnFileName);

            //获取csproj文件路径
            List<string> csprojs = GetCSProjFileName(slnFileName, slnContextLines);

            //修改sln文件
            ModifySln(slnFileName, slnContextLines, convertType);

            //修改csproj文件
            ModifyCSProjFile(csprojs, convertType);
        }

        #endregion
    }

    #region 转换类型枚举

    /// <summary>
    /// 转换类型枚举
    /// </summary>
    public enum ConvertType
    {
        None = 0,
        VS2008ToVS2005 = 1,
        VS2005ToVS2008 = 2
    }

    #endregion
}