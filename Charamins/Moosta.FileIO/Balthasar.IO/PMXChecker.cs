using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Balthasar.IO
{
	public class PMXChecker
	{
        public static void CheckPMX(string fileName)
        {   
            try
            {
                Assembly ass = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + "\\PMX2OMP\\Balthasar.PMX2OMP.dll");
                Type T = ass.GetType("Balthasar.PMX2OMP.PmxPreBuilder");
                object controller = Activator.CreateInstance(T, null);
                MethodInfo mi = T.GetMethod("CheckPMX");
                mi.Invoke(controller, new object[1] { fileName });
            }
            catch (Exception e) { System.Windows.Forms.MessageBox.Show(e.Message); };
        }
	}
}
