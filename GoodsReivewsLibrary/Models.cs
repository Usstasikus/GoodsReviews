using System;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodsReivewsLibrary
{
    public class Models : YandexResults
    {
        string _category_id, _rating, _reviews_count,
            _vendor_id;
        public Models(string id, string name, string category_id,
            string rating, string reviews_count, string vendor_id) : base(id, name)
        {
            _category_id = category_id;
            _rating = rating;
            _reviews_count = reviews_count;
            _vendor_id = vendor_id;
        }

        public static List<Models> ListForm(string url, string key)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add(String.Format("Authorization: {0}", key));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            XDocument xdoc = XDocument.Load(new StreamReader(response.GetResponseStream()));
            List<Models> ret = new List<Models>();
            var categoryQuery_inf = xdoc.Descendants("model");
            var categoryQuery_name = xdoc.Descendants("name");
            var categoryQuery_descr = xdoc.Descendants("description");

            for (int i = 0; i < categoryQuery_inf.Count(); i++)
            {
                string name = categoryQuery_name.ElementAt(i).Value;
                string category_id = GetValue(categoryQuery_inf.ElementAt(i), "category-id");
                string id = GetValue(categoryQuery_inf.ElementAt(i), "id");
                string rating = GetValue(categoryQuery_inf.ElementAt(i), "rating");
                string reviews_count = GetValue(categoryQuery_inf.ElementAt(i), "reviews-count");
                string vendor_id = GetValue(categoryQuery_inf.ElementAt(i), "vendor-id");
                ret.Add(new Models(id, name, category_id, rating,  reviews_count,  vendor_id));
            }
            return ret;
        }
        #region Properties
        public string CategoryId
        {
            get
            {
                return _category_id;
            }
        }

        public string Rating
        {
            get
            {
                return _rating;
            }
        }

        public int ReviewsCount
        {
            get
            {
                int num;
                if (!int.TryParse(_reviews_count, out num))
                {
                    throw new Exception("Models: Reviews Count exception");
                }
                return num;
            }
        }
        public string VendorId
        {
            get
            {
                return _vendor_id;
            }
        }
        #endregion
    }
}
