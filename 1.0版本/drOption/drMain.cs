using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace drOption
{
    public class drMain
    {
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        private extern static IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lbszWindow);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private extern static IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);
        const int WM_CLICK = 0x000F5;
        const int WM_GETTEXT = 0x000D;
        const int WM_SETTEXT = 0x000C;
        //问题：对没选择自动登录，记住密码，或没有填账号的做处理
        public bool autoLogin()
        {
            string lpszParentClass = "#32770";
            string lpszParentWindow = "Dr.COM Client";
            string lpszClass_Btn = "Button";
            string lpszName_Btn = "登录";

            IntPtr parentHwnd = new IntPtr(0);
            IntPtr btnHwnd = new IntPtr(0);
            IntPtr pwdHwnd = new IntPtr(0);

            parentHwnd = FindWindow(lpszParentClass, lpszParentWindow);
            if (!parentHwnd.Equals(IntPtr.Zero))
            {
                btnHwnd = FindWindowEx(parentHwnd, btnHwnd, lpszClass_Btn, lpszName_Btn);
                if (!btnHwnd.Equals(IntPtr.Zero))
                {
                    SendMessage(btnHwnd, WM_CLICK, (IntPtr)0, "0");
                }
            }
            string lpClientWindow = "";//登录成功后的窗体
            parentHwnd = new IntPtr(0);
            while (true)
            {
                parentHwnd = FindWindow(lpszParentClass, lpClientWindow);
                if (!parentHwnd.Equals(IntPtr.Zero))
                    break;
            }
            return true;
        }
        //客户端启动太慢 应启用线程
        //启用线程的进程是他的主线程？
        public  void Run()
        {
            try
            {
                
                ProcessStartInfo drmainInfo = new ProcessStartInfo(@"C:\Drcom\DrUpdateClientCUST\DrMain.exe");
                Process drmain = Process.Start(drmainInfo);
                ProcessStartInfo drclientInfo = new ProcessStartInfo(@"C:\Drcom\DrUpdateClientCUST\DrClient.exe");
                Process drClient = Process.Start(drclientInfo);
                //ProcessStartInfo updateInfo = new ProcessStartInfo(@"C:\Drcom\DrUpdateClientCUST\DrUpdate.exe");
                //Process drUpdate = Process.Start(updateInfo);

                string lpszParentClass = "#32770";
                string lpszParentWindow = "Dr.COM Client";

                IntPtr parentHwnd = new IntPtr(0);

                while (true) //等待客户端启动
                {
                    Thread.Sleep(200);
                    parentHwnd = FindWindow(lpszParentClass, lpszParentWindow);
                    if (!parentHwnd.Equals(IntPtr.Zero))
                        break;
                }
            }
            catch(Exception  e)
            {
                throw e;
            }
        }
    }
}
