using FuckTheAlipayContract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FuckTheAlipayContract.Core
{
    public class AlipayHelper
    {
        public static WebBrowser Web;
        public static CookieContainer cookies;
        public static void Init(WebBrowser WebBrowser)
        {
            Web = WebBrowser;
            cookies = new CookieContainer();
        }

        public static void Refresh()
        {
            Web.Invoke(new Action(() =>
            {
                Web.Navigate("https://my.alipay.com?k=" + DateTime.Now.Ticks);
                Console.WriteLine(DateTime.Now + "刷新...");
            }));
        }

        private static void GetCookies()
        {
            try
            {
                Web.Invoke(new Action(() =>
                {
                    string cookieStr = Web.Document.Cookie;
                    string[] cookstr = cookieStr.Split(';');
                    foreach (string str in cookstr)
                    {
                        string[] cookieNameValue = str.Split('=');
                        Cookie ck = new Cookie(cookieNameValue[0].Trim().ToString(), cookieNameValue[1].Trim().ToString());
                        ck.Domain = ".alipay.com";
                        cookies.Add(ck);
                    }
                }));
            }
            catch (Exception ex)
            {
            }
        }

        public static void KeepLogin()
        {
            if (!IsLogin())
            {
                //尝试登陆
                Web.Invoke(new Action(() =>
                {
                    Web.Navigate("https://auth.alipay.com/login/index.htm");
                    Console.WriteLine(DateTime.Now + "刷新...");
                }));
            }
        }
        public static bool IsLogin()
        {
            var islogin = false;
            var url = "https://my.alipay.com/";
            try
            {
                var code = "";
                var result = new HttpHelper(new HttpItem()
                {
                    URL = url,
                    Allowautoredirect = true,
                    CookieContainer = cookies
                }).GetHtml();
                code = result.Html;
                if (Regex.IsMatch(code, "登录.+?支付宝"))
                {
                    islogin = false;
                }
                if (Regex.IsMatch(code, "我的支付宝.+?支付宝"))
                {
                    islogin = true;
                }
            }
            catch (Exception)
            {
                islogin = false;
            }
            return islogin;
        }

        /// <summary>
        /// 根据交易号查询
        /// </summary>
        public static bool QueryNo(string no, out QueryResult result)
        {
            result = new QueryResult();
            GetCookies();
            //判断是否登陆
            if (!IsLogin())
            {
                result.Info = "没有登陆";
                return false;
            }
            var html = GetHtml(no);
            if (string.IsNullOrEmpty(html))
            {
                result.Info = "查询失败";
                return false;
            }
            result = Format(html);
            return result.IsSuccess;
        }

        /// <summary>
        /// 根据交易号查询
        /// </summary>
        public static bool QueryInfo(string info, out QueryResult result)
        {
            result = new QueryResult();
            GetCookies();
            //判断是否登陆
            if (!IsLogin())
            {
                result.Info = "没有登陆";
                return false;
            }
            var html = GetHtml(info);
            if (string.IsNullOrEmpty(html))
            {
                result.Info = "查询失败";
                return false;
            }
            result = Format(html);
            return result.IsSuccess;
        }

        private static string GetHtml(string no)
        {
            var code = "";
            try
            {
                var url = string.Format("https://shenghuo.alipay.com/send/queryTransferDetail.htm?tradeNo={0}", no);
                var result = new HttpHelper(new HttpItem()
                {
                    URL = url,
                    Allowautoredirect = true,
                    CookieContainer = cookies
                }).GetHtml();
                code = result.Html;
            }
            catch (Exception)
            {
                code = null;
            }
            return code;
        }

        private static QueryResult Format(string html)
        {
            QueryResult r = new QueryResult();
            try
            {
                if (Regex.IsMatch(html, "暂时无法查询交易详情"))
                {
                    r.IsSuccess = false;
                    r.Info = "交易号不正确";
                    return r;
                }
                var tradelist = Regex.Match(html, "<div class=\"p-trade-list\">([\\s\\S]*?)</div>").Groups[1].Value;
                var mc = Regex.Match(tradelist, "<td class=\"name\">[\\s\\S]*?<ul>[\\s\\S]*?<li>(.+?)</li>[\\s\\S]*?<li.+?>交易号(.+?)</li>[\\s\\S]*?<td class=\"amount\">(.+?)</td>[\\s\\S]*?<td class=\"postalfee\">(.+?)</td>[\\s\\S]*?<td class=\"amount\">(.+?)</td>");
                var tradeNo = mc.Groups[2].Value.Trim();
                var remark = mc.Groups[1].Value.Trim();
                var amount1 = mc.Groups[3].Value;
                var postalFee = mc.Groups[4].Value;
                postalFee = postalFee.Substring(1, postalFee.Length - 1);
                var amount2 = mc.Groups[5].Value;

                var tradeslips = Regex.Match(html, "<div class=\".+?p-trade-slips\">([\\s\\S]*?)</div>").Groups[1].Value;
                var mc2 = Regex.Matches(tradeslips, "<td class=\"time\">([\\s\\S]*?)</td>");
                var createOn = Convert.ToDateTime(mc2[0].Groups[1].Value.Trim());
                var paymentOn = Convert.ToDateTime(mc2[1].Groups[1].Value.Trim());
                var endOn = Convert.ToDateTime(mc2[2].Groups[1].Value.Trim());

                r.IsSuccess = true;
                r.TradeNo = tradeNo;
                r.Remark = remark;
                r.Amount1 = amount1;
                r.PostalFee = postalFee;
                r.Amount2 = amount2;
                r.CreateOn = createOn;
                r.PaymentOn = paymentOn;
                r.EndOn = endOn;
                return r;

            }
            catch (Exception ex)
            {
                r.IsSuccess = false;
                r.Info = "获取交易记录异常";
                return r;
            }
        }

    }
}
