using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Xml.Linq;
using System.Threading;

namespace GoodsReivewsLibrary
{
    /// <summary>
    /// Класс с методами для получения и записи комментариев из Яндекс Маркета
    /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields">Файл настройки БД</param>
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
            lg = new LogFile(@"..\..\..\Resources\last_pos\last_pos_" + _fields.FileName + ".txt");
            

        }

        /// <summary>
        /// Возвращает список совпадающих элементов
        /// </summary>
        /// <returns></returns>
        private List<MatchedGood> GetMatchedGoods()
        {
            DataTable dataTableYandexModels = new DataTable();
            string connectionStringToYandexModels = String.Format("Data Source={0};Initial Catalog={1};User id={2};Password={3};", "tcp:stasdonserver.database.windows.net,1433", "YandexModels", "ReadOnlyLogin", "ReadOnly1");
            string queryYandexModels = "SELECT * FROM GoodsInfo";
            SqlConnection sqlConnectionToYandexModels = new SqlConnection(connectionStringToYandexModels);
            SqlCommand sqlCommandToYandexModels = new SqlCommand(queryYandexModels, sqlConnectionToYandexModels);
            sqlConnectionToYandexModels.Open();

            SqlDataAdapter sql_d_YandexModels = new SqlDataAdapter(sqlCommandToYandexModels);
            sql_d_YandexModels.Fill(dataTableYandexModels);
            sqlConnectionToYandexModels.Close();
            sql_d_YandexModels.Dispose();

            DataTable dataTableUserTable = new DataTable();
            string queryUserTable = String.Format("SELECT {0}, {1} FROM {2}", _fields.GoodsNameFrom, _fields.GoodsIDFrom, _fields.TableFrom);
            SqlConnection sqlConnectionToUserTable = new SqlConnection(_connectionString);
            SqlCommand sqlCommandToUserTable = new SqlCommand(queryUserTable, sqlConnectionToUserTable);
            sqlConnectionToUserTable.Open();

            SqlDataAdapter sql_da_UserTable = new SqlDataAdapter(sqlCommandToUserTable);
            sql_da_UserTable.Fill(dataTableUserTable);
            sqlConnectionToUserTable.Close();
            sql_da_UserTable.Dispose();
            dataTableUserTable.DefaultView.Sort = _fields.GoodsIDFrom + " DESC";
            dataTableUserTable = dataTableUserTable.DefaultView.ToTable();

            DataTable YandexNames = new DataTable();
            YandexNames = dataTableYandexModels.Copy();
            YandexNames.Columns.Remove("GoodsID");

            DataTable UserNames = new DataTable();
            UserNames = dataTableUserTable.Copy();
            UserNames.Columns.Remove(_fields.GoodsIDFrom);

            DataTable matchedNames = UserNames.AsEnumerable().Intersect(YandexNames.AsEnumerable(), DataRowComparer.Default).CopyToDataTable();

            List<MatchedGood> matched_goods = new List<MatchedGood>();
            foreach(DataRow de in matchedNames.Rows)
            {
                MatchedGood matched_good = new MatchedGood();
                matched_good.GoodsName = de[0].ToString();
                string expression = String.Format("{0} = '{1}'", _fields.GoodsNameFrom, de[0].ToString());
                DataRow[] found_user_id = dataTableUserTable.Select(expression);
                matched_good.UserGoodsID =  found_user_id[0][_fields.GoodsIDFrom].ToString();

                expression = String.Format("GoodsName = '{0}'", de[0].ToString());
                DataRow[] found_yandex_id = dataTableYandexModels.Select(expression);
                matched_good.YandexGoodsID = found_yandex_id[0]["GoodsID"].ToString();

                matched_goods.Add(matched_good);
            }

            return matched_goods;
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
        /// <param name="mr"></param>
        /// <param name="goods_id"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        private SqlCommand InsertQueryStringForm(ModelReview mr, string goods_id, SqlConnection connection)
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
            query_string += String.Format(") VALUES (@{0}, ", _fields.GoodsIDTo);
            for (int i = 0; i < _fields.ya_fields.Count; i++)
            {
                query_string += string.Format("@{0}, ", _fields.ya_fields[i].FieldName);
            }

            for (int i = 0; i < _fields.unknown_fields.Count; i++)
            {
                query_string += string.Format("@{0}, ", _fields.unknown_fields[i].FieldName);
            }

            query_string = query_string.Substring(0, query_string.Length - 2);//удаление запятой 
            query_string += ")";

            SqlCommand sql_command = new SqlCommand(query_string, connection);
            sql_command.Parameters.Add(new SqlParameter(_fields.GoodsIDTo, goods_id));

            for (int i = 0; i < _fields.ya_fields.Count; i++)
            {
                sql_command.Parameters.Add(new SqlParameter(_fields.ya_fields[i].FieldName, mr.GetElementByName(_fields.ya_fields[i].YandexElementName)));
            }

            for (int i = 0; i < _fields.unknown_fields.Count; i++)
            {
                if (_fields.unknown_fields[i].Dependency == null)
                {
                        sql_command.Parameters.Add(new SqlParameter(_fields.unknown_fields[i].FieldName, _fields.unknown_fields[i].Value));
                }
                else
                {
                    sql_command.Parameters.Add(new SqlParameter(_fields.unknown_fields[i].FieldName, mr.GetElementByName(_fields.unknown_fields[i].Dependency)));
                }
            }

            return sql_command;
        }

        /// <summary>
        /// Формирование сообщения при завершении работы программы
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string FormMessage(string message)
        {
            return String.Format("\r\n{0}\r\n{3}\r\nПрограмма работала в течении {1}.\r\nТаблица {5} была дополнена {2} {4}."
                    , dt, stopWatch.Elapsed, lg.added_count, message, target, _fields.Table);
        }

        /// <summary>
        /// Формирование сообщения при завершении работы программы
        /// </summary>
        /// <param name="we"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Формирование сообщения при завершении работы программы
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private string FormMessage(Exception e)
        {
            return String.Format("\r\n{3}\r\n{0}\r\nПрограмма работала в течении {1}.\r\nТаблица была дополнена {2} {4}."
                    , dt, stopWatch.Elapsed, lg.added_count, e.Message, target);
        }

        /// <summary>
        /// Записывает комментарии в в БД пользователя
        /// </summary>
        /// <param name="progress">Объект, инициирующий обратные вызовы</param>
        /// <param name="token">Маркер отмены</param>
        /// <returns>Строку результата работы программы</returns>
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

            

            try
            {
                sqlConnection.Open();
                List<MatchedGood> matched_goods = GetMatchedGoods();
                
                if (token.IsCancellationRequested)
                {
                    throw new Exception("Программа завершила работу");
                }
                for (int i = lg.log_model_number; i < matched_goods.Count; i++)
                {
                    lg.exit_model_number = i;
                    if (i != lg.log_model_number)
                        lg.log_page_reviews_number = 0;

                    int added_count_for_this_model = 0;

                    if (token.IsCancellationRequested)
                    {
                        throw new Exception("Программа завершила работу");
                    }
                    for (int page = lg.log_page_reviews_number + 1; page <= 50; page++)
                    {
                        string url_opinion = url + string.Format("{0}/opinion.xml?geo_id=225&count=30&page={1}", matched_goods[i].YandexGoodsID, page);
                        List<ModelReview> mr;
                        mr = ModelReview.GetReviews(url_opinion, key);
                        lg.exit_page_reviews_number = page;
                        query_count++;
                        if (mr.Count == 0)
                        {
                            lg.Write();
                            break;
                        }
                        if (token.IsCancellationRequested)
                        {
                            throw new Exception("Программа завершила работу");
                        }
                        for (int pos = 0; pos < mr.Count; pos++)
                        {
                            added_count_for_this_model++;
                            SqlCommand sqlCommand = InsertQueryStringForm(mr[pos], matched_goods[i].UserGoodsID, sqlConnection);
                            SqlDataReader rd = sqlCommand.ExecuteReader();
                            rd.Close();
                            lg.added_count++;
                            seen_count++;
                        }
                    }
                    if (token.IsCancellationRequested)
                    {
                        throw new Exception("Программа завершила работу");
                    }
                    //tb.Text += String.Format("Добавлено {0} отзывов для товара {1}\n", added_count_for_this_model, matched_id[i][2]);
                    progress_line += String.Format("Добавлено {0} отзывов для товара {1}\n", added_count_for_this_model, matched_goods[i].GoodsName);
                    progress.Report(progress_line);
                }
                sqlConnection.Close();
                progress_line += FormMessage("Программа завершила работу");

            }
            catch (WebException we)
            {
                sqlConnection.Close();
                stopWatch.Stop();
                string message = FormMessage(we);
                progress_line += message; progress.Report(progress_line);
                lg.End(message, dt, stopWatch, target);
            }
            catch (Exception e)
            {
                sqlConnection.Close();
                stopWatch.Stop();
                progress_line += FormMessage(e); progress.Report(progress_line);
                lg.End(e.Message, dt, stopWatch, target);
            }
            return progress_line;
        }
        
    }
}