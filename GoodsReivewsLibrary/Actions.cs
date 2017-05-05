using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using GoodsReivewsLibrary;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Threading;

namespace GoodsReivewsLibrary
{
    public class Actions
    {
        Fields _fields;
        string _connectionString;
        SqlConnection sqlConnection;
        Stopwatch stopWatch; //секундомер работы программы
        DateTime dt;
        int repeat_counter;
        int seen_count;
        string target = "товарами";
        string url;
        string key;
        LogFile lg;

        public Actions(Fields fields)
        {
            _fields = fields;
            _connectionString = _fields.ConnectionString;
            sqlConnection = new SqlConnection(_connectionString);
            stopWatch = new Stopwatch();
            dt = DateTime.Now;
            repeat_counter = 0;
            seen_count = 0;
            url = String.Format("http://localhost:1553/https://api.content.market.yandex.ru/v1/");   //category/{0}/models.xml?geo_id=225&count=30&&page={1}");
            key = "c9rSUIhhM7SRQzeEXaYbpEQknRaVMq";
            string[] last_pos = File.ReadAllLines(@"..\..\..\Resources\last_pos\last_pos_" + _fields.FileName + ".txt");
            lg = new LogFile(last_pos);
        }
        void Yandex_Goods_Fill(string url, string key, LogFile lg, int limit)
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

        void Subcategory_View(LogFile lg, List<Category> category_list, string url, string key, int limit, ref int query_count)
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

        void Models_View(LogFile lg, List<Category> subcategory_list, string url, string key, int limit, ref int query_count)
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
                    List<Models> models_list = Models.ListForm(url_models, key);
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

        /// <summary>
        /// Возвращает значение, обозначающее, является ли тип числовым
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsNumericType(string type)
        {
            type = type.ToLower();
            if (type == "tinyint" || type == "smallint" || type == "mediumint" || type == "int" || type == "bigint" || type == "bool" ||
                type == "boolean" || type == "dec" || type == "decimal" || type == "numeric" || type == "float" || type == "double")
                return true;
            return false;
        }

