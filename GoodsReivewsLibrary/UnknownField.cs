using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodsReivewsLibrary
{
    [Serializable]
    public class UnknownField
    {
        /// <summary>
        /// Имя поля записи
        /// </summary>
        public string FieldName { get; private set; }

        /// <summary>
        /// Значение, которые нужно использовать для записи
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Тип поля
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Возвращает значение, указывающее на поле, к которому оно привязано, или null, если зависимости нет
        /// </summary>
        public string Dependency { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Fields name">Имя поля записи</param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public UnknownField(string FN, string value, string type, string dependancy = null)
        {
            FieldName = FN;
            Type = type;
            Dependency = dependancy;
            Value = value;
        }
    }
}
