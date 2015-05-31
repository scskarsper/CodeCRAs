using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace Balthasar.IO
{
    /// <summary>
    /// ini文件操作类
    /// </summary>
    class CIni
    {
        System.Text.Encoding srcEncoding = null;
        string TmpFile = "";
        public void LoadFile(string FilePath, System.Text.Encoding fileEncoding)
        {
            ReleaseTmpCache();
            WriteAllTextByEncoding(ReadAllTextByEncoding(FilePath, fileEncoding), TmpFile, System.Text.Encoding.Unicode);
            Path = TmpFile;
        }
        private string ReadAllTextByEncoding(string FilePath, System.Text.Encoding fileEncoding)
        {
            srcEncoding = fileEncoding;
            TmpFile = System.IO.Path.GetTempFileName();
            System.IO.FileStream TFS = new System.IO.FileStream(FilePath, System.IO.FileMode.OpenOrCreate);
            System.IO.StreamReader TSR = new System.IO.StreamReader(TFS, srcEncoding);
            string ALL = TSR.ReadToEnd();
            TSR.Close();
            TFS.Close();
            return ALL;
        }
        private void WriteAllTextByEncoding(string TextBeWrite, string FilePath, System.Text.Encoding fileEncoding)
        {
            System.IO.FileStream TFW = new System.IO.FileStream(FilePath, System.IO.FileMode.Create);
            System.IO.StreamWriter TSW = new System.IO.StreamWriter(TFW, fileEncoding);
            TSW.Write(TextBeWrite);
            TSW.Close();
            TFW.Close();
        }
        private void ReleaseTmpCache()
        {
            if (TmpFile != "")
            {
                try
                {
                    System.IO.File.Delete(TmpFile);
                }
                catch { ;}
                TmpFile = "";
            }
        }

        #region 段信息的获取
        //读取一个ini 文件中的所有段
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileSectionNamesW", CharSet = CharSet.Unicode)]
        private extern static int getSectionNames(
        [MarshalAs(UnmanagedType.LPWStr)] string szBuffer, int nlen, string filename);

        //读取段里的所有数据
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileSectionW", CharSet = CharSet.Unicode)]
        private extern static int getSectionValues(string Section,
        [MarshalAs(UnmanagedType.LPWStr)] string szBuffer, int nlen, string filename);
        #endregion


        #region 键值的获取和设置
        //读取键的整形值
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileIntW", CharSet = CharSet.Unicode)]
        private static extern int getKeyIntValue(string Section, string Key, int nDefault, string FileName);

        //读取字符串键值
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode)]
        private extern static int getKeyValue(string section, string key, int lpDefault,
            [MarshalAs(UnmanagedType.LPWStr)] string szValue, int nlen, string filename);

        //
        //
        //写字符串键值
        [DllImport("kernel32", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
        private static extern bool setKeyValue(string Section, string key, string szValue, string FileName);

        //写段值
        [DllImport("kernel32", EntryPoint = "WritePrivateProfileSectionW", CharSet = CharSet.Unicode)]
        private static extern bool setSectionValue(string section, string szvalue, string filename);
        #endregion

        private static readonly char[] sept = { '\0' };	//分隔字符

        private string m_Path = null;		//ini文件路径

        /// <summary>
        /// ini文件路径
        /// </summary>
        private string Path
        {
            set { m_Path = value; }
            get { return m_Path; }
        }

        public CIni() { }
        public CIni(string szPath)
        {
            m_Path = szPath;
        }

        /// <summary>
        /// 读取所有段名
        /// </summary>
        public string[] SectionNames
        {
            get
            {
                string buffer = new string('\0', 32768);
                int nlen = getSectionNames(buffer, 32768 - 1, m_Path) - 1;
                if (nlen > 0)
                {
                    return buffer.Substring(0, nlen).Split(sept);
                }
                return null;
            }
        }

        /// <summary>
        /// 读取段里的数据到一个字符串数组
        /// </summary>
        /// <param name="section">段名</param>
        /// <param name="bufferSize">读取的数据大小(字节)</param>
        /// <returns>成功则不为null</returns>
        public string[] SectionValues(string section, int bufferSize)
        {
            string buffer = new string('\0', bufferSize);
            int nlen = getSectionValues(section, buffer, bufferSize, m_Path) - 1;
            if (nlen > 0)
            {
                return buffer.Substring(0, nlen).Split(sept);
            }
            return null;
        }
        public string[] SectionValues(string section)
        {
            return SectionValues(section, 32768);
        }
        public string[] SectionValuesNew(string section)
        {
            string buffer = new string('\0', 32768);
            int nlen = getSectionValues(section, buffer, 32768, m_Path) - 1;
            if (nlen > 0)
            {
                string RT = buffer.Substring(0, nlen).Replace("\0", "\r\n");
                FileStream fs = new FileStream(m_Path, FileMode.Open);
                StreamReader SR1 = new StreamReader(fs, true);
                string RL = SR1.ReadToEnd();
                if (true || RL.IndexOf(RT) == -1)
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    //SR1.Close();
                    SR1 = new StreamReader(fs, Encoding.GetEncoding("Shift-JIS"), true);
                    RL = SR1.ReadToEnd();
                }

                SR1.Close();
                fs.Close();

                if (RL.IndexOf(RT) == -1)
                {
                    return null;
                }
                else
                {
                    int Index = RL.IndexOf(RT);
                    string T1 = RL.Substring(Index);
                    string T2 = T1.Split('[')[0];
                    return T2.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            else
            {
                return null;
            }
            return SectionValues(section, 32768);
        }

        /// <summary>
        /// 从一个段中读取其 键-值 数据
        /// </summary>
        /// <param name="section">段名</param>
        /// <param name="bufferSize">读取的数据大小(字节)</param>
        /// <returns>成功则不为null</returns>
        public Dictionary<string, string> SectionValuesEx(string section, int bufferSize)
        {
            string[] sztmp = SectionValues(section, bufferSize);
            if (sztmp != null)
            {
                int ArrayLen = sztmp.Length;
                if (ArrayLen > 0)
                {
                    Dictionary<string, string> dtRet = new Dictionary<string, string>();
                    for (int i = 0; i < ArrayLen; i++)
                    {
                        int pos1 = sztmp[i].IndexOf('=');
                        if (pos1 > 1)
                        {
                            int nlen = sztmp[i].Length;
                            //	取键名,键值
                            pos1++;
                            if (pos1 < nlen)
                                dtRet.Add(sztmp[i].Substring(0, pos1 - 1), sztmp[i].Substring(pos1, nlen - pos1));
                        }
                    }
                    return dtRet;
                }
            }
            return null;
        }
        public Dictionary<string, string> SectionValuesEx(string section)
        {
            return SectionValuesEx(section, 32768);
        }

        /// <summary>
        /// 写一个段的数据
        /// </summary>
        /// <param name="section"></param>
        /// <param name="szValue">段的数据(如果为null则删除这个段)</param>
        /// <returns>成功则为true</returns>
        public bool setSectionValue(string section, string szValue)
        {
            return setSectionValue(section, szValue, m_Path);
        }

        /// <summary>
        /// 读整形键值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns>成功则不为-1</returns>
        public int getKeyIntValue(string section, string key)
        {
            return getKeyIntValue(section, key, -1, m_Path);
        }

        /// <summary>
        /// 写整形键值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="dwValue"></param>
        /// <returns>成功则为true</returns>
        public bool setKeyIntValue(string section, string key, int dwValue)
        {
            return setKeyValue(section, key, dwValue.ToString(), m_Path);
        }

        /// <summary>
        /// 读取键值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns>成功则不为null</returns>
        public string getKeyValue(string section, string key)
        {
            string szBuffer = new string('0', 256);
            int nlen = getKeyValue(section, key, 0, szBuffer, 256, m_Path);
            string ret = szBuffer.Substring(0, nlen);
            return ret.Split('\0')[0];
        }

        /// <summary>
        /// 写字符串键值
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="key"></param>
        /// <param name="szValue"></param>
        /// <returns>成功则为true</returns>
        public bool setKeyValue(string Section, string key, string szValue)
        {
            return setKeyValue(Section, key, szValue, m_Path);
        }
    }//end class CIni
}
