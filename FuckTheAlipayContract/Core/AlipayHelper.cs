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
        public static Action<string> Log;
        public static void Init(WebBrowser WebBrowser, Action<string> log)
        {
            Web = WebBrowser;
            cookies = new CookieContainer();
            Log = log;
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

        public static void Refresh()
        {
            Web.Invoke(new Action(() =>
            {
                Web.Refresh(WebBrowserRefreshOption.Completely);
            }));
        }

        public static bool IsLogin()
        {
            var islogin = false;
            GetCookies();
            var url = "https://my.alipay.com/portal/i.htm";
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
                    islogin = false;
                if (Regex.IsMatch(code, "我的支付宝.+?支付宝"))
                    islogin = true;
            }
            catch (Exception)
            {
                islogin = false;
            }
            Log.Invoke(islogin ? "已登录" : "未登录");
            return islogin;
        }


        /// <summary>
        /// 根据交易号查询
        /// </summary>
        public static bool QueryNo(string no, out QueryResult result)
        {
            result = LocalCache.Cache.QueryTradeNo(no);
            if (result != null)
                return true;
            result = new QueryResult();
            if (!IsLogin())
            {
                result.Info = "没有登陆";
                return false;
            }
            var html = GetTradeNoHtml(no);
            if (string.IsNullOrEmpty(html))
            {
                result.Info = "查询失败";
                return false;
            }
            result = Format(html);
            if (result.IsSuccess == false)
                return false;
            LocalCache.Cache.Add(result.TradeNo, result);
            return result.IsSuccess;
        }

        /// <summary>
        /// 根据备注查询
        /// </summary>
        public static bool QueryRemark(string remark, out QueryResult result)
        {
            result = LocalCache.Cache.QueryRemark(remark);
            if (result != null)
                return true;
            result = new QueryResult();
            if (!IsLogin())
            {
                result.Info = "没有登陆";
                return false;
            }
            var dic = GetTradeNoListHtml(remark);
            if (dic == null)
            {
                result.Info = "查询失败";
                return false;
            }
            //匹配到tradeNo
            var no = "";
            foreach (var item in dic)
            {
                if (item.Value == remark)
                {
                    no = item.Key;
                    break;
                }
            }
            if (string.IsNullOrEmpty(no))
            {
                result.Info = "没有找到备注信息";
                return false;
            }
            return QueryNo(no, out result);
        }

        private static string GetTradeNoHtml(string no)
        {
            var code = "";
            try
            {
                var url = $"https://shenghuo.alipay.com/send/queryTransferDetail.htm?tradeNo={no}";
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
                    r.Info = "交易号不存在，请确保交易号正确！";
                    return r;
                }
                var tradelist = Regex.Match(html, "<div class=\"p-trade-list\">([\\s\\S]*?)</div>").Groups[1].Value;
                var mc = Regex.Match(tradelist, "<td class=\"name\">[\\s\\S]*?<ul>[\\s\\S]*?<li>(.+?)</li>[\\s\\S]*?<li.+?>交易号(.+?)</li>[\\s\\S]*?<td class=\".*?\">(.+?)</td>[\\s\\S]*?<td class=\"postalfee\">(.+?)</td>[\\s\\S]*?<td class=\"amount\">(.+?)</td>");
                var tradeNo = mc.Groups[2].Value.Trim();//交易号
                var remark = mc.Groups[1].Value.Trim();//备注
                var amount1 = mc.Groups[3].Value;//实付金额
                var postalFee = mc.Groups[4].Value;
                postalFee = postalFee.Substring(1, postalFee.Length - 1);//服务费 去掉+
                var amount2 = mc.Groups[5].Value;
                amount2 = amount2.Substring(1, amount2.Length - 1);//总额 去掉=

                var tradeslips = Regex.Match(html, "<div class=\".+?p-trade-slips\">([\\s\\S]*?)</div>").Groups[1].Value;
                var mc2 = Regex.Matches(tradeslips, "<td class=\"time\">([\\s\\S]*?)</td>");
                var createOn = Convert.ToDateTime(mc2[0].Groups[1].Value.Trim());//创建时间
                var paymentOn = Convert.ToDateTime(mc2[1].Groups[1].Value.Trim());//付款时间
                var endOn = Convert.ToDateTime(mc2[2].Groups[1].Value.Trim());//结束时间

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

        private static Dictionary<string, string> GetTradeNoListHtml(string remark)
        {

            var dic = new Dictionary<string, string>();
            try
            {
                var url = "https://lab.alipay.com/consume/record/items.htm";
                var result = new HttpHelper(new HttpItem()
                {
                    URL = url,
                    Allowautoredirect = true,
                    CookieContainer = cookies
                }).GetHtml();
                var html = result.Html;
                if (string.IsNullOrEmpty(html))
                    return null;
                //匹配列表
                var mc = Regex.Matches(html, "class=\"consumeBizNo\">([\\s\\S]*?)<[\\s\\S]*?class=\"name emoji-li\".*?>([\\s\\S]*?)<");
                foreach (Match item in mc)
                {
                    var no = item.Groups[1].Value.Replace("\r\n", "").Replace("\t", "").Replace(" ", "").Trim();
                    var info = item.Groups[2].Value.Replace("\r\n", "").Replace("\t", "").Replace(" ", "").Trim();
                    dic.Add(no, info);
                }
                return dic;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
