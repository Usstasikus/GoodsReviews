using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GoodsReivewsLibrary
{
    /// <summary>
    /// Класс для работы с лог-файлом
    /// </summary>
    public class LogFile
    {
        string _path;

        /// <summary>
        /// Текущее значение номера категории
        /// </summary>
        public int log_category_number { get; set; }

        /// <summary>
        /// Текущее значение номера подкатегории
        /// </summary>
        public int log_subcategory_number { get; set; }

        /// <summary>
        /// Текущее значение номера модели
        /// </summary>
        public int log_model_number { get; set; }

        /// <summary>
        /// Текущее значение номера страницы подкатегории
        /// </summary>
        public int log_page_number { get; set; }

        /// <summary>
        /// Текущее значение номера страницы отзывов
        /// </summary>
        public int log_page_reviews_number { get; set; }

        /// <summary>
        /// Последнее значение номера категории
        /// </summary>
        public int exit_category_number { get; set; }

        /// <summary>
        /// Последнее значение номера подкатегории
        /// </summary>
        public int exit_subcategory_number { get; set; }

        /// <summary>
        /// Последнее значение номера модели
        /// </summary>
        public int exit_model_number { get; set; }

        /// <summary>
        /// Последнее значение номера страницы подкатегории
        /// </summary>
        public int exit_page_number { get; set; }

        /// <summary>
        /// Последнее значение номера страницы отзывов
        /// </summary>
        public int exit_page_reviews_number { get; set; }

        /// <summary>
        /// Количество добавленных элементов
        /// </summary>
        public int added_count { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Путь к файлу, с последними позициями</param>
        public LogFile(string path)
        {
            _path = path;
            if (!File.Exists(path))
            {
                using (new FileStream(path, FileMode.Create)) ;
            }
            string[] last_pos = File.ReadAllLines(path);
            log_category_number = 0; log_subcategory_number = 0; log_page_number = 1; log_model_number = 0; log_page_reviews_number = 1;
            if (last_pos.Length != 0)
            {
                int cat, sub_cat, page, model, page2;
                int.TryParse(last_pos[0], out cat);
                int.TryParse(last_pos[1], out sub_cat);
                int.TryParse(last_pos[2], out page);
                int.TryParse(last_pos[3], out model);
                int.TryParse(last_pos[4], out page2);

                log_category_number = cat;
                log_subcategory_number = sub_cat;
                log_page_number = page;
                log_model_number = model;
                log_page_reviews_number = page2;

                added_count = 0;
                exit_category_number = 0;
                exit_subcategory_number = 0;
                exit_model_number = 0;
                exit_page_number = 0;
                exit_page_reviews_number = 0;
            }
        }

        /// <summary>
        /// Запись последних позиций в файл
        /// </summary>
        public void Write()
        {
            FileStream last_pos_file = new FileStream(_path, FileMode.Create);//файл с записью последних координат
            using (StreamWriter writer = new StreamWriter(last_pos_file))
            {
                writer.WriteLine(exit_category_number);
                writer.WriteLine(exit_subcategory_number);
                writer.WriteLine(exit_page_number);
                writer.WriteLine(exit_model_number);
                writer.WriteLine(exit_page_reviews_number);
            }

        }

        /// <summary>
        /// Запись окончания работы алгоритма чтения/записи комментариев 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dt"></param>
        /// <param name="stopWatch"></param>
        /// <param name="target"></param>
        public void End(string message, DateTime dt, Stopwatch stopWatch, string target)
        {
            Write();
            FileStream log_file = new FileStream(@"..\..\..\Resources\log.txt", FileMode.Append);
            using (StreamWriter sw = new StreamWriter(log_file))
            {
                sw.WriteLine("\r\n{3}\r\n{0}\r\nПрограмма работала в течении {1}.\r\nТаблица была дополнена {2} {4}."
                    , dt, stopWatch.Elapsed, added_count, message, target);
            }
        }

        /// <summary>
        /// Запись окончания работы алгоритма чтения/записи комментариев 
        /// </summary>
        /// <param name="we"></param>
        /// <param name="dt"></param>
        /// <param name="stopWatch"></param>
        /// <param name="target"></param>
        public void End(WebException we, DateTime dt, Stopwatch stopWatch, string target)
        {
            Write();
            var resp = XDocument.Load(new StreamReader(we.Response.GetResponseStream()));
            if (!File.Exists(@"..\..\..\Resources\log.txt"))
                File.Create(@"..\..\..\Resources\log.txt");
            FileStream log_file = new FileStream(@"..\..\..\Resources\log.txt", FileMode.Append);
            using (StreamWriter sw = new StreamWriter(log_file))
            {
                sw.WriteLine("\r\n{3}\r\n{0}\r\nПрограмма работала в течении {1}.\r\nТаблица была дополнена {2} {4}."
                    , dt, stopWatch.Elapsed, added_count, resp.Descendants("errors").ElementAt(0).Value, target);
            }
        }
    }
}
