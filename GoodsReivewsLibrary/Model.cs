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
    /// <summary>
    /// Класс моделей Яндекс Маркета
    /// </summary>
    public class Model : YandexResults
    {
        string _category_id, _rating, _reviews_count,
            _vendor_id;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">ID модели</param>
        /// <param name="name">Наименование модели</param>
        /// <param name="category_id">Категория</param>
        /// <param name="rating">Рейтинг</param>
        /// <param name="reviews_count">Количество отзывов</param>
        /// <param name="vendor_id">Идентификатор производителя</param>
        public Model(string id, string name, string category_id,
            string rating, string reviews_count, string vendor_id) : base(id, name)
        {
            _category_id = category_id;
            _rating = rating;
            _reviews_count = reviews_count;
            _vendor_id = vendor_id;
        }

        /// <summary>
        /// Возвращает список моделей по данному url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<Model> ListForm(string url, string key)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add(String.Format("Authorization: {0}", key));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            XDocument xdoc = XDocument.Load(new StreamReader(response.GetResponseStream()));
            List<Model> ret = new List<Model>();
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
                ret.Add(new Model(id, name, category_id, rating,  reviews_count,  vendor_id));
            }
            return ret;
        }
        #region Properties
        /// <summary>
        /// id категории
        /// </summary>
        public string CategoryId
        {
            get
            {
                return _category_id;
            }
        }

        /// <summary>
        /// Рейтинг модели
        /// </summary>
        public string Rating
        {
            get
            {
                return _rating;
            }
        }

        /// <summary>
        /// Количество отзывов
        /// </summary>
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

        /// <summary>
        /// Идентификатор производителя
        /// </summary>
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
