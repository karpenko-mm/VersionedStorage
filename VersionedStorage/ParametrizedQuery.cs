using System;
using System.Collections.Generic;

namespace VersionedStorage
{
    /// <summary>
    /// Параметризованный запрос к источнику данных
    /// </summary>
    public class ParametrizedQuery: IParametrizedQuery
    {
        /// <summary>
        /// Текст запроса
        /// </summary>
        public String Text { get; set; }

        /// <summary>
        /// Набор параметров запроса
        /// </summary>
        public Dictionary<String, object> Parameters { get; set; }


        /// <summary>
        /// Создание параметризованного запроса к источнику данных с пустым текстом запроса и с пустым набором параметров
        /// </summary>
        public ParametrizedQuery()
        {
            Parameters = new Dictionary<string, object>();
        }


        /// <summary>
        /// Создание параметризованного запроса к источнику данных с заданным текстом запроса и с пустым набором параметров
        /// </summary>
        /// <param name="text">Текст запроса</param>
        public ParametrizedQuery(String text)
        {
            if (String.IsNullOrWhiteSpace(text))
                throw Exceptions.ArgumentEmptyException("text");

            Parameters = new Dictionary<string, object>();
            Text = text;
        }


        /// <summary>
        /// Создание параметризованного запроса к источнику данных с заданным текстом запроса и списком параметров
        /// </summary>
        /// <param name="text">Текст запроса</param>
        /// <param name="parameters">Набор параметров запроса</param>
        public ParametrizedQuery(String text, Dictionary<String, object> parameters)
        {
            if (String.IsNullOrWhiteSpace(text))
                throw Exceptions.ArgumentEmptyException("text");
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            Text = text;
            Parameters = parameters;
        }
    }
}
