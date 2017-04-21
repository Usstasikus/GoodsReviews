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
    public class LogFile
    {
        public int log_category_number { get; set; }
        public int log_subcategory_number { get; set; }
        public int log_model_number { get; set; }
        public int log_page_number { get; set; }
        public int log_page_reviews_number { get; set; }
        public int exit_category_number { get; set; }
        public int exit_subcategory_number { get; set; }
        public int exit_model_number { get; set; }
        public int exit_page_number { get; set; }
        public int exit_page_reviews_number { get; set; }
        public int added_count { get; set; }
        public LogFile(string[] last_pos)
        {
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

        public void Write()
        {
            FileStream last_pos_file = new FileStream("last_pos.txt", FileMode.Create);//файл с записью последних координат
            using (StreamWriter writer = new StreamWriter(last_pos_file))
            {
                writer.WriteLine(exit_category_number);
                writer.WriteLine(exit_subcategory_number);
                writer.WriteLine(exit_page_number);
                writer.WriteLine(exit_model_number);
                writer.WriteLine(exit_page_reviews_number);
            }

        }

        public void End(string message, DateTime dt, Stopwatch stopWatch, string target)
        {
            Write();
            FileStream log_file = new FileStream("log.txt", FileMode.Append);
            using (StreamWriter sw = new StreamWriter(log_file))
            {
                sw.WriteLine("\r\n{3}\r\n{0}\r\nПрограмма работала в течении {1}.\r\nПревышение количества допустимых запросов. \r\nТаблица была дополнена {2} {4}."
                    , dt, stopWatch.Elapsed, added_count, message, target);
            }
        }
        
        public void End(WebException we, DateTime dt, Stopwatch stopWatch, string target)
        {
            Write();
            var resp = XDocument.Load(new StreamReader(we.Response.GetResponseStream()));
            FileStream log_file = new FileStream("log.txt", FileMode.Append);
            using (StreamWriter sw = new StreamWriter(log_file))
            {
                sw.WriteLine("\r\n{3}\r\n{0}\r\nПрограмма работала в течении {1}.\r\nПревышение количества допустимых запросов. \r\nТаблица была дополнена {2} {4}."
                    , dt, stopWatch.Elapsed, added_count, resp.Descendants("errors").ElementAt(0).Value, target);
            }
        }
    }
}
