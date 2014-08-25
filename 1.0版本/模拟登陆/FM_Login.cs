using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using VerifyCode;

namespace FreeLogin
{
    public partial class FM_Login : Form
    {
        //与登录过程有关的
        private CookieContainer cookieContainer;
        private Uri url;
        private string currentPwd;//pwd
        private string loginCode;//添加在密码后的随机码
        private string verify;
        //类库
        private drOption.Tools tool;
        private drOption.drMain client;
        //验证码
        private opImage opImage;
        private List<verifyChar> list;
        
        static AutoResetEvent autoEvent;
        public FM_Login()
        {
            checkClient();
           
            url = new Uri("http://self.cust.edu.cn/");
            loginCode = "";
            cookieContainer = new CookieContainer();
            tool = new drOption.Tools();
            client = new drOption.drMain();
            opImage = new opImage();
            list = new List<verifyChar> { };
            opImage.loadXml(ref list);

            getCookie(url);
            
            Identify();
            InitializeComponent();
            getUserData();

        }
        /// <summary>
        /// get 请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        //返回什么类型的比较好 
        private string doGet(Uri url)
        {
            HttpWebRequest req;
            HttpWebResponse resp;
            req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "get";
            req.KeepAlive = true;
            req.CookieContainer = this.cookieContainer;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.117 Safari/537.36\r\n";
            string s = "";
            try
            {
                using (resp = (HttpWebResponse)req.GetResponse())
                {
                    if (this.cookieContainer.Count == 0)
                        cookieContainer.Add(resp.Cookies);
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        if (resp.StatusCode != HttpStatusCode.OK)
                        {//状态码验证
                            s = resp.StatusDescription.ToString();
                            return s;
                        }
                        s = reader.ReadToEnd();
                        return s;
                    }
                }
            }
            catch (WebException em)
            {
                throw (em);
            }
        }

