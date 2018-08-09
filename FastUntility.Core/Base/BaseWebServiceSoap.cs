using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FastUntility.Core.Base
{
    /// <summary>
    /// web service 动态调用，通过soap协议，可以设置超时时间
    /// </summary>
    public static class BaseWebServiceSoap
    {
        //缓存xmlNamespace，避免重复调用GetNamespace  
        private static Hashtable xmlNameSpace = new Hashtable();

        #region 通过SOAP协议动态调用webservice
        /// <summary>  
        /// 通过SOAP协议动态调用webservice   
        /// </summary>  
        /// <param name="url"> webservice地址</param>  
        /// <param name="methodName"> 调用方法名</param>  
        /// <param name="pars"> 参数表</param>  
        /// <returns> 结果集xml</returns>  
        public static XmlDocument QuerySoapWebService(string url, string methodName, Hashtable pars, int timeOut)
        {
            var refValue = "";
            //名字空间在缓存中是否存在
            if (xmlNameSpace.ContainsKey(url))
            {
                //存在时，读取缓存，然后执行调用  
                return QuerySoapWebService(url, methodName, pars, xmlNameSpace[url].ToString(), timeOut, ref refValue);
            }
            else
            {
                //不存在时直接从wsdl的请求中读取名字空间，然后执行调用  
                return QuerySoapWebService(url, methodName, pars, GetNamespace(url), timeOut, ref refValue);
            }
        }
        #endregion

        #region 通过SOAP协议动态调用webservice
        /// <summary>  
        /// 通过SOAP协议动态调用webservice   
        /// </summary>  
        /// <param name="url"> webservice地址</param>  
        /// <param name="methodName"> 调用方法名</param>  
        /// <param name="pars"> 参数表</param>  
        /// <returns> 结果集xml</returns>  
        public static XmlDocument QuerySoapWebService(string url, string methodName, Hashtable pars, int timeOut, ref string refValue)
        {
            //名字空间在缓存中是否存在
            if (xmlNameSpace.ContainsKey(url))
            {
                //存在时，读取缓存，然后执行调用  
                return QuerySoapWebService(url, methodName, pars, xmlNameSpace[url].ToString(), timeOut, ref refValue);
            }
            else
            {
                //不存在时直接从wsdl的请求中读取名字空间，然后执行调用  
                return QuerySoapWebService(url, methodName, pars, GetNamespace(url), timeOut, ref refValue);
            }
        }
        #endregion

        #region 通过SOAP协议动态调用webservice
        /// <summary>  
        /// 通过SOAP协议动态调用webservice    
        /// </summary>  
        /// <param name="url"> webservice地址</param>  
        /// <param name="methodName"> 调用方法名</param>  
        /// <param name="pars"> 参数表</param>  
        /// <param name="xmlNs"> 名字空间</param>  
        /// <returns> 结果集</returns>  
        private static XmlDocument QuerySoapWebService(string url, string methodName, Hashtable pars, string xmlNs, int timeOut, ref string refValue)
        {
            xmlNameSpace[url] = xmlNs;//加入缓存，提高效率  

            //并发连接数
            System.Net.ServicePointManager.DefaultConnectionLimit = 200;

            // 获取请求对象  
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

            // 设置请求head  
            request.Timeout = 1000 * 60 * timeOut;
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.Headers.Add("SOAPAction", "\"" + xmlNs + (xmlNs.EndsWith("/") ? "" : "/") + methodName + "\"");

            // 设置请求身份  
            SetWebRequest(request);

            // 获取soap协议  
            byte[] data = EncodeParsToSoap(pars, xmlNs, methodName);

            // 将soap协议写入请求  
            WriteRequestData(request, data);
            XmlDocument returnDoc = new XmlDocument();
            XmlDocument returnValueDoc = new XmlDocument();

            // 读取服务端响应  
            returnDoc = ReadXmlResponse(request.GetResponse());

            XmlNamespaceManager mgr = new XmlNamespaceManager(returnDoc.NameTable);
            mgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");

            // 返回结果  
            refValue = returnDoc.InnerText;

            var length = refValue.IndexOf('<');

            if (length != -1)
                refValue = refValue.Substring(length, refValue.Length - length);

            string RetXml = returnDoc.SelectSingleNode("//soap:Body/*/*", mgr).InnerXml;

            returnValueDoc.LoadXml("<root>" + RetXml + "</root>");
            AddDelaration(returnValueDoc);

            return returnValueDoc;
        }
        #endregion

        #region 获取wsdl中的名字空间
        /// <summary>  
        /// 获取wsdl中的名字空间  
        /// </summary>  
        /// <param name="url"> wsdl地址</param>  
        /// <returns> 名字空间</returns>  
        private static string GetNamespace(String url)
        {
            // 创建wsdl请求对象，并从中读取名字空间  
            var request = (HttpWebRequest)WebRequest.Create(url + "?WSDL");
            SetWebRequest(request);
            var response = request.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                var doc = new XmlDocument();
                doc.LoadXml(sr.ReadToEnd());
                sr.Close();
                return doc.SelectSingleNode("//@targetNamespace").Value;
            }
        }
        #endregion

        #region 加入soapheader节点
        /// <summary>  
        /// 加入soapheader节点  
        /// </summary>  
        /// <param name="doc"> soap文档</param>  
        private static void InitSoapHeader(XmlDocument doc)
        {
            // 添加soapheader节点  
            var soapHeader = doc.CreateElement("soap", "Header", "http://schemas.xmlsoap.org/soap/envelope/");

            doc.ChildNodes[0].AppendChild(soapHeader);
        }
        #endregion

        #region 将以字节数组的形式返回soap协议
        /// <summary>  
        /// 将以字节数组的形式返回soap协议  
        /// </summary>  
        /// <param name="pars"> 参数表</param>  
        /// <param name="xmlNs"> 名字空间</param>  
        /// <param name="methodName"> 方法名</param>  
        /// <returns> 字节数组</returns>  
        private static byte[] EncodeParsToSoap(Hashtable pars, string xmlNs, string methodName)
        {
            var doc = new XmlDocument();

            // 构建soap文档  
            doc.LoadXml("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"></soap:Envelope>");

            // 加入soapbody节点  
            InitSoapHeader(doc);

            // 创建soapbody节点  
            var soapBody = doc.CreateElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");

            // 根据要调用的方法创建一个方法节点  
            var soapMethod = doc.CreateElement(methodName);
            soapMethod.SetAttribute("xmlns", xmlNs);

            // 遍历参数表中的参数键  
            foreach (string key in pars.Keys)
            {
                // 根据参数表中的键值对，生成一个参数节点，并加入方法节点内  
                var soapPar = doc.CreateElement(key);
                soapPar.InnerXml = ObjectToSoapXml(pars[key]);
                soapMethod.AppendChild(soapPar);
            }

            // soapbody节点中加入方法节点  
            soapBody.AppendChild(soapMethod);

            // soap文档中加入soapbody节点  
            doc.DocumentElement.AppendChild(soapBody);

            // 添加声明  
            AddDelaration(doc);

            // 传入的参数有DataSet类型，必须在序列化后的XML中的diffgr:diffgram/NewDataSet节点加xmlns='' 否则无法取到每行的记录。  
            var node = doc.DocumentElement.SelectSingleNode("//NewDataSet");
            if (node != null)
            {
                var attr = doc.CreateAttribute("xmlns");
                attr.InnerText = "";
                node.Attributes.Append(attr);
            }

            // 以字节数组的形式返回soap文档  
            return Encoding.UTF8.GetBytes(doc.OuterXml);
        }
        #endregion

        #region 将参数对象中的内容取出
        /// <summary>  
        /// 将参数对象中的内容取出  
        /// </summary>  
        /// <param name="o">参数值对象</param>  
        /// <returns>字符型值对象</returns>  
        private static string ObjectToSoapXml(object o)
        {
            using (var ms = new MemoryStream())
            {
                var mySerializer = new XmlSerializer(o.GetType());
                mySerializer.Serialize(ms, o);
                var doc = new XmlDocument();
                doc.LoadXml(Encoding.UTF8.GetString(ms.ToArray()));
                if (doc.DocumentElement != null)
                {
                    return doc.DocumentElement.InnerXml;
                }
                else
                {
                    return o.ToString();
                }
            }
        }
        #endregion

        #region 设置请求身份
        /// <summary>  
        /// 设置请求身份  
        /// </summary>  
        /// <param name="request"> 请求</param>  
        private static void SetWebRequest(HttpWebRequest request)
        {
            request.Credentials = CredentialCache.DefaultCredentials;
            //request.Timeout = 10000;  
        }
        #endregion

        #region 将soap协议写入请求
        /// <summary>  
        /// 将soap协议写入请求  
        /// </summary>  
        /// <param name="request"> 请求</param>  
        /// <param name="data"> soap协议</param>  
        private static void WriteRequestData(HttpWebRequest request, byte[] data)
        {
            request.ContentLength = data.Length;

            using (var writer = request.GetRequestStream())
            {
                writer.Write(data, 0, data.Length);
                writer.Close();
            }
        }
        #endregion

        #region 将响应对象读取为xml对象
        /// <summary>  
        /// 将响应对象读取为xml对象  
        /// </summary>  
        /// <param name="response"> 响应对象</param>  
        /// <returns> xml对象</returns>  
        private static XmlDocument ReadXmlResponse(WebResponse response)
        {
            using (var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                var retXml = sr.ReadToEnd();
                sr.Close();

                var doc = new XmlDocument();
                doc.LoadXml(retXml);

                return doc;
            }
        }
        #endregion

        #region 给xml文档添加声明
        /// <summary>  
        /// 给xml文档添加声明  
        /// </summary>  
        /// <param name="doc"> xml文档</param>  
        private static void AddDelaration(XmlDocument doc)
        {
            var decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.InsertBefore(decl, doc.DocumentElement);
        }
        #endregion

        #region 通过SOAP协议动态调用webservice
        /// <summary>
        /// 通过SOAP协议动态调用webservice
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="methodName">方法名</param>
        /// <param name="pars">参数</param>
        /// <param name="timeOut">时间</param>
        /// <returns></returns>
        public static String QuerySoapWebServiceString(string url, string methodName, Hashtable pars, int timeOut)
        {
            var refValue = "";
            var doc = QuerySoapWebService(url, methodName, pars, timeOut, ref refValue);
            return doc.InnerText;
        }
        #endregion

        #region 通过SOAP协议动态调用webservice
        /// <summary>
        /// 通过SOAP协议动态调用webservice
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="methodName">方法名</param>
        /// <param name="pars">参数</param>
        /// <param name="timeOut">时间</param>
        /// <returns></returns>
        public static String QuerySoapWebServiceString(string url, string methodName, Hashtable pars, int timeOut, ref string refValue)
        {
            var doc = QuerySoapWebService(url, methodName, pars, timeOut, ref refValue);
            return doc.InnerText;
        }
        #endregion
    }
}
