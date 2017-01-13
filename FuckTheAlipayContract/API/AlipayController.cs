using FuckTheAlipayContract.Core;
using FuckTheAlipayContract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace FuckTheAlipayContract.API
{
    public class AlipayController : ApiController
    {
        [HttpGet]
        public QueryResult QueryNo(string s)
        {
            var result = new QueryResult();
            if (string.IsNullOrEmpty(s))
            {
                result.IsSuccess = false;
                result.Info = "交易号为空！";
                return result;
            }
            AlipayHelper.QueryNo(s, out result);
            return result;
        }

        [HttpGet]
        public QueryResult QueryRemark(string s)
        {
            var result = new QueryResult();
            if (string.IsNullOrEmpty(s))
            {
                result.IsSuccess = false;
                result.Info = "备注为空！";
                return result;
            }
            AlipayHelper.QueryRemark(s, out result);
            return result;
        }

       

    }
}
