using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace NHExt.Runtime.Extend.Util
{
    public class HttpRequestHelper
    {
        public static NHExt.Runtime.Web.HttpHandler.DirectResponse DoRemoteRequest(string url, string action, string args, string auth)
        {
            string remotePostDate = "action=" + action + "&Args=" + args + "&Auth=" + auth + "}";

            byte[] data = new UTF8Encoding().GetBytes(remotePostDate.ToString());
            // 发送请求
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = data.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            // 获得回复
            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string resultStr = reader.ReadToEnd();
            reader.Close();
            JToken jToken = JToken.Parse(resultStr);
            return NHExt.Runtime.Web.DirectResponseSerialize.DeSerialize<object>(jToken);
        }

        public static NHExt.Runtime.Web.HttpHandler.DirectResponse DoRemoteRequest(string url, string action, string args)
        {
            string remotePostDate = "action=" + action + "&Args=" + args + "}";

            byte[] data = new UTF8Encoding().GetBytes(remotePostDate.ToString());
            // 发送请求
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = data.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            // 获得回复
            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string resultStr = reader.ReadToEnd();
            reader.Close();
            JToken jToken = JToken.Parse(resultStr);
            return NHExt.Runtime.Web.DirectResponseSerialize.DeSerialize<object>(jToken);


        }

        public static string DoRemoteStringRequest(string url, string action, string args, string auth)
        {
            string remotePostDate = "action=" + action + "&Args=" + args + "&Auth=" + auth + "}";

            byte[] data = new UTF8Encoding().GetBytes(remotePostDate.ToString());
            // 发送请求
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            // 获得回复
            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string resultStr = reader.ReadToEnd();
            reader.Close();
            return resultStr;

        }
        public static string DoRemoteStringRequest(string url, string action, string args)
        {
            string remotePostDate = "action=" + action + "&Args=" + args + "}";

            byte[] data = new UTF8Encoding().GetBytes(remotePostDate.ToString());
            // 发送请求
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            // 获得回复
            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string resultStr = reader.ReadToEnd();
            reader.Close();
            return resultStr;

        }
    }
}
