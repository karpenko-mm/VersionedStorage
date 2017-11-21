using System;
using System.Collections.Generic;

namespace VersionedStorage
{
    /// <summary>
    /// Генератор простых Insert-Update-Delete-Select SQL-запросов с параметрами
    /// </summary>
    public class SqlQueryGenerator : IQueryGenerator
    {
        /// <summary>
        /// Создание параметризованного запроса для создания записи в источнике данных
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="fieldValues">Значения полей добавляемой записи</param>
        /// <returns>Параметризованный запрос</returns>
        public virtual IParametrizedQuery GenerateInsert(string tableName, Dictionary<String, object> fieldValues)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw Exceptions.ArgumentEmptyException("tableName");
            if (fieldValues == null)
                throw new ArgumentNullException("fieldValues");
            if (fieldValues.Count == 0)
                throw Exceptions.CollectionEmptyException("fieldValues");

            IParametrizedQuery query = new ParametrizedQuery();

            //создание частей "values (...)" в sql-запросе
            int paramIndex = 0;
            string[] valuesParts = new string[fieldValues.Count];
            foreach (var fieldValue in fieldValues)
            {
                String paramName = String.Format("param{0}", paramIndex);
                String sqlParamName = GenerateParameter(paramName);
                valuesParts[paramIndex] = sqlParamName;
                query.Parameters.Add(paramName, fieldValue.Value);
                paramIndex++;
            }

            query.Text = String.Format("insert into {0} ({1}) values ({2})",
                tableName,
                String.Join(", ", fieldValues.Keys),
                String.Join(", ", valuesParts));

            return query;
        }


        /// <summary>
        /// Создание параметризованного запроса для обновления записи в источнике данных
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="keyFieldValues">Значения ключевых полей обновляемой записи</param>
        /// <param name="newFieldValues">Новые значения полей обновляемой записи</param>
        /// <returns>Параметризованный запрос</returns>
        public virtual IParametrizedQuery GenerateUpdate(string tableName, Dictionary<String, object> keyFieldValues, Dictionary<String, object> updateFieldValues)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw Exceptions.ArgumentEmptyException("tableName");
            if (keyFieldValues == null)
                throw new ArgumentNullException("keyFieldValues");
            if (keyFieldValues.Count == 0)
                throw Exceptions.CollectionEmptyException("keyFieldValues");
            if (updateFieldValues == null)
                throw new ArgumentNullException("updateFieldValues");
            if (updateFieldValues.Count == 0)
                throw Exceptions.CollectionEmptyException("updateFieldValues");

            IParametrizedQuery query = new ParametrizedQuery();

            //создание частей "where ..." в sql-запросе
            int paramIndex = 0;
            String[] whereParts = new String[keyFieldValues.Count];
            foreach (var keyFieldValue in keyFieldValues)
            {
                String paramName = String.Format("param{0}", paramIndex);
                String sqlParamName = GenerateParameter(paramName);

                if (!CheckValueIsEquivalentToDbNull(keyFieldValue.Value))
                {
                    whereParts[paramIndex] = String.Format("{0}={1}", keyFieldValue.Key, sqlParamName);
                    query.Parameters.Add(paramName, keyFieldValue.Value);
                }
                else
                {
                    whereParts[paramIndex] = String.Format("{0} is NULL", keyFieldValue.Key);
                }

                paramIndex++;
            }

            //создание частей "set ..." в sql-запросе
            String[] setParts = new String[updateFieldValues.Count];
            foreach (var updateFieldValue in updateFieldValues)
            {
                String paramName = String.Format("param{0}", paramIndex);
                String sqlParamName = GenerateParameter(paramName);
                setParts[paramIndex - whereParts.Length] = String.Format("{0}={1}", updateFieldValue.Key, sqlParamName);
                query.Parameters.Add(paramName, updateFieldValue.Value);
                paramIndex++;
            }

            query.Text = String.Format("update {0} set {1} where {2}",
                tableName,
                String.Join(", ", setParts),
                String.Join(" and ", whereParts));