        #region 初始化操作
        //检查客户端
        private void checkClient()
        {
            Process[] procs = Process.GetProcesses();
            foreach (Process p in procs)
            {
                if (p.ProcessName == "DrMain")
                {
                    DialogResult dr = MessageBox.Show("发现校园网客户端正在运行,请先注销", "Sorry");
                    //没解决关闭进程 检测不到是否结束 所以不自动结束 p.WaitForExit(); 没用

                    System.Environment.Exit(0);//终止此进程并为基础操作系统提供指定的退出代码。
                    // Application.ExitThread();
                    //return;
                }
            }
        }
        /// <summary>
        /// 获取cookie
        /// </summary>
        private void getCookie(Uri url)
        {
            HttpWebRequest req;
            HttpWebResponse response;
            req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "get";
            req.KeepAlive = true;
            req.CookieContainer = this.cookieContainer;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.117 Safari/537.36\r\n";
            try
            {
                using (response = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader strReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        if(cookieContainer.Count==0)
                        cookieContainer.Add(response.Cookies);
                        while (!loginCode.Contains("loginRandom ="))
                        {
                            loginCode = strReader.ReadLine();
                        }
                        loginCode = loginCode.Substring(loginCode.IndexOf("\"") + 1, 12);//目前是12位
                        /*
                         * _utma _utmz是网站使用GoogleAnalyse获取到的用户信息
                         * 
                        Cookie cook = new Cookie("_utma", "127612431.953186175.1393515023.1393588576.1393591103.7", "/", ".self.cust.edu.cn");
                        Cookie cook2 = new Cookie("_utmz", "127612431.1393515023.1.1.utmcsr=ic.cust.edu.cn|utmccn=(referral)|utmcmd=referral|utmcct=/", "/", ".self.cust.edu.cn");
                        cookieContainer.Add(cook);*/
                    }
                }
            }
            catch (WebException em)
            {
                MessageBox.Show(em.Message);
            }
        }
        //获取识别验证码
        private void Identify() 
        {
            Image img;
            List<verifyChar> lib=new List<verifyChar>{};
            while(lib.Count!=5)
            { 
                img=getCheckImage();
                lib = opImage.toList(img);
            }
            verify="";
            string result="";
            result=opImage.identifyImg(lib, list);
            if (result.Length == 5)
                verify = result;
            else
            {
                
                MessageBox.Show("无法识别验证码");
            }
        }
        /// <summary>
        /// 获取验证码图片
        /// </summary>
        private Image getCheckImage()
        {
            Uri newUrl = new Uri(url + "RandomCodeAction.action?randomNum=0.2646553386002779");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(newUrl);
            HttpWebResponse response;
            req.Method = "get";
            req.KeepAlive = true;
            req.Accept = "image/webp,*/*;q=0.8";
            req.Referer = "http://self.cust.edu.cn/\r\n";
            req.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            req.Headers.Add("Accept-Language", "zh,zh-TW;q=0.8,zh-CN;q=0.6");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.117 Safari/537.36\r\n";
            req.CookieContainer = this.cookieContainer;
            try
            {
                using (response = (HttpWebResponse)req.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        if (stream == null)
                        {
                            lblTips.Text = "验证码获取失败 - -!";
                            return null;
                        }
                        Image img = Image.FromStream(stream);
                        return img;
                    }
                }

            }
            catch (WebException ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 获取保存的账号
        /// </summary>
        private void getUserData() {
            List<string> account=tool.Read();
            if (account.Count == 2) 
            {
                tbxName.Text = account[0].ToString();
                string pwd = account[1].ToString();
                tbxPwd.Text = getPwd(pwd);
                cbxSavePwd.Checked = true;
            }
        }
        /// <summary>
        /// 账号处理 是否已保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxName_TextChanged(object sender, EventArgs e)
        {
            lblTips.Text = "";
            if (tbxName.Text == "")
            {
                lblTips.Text = "请输入账号";
                return;
            }
            string name = tbxName.Text;
            List<string> account = tool.CheckId(name);
            if (account.Count == 2)
            {
                tbxName.Text = account[0].ToString();
                string pwd = account[1].ToString();
                if (getPwd(pwd) == "")
                    return;
                else
                    tbxPwd.Text = getPwd(pwd);
                cbxSavePwd.Checked = true;
            }
            else
            {
                tbxPwd.Clear();
                cbxSavePwd.Checked = false;
            }
        }
        #endregion
        #region 登录操作
       
        /// <summary>
        /// 刷新验证码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void pbx_Click(object sender, EventArgs e)
        //{
        //    getCheckImage();
        //}
        /// <summary>
        /// 获取密码
        /// </summary>
        /// <returns></returns>
        private string getPwd(string code)
        {
            currentPwd = code;
            string constStr = "callCount=1\nc0-scriptName=drcom\nc0-methodName=Cipher\n";
            Random rand = new Random();
            //实现js Date().getTime()
            long time = (DateTime.Now.Ticks - DateTime.Parse("1970-1-1").Ticks) / 10000;
            string id = "c0-id=" + Math.Floor(rand.NextDouble() * 10001).ToString() + "_" + time.ToString() + "\n";
            string pwd = "c0-param0=string:" + code.Trim() + "\n";
            string tail = "xml=true\n";
            string data = constStr + id + pwd + tail;

            HttpWebRequest req;
            HttpWebResponse resp;

            ASCIIEncoding encode = new ASCIIEncoding();
            byte[] byteData = encode.GetBytes(data);
            req = (HttpWebRequest)WebRequest.Create(url.ToString() + "dwr/exec/drcom.Cipher.dwr");
            req.SendChunked = true;
            req.Method = "post";
            req.KeepAlive = true;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.117 Safari/537.36\r\n";
            req.CookieContainer = this.cookieContainer;
            req.ContentLength = byteData.Length;
            req.ServicePoint.Expect100Continue = false;
            req.ContentType = "text/plain";
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(byteData, 0, byteData.Length);
            }
            try
            {
                using (resp = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        string pass = "";
                        while (!pass.Contains("var"))
                        {
                            pass = reader.ReadLine();
                        }
                        //response返回的字符串本身含转义字符
                        //而读取时忽略了转义字符为正常字符
                        //暂时用去掉\的方式
                        pass = pass.Substring(pass.IndexOf("\"") + 1, pass.LastIndexOf("\"") - pass.IndexOf("\"") - 1);
                        int i = pass.IndexOf("\\");
                        if (i > 0)
                            pass = pass.Remove(pass.IndexOf("\\"), 1);
                        return pass;
                    }
                }
            }
            catch (WebException em)
            {
                throw em;
            }
        }
        /// <summary>
        /// 密码处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxPwd_Leave(object sender, EventArgs e)
        {
            string pwd = tbxPwd.Text;
            if (pwd == "")
                lblTips.Text = "请先输入密码";
            else
            {
                lblTips.Text = "";
                try
                {
                    string str = getPwd(pwd);
                    if (str != "")
                        tbxPwd.Text = str;
                    else
                        MessageBox.Show("获取密码失败");
                }
                catch (WebException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                catch (Exception em)
                {
                    MessageBox.Show(em.Message);
                }
            }
        }

        private void tbxPwd_Click(object sender, EventArgs e)
        {
            tbxPwd.Clear();
        }
        /// <summary>
        /// post向LoginAction.action提交出错
        /// 使用get url=LoginAction.action?+参数提交成功
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool login(string data)
        {
            #region=====post方式=====
            //UTF8Encoding encode = new UTF8Encoding();
            //byte[] byteData = encode.GetBytes(data);

            //req = (HttpWebRequest)WebRequest.Create(url.ToString() + "LoginAction.action");
            //req.SendChunked = true;
            //req.Method = "post";
            //req.KeepAlive = true;
            //req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            //req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.117 Safari/537.36\r\n";
            //req.ContentType = "application/x-www-form-urlencoded";
            //req.CookieContainer = this.cookieContainer;
            //req.ContentLength = byteData.Length;
            ////req.ServicePoint.Expect100Continue = false;
            //using (Stream stream = req.GetRequestStream())
            //{
            //    stream.Write(byteData, 0, byteData.Length);
            //}
            #endregion
            Uri loginUrl = new Uri(url.ToString() + "LoginAction.action?" + data);
            bool isDone = false;
            try
            {
                string resqContent = doGet(loginUrl);
                if (resqContent.Contains("验证码错误"))
                {
                    lblTips.Text = "登录失败, 验证码错误";
                
                }
                else if (resqContent.Contains("账号或密码错误"))
                {
                    lblTips.Text = "登录失败, 账号或密码错误";
                    tbxPwd.Focus();
                }
                else if (resqContent.Contains("欢迎您！您当前剩余储值"))
                {
                    isDone = true;
                }
               
                return isDone;
            }
            catch (WebException em)
            {
                throw em;
            }
        }
        #endregion
        #region 账号状态处理
        private bool selfStop() {
            bool isDone = false;
            try
            {
                Uri urlStop = new Uri(url.ToString() + "nav_selfstopNow");
                //Uri urlReopen = new Uri(url.ToString() + "nav_SelfReopenNow");
                string content = doGet(urlStop);
                if (content.Contains("<p>操作状态：成功/正常。请注销后重新登录。</p>"))
                {
                    isDone = true;

                }
                else if (content.Contains("<p>操作状态：账号状态不符。</p>"))
                {
                    isDone = true;
                }
                else if (content.Contains("立即报停出现异常"))
                {
                    isDone = false;
                    MessageBox.Show("报停出现异常");
                }
                return isDone;
            }
            catch (WebException em) {
                throw em;
            }
        }
        private bool reOpen() {
            bool isDone = false;
            try
            {
                Uri urlReopen = new Uri(url.ToString() + "nav_SelfReopenNow");
                string content = doGet(urlReopen);
                if (content.Contains("<p>操作状态：成功/正常。请注销后重新登录。</p>"))
                {
                    isDone = true;
                }
                else if (content.Contains("<p>操作状态：账号状态不符。</p>"))
                {
                    isDone = true;
                }
                else if (content.Contains("立即复通出现异常"))
                {
                    isDone = false;
                    MessageBox.Show("立即复通出现异常");
                }
                return isDone;
            }
            catch (WebException em)
            {
                throw em;
            }
        }
        #endregion
        #region 线程的操作
        delegate void myDelegate();
        private void updateUI() 
        {
            for (int i = 0; i < 200; i++)
            {
                Thread.Sleep(5);
                //this.lblProgress.Text = progressBar.Value.ToString();
                this.progressBar.PerformStep();
            }
        }
        private void opClient()
        {
             autoEvent = new AutoResetEvent(false);
             try
             {
                 Delegate show = new myDelegate(updateUI);
                client.Run();//3、运行客户端，登录
                //updateUI("正在启动校园网客户端");
                Thread.Sleep(2000);
                client.autoLogin();
                this.BeginInvoke(show);//子线程告诉窗体线程来完成相应的控件操作。
                autoEvent.Set();
             }
             catch
             {
                MessageBox.Show("无法启动客户端，请尝试手动登录");
              }
        }
        #endregion
        
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //复通-->自动登录--->报停
       //这个函数怎么不面向过程设计
        private void btnLogin_Click(object sender, EventArgs e)
        {
            #region 登录准备工作
            if (loginCode == "")
            {
                MessageBox.Show("Error");
                return;
            }
            lblTips.Text = "";
            string account = "1105221" + tbxName.Text.ToString().Trim();
            if (account == "")
            {
                lblTips.Text = "账号在哪里？";
                tbxName.Focus();
                return;
            }
            if (verify == "")
            {
                lblTips.Text = "验证码识别错误";
                return;
            }
            string pwd = tbxPwd.Text.Trim();
            if (pwd == "")
            {
                lblTips.Text = "密码在哪里？";
                tbxPwd.Focus();
                return;
            }
            pwd += loginCode.Trim();
            bool result;
            MD5.MD5Encryption enc = new MD5.MD5Encryption();
            string data = "account=" + account + "&password=" + enc.toMD5(pwd) + "&code=" + verify.Trim();
            #endregion
            try
            {
                progressBar.Visible = true;
                progressBar.Minimum = 0;
                progressBar.Maximum = 1000;
                progressBar.Value = 0;
                progressBar.Step = 1;
                //调用客户端进程太慢，创建线程

                result = login(data);//1、登录网站

                updateUI();
                Thread th = new Thread(new ThreadStart(opClient));
                th.IsBackground = true;
                th.Start();
                if (result == true)
                {
                    if (reOpen() == true)//2、复通账号
                    {
                        updateUI();
                        autoEvent.WaitOne();
                        updateUI();
                        if (client.autoLogin())
                        {
                            if (cbxSavePwd.Checked)//记住密码
                                tool.Save(tbxName.Text, currentPwd);
                            else
                                tool.Delete();
                            
                           //Thread.Sleep(3000);//还是要等登录完成
                           updateUI();
                            
                            result = selfStop();//4、报停
                            MessageBox.Show( "正在免费使用");
                            this.Close();     
                      }
                    }
                }
                else
                {
                    getCookie(url);
                    Identify();
                    progressBar.Value = 0;
                }
            }
            catch (WebException we)
            {
                MessageBox.Show(we.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
