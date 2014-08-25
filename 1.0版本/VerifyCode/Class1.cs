using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;

namespace VerifyCode
{
    public  class verifyChar//验证码符号 类 
    {
        public verifyChar(string k, string v, int l)
        {
            key = k;
            value = v;
            length = l;
        }
        public string key { get; set; }
        public string value { get; set; }
        public int length { get; set; }

    }
    public class opImage
    {
        private int intPara;
        private string strPara;
        //二值化读取图片数组
        private int[,] readImageNum(Image img) //返回值类型不同不能重载？因为返回值类型不是函数签名（名字，参数类型、个数）的一部分
        {
            int width = img.Width;
            int height = img.Height;
            int[,] numbers = new int[width, height];
            Bitmap bitMap = new Bitmap(img);
            Color pixelColor;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixelColor = bitMap.GetPixel(x, y);
                    int r, g, b, result = 0;
                    r = pixelColor.R;
                    g = pixelColor.G;
                    b = pixelColor.B;
                    result = ((int)(0.7 * r) + (int)(0.2 * g) + (int)(0.1 * b));
                    if (result < 165)
                        numbers[x, y] = 1;
                    else
                        numbers[x, y] = 0;
                }
            }
            return numbers;
        }
        //分割验证码 用集合保存字符
        //对返回类verifyChar要求可访问性限制，对成员函数readImageNum不要求？
        public List<verifyChar> toList(Image img)
        {
            int[,] nums = readImageNum(img);
            int count = 0;//列计数
            string s = "";//保存列植

            // 键值对也可用IDictionary.......支持两项
            //IDictionary<string,string>
            StringBuilder sb = new StringBuilder();
            List<verifyChar> lib = new List<verifyChar> { };
            for (int w = 0; w < 70; w++)
            {
                count = 0;
                s = "";
                for (int h = 4; h < 16; h++)
                {
                    if (nums[w, h] == 1)
                        count++;
                    s += nums[w, h].ToString();
                }
                if (count > 0)
                {
                    sb.Append(s);
                    if (w == 69)
                    {
                        verifyChar v = new verifyChar("", sb.ToString(), sb.ToString().Length);
                        lib.Add(v);
                        sb.Remove(0, sb.Length);
                    }
                }
                else if (sb.Length > 0)//一个字符读取结束
                {
                    verifyChar v = new verifyChar("", sb.ToString(), sb.ToString().Length);
                    lib.Add(v);
                    sb.Remove(0, sb.Length);
                }
            }
            return lib;
        }
        //从xml 文件读取验证码校验数据
        public void loadXml(ref List<verifyChar> list)
        {
            string path = System.Environment.CurrentDirectory + @"\data.xml";
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(path);
                XmlNode root = doc.DocumentElement;


                foreach (XmlNode node in root.ChildNodes)
                {
                    string key = node.Name;
                    int length = Convert.ToInt32(node.Attributes["length"].Value);
                    string value = node.FirstChild.InnerText;
                    verifyChar vc = new verifyChar(key, value, length);
                    list.Add(vc);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //识别验证码
        private bool findValue(verifyChar lib)
        {
            if ((lib.length == intPara) && (lib.value == strPara))
                return true;
            else
                return false;
        }
        public string identifyImg(List<verifyChar> temp, List<verifyChar> list) 
        {
            verifyChar result;
            string rtnKey = "";
            foreach (verifyChar vc in temp)
            {
                intPara = vc.length;
                strPara = vc.value;
                result = list.Find(findValue);
                //is null or empty
                bool got = string.IsNullOrEmpty(result.key);//(result == null ? false : (result.key == "" ? false : true));
                if (!got)
                {
                    rtnKey += result.key.Substring(1, 1);
                }
                else
                    break;
            }
            return rtnKey;
        }
    }
}
