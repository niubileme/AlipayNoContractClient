using FuckTheAlipayContract.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckTheAlipayContract.Core
{
    public class LocalCache
    {
        private static object synObj = new object();
        private static LocalCache _cache;
        public static LocalCache Cache
        {
            get
            {
                if (_cache == null)
                {
                    lock (synObj)
                    {
                        if (_cache == null)
                        {
                            _cache = new LocalCache();
                        }
                    }
                }
                return _cache;
            }
        }

        public LocalCache()
        {
            Load();
        }

        private ConcurrentDictionary<string, QueryResult> innerData = null;

        private void Load()
        {
            innerData = new ConcurrentDictionary<string, QueryResult>();
        }


        public QueryResult QueryTradeNo(string tradeno)
        {
            var result = default(QueryResult);
            innerData.TryGetValue(tradeno, out result);
            return result;
        }

        public QueryResult QueryRemark(string remark)
        {
            var result = default(QueryResult);
            foreach (var item in innerData)
            {
                if (item.Value.Remark == remark)
                {
                    result = item.Value;
                    break;
                }
            }
            return result;
        }

        public List<QueryResult> Get()
        {
            return innerData.Values.ToList();
        }

        public bool Add(string key, QueryResult result)
        {
            return innerData.TryAdd(key, result);
        }

        public bool Remove(string key, out QueryResult result)
        {
            return innerData.TryRemove(key, out result);
        }

       
    }
}
