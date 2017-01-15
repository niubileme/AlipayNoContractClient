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
            AlipayHelper.Init(webBrowser1, Show);
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000 * 60 * 2);
                    Show("检查登录...");
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
            var config = new HttpSelfHostConfiguration("http://127.0.0.1:8999");
            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{action}/{s}",
                new { id = RouteParameter.Optional });
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            var server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread.Sleep(1000 * 5);
            webBrowser1.Navigate("https://auth.alipay.com/login/index.htm");
        }

        private void btnquery_Click(object sender, EventArgs e)
        {
            var type = Convert.ToInt32(btnquery.Tag) == 0 ? "交易号" : "备注";
            var str = txtNumber.Text.Trim();
            if (string.IsNullOrEmpty(str))
            {
                MessageBox.Show($"{type}不能为空!");
                return;
            }
            var result = new QueryResult();
            var IsOK = false;
            switch (type)
            {
                case "交易号":
                    IsOK = AlipayHelper.QueryNo(str, out result);
                    break;
                case "备注":
                    IsOK = AlipayHelper.QueryRemark(str, out result);
                    break;
            }
            if (IsOK)
            {
                var text = $"[交易号]{result.TradeNo}[备注信息]{result.Remark}[实付金额]{result.Amount1}[付款时间]{result.PaymentOn}\r\n";
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
            //UserName = txtUserName.Text;
            PassWord = textPwd.Text;
            if (string.IsNullOrEmpty(PassWord))
            {
                return;
            }
            if (Regex.IsMatch(e.Url.ToString(), "https://auth.alipay.com/login/index.htm"))
            {
                Task.Factory.StartNew(() =>
                {
                    webBrowser1.Invoke(new Action(() =>
                    {
                        //var unameinput = webBrowser1.Document.GetElementById("J-input-user");
                        //unameinput.SetAttribute("autocomplete", "on");
                        //unameinput.SetAttribute("value", UserName);
                        var pwdinput = webBrowser1.Document.GetElementById("password_rsainput");
                        pwdinput.SetAttribute("value", PassWord);
                        var subbtn = webBrowser1.Document.GetElementById("J-login-btn");
                        subbtn.InvokeMember("click");
                        Thread.Sleep(1000 * 5);
                        AlipayHelper.IsLogin();
                    }));
                });
            }

        }

        public void Show(string msg)
        {
            Task.Factory.StartNew(() =>
            {
                statusStrip1.Invoke(new Action<string>(x =>
                {
                    toolStripStatusLabel1.Text = $"{x},上次检查时间[{DateTime.Now.ToString("HH:mm:ss")}]";
                }), msg);
            });

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked)
                return;
            btnquery.Tag = 0;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked)
                return;
            btnquery.Tag = 1;
        }
    }
}
