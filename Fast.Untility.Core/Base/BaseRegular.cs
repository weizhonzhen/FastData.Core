using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using Fast.Untility.Core.Attributes;

namespace Fast.Untility.Core.Base
{
    public static class BaseRegular
    {
        #region 转时间格式化
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：转时间格式化
        /// </summary>
        public static DateTime? ToDate(this object strValue)
        {
            if (strValue.ToStr() == "")
                return null;
            return Convert.ToDateTime(strValue);
        }

        public static DateTime ToDate(this string strValue)
        {
            if (strValue.ToStr() == "")
                return DateTime.MinValue;
            return Convert.ToDateTime(strValue);
        }

        public static string ToDate(this object strValue, string format)
        {
            if (strValue.ToStr() == "")
                return null;
            else
                return Convert.ToDateTime(strValue).ToString(format);
        }

        public static string ToDate(this DateTime strValue, string format)
        {
            if (strValue.ToStr() == "")
                return null;
            else
                return Convert.ToDateTime(strValue).ToString(format);
        }
        #endregion

        #region 验证时间
        /// <summary>
        /// 验证时间
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsDate(this string dateValue)
        {
            DateTime date = new DateTime();

            if (DateTime.TryParse(dateValue, out date))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region string转向Int
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：string型转换为Int32类型
        /// </summary>
        /// <returns></returns>
        public static int ToInt(this string str, int defValue)
        {
            int tmp = 0;
            if (Int32.TryParse(str, out tmp))
                return (int)tmp;
            else
                return defValue;
        }
        #endregion

        #region string型转换为float型
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的float类型结果</returns>
        public static float ToFloat(this string strValue, float defValue)
        {
            float tmp = 0;
            if (float.TryParse(strValue, out tmp))
                return (float)tmp;
            else
                return defValue;
        }
        #endregion

        #region string型转换为double型
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：string型转换为double型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的double类型结果</returns>
        public static double ToDouble(this string strValue, double defValue)
        {
            double tmp = 0;
            if (double.TryParse(strValue, out tmp))
                return (double)tmp;
            else
                return defValue;
        }
        #endregion

        #region string型转换为long型
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：string型转换为long型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的double类型结果</returns>
        public static long ToLong(this string strValue, long defValue)
        {
            long tmp = 0;
            if (Int64.TryParse(strValue, out tmp))
                return (long)tmp;
            else
                return defValue;
        }
        #endregion

        #region string型转换为Decimal型
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：string型转换为Decimal型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的double类型结果</returns>
        public static Decimal ToDecimal(this string strValue, Decimal defValue)
        {
            Decimal tmp = 0;
            if (Decimal.TryParse(strValue, out tmp))
                return (decimal)tmp;
            else
                return defValue;
        }
        #endregion

        #region string型转换为byte型
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：string型转换为byte型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的byte类型结果</returns>
        public static byte ToByte(this string strValue, byte defValue)
        {
            byte tmp = 0;
            if (byte.TryParse(strValue, out tmp))
                return (byte)tmp;
            else
                return defValue;
        }
        #endregion

        #region string型转换为Int16型
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：string型转换为Int16型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的Int16类型结果</returns>
        public static Int16 ToInt16(this string strValue, Int16 defValue)
        {
            Int16 tmp = 0;
            if (Int16.TryParse(strValue, out tmp))
                return (Int16)tmp;
            else
                return defValue;
        }
        #endregion

        #region object型转换为string
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：object型转换为string
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <returns>转换后的Int16类型结果</returns>
        public static string ToStr(this object strValue)
        {
            if (strValue == null)
                return "";
            else
                return strValue.ToString();
        }
        #endregion

        #region 验证固定号码
        /// <summary>
        /// 验证固定号码
        /// </summary>
        /// <param name="telephone"></param>
        /// <returns></returns>
        public static bool IsTelephone(this string telephone)
        {
            if (telephone == null)
                return false;
            return Regex.IsMatch(telephone, @"^(\d{3,4}-)?\d{6,8}$");
        }
        #endregion

        #region 验证手机号码
        /// <summary>
        /// 验证手机号码
        /// </summary>
        /// <param name="mobilePhone"></param>
        /// <returns></returns>
        public static bool IsMobilePhone(this string mobilePhone)
        {
            if (mobilePhone == null || mobilePhone.Length > 11)
                return false;
            return Regex.IsMatch(mobilePhone, @"^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}");
        }
        #endregion

        #region 验证身份证号
        /// <summary>  
        /// 验证身份证号（不区分一二代身份证号）  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsIDCard(this string input)
        {
            if (input.Length == 18)
                return IsIDCard18(input);
            else if (input.Length == 15)
                return IsIDCard15(input);
            else
                return false;
        }
        #endregion

        #region 验证一代身份证号（15位数）
        /// <summary>  
        /// 验证一代身份证号（15位数）  
        /// [长度为15位的数字；匹配对应省份地址；生日能正确匹配]  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        private static bool IsIDCard15(string input)
        {
            //验证是否可以转换为15位整数  
            long l = 0;
            if (!long.TryParse(input, out l) || l.ToString().Length != 15)
            {
                return false;
            }

            //验证省份是否匹配  
            //1~6位为地区代码，其中1、2位数为各省级政府的代码，3、4位数为地、市级政府的代码，5、6位数为县、区级政府代码。  
            string address = "11,12,13,14,15,21,22,23,31,32,33,34,35,36,37,41,42,43,44,45,46,50,51,52,53,54,61,62,63,64,65,71,81,82,91,";
            if (!address.Contains(input.Remove(2) + ","))
            {
                return false;
            }
            //验证生日是否匹配  
            string birthdate = input.Substring(6, 6).Insert(4, "/").Insert(2, "/");
            DateTime dt;
            if (!DateTime.TryParse(birthdate, out dt))
            {
                return false;
            }

            return true;
        }
        #endregion

        #region 验证二代身份证号（18位数，GB11643-1999标准）
        /// <summary>  
        /// 验证二代身份证号（18位数，GB11643-1999标准）  
        /// [长度为18位；前17位为数字，最后一位(校验码)可以为大小写x；匹配对应省份地址；生日能正确匹配；校验码能正确匹配]  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        private static bool IsIDCard18(string input)
        {
            //验证是否可以转换为正确的整数  
            long l = 0;
            if (!long.TryParse(input.Remove(17), out l) || l.ToString().Length != 17 || !long.TryParse(input.Replace('x', '0').Replace('X', '0'), out l))
            {
                return false;
            }
            //验证省份是否匹配  
            //1~6位为地区代码，其中1、2位数为各省级政府的代码，3、4位数为地、市级政府的代码，5、6位数为县、区级政府代码。  
            string address = "11,12,13,14,15,21,22,23,31,32,33,34,35,36,37,41,42,43,44,45,46,50,51,52,53,54,61,62,63,64,65,71,81,82,91,";
            if (!address.Contains(input.Remove(2) + ","))
            {
                return false;
            }
            //验证生日是否匹配  
            string birthdate = input.Substring(6, 8).Insert(6, "/").Insert(4, "/");
            DateTime dt;
            if (!DateTime.TryParse(birthdate, out dt))
            {
                return false;
            }
            //校验码验证  
            //校验码：  
            //（1）十七位数字本体码加权求和公式   
            //S = Sum(Ai * Wi), i = 0, ... , 16 ，先对前17位数字的权求和   
            //Ai:表示第i位置上的身份证号码数字值   
            //Wi:表示第i位置上的加权因子   
            //Wi: 7 9 10 5 8 4 2 1 6 3 7 9 10 5 8 4 2   
            //（2）计算模   
            //Y = mod(S, 11)   
            //（3）通过模得到对应的校验码   
            //Y: 0 1 2 3 4 5 6 7 8 9 10   
            //校验码: 1 0 X 9 8 7 6 5 4 3 2   
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = input.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != input.Substring(17, 1).ToLower())
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 取身份证的出生日期
        /// <summary>
        /// 取身份证的出生日期
        /// </summary>
        /// <param name="IdCard">身份证</param>
        /// <returns></returns>
        public static DateTime GetIdCardBirthday(this string IdCard)
        {
            //计算出生日期
            var birthday = "";

            //处理18位的身份证号码
            if (IdCard.Length == 18)
                birthday = string.Format("{0}-{1}-{2}", IdCard.Substring(6, 4), IdCard.Substring(10, 2), IdCard.Substring(12, 2));


            //处理15位的身份证号码
            if (IdCard.Length == 15)
                birthday = string.Format("19{0}-{1}-{2}", IdCard.Substring(6, 2), IdCard.Substring(8, 2), IdCard.Substring(10, 2));

            if (birthday.IsDate())
                return Convert.ToDateTime(birthday);
            else
                return Convert.ToDateTime("1900-01-01");
        }
        #endregion

        #region 是否中文,空默认为中文
        /// <summary>
        /// 是否中文,空默认为中文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsZhString(this string str, bool IsDefaule = true)
        {
            if (String.IsNullOrEmpty(str))
                return IsDefaule;

            try
            {
                Match mInfo = Regex.Match(str, @"[\u4e00-\u9fa5]");

                if (mInfo.Success)
                    return true;
                else
                    return false;
            }
            catch
            {
                return IsDefaule;
            }
        }
        #endregion

        #region 获取特性内容
        /// <summary>
        /// 获取特性内容
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="item">泛型成员</param>
        /// <returns></returns>
        public static string ToEnum(this Enum item)
        {
            var value = "";
            foreach (Attribute temp in item.GetType().GetField(item.ToString()).GetCustomAttributes())
            {
                if (temp.GetType() == typeof(RemarkAttribute))
                {
                    value = ((RemarkAttribute)temp).Remark;
                    break;
                }
            }

            return value;
        }
        #endregion

        #region url
        /// <summary>
        /// url 
        /// </summary>
        /// <param name="HtmlString"></param>
        /// <returns></returns>
        public static string Transform(string HtmlString)
        {
            HtmlString = HtmlString.Replace(" ", "-");
            HtmlString = HtmlString.Replace("<", "-");
            HtmlString = HtmlString.Replace(">", "-");
            HtmlString = HtmlString.Replace("*", "-");
            HtmlString = HtmlString.Replace("?", "-");
            HtmlString = HtmlString.Replace(",", "");
            HtmlString = HtmlString.Replace("/", "-");
            HtmlString = HtmlString.Replace(";", "-");
            HtmlString = HtmlString.Replace("*/", "-");
            HtmlString = HtmlString.Replace("&amp", "");
            HtmlString = HtmlString.Replace("&", "");
            HtmlString = HtmlString.Replace("\r\n", "-");
            HtmlString = HtmlString.Replace("+", "-");
            return HtmlString;
        }
        #endregion

        #region arrary to list
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(T[] list)
        {
            var result = new List<T>();
            foreach(var item in list)
            {
                result.Add(item);
            }

            return result;
        }
        #endregion
    }
}
