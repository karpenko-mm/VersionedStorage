using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace VersionedStorage.Tests
{
    /// <summary>
    /// Тесты класса SqlQueryExecutor
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SqlQueryExecutorTests
    {
        #region Инициализация тестов

        private Mock<IDbTransaction> DbTransactionMock;

        private Mock<IDbConnection> DbConnectionMock;
        
        private Mock<IDbCommand> DbCommandMock;

        private Mock<IDataReader> DataReaderMock;


        public SqlQueryExecutorTests()
        {
            DbConnectionMock = new Mock<IDbConnection>();
            DbTransactionMock = new Mock<IDbTransaction>();

            DataReaderMock = SetupDataReaderMock();
            DbCommandMock = SetupDbCommandMock();

            DbConnectionMock.Setup(t => t.CreateCommand()).Returns(() =>
            {
                DbCommandMock.Object.Connection = DbConnectionMock.Object;
                return DbCommandMock.Object;
            });

            DbTransactionMock.Setup(t => t.Connection).Returns(DbConnectionMock.Object);
        }


        private Mock<IDbCommand> SetupDbCommandMock()
        {
            Dictionary<String, object> dataParameters = new Dictionary<string, object>();
            Mock<IDataParameterCollection> dataParameterCollectionMock = new Mock<IDataParameterCollection>();
            dataParameterCollectionMock.Setup(t => t.Add(It.IsAny<IDbDataParameter>()))
                .Returns((IDbDataParameter p) =>
                {
                    dataParameters[p.ParameterName] = p.Value;
                    return dataParameters.Count();
                });
            dataParameterCollectionMock.Setup(t => t[It.IsAny<String>()])
                .Returns((String key) =>
                {
                    return dataParameters[key];
                });

            Mock<IDbCommand> commandMock = new Mock<IDbCommand>();
            commandMock.SetupProperty(t => t.Transaction);
            commandMock.SetupProperty(t => t.Connection);
            commandMock.SetupProperty(t => t.CommandText);
            commandMock.SetupGet(t => t.Parameters).Returns(dataParameterCollectionMock.Object);
            commandMock.Setup(t => t.CreateParameter()).Returns(() =>
            {
                Mock<IDbDataParameter> parameterMock = new Mock<IDbDataParameter>();
                parameterMock.SetupProperty(t => t.Direction);
                parameterMock.SetupProperty(t => t.ParameterName);
                parameterMock.SetupProperty(t => t.Value);
                return parameterMock.Object;
            });

            return commandMock;
        }


        private Mock<IDataReader> SetupDataReaderMock()
        {
            var dataReaderMock = new Mock<IDataReader>();
            dataReaderMock.Setup(m => m.FieldCount).Returns(2);
            dataReaderMock.Setup(m => m.GetName(0)).Returns("DUMMY1");
            dataReaderMock.Setup(m => m.GetName(1)).Returns("DUMMY2");
            dataReaderMock.Setup(m => m.GetFieldType(0)).Returns(typeof(string));
            dataReaderMock.Setup(m => m.GetFieldType(1)).Returns(typeof(Int32));
            dataReaderMock.Setup(m => m.GetValue(0)).Returns("TEST_VALUE");
            dataReaderMock.Setup(m => m.GetValue(1)).Returns(123);

            dataReaderMock.SetupSequence(m => m.Read())
                .Returns(true)
                .Returns(true)
                .Returns(true)
                .Returns(true)
                .Returns(true)
                .Returns(true)
                .Returns(false);

            return dataReaderMock;
        }

        #endregion


        #region Тесты конструктора

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_TransactionIsNull_Exception()
        {            
            SqlQueryExecutor executor = new SqlQueryExecutor(null);
        }


        [TestMethod]
        public void Constructor_TransactionIsNotNull_FieldsAreInitialized()
        {
            Mock<IDbTransaction> transactionMock = new Mock<IDbTransaction>();

            SqlQueryExecutor executor = new SqlQueryExecutor(transactionMock.Object);

            Assert.AreEqual(transactionMock.Object, executor.Transaction);
        }

        #endregion


        #region Тесты ExecuteNonQuery()

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteNonQuery_QueryIsNull_Exception()
        {
            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            executor.ExecuteNonQuery(null);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExecuteNonQuery_QueryTextIsNullOrEmpty_Exception()
        {
            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);

            var result = executor.ExecuteNonQuery(new ParametrizedQuery()
            {
                Text = ""
            });
        }


        [TestMethod]
        public void ExecuteNonQuery_Query_DbCommandSetupOk()
        {
            IParametrizedQuery query = new ParametrizedQuery(
                "update TEST_TABLE set DUMMY=@param1 where DUMMY=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "OLD_VALUE" },
                    { "param1", "NEW_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            executor.ExecuteNonQuery(query);

            Assert.AreEqual(DbConnectionMock.Object, DbCommandMock.Object.Connection);
            Assert.AreEqual(DbTransactionMock.Object, DbCommandMock.Object.Transaction);
        }


        [TestMethod]
        public void ExecuteNonQuery_Query_DbCommandParamsPassed()
        {
            IParametrizedQuery query = new ParametrizedQuery(
                "update TEST_TABLE set DUMMY=@param1 where DUMMY=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "OLD_VALUE" },
                    { "param1", "NEW_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            executor.ExecuteNonQuery(query);

            Assert.AreEqual(query.Text, DbCommandMock.Object.CommandText);
            Assert.AreEqual(2, query.Parameters.Count);
            Assert.AreEqual("OLD_VALUE", DbCommandMock.Object.Parameters["param0"]);
            Assert.AreEqual("NEW_VALUE", DbCommandMock.Object.Parameters["param1"]);
        }


        [TestMethod]
        public void ExecuteNonQuery_Query_DbCommandExecuted()
        {
            DbCommandMock.Setup(t => t.ExecuteNonQuery()).Returns(1);

            IParametrizedQuery query = new ParametrizedQuery(
                "update TEST_TABLE set DUMMY=@param1 where DUMMY=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "OLD_VALUE" },
                    { "param1", "NEW_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            int rowsAffected = executor.ExecuteNonQuery(query);

            Assert.AreEqual(1, rowsAffected);
        }

        #endregion


        #region Тесты ExecuteScalar()

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteScalar_QueryIsNull_Exception()
        {
            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);

            executor.ExecuteScalar(null);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExecuteScalar_QueryTextIsNullOrEmpty_Exception()
        {
            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);

            var result = executor.ExecuteScalar(new ParametrizedQuery()
            {
                Text = ""
            });
        }


        [TestMethod]
        public void ExecuteScalar_Query_DbCommandSetupOk()
        {
            IParametrizedQuery query = new ParametrizedQuery(
                "select max(DUMMY) from TEST_TABLE where TEST_FIELD=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "PARAM_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            executor.ExecuteScalar(query);

            Assert.AreEqual(DbConnectionMock.Object, DbCommandMock.Object.Connection);
            Assert.AreEqual(DbTransactionMock.Object, DbCommandMock.Object.Transaction);
        }


        [TestMethod]
        public void ExecuteScalar_Query_DbCommandParamsPassed()
        {
            IParametrizedQuery query = new ParametrizedQuery(
                "select max(DUMMY) from TEST_TABLE where TEST_FIELD=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "PARAM_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            executor.ExecuteScalar(query);

            Assert.AreEqual(query.Text, DbCommandMock.Object.CommandText);
            Assert.AreEqual(1, query.Parameters.Count);
            Assert.AreEqual("PARAM_VALUE", DbCommandMock.Object.Parameters["param0"]);
        }


        [TestMethod]
        public void ExecuteScalar_Query_DbCommandExecuted()
        {
            DbCommandMock.Setup(t => t.ExecuteScalar()).Returns("execute scalar test");

            IParametrizedQuery query = new ParametrizedQuery(
                "select max(DUMMY) from TEST_TABLE where TEST_FIELD=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "PARAM_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            object scalarValue = executor.ExecuteScalar(query);

            Assert.AreEqual("execute scalar test", scalarValue);
        }

        #endregion


        #region Тесты ExecuteSelect()

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteSelect_QueryIsNull_Exception()
        {
            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);

            executor.ExecuteSelect(null);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExecuteSelect_QueryTextIsNullOrEmpty_Exception()
        {
            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);

            var result = executor.ExecuteSelect(new ParametrizedQuery()
            {
                Text = ""
            });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ExecuteSelect_ZeroRowLimit_Exception()
        {
            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);

            var result = executor.ExecuteSelect(new ParametrizedQuery()
            {
                Text = "select DUMMY FROM TEST_TABLE"
            }, 0);
        }


        [TestMethod]
        public void ExecuteSelect_Query_DbCommandSetupOk()
        {
            Mock<IDataReader> dataReaderMock = SetupDataReaderMock();
            DbCommandMock.Setup(t => t.ExecuteReader()).Returns(dataReaderMock.Object);

            IParametrizedQuery query = new ParametrizedQuery(
                "select DUMMY1, DUMMY2 from TEST_TABLE where TEST_FIELD=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "PARAM_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            executor.ExecuteSelect(query);

            Assert.AreEqual(DbConnectionMock.Object, DbCommandMock.Object.Connection);
            Assert.AreEqual(DbTransactionMock.Object, DbCommandMock.Object.Transaction);
        }


        [TestMethod]
        public void ExecuteSelect_Query_DbCommandParamsPassed()
        {
            Mock<IDataReader> dataReaderMock = SetupDataReaderMock();
            DbCommandMock.Setup(t => t.ExecuteReader()).Returns(dataReaderMock.Object);

            IParametrizedQuery query = new ParametrizedQuery(
                "select DUMMY1, DUMMY2 from TEST_TABLE where TEST_FIELD=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "PARAM_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            executor.ExecuteSelect(query);

            Assert.AreEqual(query.Text, DbCommandMock.Object.CommandText);
            Assert.AreEqual(1, query.Parameters.Count);
            Assert.AreEqual("PARAM_VALUE", DbCommandMock.Object.Parameters["param0"]);
        }


        [TestMethod]
        public void ExecuteSelect_Query_ReturnFieldsIsMatch()
        {
            Mock<IDataReader> dataReaderMock = SetupDataReaderMock();
            DbCommandMock.Setup(t => t.ExecuteReader()).Returns(dataReaderMock.Object);

            IParametrizedQuery query = new ParametrizedQuery(
                "select DUMMY1, DUMMY2 from TEST_TABLE where TEST_FIELD=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "PARAM_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            DataTable dataTable = executor.ExecuteSelect(query);

            Assert.AreEqual(2, dataTable.Columns.Count);
            Assert.AreEqual("DUMMY1", dataTable.Columns[0].ColumnName);
            Assert.AreEqual("DUMMY2", dataTable.Columns[1].ColumnName);
            Assert.AreEqual(typeof(string), dataTable.Columns[0].DataType);
            Assert.AreEqual(typeof(Int32), dataTable.Columns[1].DataType);
        }


        [TestMethod]
        public void ExecuteSelect_QueryNoRowLimit_AllRowsReturned()
        {
            Mock<IDataReader> dataReaderMock = SetupDataReaderMock();
            DbCommandMock.Setup(t => t.ExecuteReader()).Returns(dataReaderMock.Object);

            IParametrizedQuery query = new ParametrizedQuery(
                "select DUMMY1, DUMMY2 from TEST_TABLE where TEST_FIELD=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "PARAM_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            DataTable dataTable = executor.ExecuteSelect(query);

            Assert.AreEqual(6, dataTable.Rows.Count);
        }


        [TestMethod]
        public void ExecuteSelect_QueryRowLimit_LimitRowsReturned()
        {
            Mock<IDataReader> dataReaderMock = SetupDataReaderMock();
            DbCommandMock.Setup(t => t.ExecuteReader()).Returns(dataReaderMock.Object);

            IParametrizedQuery query = new ParametrizedQuery(
                "select DUMMY1, DUMMY2 from TEST_TABLE where TEST_FIELD=@param0",
                new Dictionary<string, object>()
                {
                    { "param0", "PARAM_VALUE" }
                });

            SqlQueryExecutor executor = new SqlQueryExecutor(DbTransactionMock.Object);
            DataTable dataTable = executor.ExecuteSelect(query, 3);

            Assert.AreEqual(3, dataTable.Rows.Count);
        }

        #endregion
    }
}