            return query;
        }


        /// <summary>
        /// Создание параметризованного запроса для удаления записи в источнике данных
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="keyFieldValues">Значения ключевых полей удаляемой записи</param>
        /// <returns>Параметризованный запрос</returns>
        public virtual IParametrizedQuery GenerateDelete(string tableName, Dictionary<String, object> keyFieldValues)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw Exceptions.ArgumentEmptyException("tableName");
            if (keyFieldValues == null)
                throw new ArgumentNullException("keyFieldValues");
            if (keyFieldValues.Count == 0)
                throw Exceptions.CollectionEmptyException("keyFieldValues");

            IParametrizedQuery query = new ParametrizedQuery();

            //создание частей "where ..." в sql-запросе
            int paramIndex = 0;
            String[] whereParts = new String[keyFieldValues.Count];
            foreach (var keyFieldValue in keyFieldValues)
            {
                String paramName = String.Format("param{0}", paramIndex);
                String sqlParamName = GenerateParameter(paramName);

                if (!CheckValueIsEquivalentToDbNull(keyFieldValue.Value))
                {
                    whereParts[paramIndex] = String.Format("{0}={1}", keyFieldValue.Key, sqlParamName);
                    query.Parameters.Add(paramName, keyFieldValue.Value);
                }
                else
                {
                    whereParts[paramIndex] = String.Format("{0} is NULL", keyFieldValue.Key);
                }

                paramIndex++;
            }

            query.Text = String.Format("delete from {0} where {1}",
                tableName,
                String.Join(" and ", whereParts));

            return query;
        }


        /// <summary>
        /// Создание параметризованного запроса для выборки записей из источника данных
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="selectFields">Список полей или выражений для получения из источника данных</param>
        /// <param name="keyFieldValues">Значения ключевых полей для фильтрации отбираемых записей</param>
        /// <returns>Параметризованный запрос</returns>
        public virtual IParametrizedQuery GenerateSelect(string tableName, string[] selectFields, Dictionary<string, object> keyFieldValues)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw Exceptions.ArgumentEmptyException("tableName");
            if (selectFields == null)
                throw new ArgumentNullException("selectFields");
            if (selectFields.Length == 0)
                throw Exceptions.CollectionEmptyException("selectFields");
            if (keyFieldValues == null)
                throw new ArgumentNullException("keyFieldValues");

            IParametrizedQuery query = new ParametrizedQuery();

            //создание частей "where ..." в sql-запросе
            int paramIndex = 0;
            String[] whereParts = new String[keyFieldValues.Count];
            foreach (var keyFieldValue in keyFieldValues)
            {
                String paramName = String.Format("param{0}", paramIndex);
                String sqlParamName = GenerateParameter(paramName);

                if (!CheckValueIsEquivalentToDbNull(keyFieldValue.Value))
                {
                    whereParts[paramIndex] = String.Format("{0}={1}", keyFieldValue.Key, sqlParamName);
                    query.Parameters.Add(paramName, keyFieldValue.Value);
                }
                else
                {
                    whereParts[paramIndex] = String.Format("{0} is NULL", keyFieldValue.Key);
                }

                paramIndex++;
            }

            String whereClause = (whereParts.Length > 0)
                ? String.Format(" where {0}", String.Join(" and ", whereParts))
                : "";

            query.Text = String.Format("select {0} from {1}{2}",
                String.Join(", ", selectFields),
                tableName,
                whereClause);

            return query;
        }


        /// <summary>
        /// Формирование объявления параметра в запросе к источнику данных
        /// </summary>
        /// <param name="parameterName">Имя параметра</param>
        /// <returns>Параметризованный запрос</returns>
        public virtual String GenerateParameter(string parameterName)
        {
            if (String.IsNullOrWhiteSpace(parameterName))
                throw Exceptions.ArgumentEmptyException(parameterName);

            return String.Format("@{0}", parameterName);
        }


        /// <summary>
        /// Проверка необходимости конвертирования значения в System.DbNull.Value для хранения пустых значений в источнике данных
        /// </summary>
        /// <param name="value">Проверяемое значение</param>
        /// <returns>true, если value содержит недействительное или пустое значение, которое в источнике данных следует хранить как System.DbNull.Value</returns>
        private bool CheckValueIsEquivalentToDbNull(object value)
        {
            return (value == System.DBNull.Value
                || value == null
                || (value is DateTime && Convert.ToDateTime(value) == DateTime.MinValue)
                || (value is String && String.IsNullOrEmpty(value as String))
                );
        }
    }
}
