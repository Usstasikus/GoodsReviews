﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodsReivewsLibrary
{
    /// <summary>
    /// Класс настроек полей БД для работы с алгоритмом записи комментариев
    /// </summary>
    [Serializable]
    public class Fields
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Адресс БД
        /// </summary>
        public string Adress { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Имя БД
        /// </summary>
        public string DB { get; set; }
        
        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Строка соединения
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Имя таблицы комментариев
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Таблица чтения ID и наименования товаров
        /// </summary>
        public string TableFrom { get; set; }

        /// <summary>
        /// Имя столбца чтения ID товара
        /// </summary>
        public string GoodsIDFrom { get; set; }

        /// <summary>
        /// Имя столбца записи ID товара
        /// </summary>
        public string GoodsIDTo { get; set; }

        /// <summary>
        /// Имя столбца чтения наименования товара
        /// </summary>
        public string GoodsNameFrom { get; set; }

        /// <summary>
        /// Список столбца, известных Яндексу
        /// </summary>
        public List<KnownField> ya_fields;

        /// <summary>
        ///  Список столбцов, неизсвестных Яндексу
        /// </summary>
        public List<UnknownField> unknown_fields;// список полей, не изсвестных Яндексу
        
        public Fields()
        {
            ya_fields = new List<KnownField>();
            unknown_fields = new List<UnknownField>();
        }
    }
}
