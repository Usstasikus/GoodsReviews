
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodsReivewsLibrary
{
    [Serializable]
    public class KnownField
    {
        /// <summary>
        /// Имя параметра Яндекс Маркета
        /// </summary>
        public string YandexElementName { get; private set; }

        /// <summary>
        /// Имя поля записи
        /// </summary>
        public string FieldName { get; private set; }

        /// <summary>
        /// Тип пользовательского поля
        /// </summary>
        public string Type { get; private set; }


        public KnownField(string YEN, string FN, string type)
        {
            YandexElementName = YEN;
            FieldName = FN;
            Type = type;
        }
    }
}
