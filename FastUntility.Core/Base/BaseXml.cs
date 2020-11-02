using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace FastUntility.Core.Base
{/// <summary>
 /// 标签：2015.7.13，魏中针
 /// 说明：XML处理
 /// </summary>
    public static class BaseXml
    {
        #region 返回字符串列表 文件 
        /// <summary>
        /// 返回字符串列表 文件 
        /// </summary>
        /// <param name="FileName">文件名</param>
        /// <param name="xmlNodel">结点</param>
        /// <returns></returns>
        public static List<string> GetXmlListForFile(string FileName, string xmlNodel)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(File.ReadAllText(FileName));
                var nodelList = xmlDoc.SelectNodes(xmlNodel);
                var list = new List<string>();

                foreach (XmlNode item in nodelList)
                {
                    list.Add(item.InnerXml);
                }

                xmlDoc = null;
                return list;
            }
            catch
            {
                return new List<string>();
            }
        }
        #endregion

        #region 根据XML返回结点中的对象列表
        /// <summary>
        /// 根据XML返回结点中的对象列表
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="xmlValue">xml值</param>
        /// <param name="xmlNodel">结点</param>
        /// <returns></returns>
        public static List<T> GetXmlList<T>(string xmlValue, string xmlNodel)
        {
            try
            {
                var xmlHead = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
                var xmlDoc = new XmlDocument();
                var list = new List<T>();
                xmlDoc.LoadXml(xmlValue);
                var nodelList = xmlDoc.SelectNodes(xmlNodel);

                foreach (XmlNode item in nodelList)
                {
                    xmlValue = xmlHead + "<" + item.LocalName + ">" + item.InnerXml + "</" + item.LocalName + ">";
                    using (var sR = new StringReader(xmlValue))
                    {
                        var xmlDes = new XmlSerializer(typeof(T));
                        list.Add((T)xmlDes.Deserialize(sR));
                        sR.Close();
                    }
                }

                xmlDoc = null;
                return list;
            }
            catch
            {
                return new List<T>();
            }
        }
        #endregion
        
        #region 返回字符串列表
        /// <summary>
        /// 返回字符串列表
        /// </summary>
        /// <param name="xmlValue">xml</param>
        /// <param name="xmlNodel">结点</param>
        /// <returns></returns>
        public static List<string> GetXmlList(string xmlValue, string xmlNodel)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlValue);
                var nodelList = xmlDoc.SelectNodes(xmlNodel);
                var list = new List<string>();

                foreach (XmlNode item in nodelList)
                {
                    list.Add(item.InnerXml);
                }
                xmlDoc = null;
                return list;
            }
            catch
            {
                return new List<string>();
            }
        }
        #endregion
        
        #region 返回字符串
        /// <summary>
        /// 返回字符串
        /// </summary>
        /// <param name="xmlValue">xml</param>
        /// <param name="xmlNodel">结点</param>
        /// <returns></returns>
        public static string GetXmlString(string xmlValue, string xmlNodel)
        {
            var list = GetXmlList(xmlValue, xmlNodel);

            if (list.Count != 0)
                return list.First();
            else
                return "";
        }
        #endregion
    }
}