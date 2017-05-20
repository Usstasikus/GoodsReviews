using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Diagnostics;
using GoodsReivewsLibrary;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

namespace Server
{

    class Program
    {
        static string _connectionString = @"Server=127.0.0.1;Database=YandexReviews;Uid=StasDon;Pwd=stasik99;";
        static SqlConnection sqlConnection = new SqlConnection(_connectionString);
        static Stopwatch stopWatch = new Stopwatch(); //секундомер работы программы
        static DateTime dt = DateTime.Now;
        static int repeat_counter = 0;
        static int seen_count = 0;
        static string target = "товарами";
        LogFile lg;

        /// <summary>
        /// Запись комментариев из Яндекс Маркета в БД сервера
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <param name="lg"></param>
        /// <param name="limit"></param>
        static void Yandex_Goods_Fill(string url, string key, LogFile lg, int limit)
        {
            stopWatch.Start();
            lg.Write();
            lg.exit_page_reviews_number = lg.log_page_reviews_number;
            lg.exit_model_number = lg.log_model_number;
            sqlConnection.Open();
            int query_count = 0;
            string url_cat = url + "category.xml?geo_id=225&sort=NAME&count=30";
            List<Category> category_list = Category.ListForm(url_cat, key);//формирование списка категорий
            query_count++;
            if (query_count >= limit)
            {
                query_count = 0;
                sqlConnection.Close();
                throw new Exception("Достигнут лимит запросов.");
            }
            Subcategory_View(lg, category_list, url, key, limit, ref query_count);
        }

        /// <summary>
        /// Просмотр подкатегорий
        /// </summary>
        /// <param name="lg"></param>
        /// <param name="category_list"></param>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <param name="limit"></param>
        /// <param name="query_count"></param>
        static void Subcategory_View(LogFile lg, List<Category> category_list, string url, string key, int limit, ref int query_count)
        {
            lg.Write();
            for (int i = lg.log_category_number; i < category_list.Count; i++)
            {
                lg.exit_category_number = i;
                if (i != lg.log_category_number)
                {
                    lg.log_subcategory_number = 0;
                    lg.log_page_number = 0;
                }

                if (category_list[i].ModelsNum == 0) continue;
                string url_subcat = url + string.Format("category/{0}/children.xml?geo_id=225&sort=NAME&count=30", category_list[i].Id);
                List<Category> subcategory_list = Category.ListForm(url_subcat, key); //формирование списка подкатегорий
                query_count++;
                if (query_count >= limit)
                {
                    query_count = 0;
                    sqlConnection.Close();
                    throw new Exception("Достигнут лимит запросов.");
                }
                Models_View(lg, subcategory_list, url, key, limit, ref query_count);
            }
        }

        /// <summary>
        /// Просмотр моделей в подкатегории
        /// </summary>
        /// <param name="lg"></param>
        /// <param name="subcategory_list"></param>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <param name="limit"></param>
        /// <param name="query_count"></param>
        static void Models_View(LogFile lg, List<Category> subcategory_list, string url, string key, int limit, ref int query_count)
        {
            if (query_count >= limit)
            {
                query_count = 0;
                sqlConnection.Close();
                throw new Exception("Достигнут лимит запросов.");
            }
            for (int j = lg.log_subcategory_number; j < subcategory_list.Count; j++)
            {
                lg.exit_subcategory_number = j;
                if (j != lg.log_subcategory_number)
                {
                    lg.log_page_number = 0;
                }
                if (subcategory_list[j].ModelsNum == 0) continue;
                for (int page = lg.log_page_number + 1; page <= 50; page++)
                {
                    lg.Write();
                    lg.exit_page_number = page;
                    string url_models = url + string.Format("category/{0}/models.xml?geo_id=225&sort=DATE&count=30&page={1}", subcategory_list[j].Id, page);
                    List<Model> models_list = Model.ListForm(url_models, key);
                    query_count++;
                    if (query_count > limit)
                    {
                        query_count = 0;
                        sqlConnection.Close();
                        throw new Exception("Достигнут лимит запросов.");
                    }

                    for (int page_pos = 0; page_pos < models_list.Count; page_pos++)
                    {
                        string name_without_single_inverted_commas = models_list[page_pos].Name.Replace("\'", "\""); //на случай, если попались кавычки в названии
                        string queryString = string.Format(@"INSERT INTO [dbo].[GoodsInfo] ([GoodsName] ,[GoodsID]) VALUES ('{0}', '{1}')",
                            name_without_single_inverted_commas, models_list[page_pos].Id);
                        try
                        {
                            SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);
                            SqlDataReader rd = sqlCommand.ExecuteReader();
                            rd.Close();
                            lg.added_count++;
                        }
                        catch (SqlException)
                        {
                            repeat_counter++;
                        }
                        seen_count++;
                        Console.Clear();
                        Console.WriteLine("Просмотренно {0} товаров.", seen_count);


                    }
                    if (models_list.Count < 30) break;
                }
            }
        }

        /// <summary>
        /// Формирование сообщения при завершении работы программы
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string FormMessage(string message, LogFile lg)
        {
            return String.Format("\r\n{0}\r\n{3}\r\nПрограмма работала в течении {1}.\r\nТаблица была дополнена {2} {4}."
                    , dt, stopWatch.Elapsed, lg.added_count, message, target);
        }