        /// <summary>
        /// Формирование строки запроса для записи в таблицу отзывов
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="mr"></param>
        /// <param name="pos_on_page"></param>
        /// <param name="goods_id"></param>
        /// <returns></returns>
        private string InsertQueryStringForm(ModelReview mr, string goods_id)
        {
            string query_string = string.Format("INSERT INTO [dbo].[{0}] ([{1}], ", _fields.Table, _fields.GoodsIDTo);

            for (int i = 0; i < _fields.ya_fields.Count; i++)
            {
                query_string += string.Format("[{0}], ", _fields.ya_fields[i].FieldName);
            }

            for (int i = 0; i < _fields.unknown_fields.Count; i++)
            {
                query_string += string.Format("[{0}], ", _fields.unknown_fields[i].FieldName);
            }

            query_string = query_string.Substring(0, query_string.Length - 2);//удаление запятой
            query_string += String.Format(") VALUES ({0}, ", goods_id);

            for (int i = 0; i < _fields.ya_fields.Count; i++)
            {
                if (!IsNumericType(_fields.ya_fields[i].Type))
                    query_string += string.Format("'{0}', ", mr.GetElementByName(_fields.ya_fields[i].YandexElementName));
                else
                    query_string += string.Format("{0}, ", mr.GetElementByName(_fields.ya_fields[i].YandexElementName));
            }

            for (int i = 0; i < _fields.unknown_fields.Count; i++)
            {
                if (_fields.unknown_fields[i].Dependency == null)
                {
                    if (!IsNumericType(_fields.unknown_fields[i].Type))
                        query_string += string.Format("'{0}', ", _fields.unknown_fields[i].Value);
                    else
                        query_string += string.Format("{0}, ", _fields.unknown_fields[i].Value);
                }
                else
                {
                    if (!IsNumericType(_fields.unknown_fields[i].Type))
                        query_string += string.Format("'{0}', ", mr.GetElementByName(_fields.unknown_fields[i].Dependency));
                    else
                        query_string += string.Format("{0}, ", mr.GetElementByName(_fields.unknown_fields[i].Dependency));
                }
            }

            query_string = query_string.Substring(0, query_string.Length - 2);//удаление запятой
            query_string += ")";

            return query_string;
        }
        private string FormMessage(string message)
        {
            return String.Format("\r\n{3}\r\n{0}\r\nПрограмма работала в течении {1}.\r\nТаблица была дополнена {2} {4}."
                    , dt, stopWatch.Elapsed, lg.added_count, message, target);
        }
        private string FormMessage(WebException we)
        {
            try
            {
                XDocument resp = XDocument.Load(new StreamReader(we.Response.GetResponseStream()));
                if (resp.Descendants("errors").ElementAt(0).Value == "Rate limit exceeded")
                    return FormMessage("Превышение максимального количества запросов.");
                return String.Format("\r\n{3}\r\n{0}\r\nПрограмма работала в течении {1}.\r\nТаблица была дополнена {2} {4}."
                        , dt, stopWatch.Elapsed, lg.added_count, resp.Descendants("errors").ElementAt(0).Value, target);
            }
            catch (Exception e)
            {
                return FormMessage("Проблемы с подключением к серверу.");
            }
            
        }
        private string FormMessage(Exception e)
        {
            return String.Format("\r\n{3}\r\n{0}\r\nПрограмма работала в течении {1}.\r\nТаблица была дополнена {2} {4}."
                    , dt, stopWatch.Elapsed, lg.added_count, e.Message, target);
        }
        /// <summary>
        /// Загрузка комментариев
        /// </summary>
        /// <param name="tb">TextBox вывода</param>
        public string LoadNewReviews(IProgress<string> progress, CancellationToken token)//(TextForControl text)
        {
            url = String.Format("http://localhost:1553/https://api.content.market.yandex.ru/v1/");

            string progress_line = string.Empty;
            stopWatch.Start();
            target = "комментариями";

            lg.exit_category_number = lg.log_category_number;
            lg.exit_subcategory_number = lg.log_subcategory_number;
            lg.exit_page_number = lg.log_page_number;

            int query_count = 0;
            url += "model/";

            SqlConnection sqlConnection = new SqlConnection(_connectionString);
            sqlConnection.Open();
            
            string queryString = string.Format(@"SELECT GI1.GoodsID, {0}, GI1.GoodsName FROM {1} LEFT JOIN YandexReviews.dbo.GoodsInfo as GI1 ON {2} =  GI1.GoodsName collate Cyrillic_General_CI_AS WHERE GI1.GoodsName is not null ORDER BY Shop4KNS.dbo.GoodsInfo.GoodsID",
                            _fields.DB + ".dbo." + _fields.TableFrom + "." + _fields.GoodsIDFrom, _fields.DB + ".dbo." + _fields.TableFrom, _fields.DB + ".dbo." + _fields.GoodsNameTableFrom + "." + _fields.GoodsNameFrom);
            try
            {

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
                    if (token.IsCancellationRequested)
                    {
                        throw new Exception("Программа звершила работу");
                    }
                    for (int i = lg.log_model_number; i < matched_id.Count; i++)
                    {
                        lg.exit_model_number = i;
                        if (i != lg.log_model_number)
                            lg.log_page_reviews_number = 0;

                        int added_count_for_this_model = 0;

                        if (token.IsCancellationRequested)
                        {
                            throw new Exception("Программа звершила работу");
                        }
                        for (int page = lg.log_page_reviews_number + 1; page <= 50; page++)
                        {
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
                            if (token.IsCancellationRequested)
                            {
                                throw new Exception("Программа звершила работу");
                            }
                            for (int pos = 0; pos < mr.Count; pos++)
                            {
                                added_count_for_this_model++;
                                queryString = InsertQueryStringForm(mr[pos], matched_id[i][1]);
                                sqlCommand = new SqlCommand(queryString, sqlConnection);
                                rd = sqlCommand.ExecuteReader();
                                rd.Close();
                                lg.added_count++;
                                seen_count++;
                            }
                        }
                        if (token.IsCancellationRequested)
                        {
                            throw new Exception("Программа звершила работу");
                        }
                        //tb.Text += String.Format("Добавлено {0} отзывов для товара {1}\n", added_count_for_this_model, matched_id[i][2]);
                        progress_line += String.Format("Добавлено {0} отзывов для товара {1}\n", added_count_for_this_model, matched_id[i][2]);
                        progress.Report(progress_line);
                    }
                    sqlConnection.Close();
                    progress_line += FormMessage("Программа звершила работу");
                }
                else
                {
                    sqlConnection.Close();
                    throw new Exception("Не найдено совпадающих товаров.");
                }
            }
            catch (WebException we)
            {
                stopWatch.Stop();
                string message = FormMessage(we);
                progress_line += message; progress.Report(progress_line);
                lg.End(message, dt, stopWatch, target);
            }
            catch (Exception e)
            {
                stopWatch.Stop();
                progress_line += FormMessage(e); progress.Report(progress_line);
                lg.End(e.Message, dt, stopWatch, target);
            }
            return progress_line;
        }

        public void End(IProgress<string> progress)
        {
            
        }
    }
}