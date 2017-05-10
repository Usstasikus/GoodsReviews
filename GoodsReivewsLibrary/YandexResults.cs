using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodsReivewsLibrary
{
    /// <summary>
    /// Класс ответов Яндекс Маркета
    /// </summary>
    public class YandexResults
    {
        /// <summary>
        /// id
        /// </summary>
        string _id;

        /// <summary>
        /// Наименование
        /// </summary>
        string _name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="name">Наименование</param>
        public YandexResults(string id, string name)
        {
            _id = id;
            _name = name;
        }

        /// <summary>
        /// Метод, возвращающий нужное значение из xml документа
        /// </summary>
        /// <param name="category"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetValue(XElement category, string name)
        {
            try
            {
                return category.Attribute(name).Value;
            }

            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Get для поля id
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Get для поля name
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
    }
}
