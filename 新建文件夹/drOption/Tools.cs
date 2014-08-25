using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;//序列化为二进制

namespace drOption
{   
    [Serializable]
    public class User 
    {
    private string loginID = string.Empty;
    public string LoginID
    {
        get { return loginID; }
        set { loginID = value; }
    }
    private string pwd = string.Empty;
    public string Pwd
    {
        get { return pwd; }
        set { pwd = value; }
    }
    }
  
    public  class Tools
    {
        //窗体加载
        public List<string>  Read() {
            List<string> userData=new List<string>();
            User user = new User();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\YouKnow\\user.dat";
            if (!File.Exists(path))
                return userData;
            
            FileStream readFile = new FileStream(path, FileMode.Open, FileAccess.Read);
            IFormatter deserializer = new BinaryFormatter();
            user=deserializer.Deserialize(readFile) as User;
            userData.Add(user.LoginID);
            userData.Add(user.Pwd);
            readFile.Close();
            return userData;
        }
        //验证账号是否保存
        public List<string> CheckId(string id)
        {
            List<string> userData = new List<string>();
            User user = new User();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\YouKnow\\user.dat";
            if (!File.Exists(path))
                return userData;

            FileStream readFile = new FileStream(path, FileMode.Open, FileAccess.Read);
            IFormatter deserializer = new BinaryFormatter();
            user = deserializer.Deserialize(readFile) as User;
            if (user.LoginID == id)
            {
                userData.Add(user.LoginID);
                userData.Add(user.Pwd);
            }
            readFile.Close();
            return userData;
        }
        //只对应一个账号
        public void Delete(){
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\YouKnow\\user.dat";
            if(Directory.Exists(path))
            File.Delete(path);
        }
        //保存登录信息
        //只记住一个账户
        public void Save(string id,string pwd) {
            User userData = new User();
            userData.LoginID = id;
            userData.Pwd = pwd;
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\YouKnow";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                FileStream saveFile = new FileStream(path+"\\user.dat", FileMode.Create, FileAccess.Write, FileShare.None);
                IFormatter serializer = new BinaryFormatter();
                serializer.Serialize(saveFile, userData);
                saveFile.Close();
            }
            catch (SerializationException se)
            {
                throw se;
            }
            catch (IOException ioe)
            {
                throw ioe;
            }
        }
    }
}
