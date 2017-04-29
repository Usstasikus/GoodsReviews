﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GoodsReivewsLibrary
{
    public static class MyMethods
    {
        public static List<string> GetDBList(string data_source)
        {
            List<string> dbs = new List<string>();
            SqlConnectionStringBuilder conStr = new SqlConnectionStringBuilder();
            conStr.DataSource = data_source;
            conStr.IntegratedSecurity = true;
            string query = "SELECT name FROM sys.databases WHERE database_id > 4";
            DataTable table = new DataTable("Dbs");
            try
            {
                using (SqlConnection con = new SqlConnection(conStr.ToString()))
                {
                    con.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                    adapter.Fill(table);

                    con.Close();
                }
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    dbs.Add(table.Rows[i][0].ToString());
                }
                return dbs;
            }
            catch (Exception)
            {
                MessageBox.Show("Неверно введен адрес сервера");
            }
            return null;
        }

        /// <summary>
        /// Возвращает список таблиц в данной БД
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static List<string> GetTablesList(string connectionString)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            try
            {
                string queryString = @"SELECT * FROM sys.tables WHERE type_desc = 'USER_TABLE'";
                SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);
                SqlDataReader rd = sqlCommand.ExecuteReader();
                List<string> tables = new List<string>();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        tables.Add(rd.GetString(0));
                    }
                    rd.Close();
                    tables.Sort();
                }
                sqlConnection.Close();
                return tables;
            }
            catch (Exception)
            {
                MessageBox.Show("Неизвестная ошибка");
            }
            sqlConnection.Close();
            return null;
        }


        /// <summary>
        /// Возвращает список полей в данной таблице
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table_name"></param>
        /// <returns></returns>
        public static List<string> GetFieldsList(string connectionString, string table_name)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            string queryString = String.Format("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", table_name);
            try
            {
                SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);
                SqlDataReader rd = sqlCommand.ExecuteReader();
                List<string> fields = new List<string>();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        fields.Add(rd.GetString(0));
                    }
                    rd.Close();
                    fields.Sort();
                }
                sqlConnection.Close();
                return fields;
            }
            catch (Exception)
            {
                MessageBox.Show("Неизвестная ошибка");
            }
            return null;
        }

        /// <summary>
        /// Возвращает список полей, предоставляемых Яндекс Маркетом
        /// </summary>
        /// <returns></returns>
        public static List<string> GetYandexFields()
        {
            List<string> ya_fields = new List<string>();
            ya_fields.Add("Оценка");
            ya_fields.Add("Количество согласных");
            ya_fields.Add("Количество несогласных");
            ya_fields.Add("Дата написания отзыва");
            ya_fields.Add("Имя автора отзыва");
            ya_fields.Add("Текст отзыва");
            ya_fields.Add("Описание достоинств");
            ya_fields.Add("Описание недостатков");
            ya_fields.Add("Опыт использования модели");
            return ya_fields;
        }

        /// <summary>
        /// Возвращает английский аналог русского названия поля, предоставляемого Яндекс Маркетом 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string TurnRussianYandexFieldNameToEnglish(string name)
        {
            switch (name)
            {
                case "Оценка": return "Grade";
                case "Количество согласных": return "Agree";
                case "Количество несогласных": return "Reject";
                case "Дата написания отзыва": return "Date";
                case "Имя автора отзыва": return "Name";
                case "Текст отзыва": return "Text";
                case "Описание достоинств": return "Pro";
                case "Описание недостатков": return "Contra";
                case "Опыт использования модели": return "UsageTime";
                default : return null;
            }
        }

        /// <summary>
        /// Возвращает русский аналог английского названия поля, предоставляемого Яндекс Маркетом 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string TurnEnglishYandexFieldNameToRussian(string name)
        {
            switch (name)
            {
                case "Grade": return "Оценка";
                case "Agree": return "Количество согласных";
                case "Reject": return "Количество несогласных";
                case "Date": return "Дата написания отзыва";
                case "Name": return "Имя автора отзыва";
                case "Text": return "Текст отзыва";
                case "Pro": return "Описание достоинств";
                case "Contra": return "Описание недостатков";
                case "UsageTime": return "Опыт использования модели";
                default: return null;
            }
        }

        /// <summary>
        /// Заполняет ComboBox элементами из списка
        /// </summary>
        /// <param name="cmb"></param>
        /// <param name="values"></param>
        public static void ComboBoxFill(ComboBox cmb, List<string> values)
        {
            cmb.IsEnabled = true;
            cmb.Items.Clear();
            for (int i = 0; i < values.Count; i++)
            {
                cmb.Items.Add(values[i]);
            }

        }

        /// <summary>
        /// Удаляет из списка массива строк тот элемент, у которого первое значение массива совпадает с данной строкой
        /// </summary>
        /// <param name="list"></param>
        /// <param name="val"></param>
        public static void RemoveByFirstValue(List<string[]> list, string val)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i][0] == val)
                {
                    list.RemoveAt(i);
                    break;
                }
            }
        }/// <summary>
         /// Удаляет из списка KnownField тот элемент, у которого значение TableElementName совпадает с данной строкой
         /// </summary>
         /// <param name="list"></param>
         /// <param name="val"></param>
        public static void RemoveByFirstValue(List<KnownField> list, string val)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].FieldName == val)
                {
                    list.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Возвращает словарь из поля/типа
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetFieldsType(string connectionString, string table)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            string queryString = String.Format("SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", table);
            SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);
            SqlDataReader rd = sqlCommand.ExecuteReader();
            Dictionary<string, string> fields_types = new Dictionary<string, string>();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    fields_types.Add(rd.GetString(0), rd.GetString(1));
                }
                rd.Close();
            }
            return fields_types;
        }

        /// <summary>
        /// Первый индекс - имя поля, второй - тип поля
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns>Cписок всех полей в таблице, не принимающих значение NULL</returns>
        public static List<string[]> GetNotNullableFields(string connectionString, string table)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            string queryString = String.Format("SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", table);
            SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);
            SqlDataReader rd = sqlCommand.ExecuteReader();
            List<string[]> not_nullable_fields = new List<string[]>();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    if (rd.GetString(2) == "NO")
                        not_nullable_fields.Add(new string[] { rd.GetString(0), rd.GetString(1) });
                }
                rd.Close();
                not_nullable_fields.Sort(delegate (string[] x, string[] y)
                {
                    return String.Compare(x[0], y[0]);
                });
            }
            return not_nullable_fields;
        }

        public static void DictionaryAdd(Dictionary<string, string> pairs, string key, string value)
        {
            try
            {
                pairs.Add(key, value);
            }
            catch (ArgumentException)
            {
                pairs.Remove(key);
                pairs.Add(key, value);
            }
            catch (Exception)
            {
                MessageBox.Show("Неизвестная ошибка");
            }
        }

        public static void AddToList(List<UnknownField> unknown_fields, UnknownField uf)
        {
            for (int i = 0; i < unknown_fields.Count; i++)
            {
                if (unknown_fields[i].FieldName == uf.FieldName)
                {
                    unknown_fields[i] = uf;
                    return;
                }
            }
            unknown_fields.Add(uf);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns>Возвращает ссылку на TextBox с совпадающим именем</returns>
        public static TextBox GetTextBoxByName(StackPanel parent, string name)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                try
                {
                    if (!(parent.Children[i] is TextBox))
                        continue;
                    if (((TextBox)parent.Children[i]).Name == name)
                        return (TextBox)parent.Children[i];
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns>Возвращает ссылку на CheckBox с совпадающим именем</returns>
        public static CheckBox GetCheckBoxByName(StackPanel parent, string name)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                try
                {
                    if (!(parent.Children[i] is CheckBox))
                        continue;
                    if (((CheckBox)parent.Children[i]).Name == name)
                        return (CheckBox)parent.Children[i];
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns>Возвращает ссылку на ComboBox с совпадающим именем</returns>
        public static ComboBox GetComboBoxByName(StackPanel parent, string name)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                try
                {
                    if (!(parent.Children[i] is ComboBox))
                        continue;
                    if (((ComboBox)parent.Children[i]).Name == name)
                        return (ComboBox)parent.Children[i];
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Возвращает номер элемента по его имени
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetElementID(string name)
        {
            int i;
            for (i = 0; i < name.Length; i++)
            {
                if (name[i] == '_')
                {
                    i++;
                    break;
                }
            }
            string id = name.Substring(i);
            return id;
        }

        /// <summary>
        /// Возвращает номер TextBox
        /// </summary>
        /// <param name="tb"></param>
        /// <returns></returns>
        public static string GetElementID(TextBox tb)
        {
            string name = tb.Name;
            return GetElementID(name);
        }

        /// <summary>
        /// Возвращает номер CheckBox
        /// </summary>
        /// <param name="chb"></param>
        /// <returns></returns>
        public static string GetElementID(CheckBox chb)
        {
            string name = chb.Name;
            return GetElementID(name);
        }

        /// <summary>
        /// Возвращает номер ComboBox
        /// </summary>
        /// <param name="cmb"></param>
        /// <returns></returns>
        public static string GetElementID(ComboBox cmb)
        {
            string name = cmb.Name;
            return GetElementID(name);
        }
        
    }
}
