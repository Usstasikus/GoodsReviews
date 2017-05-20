using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodsReivewsLibrary
{
    public class MatchedGood
    {
        /// <summary>
        /// ID товара в БД пользователя
        /// </summary>
        public string UserGoodsID { get; set; }
        
        /// <summary>
        /// ID товара в Яндекс Маркете
        /// </summary>
        public string YandexGoodsID { get; set; }

        /// <summary>
        /// Имя товара
        /// </summary>
        public string GoodsName { get; set; }


        public MatchedGood()
        {
            UserGoodsID = string.Empty;
            YandexGoodsID = string.Empty;
            GoodsName = string.Empty;
        }
    }
}
