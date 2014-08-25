using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FreeLogin
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);//false GDI true gdi+
            FM_Login form = new FM_Login();
            Sunisoft.IrisSkin.SkinEngine skin=new Sunisoft.IrisSkin.SkinEngine((System.ComponentModel.Component)form);
            skin.SkinFile = "Longhorn.ssk";
            skin.TitleFont =new System.Drawing.Font("微软雅黑",10f);
            Application.Run(form);
        }
    }
}
