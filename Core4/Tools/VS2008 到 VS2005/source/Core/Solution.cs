using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VS2008ToVS2005.Core
{
    public sealed class Solution
    {
        #region ��ȡcsproj�ļ�·��

        /// <summary>
        /// ��ȡcsproj�ļ�·��
        /// </summary>
        /// <param name="slnFileName">sln�ļ���</param>
        /// <param name="slnContextLines">sln�ļ�����</param>
        /// <returns>csproj�ļ�·��</returns>
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

        #region ��ȡsln�ļ�

        /// <summary>
        ///  ��ȡsln�ļ�
        /// </summary>
        /// <param name="slnFileName">sln�ļ���</param>
        /// <returns>sln����</returns>
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

        #region �޸�Sln�ļ�

        /// <summary>
        /// �޸�Sln�ļ�
        /// </summary>
        /// <param name="slnFileName">Sln�ļ���</param>
        /// <param name="slnContextLines">sln�ļ�����</param>
        /// <param name="convertType">ת������</param>
        /// <returns></returns>
        public static void ModifySln(string slnFileName, string[] slnContextLines, ConvertType convertType)
        {
            //д��sln�ļ�
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

        #region �޸�csproj�ļ�

        /// <summary>
        /// �޸�csproj�ļ�
        /// </summary>
        /// <param name="csprojs">csproj�ļ���</param>
        /// <param name="convertType">ת������</param>
        public static void ModifyCSProjFile(IEnumerable<string> csprojs, ConvertType convertType)
        {
            switch (convertType)
            {
                case ConvertType.VS2008ToVS2005:
                    {
                        //����csproj�ļ�
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

                                //д��csproj�ļ�
                                File.WriteAllText(csproj, txt, Encoding.UTF8);
                            }
                        }

                        break;
                    }
                case ConvertType.VS2005ToVS2008:
                    {
                        //����csproj�ļ�
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

                                //д��csproj�ļ�
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

        #region ת��Solution

        /// <summary>
        /// ת��Solution
        /// </summary>
        /// <param name="slnFileName">sln�ļ�</param>
        /// <param name="convertType">ת������</param>
        public static void ParseSolution(string slnFileName, ConvertType convertType)
        {
            //��ȡsln�ļ�����
            string[] slnContextLines = ReadSlnFile(slnFileName);

            //��ȡcsproj�ļ�·��
            List<string> csprojs = GetCSProjFileName(slnFileName, slnContextLines);

            //�޸�sln�ļ�
            ModifySln(slnFileName, slnContextLines, convertType);

            //�޸�csproj�ļ�
            ModifyCSProjFile(csprojs, convertType);
        }

        #endregion
    }

    #region ת������ö��

    /// <summary>
    /// ת������ö��
    /// </summary>
    public enum ConvertType
    {
        None = 0,
        VS2008ToVS2005 = 1,
        VS2005ToVS2008 = 2
    }

    #endregion
}