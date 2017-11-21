using System;
using System.Collections.Generic;

namespace VersionedStorage
{
    /// <summary>
    /// Интерфейс параметризованного запроса к источнику данных
    /// </summary>
    public interface IParametrizedQuery
    {
        /// <summary>
        /// Текст запроса
        /// </summary>
        String Text { get; set; }

        /// <summary>
        /// Набор параметров запроса
        /// </summary>
        Dictionary<String, object> Parameters { get; set; }
    }
}
