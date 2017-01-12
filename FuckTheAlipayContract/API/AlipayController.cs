using FuckTheAlipayContract.Core;
using FuckTheAlipayContract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace FuckTheAlipayContract.API
{
    public class AlipayController : ApiController
    {
        [HttpGet]
        public QueryResult Query(string no)
        {
            var result = new QueryResult();
            if (string.IsNullOrEmpty(no))
            {
                result.IsSuccess = false;
                result.Info = "交易号为空！";
                return result;
            }
            AlipayHelper.Query(no, out result);
            return result;
        }
    }
}
