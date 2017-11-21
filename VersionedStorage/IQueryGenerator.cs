using System;
using System.Collections.Generic;

namespace VersionedStorage
{
    /// <summary>
    /// Интерфейс генератора Insert-Update-Delete-Select запросов к источнику данных
    /// </summary>
    public interface IQueryGenerator
    {
        /// <summary>
        /// Создание параметризованного запроса для создания записи в источнике данных
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="fieldValues">Значения полей добавляемой записи</param>
        /// <returns>Параметризованный запрос</returns>
        IParametrizedQuery GenerateInsert(String tableName, Dictionary<String, object> fieldValues);

        /// <summary>
        /// Создание параметризованного запроса для обновления записи в источнике данных
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="keyFieldValues">Значения ключевых полей обновляемой записи</param>
        /// <param name="newFieldValues">Новые значения полей обновляемой записи</param>
        /// <returns>Параметризованный запрос</returns>
        IParametrizedQuery GenerateUpdate(String tableName, Dictionary<String, object> keyFieldValues, Dictionary<String, object> newFieldValues);

        /// <summary>
        /// Создание параметризованного запроса для удаления записи в источнике данных
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="keyFieldValues">Значения ключевых полей удаляемой записи</param>
        /// <returns>Параметризованный запрос</returns>
        IParametrizedQuery GenerateDelete(String tableName, Dictionary<String, object> keyFieldValues);

        /// <summary>
        /// Создание параметризованного запроса для выборки записей из источника данных
        /// </summary>
        /// <param name="tableName">Имя таблицы / набора данных</param>
        /// <param name="selectFields">Список полей или выражений для получения из источника данных</param>
        /// <param name="keyFieldValues">Значения ключевых полей для фильтрации отбираемых записей</param>
        /// <returns>Параметризованный запрос</returns>
        IParametrizedQuery GenerateSelect(String tableName, String[] selectFields, Dictionary<String, object> keyFieldValues);

        /// <summary>
        /// Формирование объявления параметра в запросе к источнику данных
        /// </summary>
        /// <param name="parameterName">Имя параметра</param>
        /// <returns>Параметризованный запрос</returns>
        String GenerateParameter(String parameterName);
    }
}