        /// <summary>
        /// Формирование сообщения при завершении работы программы
        /// </summary>
        /// <param name="we"></param>
        /// <returns></returns>
        private static string FormMessage(WebException we, LogFile lg)
        {
            XDocument resp = XDocument.Load(new StreamReader(we.Response.GetResponseStream()));
            if (resp.Descendants("errors").ElementAt(0).Value == "Rate limit exceeded")
                return FormMessage("Превышение максимального количества запросов.", lg);
            return String.Format("\r\n{3}\r\n{0}\r\nПрограмма работала в течении {1}.\r\nТаблица была дополнена {2} {4}."
                    , dt, stopWatch.Elapsed, lg.added_count, resp.Descendants("errors").ElementAt(0).Value, target);

        }

        private static string XmlToString(XDocument xdoc)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                xdoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }


        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Дополнить таблицу с товарами из Яндекс Маркета - 1\nЗапустить сервер - 2");
                int choice = int.Parse(Console.ReadLine());

                if (choice == 1)
                {
                    string url = String.Format("https://api.content.market.yandex.ru/v1/");   //category/{0}/models.xml?geo_id=225&count=30&&page={1}");
                    string key = "c9rSUIhhM7SRQzeEXaYbpEQknRaVMq";
                    int limit;
                    LogFile lg = new LogFile(@"last_pos.txt");
                    Console.Write("Введите ограничение на количество запросов (<=100):");
                    while (!int.TryParse(Console.ReadLine(), out limit) || limit > 100)
                    {
                        Console.Write("Введите число меньше 100");
                    }
                    //AddNewGoods(url, key);
                    try
                    {
                        Yandex_Goods_Fill(url, key, lg, limit);
                    }
                    catch (WebException we)
                    {
                        stopWatch.Stop();
                        string message = FormMessage(we, lg);
                        Console.WriteLine(message);
                        lg.End(message, dt, stopWatch, target);
                    }
                    catch (Exception e)
                    {
                        stopWatch.Stop();
                        lg.End(e.Message, dt, stopWatch, target);
                    }
                }
                else
                {
                    HttpListener listener = new HttpListener();
                    listener.Prefixes.Add("http://localhost:1553/");
                    listener.Start();
                    while (true)
                    {
                        try
                        {

                            Console.WriteLine("Ожидание подключений...");
                            HttpListenerContext context = listener.GetContext();
                            HttpListenerRequest request = context.Request;
                            HttpListenerResponse response = context.Response;
                            string url = request.RawUrl.Substring(1);
                            string key = "c9rSUIhhM7SRQzeEXaYbpEQknRaVMq";
                            HttpWebRequest ya_request = (HttpWebRequest)WebRequest.Create(url);
                            ya_request.Headers.Add(String.Format("Authorization: {0}", key));

                            byte[] buffer;
                            try
                            {
                                HttpWebResponse ya_response = (HttpWebResponse)ya_request.GetResponse();
                                XDocument xdoc = XDocument.Load(new StreamReader(ya_response.GetResponseStream()));
                                string ya_response_string = XmlToString(xdoc);
                                buffer = System.Text.Encoding.UTF8.GetBytes(ya_response_string);

                                response.ContentLength64 = buffer.Length;

                            }
                            catch (WebException we)
                            {
                                XDocument xdoc = XDocument.Load(new StreamReader(we.Response.GetResponseStream()));
                                string ya_response_string = XmlToString(xdoc);

                                buffer = System.Text.Encoding.UTF8.GetBytes(ya_response_string);
                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                response.ContentLength64 = buffer.Length;
                                //response.Close();

                            }

                            Stream output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        }
                        catch (Exception) { }

                    }
                }
            }
            /*
            string url = String.Format("https://api.content.market.yandex.ru/v1/");   //category/{0}/models.xml?geo_id=225&count=30&&page={1}");
            string key = "c9rSUIhhM7SRQzeEXaYbpEQknRaVMq";
            string[] last_pos = File.ReadAllLines("last_pos.txt");
            int limit;
            LogFile lg = new LogFile(last_pos);
            Console.WriteLine("Дополнить таблицу с товарами из Яндекс Маркета - 1\nДополнить таблицу с отзывами о товарах - 2");
            int choice = int.Parse(Console.ReadLine());
            try
            {
                if (choice == 1)
                {
                    Console.Write("Введите ограничение на количество запросов (<=100):");
                    while (!int.TryParse(Console.ReadLine(), out limit) || limit > 100)
                    {
                        Console.Write("Введите число меньше 100");
                    }
                    //AddNewGoods(url, key);
                    Yandex_Goods_Fill(url, key, lg, limit);
                }
                else
                {
                    Console.Write("Введите ограничение на количество запросов (<=100):");
                    while (!int.TryParse(Console.ReadLine(), out limit) || limit > 100)
                    {
                        Console.Write("Введите число меньше 100");
                    }

                    LoadNewReviews(url, key, lg, limit, "Server=127.0.0.1;Database=Shop4KNS;Uid=StasDon;Pwd=stasik99;");
                }
            }
            catch (WebException we)
            {
                stopWatch.Stop();
                lg.End(we, dt, stopWatch, target);
            }
            catch (Exception e)
            {
                stopWatch.Stop();
                lg.End(e.Message, dt, stopWatch, target);
            }

            Console.ReadKey();
            */
        }
    }
}
