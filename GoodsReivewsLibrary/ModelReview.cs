﻿using System;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoodsReivewsLibrary
{
    /// <summary>
    /// Класс для работы с отзывами о товарах
    /// </summary>
    public class ModelReview
    {
        /// <summary>
        /// Оценка товара
        /// </summary>
        public int Grade { get; set; }

        /// <summary>
        /// Комментарий к товару
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// Имя автора отзыва
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// Дата, когда был написан отзыв
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Положительный стороны модели
        /// </summary>
        public string Pro { get; set; }

        /// <summary>
        /// Отрицательные стороны модели
        /// </summary>
        public string Contra { get; set; }

        /// <summary>
        /// Количество согласных
        /// </summary>
        public int Agree { get; set; }

        /// <summary>
        /// Количество несогласных
        /// </summary>
        public int Reject { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grade">Оценка</param>
        /// <param name="text">Комментарий</param>
        /// <param name="pro">Достоинства</param>
        /// <param name="contra">Недостатки</param>
        /// <param name="date">Дата создания отзыва</param>
        /// <param name="author">Имя автора</param>
        /// <param name="agree">Количество согласных</param>
        /// <param name="reject">Количесвто несогласных</param>
        public ModelReview(int grade, string text, string pro, string contra, DateTime date, string author, int agree, int reject)
        {
            Grade = grade+3;
            Text = GetTextContainsInvertedComma(text);
            Pro = GetTextContainsInvertedComma(pro);
            Contra = GetTextContainsInvertedComma(contra);
            Date = date;
            AuthorName = GetTextContainsInvertedComma(author);
            Agree = agree;
            Reject = reject;
        }
        
        /// <summary>
        /// Доступ к нужному элементу по его названию
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetElementByName(string name)
        {
            switch (name)
            {
                case "Grade":
                    return Grade.ToString();
                case "Text":
                    return Text;
                case "Pro":
                    return Pro;
                case "Contra":
                    return Contra;
                case "Date":
                    return Date.ToString();
                case "Name":
                    return AuthorName;
                case "Agree":
                    return Agree.ToString();
                case "Reject":
                    return Reject.ToString();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Изменяет символ ' во входящей строке на ''
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string GetTextContainsInvertedComma(string text)
        {
            StringBuilder text_without_inverted_commas = new StringBuilder(text);
            text_without_inverted_commas = text_without_inverted_commas.Replace("'", "''");
            return text_without_inverted_commas.ToString();
        }

        /// <summary>
        /// Возвращает список отзывов о модели
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<ModelReview> GetReviews(string url, string key)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add(String.Format("Authorization: {0}", key));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            XDocument xdoc = XDocument.Load(new StreamReader(response.GetResponseStream()));

            List<ModelReview> ret = new List<ModelReview>();
            var categoryQuery_inf = xdoc.Descendants("opinion");

            for (int i = 0; i < categoryQuery_inf.Count(); i++)
            {
                int grade = int.Parse(YandexResults.GetValue(categoryQuery_inf.ElementAt(i), "grade"));
                int agree = int.Parse(YandexResults.GetValue(categoryQuery_inf.ElementAt(i), "agree"));
                int reject = int.Parse(YandexResults.GetValue(categoryQuery_inf.ElementAt(i), "reject"));
                string pro, contra;
                string text;
                string author;
                try
                {text = categoryQuery_inf.ElementAt(i).Element("text").Value; }
                catch (NullReferenceException) { text = ""; }

                try { pro = categoryQuery_inf.ElementAt(i).Element("pro").Value; }
                catch (NullReferenceException) { pro = ""; }

                try { contra = categoryQuery_inf.ElementAt(i).Element("contra").Value; }
                catch (NullReferenceException) { contra = ""; }

                DateTime date = DateTime.Parse(categoryQuery_inf.ElementAt(i).Element("date").Value);

                try { author = categoryQuery_inf.ElementAt(i).Element("author").Value; }
                catch(NullReferenceException) { author = "Неизвестный"; }

                ret.Add(new ModelReview(grade, text, pro, contra, date, author, agree, reject));
            }
            return ret;

        }
    }
}
