using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GoodsReivewsLibrary
{
    /// <summary>
    /// Класс методов для работы с БД
    /// </summary>
    public static class MyMethods
    {
        /// <summary>
        /// Формирует список БД на данном сервере
        /// </summary>
        /// <param name="data_source">Адрес сервера</param>
        /// <returns>Список БД на данном сервере</returns>
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
            }
            return null;
        }

        /// <summary>
        /// Формирует список таблиц в данной БД
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <returns>Список таблиц в данной БД</returns>
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
        /// Формирует список полей в данной таблице
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="table_name">Имя таблицы</param>
        /// <returns>Список полей в данной таблице</returns>
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
        /// Формирует список полей, предоставляемых Яндекс Маркетом
        /// </summary>
        /// <returns>Список полей, предоставляемых Яндекс Маркетом</returns>
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
        /// Переводит называние параметра Яндекс Маркета с русского на английский
        /// </summary>
        /// <param name="name">Наименование параметра</param>
        /// <returns>Английский аналог русского названия поля, предоставляемого Яндекс Маркетом </returns>
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
        /// Переводит называние параметра Яндекс Маркета с английского на русский 
        /// </summary>
        /// <param name="name">Наименование параметра</param>
        /// <returns>Русский аналог английского названия поля, предоставляемого Яндекс Маркетом</returns>
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
        /// <param name="cmb">Ссылка на ComboBox</param>
        /// <param name="values">Список элементов для заполнения</param>
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
        /// <param name="list">Ссылка на список строк</param>
        /// <param name="val">Значение, которое нужно удалить</param>
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
         /// <param name="list">Ссылка на список полей параметров, известных Яндекс Маркету</param>
         /// <param name="val">Значение, которое нужно удалить</param>
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
        /// Извлекает данные о типе поля
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="table">Имя таблица</param>
        /// <returns>Возвращает словарь из поля/типа</returns>
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
        /// Извлекает поля, указанной таблицы, не принимающие значение NULL, по указанной строке подключения
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="table">Имя таблицы</param>
        /// <returns>Cписок всех полей в таблице, не принимающих значение NULL</returns>
        public static List<string[]> GetNotNullableFields(string connectionString, string table)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            string queryString = String.Format("SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", table);
            SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);
            SqlDataReader rd = sqlCommand.ExecuteReader();
            List<string[]> not_nullable_fields = new List<string[]>();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    string test = null;
                    try { test = rd.GetString(3); }
                    catch (SqlNullValueException) {}
                    if (rd.GetString(2) == "NO" && test == null)
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

        /// <summary>
        /// Находит TextBox с совпадающим именем в указанном родительском контроле
        /// </summary>
        /// <param name="parent">Контрол, в котором находится TextBox</param>
        /// <param name="name">Имя TextBox</param>
        /// <returns>Ссылку на TextBox с совпадающим именем</returns>
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
        /// Находит CheckBox с совпадающим именем в указанном родительском контроле 
        /// </summary>
        /// <param name="parent">Контрол, в котором находится CheckBox</param>
        /// <param name="name">Имя CheckBox</param>
        /// <returns>Ссылку на CheckBox с совпадающим именем</returns>
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
        /// Находит ComboBox с совпадающим именем в указанном родительском контроле
        /// </summary>
        /// <param name="parent">Контрол, в котором находится ComboBox</param>
        /// <param name="name">Имя ComboBox</param>
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
        /// Формирует номер элемента по его имени
        /// </summary>
        /// <param name="name">Имя элемента</param>
        /// <returns>Номер элемента по его имени</returns>
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
        /// Формирует номер TextBox
        /// </summary>
        /// <param name="tb">Ссылка на TextBox</param>
        /// <returns>Номер TextBox</returns>
        public static string GetElementID(TextBox tb)
        {
            string name = tb.Name;
            return GetElementID(name);
        }

        /// <summary>
        /// Формирует номер CheckBox
        /// </summary>
        /// <param name="chb">Ссылка на CheckBox</param>
        /// <returns>Номер CheckBox</returns>
        public static string GetElementID(CheckBox chb)
        {
            string name = chb.Name;
            return GetElementID(name);
        }

        /// <summary>
        /// Формирует номер ComboBox
        /// </summary>
        /// <param name="cmb">Ссылка на ComboBox</param>
        /// <returns>Номер ComboBox</returns>
        public static string GetElementID(ComboBox cmb)
        {
            string name = cmb.Name;
            return GetElementID(name);
        }


        /// <summary>
        /// Проверяет наличие файла настройки с таким именем
        /// </summary>
        /// <param name="name">Имя файла настройки</param>
        /// <returns>True, если файл с таким именем существует</returns>
        public static bool IsNameExist(string name)
        {
            DirectoryInfo dir = new DirectoryInfo("../../../Resources/DBS");
            FileInfo[] files = dir.GetFiles();
            List<string> names = new List<string>();
            for (int i = 0; i < files.Length; i++)
                names.Add(files[i].Name);

            if (names.IndexOf(name + ".dbs") == -1)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Десериализует файл, выбранный c данным именем
        /// </summary>
        /// <param name="file_name">Имя файла</param>
        /// <returns>Объект файла настройки</returns>
        public static Fields Deserialize(string file_name)
        {
            BinaryFormatter bin_form = new BinaryFormatter();
            Fields fields;
            using (FileStream fs = new FileStream(@"..\..\..\Resources\DBS\" + file_name + ".dbs", FileMode.Open))
            {
                fields = (Fields)bin_form.Deserialize(fs);
            }
            return fields;
        }

    }
}
