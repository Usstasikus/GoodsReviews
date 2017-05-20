using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Diagnostics;
using GoodsReivewsLibrary;
using System.Xml.Linq;
using System.Xml;

namespace GoodsReviews
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

        static void Yandex_Goods_Fill(string url, string key, LogFile lg, int limit)
        {
            stopWatch.Start();
            lg.Write();
            lg.exit_page_reviews_number = lg.log_page_reviews_number;
            lg.exit_model_number = lg.log_model_number;
            sqlConnection.Open();
            int query_count = 0;
            string url_cat = url + "category.xml?geo_id=225&count=30";
            List<Category> category_list = Category.ListForm(url_cat, key);//формирование списка категорий
            query_count++;
            if (query_count >= limit)
                throw new WebException("Достигнут лимит запросов.");
            Subcategory_View(lg, category_list, url, key, limit, ref query_count);
        }

        static void Subcategory_View(LogFile lg, List<Category> category_list, string url, string key, int limit, ref int query_count)
        {
            lg.Write();
            for (int i = lg.log_category_number; i < category_list.Count; i++)
            {
                lg.exit_category_number = i;
                if (i != lg.log_category_number)
                {
                    lg.log_subcategory_number = 0;
                    lg.log_page_number = 1;
                }

                if (category_list[i].ModelsNum == 0) continue;
                string url_subcat = url + string.Format("category/{0}/children.xml?geo_id=225&count=30", category_list[i].Id);
                List<Category> subcategory_list = Category.ListForm(url_subcat, key); //формирование списка подкатегорий
                query_count++;
                if (query_count >= limit)
                    throw new WebException("Достигнут лимит запросов.");
                Models_View(lg, subcategory_list, url, key, limit, ref query_count);
            }
        }

        static void Models_View(LogFile lg, List<Category> subcategory_list, string url, string key, int limit, ref int query_count)
        {
            if (query_count >= limit)
                throw new Exception("Достигнут лимит запросов.");
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
                    string url_models = url + string.Format("category/{0}/models.xml?geo_id=225&count=30&page={1}", subcategory_list[j].Id, page);
                    List<Model> models_list = Model.ListForm(url_models, key);
                    query_count++;
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

        #region Old Goods-Add Method
        //static void AddNewGoods(string url, string key)
        //{
        //    string connectionString = @"Server=127.0.0.1;Database=YandexReviews;Uid=StasDon;Pwd=stasik99;";
        //    SqlConnection sqlConnection = new SqlConnection(connectionString);
        //    sqlConnection.Open();

        //    int seen_count = 0;//счетчик просмотренных записей
        //    int added_count = 0;//счетчик добавленных записей
        //    int repeat_counter = 0;//счетчик повторений
        //    DateTime dt = DateTime.Now;
        //    Stopwatch stopWatch = new Stopwatch(); //секундомер работы программы
        //    string[] last_pos; // определение последних позиций
        //    last_pos = File.ReadAllLines("last_pos.txt");
        //    /*
        //    int lg.category_number = 0, log_subcategory_number = 0, log_page_number = 1;
        //    if (last_pos.Length != 0)
        //    {
        //        int.TryParse(last_pos[0], out log_category_number);
        //        int.TryParse(last_pos[1], out log_subcategory_number);
        //        int.TryParse(last_pos[2], out log_page_number);
        //    }
        //    */

        //    LogFile lg = new LogFile(last_pos);
        //    int exit_category_number = lg.log_category_number,
        //        exit_subcategor_number = lg.log_subcategory_number,
        //        exit_page_number = lg.log_page_number;
        //    List<Category> category_list = new List<Category>(); //список категорий

        //    string url_cat = url + "category.xml?geo_id=225&count=30";
        //    try
        //    {
        //        category_list = Category.ListForm(url_cat, key);//формирование списка категорий
        //    }
        //    catch (WebException we)
        //    {
        //        FileStream last_pos_file = new FileStream("last_pos.txt", FileMode.Create);//файл с записью последних координат
        //        using (StreamWriter writer = new StreamWriter(last_pos_file))
        //        {
        //            writer.WriteLine(lg.log_category_number);
        //            writer.WriteLine(lg.log_subcategory_number);
        //            writer.WriteLine(lg.log_page_number);
        //        }

        //        var resp = XDocument.Load(new StreamReader(we.Response.GetResponseStream()));
        //        Console.WriteLine(we.Message);
        //        if (resp.Descendants("errors").ElementAt(0).Value.ToString() == "Rate limit exceeded")
        //        {
        //            FileStream log_file = new FileStream("log.txt", FileMode.Append);
        //            using (StreamWriter sw = new StreamWriter(log_file))
        //            {
        //                sw.WriteLine("\r\n{0}\r\nПрограмма работала в течении {1}.\r\nПревышение количества допустимых запросов. \r\nТаблица было дополнена {2} элементами."
        //                    , dt, stopWatch.Elapsed, added_count);
        //            }
        //        }
        //        else throw;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //    try
        //    {
        //        for (int i = lg.log_category_number; i < category_list.Count; i++)
        //        {
        //            if (i != lg.log_category_number)
        //            {
        //                lg.log_subcategory_number = 0;
        //                lg.log_page_number = 1;
        //            }

        //            if (category_list[i].ModelsNum == 0) continue;
        //            string url_subcat = url + string.Format("category/{0}/children.xml?geo_id=225&count=30", category_list[i].Id);
        //            List<Category> subcategory_list = Category.ListForm(url_subcat, key); //формирование списка подкатегорий
        //            for (int j = lg.log_subcategory_number; j < subcategory_list.Count; j++)
        //            {
        //                if (j != lg.log_subcategory_number)
        //                {
        //                    lg.log_page_number = 1;
        //                }
        //                if (subcategory_list[j].ModelsNum == 0) continue;
        //                for (int page = lg.log_page_number; page <= 50; page++)
        //                {
        //                    exit_category_number = i; exit_subcategor_number = j; exit_page_number = page;
        //                    string url_models = url + string.Format("category/{0}/models.xml?geo_id=225&count=30&page={1}", subcategory_list[j].Id, page);
        //                    List<Models> models_list = Models.ListForm(url_models, key);
        //                    for (int page_pos = 0; page_pos < models_list.Count; page_pos++)
        //                    {
        //                        string name_without_single_inverted_commas = models_list[page_pos].Name.Replace("\'", "\""); //на случай, если попались кавычки в названии
        //                        string queryString = string.Format(@"INSERT INTO [dbo].[GoodsInfo] ([GoodsName] ,[GoodsID]) VALUES ('{0}', '{1}')",
        //                            name_without_single_inverted_commas, models_list[page_pos].Id);
        //                        try
        //                        {
        //                            SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);
        //                            SqlDataReader rd = sqlCommand.ExecuteReader();
        //                            rd.Close();
        //                            added_count++;
        //                        }
        //                        catch (SqlException)
        //                        {
        //                            repeat_counter++;
        //                        }
        //                        seen_count++;
        //                        Console.Clear();
        //                        Console.WriteLine("Просмотренно {0} товаров.", seen_count);

        //                        FileStream last_pos_file = new FileStream("last_pos.txt", FileMode.Create);//файл с записью последних координат
        //                        using (StreamWriter writer = new StreamWriter(last_pos_file))
        //                        {
        //                            writer.WriteLine(i);
        //                            writer.WriteLine(j);
        //                            writer.WriteLine(page);
        //                        }

        //                    }
        //                    if (models_list.Count < 30) break;
        //                }
        //            }
        //        }
        //    }
        //    catch (WebException we)
        //    {
        //        stopWatch.Stop();
        //        var resp = XDocument.Load(new StreamReader(we.Response.GetResponseStream()));
        //        Console.WriteLine(we.Message);

        //        FileStream last_pos_file = new FileStream("last_pos.txt", FileMode.Create);//файл с записью последних координат
        //        using (StreamWriter writer = new StreamWriter(last_pos_file))
        //        {
        //            writer.WriteLine(exit_category_number);
        //            writer.WriteLine(exit_subcategor_number);
        //            writer.WriteLine(exit_page_number);
        //        }
        //        if (resp.Descendants("errors").ElementAt(0).Value.ToString() == "Rate limit exceeded")
        //        {
        //            FileStream log_file = new FileStream("log.txt", FileMode.Append);
        //            using (StreamWriter sw = new StreamWriter(log_file))
        //            {
        //                sw.WriteLine("\r\n{0}\r\nПрограмма работала в течении {1}.\r\nПревышение количества допустимых запросов. \r\nТаблица было дополнена {2} элементами."
        //                    , dt, stopWatch.Elapsed, added_count);
        //            }
        //        }
        //        else throw;
        //        Console.WriteLine("Кол-во повторений: {0}", repeat_counter);
        //    }
        //    catch (Exception e)
        //    {
        //        FileStream log_file = new FileStream("log.txt", FileMode.Append);
        //        using (StreamWriter sw = new StreamWriter(log_file))
        //        {
        //            sw.WriteLine("\r\n{0}\r\nПрограмма работала в течении {1}.\r\n{2}\r\nТаблица было дополнена {3} элементами."
        //                , dt, stopWatch.Elapsed, e.Message, added_count);
        //        }
        //    }
        //    sqlConnection.Close();
        //}
        #endregion
        static void LoadNewReviews(string url, string key, LogFile lg, int limit, string connectionString)
        {
            stopWatch.Start();
            target = "комментариями";
            lg.exit_category_number = lg.log_category_number;
            lg.exit_subcategory_number = lg.log_subcategory_number;
            lg.exit_page_number = lg.log_page_number;
            int query_count = 0;
            url += "model/";
            //string connectionString = "Server=127.0.0.1;Database=Shop4KNS;Uid=StasDon;Pwd=stasik99;";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();





            //ОТРЕДАКТИРОВАТЬ ЧАСТЬ С ТАБЛИЦАМИ!!!!!!!!!





            string queryString = string.Format(@"SELECT GI1.GoodsID, Shop4KNS.dbo.GoodsInfo.GoodsID, GI1.GoodsName 
                            FROM Shop4KNS.dbo.GoodsInfo LEFT JOIN YandexReviews.dbo.GoodsInfo as GI1 ON
                            Shop4KNS.dbo.GoodsInfo.GoodsName =  GI1.GoodsName collate Cyrillic_General_CI_AS WHERE GI1.GoodsName is not null");
            SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);
            SqlDataReader rd = sqlCommand.ExecuteReader();
            List<string[]> matched_id = new List<string[]>();

            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    matched_id.Add(new string[] { rd.GetString(0), rd.GetInt32(1).ToString(), rd.GetString(2) });
                }
                rd.Close();
                for (int i = lg.log_model_number; i < matched_id.Count; i++)
                {
                    lg.exit_model_number = i;
                    if (i != lg.log_model_number)
                        lg.log_page_reviews_number = 0;
                    for (int page = lg.log_page_reviews_number + 1; page <= 50; page++)
                    {
                        if (query_count >= limit)
                            throw new Exception("Достигнут лимит запросов.");
                        lg.exit_page_reviews_number = page;
                        string url_opinion = url + string.Format("{0}/opinion.xml?geo_id=225&count=30&page={1}", matched_id[i][0], page);
                        List<ModelReview> mr;
                        mr = ModelReview.GetReviews(url_opinion, key);
                        query_count++;
                        if (mr.Count == 0)
                        {
                            lg.Write();
                            break;
                        }
                        for (int pos = 0; pos < mr.Count; pos++)
                        {
                            queryString = string.Format(@"INSERT INTO [dbo].[GoodsOpinion] ([GoodsID], [Mark], [MarkQuality], [MarkPrice], [MarkUsability], [UsageExperienceInMonths],
                            [_date], [UsefulCount], [NotUsefulCount], [CreationDateTime], [Comment], [CommentAdvantages], 
                            [CommentDisadvantages], [ClientIP], [ClientProxy], [SortCode], [UserFullName]) VALUES ({0}, 0, 0, 0, 0, {1}, {1}, {6}, {7}, '{2}','{3}','{4}','{5}', '', '', 0, '{8}')", 
                            matched_id[i][1], mr[pos].Grade+3, mr[pos].Date, mr[pos].Text, mr[pos].Pro, mr[pos].Contra, mr[pos].Agree, mr[pos].Reject, mr[pos].AuthorName);
                            
                                sqlCommand = new SqlCommand(queryString, sqlConnection);
                                rd = sqlCommand.ExecuteReader();
                                rd.Close();
                                lg.added_count++;
                            
                            seen_count++;
                        }

                        Console.Clear();
                        Console.WriteLine("Просмотренно {0} товаров.", seen_count);

                    }
                }
            }
            else
            {
                throw new Exception("Не найдено совпадающих товаров.");
            }
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
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:1553/");
            listener.Start();
            while (true)
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
