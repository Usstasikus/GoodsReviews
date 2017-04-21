using System;
using System.Net;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodsReivewsLibrary
{
    public class Category: YandexResults
    {
        string _child_count, _models_num, _parent_id;

        public Category(string id, string name, string child_count, string models_num,
            string parent_id):base(id, name)
        {
            _child_count = child_count;
            _models_num = models_num;
            _parent_id = parent_id;
        }

        public static List<Category> ListForm(string url, string key)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization: c9rSUIhhM7SRQzeEXaYbpEQknRaVMq");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            List<Category> ret = new List<Category>();
            XDocument xdoc = XDocument.Load(new StreamReader(response.GetResponseStream()));
            var categoryQuery = xdoc.Descendants("category");
            
            for (int i = 0; i < categoryQuery.Count(); i++)
            {
                string id = GetValue(categoryQuery.ElementAt(i), "id");
                string child_count = GetValue(categoryQuery.ElementAt(i), "children-count");
                string models_num = GetValue(categoryQuery.ElementAt(i), "models-num");
                string name = GetValue(categoryQuery.ElementAt(i), "name");
                string parent_id = GetValue(categoryQuery.ElementAt(i), "parent-id");
                ret.Add(new Category(id, name, child_count, models_num, parent_id));
            }
            return ret;
        }


        #region Properties
        public int ChildCount
        {
            get
            {
                int num;
                if(!int.TryParse(_child_count, out num))
                {
                    throw new Exception("Category: Child Count exception");
                }
                return num;
            }
        }
        
        public int ModelsNum
        {
            get
            {
                int num;
                if (!int.TryParse(_models_num, out num))
                {
                    throw new Exception("Category: Models num exception");
                }
                return num;
            }
        }

        public string ParentId
        {
            get
            {
                return _parent_id;
            }
        }
        #endregion
    }
}
