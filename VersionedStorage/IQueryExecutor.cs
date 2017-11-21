using System;
using System.Data;

namespace VersionedStorage
{
    /// <summary>
    /// Интерфейс исполнителя запросов к источнику данных
    /// </summary>
    public interface IQueryExecutor
    {
        /// <summary>
        /// Выполнение инструкции источника данных
        /// </summary>
        /// <param name="query">Параметризованный запрос</param>
        /// <returns>Количество задействованных в запросе строк источника данных</returns>
        int ExecuteNonQuery(IParametrizedQuery query);

        /// <summary>
        /// Выполнение запроса к источнику данных, возвращающего одно значение
        /// </summary>
        /// <param name="query">Параметризованный запрос</param>
        /// <returns>Значение, возвращенное источником данных</returns>
        object ExecuteScalar(IParametrizedQuery query);

        /// <summary>
        /// Выполнение запроса к источнику данных, возвращающего набор записей
        /// </summary>
        /// <param name="query">Параметризованный запрос</param>
        /// <param name="rowLimit">Ограничение на число записей, получаемое из источника. Записи сверх указанного количества не будут получены из источника</param>
        /// <returns>Таблица с набором записей</returns>
        DataTable ExecuteSelect(IParametrizedQuery query, int rowLimit = Int32.MaxValue);
    }
}
