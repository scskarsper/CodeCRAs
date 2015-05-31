using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Balthasar.IO
{
	public class PackageLang
	{
        string PackageFile = "";
        CIni ini = new CIni();
        public PackageLang(string PkgName)
        {
            string PkgName2 = PkgName;
            string[] PL = PkgName.Split(new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' });
            PkgName2 = String.Join("", PL);
            try
            {
                System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\motions\\langs");
            }
            catch { ;}
            PackageFile = AppDomain.CurrentDomain.BaseDirectory + "\\motions\\langs\\" + PkgName2 + ".lang";
            ini = new CIni(PackageFile);
        }
        public string GetString(int langid,string OString)
        {
            if (langid != 1) return OString;
            return GetString(OString, OString);
        }
        public string GetString(string OString)
        {
            return GetString(OString, OString);
        }
        public string GetString(string oldString, string defString)
        {
            string Ret = defString;
            if (PackageFile != "")
            {
                string s=ini.getKeyValue(System.Globalization.CultureInfo.InstalledUICulture.Name, oldString);
                if (s == "")
                {
                    ini.setKeyValue(System.Globalization.CultureInfo.InstalledUICulture.Name, oldString, defString);
                }
                else
                {
                    Ret = s;
                }
            }
            return Ret;
        }
	}
}
