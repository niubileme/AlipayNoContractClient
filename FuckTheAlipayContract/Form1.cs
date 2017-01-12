using FuckTheAlipayContract.Core;
using FuckTheAlipayContract.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Windows.Forms;

namespace FuckTheAlipayContract
{
    public partial class Form1 : Form
    {
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public Form1()
        {
            InitializeComponent();
            richTextBox1.AppendText("交易号|备注|金额|付款时间" + "\r\n");
            AlipayHelper.Init(webBrowser1);
            //开启后台线程 保持登陆
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000 * 60 * 2);
                    if (!AlipayHelper.IsLogin())
                    {
                        webBrowser1.Navigate("https://auth.alipay.com/login/index.htm");
                    }
                }

            });
            HostStar();
        }

        public void HostStar()
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8999");
            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{action}/{no}",
                new { id = RouteParameter.Optional });
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            var server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigate("https://auth.alipay.com/login/index.htm");
        }

        private void btnquery_Click(object sender, EventArgs e)
        {
            var tradeno = txtNumber.Text;
            if (string.IsNullOrEmpty(tradeno))
            {
                MessageBox.Show("交易号不能为空");
                return;
            }
            var result = new QueryResult();
            if (AlipayHelper.Query(tradeno, out result))
            {
                var text = string.Format("{0}|{1}|{2}|{3}", result.TradeNo, result.Remark, result.Amount1, result.PaymentOn) + "\r\n";
                richTextBox1.AppendText(text);
            }
            else
            {
                var text = string.Format("{0}:{1}", "错误", result.Info) + "\r\n";
                richTextBox1.AppendText(text);
            }

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var pwd = textPwd.Text;
            if (string.IsNullOrEmpty(pwd))
            {
                return;
            }
            var url = e.Url.ToString();
            if (Regex.IsMatch(url, "https://auth.alipay.com/login/index.htm"))
            {
                Thread.Sleep(1000 * 10);
                //var unameinput = webBrowser1.Document.GetElementById("J-input-user");
                //unameinput.SetAttribute("value", uname);
                var pwdinput = webBrowser1.Document.GetElementById("password_rsainput");
                pwdinput.SetAttribute("value", pwd);
                var subbtn = webBrowser1.Document.GetElementById("J-login-btn");
                subbtn.InvokeMember("click");
            }
        }
    }
}
