using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Moq;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace VersionedStorage.Tests
{
    /// <summary>
    /// Тесты класса GenericVersionedStorage
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class GenericVersionedStorageTests
    {
        #region Инициализация тестов

        private Mock<IQueryGenerator> QueryGeneratorMock;
        private Mock<IQueryExecutor> QueryExecutorMock;
        private String UserId;

        public GenericVersionedStorageTests()
        {
            QueryGeneratorMock = new Mock<IQueryGenerator>();
            QueryExecutorMock = new Mock<IQueryExecutor>();
            UserId = "testUser";
        }

        #endregion


        #region Тесты конструктора

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_QueryGeneratorIsNull_Exception()
        {
            IVersionedStorage storage = new GenericVersionedStorage(null, QueryExecutorMock.Object, UserId);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_QueryExecutorIsNull_Exception()
        {
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, null, UserId);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_UserIdIsNullOrEmpty_Exception()
        {
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, null);
        }


        [TestMethod]
        public void Constructor_ParametersIsNotEmpty_FieldsAreInitialized()
        {
            GenericVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);

            Assert.AreEqual(QueryExecutorMock.Object, storage.QueryExecutor);
            Assert.AreEqual(QueryGeneratorMock.Object, storage.QueryGenerator);
            Assert.AreEqual(UserId, storage.UserId);
        }

        #endregion


        #region Тесты Insert()

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Insert_TableNameIsNullOrEmpty_Exception()
        {
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);

            storage.Insert(
                null,
                new Dictionary<string, object>()
                {
                    { "FIELD1", 1 },
                    { "FIELD2", 2 }
                });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Insert_FieldValuesEmpty_Exception()
        {
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);

            storage.Insert(
                "TEST_TABLE",
                new Dictionary<string, object>());
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Insert_FieldValuesIsNull_Exception()
        {
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);

            storage.Insert(
                "TEST_TABLE",
                null);
        }


        [TestMethod]
        public void Insert_FieldValues_NewRecordCreated()
        {
            //arrange
            String insert_TableName = null;
            Dictionary<String, object> insert_fieldValues = null;

            int generateInsertCalls = 0;
            int executeNonQueryCalls = 0;

            QueryGeneratorMock.Setup(t => t.GenerateInsert(
                    It.IsAny<String>(),
                    It.IsAny<Dictionary<String, object>>())
                )
                .Returns((String tableName,
                    Dictionary<String, object> fieldValues) =>
                {
                    insert_TableName = tableName;
                    insert_fieldValues = fieldValues;
                    generateInsertCalls++;
                    return new Mock<IParametrizedQuery>().Object;
                });

            QueryExecutorMock.Setup(t => t.ExecuteNonQuery(It.IsAny<IParametrizedQuery>())).Returns(() =>
            {
                executeNonQueryCalls++;
                return 1;
            });

            //act
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);
            storage.Insert("TEST_TABLE", new Dictionary<string, object>()
            {
                { "FIELD1", 11 },
                { "FIELD2", "22" }
            });

            //assert
            Assert.AreEqual(1, generateInsertCalls);
            Assert.AreEqual(insert_TableName, "TEST_TABLE");
            Assert.AreEqual(3 + 2, insert_fieldValues.Count);
            Assert.IsTrue(insert_fieldValues.ContainsKey("CREATED_DATE"));
            Assert.IsTrue(insert_fieldValues.ContainsKey("CREATED_BY"));
            Assert.IsTrue(insert_fieldValues.ContainsKey("IS_ACTIVE"));
            Assert.IsTrue(insert_fieldValues.ContainsKey("FIELD1"));
            Assert.IsTrue(insert_fieldValues.ContainsKey("FIELD2"));
            Assert.AreEqual(UserId, insert_fieldValues["CREATED_BY"]);
            Assert.AreEqual(1, insert_fieldValues["IS_ACTIVE"]);
            Assert.AreEqual(11, insert_fieldValues["FIELD1"]);
            Assert.AreEqual("22", insert_fieldValues["FIELD2"]);
        }


        [TestMethod]
        public void Insert_FieldValues_NewRecordIdReturned()
        {
            //arrange
            int executeScalarCalls = 0;

            QueryExecutorMock.Setup(t => t.ExecuteNonQuery(It.IsAny<IParametrizedQuery>())).Returns(1);

            QueryExecutorMock.Setup(t => t.ExecuteScalar(It.IsAny<IParametrizedQuery>())).Returns(() =>
            {
                executeScalarCalls++;
                return 101;
            });

            //act
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);
            int createdRecordId = storage.Insert("TEST_TABLE", new Dictionary<string, object>()
            {
                { "FIELD1", 11 },
                { "FIELD2", "22" }
            });

            //assert
            Assert.AreEqual(1, executeScalarCalls);
            Assert.AreEqual(101, createdRecordId);
        }

        #endregion


        #region Тесты Update()

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Update_TableNameIsNullOrEmpty_Exception()
        {
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);

            storage.Update(
                null,
                1,
                new Dictionary<string, object>()
                {
                    { "FIELD1", 1 },
                    { "FIELD2", "2" }
                });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Update_NewFieldValuesEmpty_Exception()
        {
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);

            storage.Update(
                "TEST_TABLE",
                1,
                new Dictionary<string, object>());
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Update_NewFieldValuesIsNull_Exception()
        {
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);

            storage.Update(
                "TEST_TABLE",
                1,
                null);
        }


        [TestMethod]
        public void Update_ExistingRecord_SetIsActive0()
        {
            //arrange
            String update_tableName = null;
            Dictionary<String, object> update_keyFieldValues = null;
            Dictionary<String, object> update_newFieldValues = null;

            int generateUpdateCalls = 0;

            QueryGeneratorMock.Setup(t => t.GenerateUpdate(
                    It.IsAny<String>(),
                    It.IsAny<Dictionary<String, object>>(),
                    It.IsAny<Dictionary<String, object>>())
                )
                .Returns((String tableName,
                    Dictionary<String, object> keyFieldValues,
                    Dictionary<String, object> newFieldValues) =>
                {
                    update_tableName = tableName;
                    update_keyFieldValues = keyFieldValues;
                    update_newFieldValues = newFieldValues;
                    generateUpdateCalls++;
                    return new Mock<IParametrizedQuery>().Object;
                });

            QueryExecutorMock.Setup(t => t.ExecuteNonQuery(It.IsAny<IParametrizedQuery>())).Returns(1);

            //act
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);
            storage.Update(
                "TEST_TABLE", 
                101,
                new Dictionary<string, object>()
                {
                    { "FIELD_1", "new_value" }
                });

            //assert
            Assert.AreEqual(1, generateUpdateCalls);
            Assert.AreEqual(update_tableName, "TEST_TABLE");
            Assert.IsTrue(update_keyFieldValues.ContainsKey("RECORD_ID"));
            Assert.IsTrue(update_keyFieldValues.ContainsKey("IS_ACTIVE"));
            Assert.AreEqual(101, update_keyFieldValues["RECORD_ID"]);
            Assert.AreEqual(1, update_keyFieldValues["IS_ACTIVE"]);
            Assert.IsTrue(update_newFieldValues.ContainsKey("IS_ACTIVE"));
            Assert.AreEqual(0, update_newFieldValues["IS_ACTIVE"]);
        }


        [TestMethod]
        public void Update_ExistingRecord_NewVersionCreated()
        {
            //arrange
            String insert_tableName = null;
            Dictionary<String, object> insert_fieldValues = null;

            int generateInsertCalls = 0;

            QueryGeneratorMock.Setup(t => t.GenerateInsert(
                    It.IsAny<String>(),
                    It.IsAny<Dictionary<String, object>>())
                )
                .Returns((String tableName,
                    Dictionary<String, object> fieldValues) =>
                {
                    insert_tableName = tableName;
                    insert_fieldValues = fieldValues;
                    generateInsertCalls++;
                    return new Mock<IParametrizedQuery>().Object;
                });

            QueryExecutorMock.Setup(t => t.ExecuteNonQuery(It.IsAny<IParametrizedQuery>())).Returns(1);

            //act
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);
            storage.Update(
                "TEST_TABLE",
                101,
                new Dictionary<string, object>()
                {
                    { "FIELD_1", "new_value" }
                });

            //assert
            Assert.AreEqual(1, generateInsertCalls);
            Assert.AreEqual(insert_tableName, "TEST_TABLE");
            Assert.IsTrue(insert_fieldValues.ContainsKey("PREV_RECORD_ID"));
            Assert.IsTrue(insert_fieldValues.ContainsKey("IS_ACTIVE"));
            Assert.IsTrue(insert_fieldValues.ContainsKey("CREATED_BY"));
            Assert.IsTrue(insert_fieldValues.ContainsKey("CREATED_DATE"));
            Assert.IsTrue(insert_fieldValues.ContainsKey("FIELD_1"));
            Assert.AreEqual(101, insert_fieldValues["PREV_RECORD_ID"]);
            Assert.AreEqual(1, insert_fieldValues["IS_ACTIVE"]);
            Assert.AreEqual(UserId, insert_fieldValues["CREATED_BY"]);
            Assert.AreEqual("new_value", insert_fieldValues["FIELD_1"]);
        }


        [TestMethod]
        public void Update_ExistingRecord_NewRecordIdReturned()
        {
            //arrange
            int executeScalarCalls = 0;

            QueryExecutorMock.Setup(t => t.ExecuteNonQuery(It.IsAny<IParametrizedQuery>())).Returns(1);

            QueryExecutorMock.Setup(t => t.ExecuteScalar(It.IsAny<IParametrizedQuery>()))
                .Returns((IParametrizedQuery query) =>
                {
                    executeScalarCalls++;
                    return 102;
                });            

            //act
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);
            int newRecordVersionId = storage.Update(
                "TEST_TABLE",
                101,
                new Dictionary<string, object>()
                {
                    { "FIELD_1", "new_value" }
                });

            //assert
            Assert.AreEqual(1, executeScalarCalls);
            Assert.AreEqual(102, newRecordVersionId);
        }


        [TestMethod]
        [ExpectedException(typeof(DBConcurrencyException))]
        public void Update_RecordNotExists_Exception()
        {
            QueryExecutorMock.Setup(t => t.ExecuteNonQuery(It.IsAny<IParametrizedQuery>()))
                .Returns((IParametrizedQuery query) =>
                {
                    return 0;
                });

            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);

            storage.Update(
                "TEST_TABLE",
                101,
                new Dictionary<string, object>()
                {
                    { "FIELD_1", 1 }
                });
        }

        #endregion


        #region Тесты Delete()

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Delete_TableNameIsNullOrEmpty_Exception()
        {
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);

            storage.Delete(null, 1);
        }


        [TestMethod]
        public void Delete_ExistingRecord_SetIsAcive0()
        {
            //arrange
            String update_tableName = null;
            Dictionary<String, object> update_keyFieldValues = null;
            Dictionary<String, object> update_newFieldValues = null;

            int generateUpdateCalls = 0;

            QueryGeneratorMock.Setup(t => t.GenerateUpdate(
                    It.IsAny<String>(),
                    It.IsAny<Dictionary<String, object>>(),
                    It.IsAny<Dictionary<String, object>>())
                )
                .Returns((String tableName,
                    Dictionary<String, object> keyFieldValues,
                    Dictionary<String, object> newFieldValues) =>
                    {
                        update_tableName = tableName;
                        update_keyFieldValues = keyFieldValues;
                        update_newFieldValues = newFieldValues;
                        generateUpdateCalls++;
                        return new Mock<IParametrizedQuery>().Object;
                    });

            QueryExecutorMock.Setup(t => t.ExecuteNonQuery(It.IsAny<IParametrizedQuery>())).Returns(1);

            //act
            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);
            storage.Delete("TEST_TABLE", 101);

            //assert
            Assert.AreEqual(1, generateUpdateCalls);
            Assert.AreEqual(update_tableName, "TEST_TABLE");
            Assert.IsTrue(update_keyFieldValues.ContainsKey("RECORD_ID"));
            Assert.IsTrue(update_keyFieldValues.ContainsKey("IS_ACTIVE"));
            Assert.AreEqual(101, update_keyFieldValues["RECORD_ID"]);
            Assert.AreEqual(1, update_keyFieldValues["IS_ACTIVE"]);
            Assert.IsTrue(update_newFieldValues.ContainsKey("DELETED_DATE"));
            Assert.IsTrue(update_newFieldValues.ContainsKey("DELETED_BY"));
            Assert.IsTrue(update_newFieldValues.ContainsKey("IS_ACTIVE"));
            Assert.AreEqual(UserId, update_newFieldValues["DELETED_BY"]);
            Assert.AreEqual(0, update_newFieldValues["IS_ACTIVE"]);
        }


        [TestMethod]
        [ExpectedException(typeof(DBConcurrencyException))]
        public void Delete_RecordNotExists_Exception()
        {
            QueryGeneratorMock.Setup(t => t.GenerateUpdate(
                    It.IsAny<String>(),
                    It.IsAny<Dictionary<String, object>>(),
                    It.IsAny<Dictionary<String, object>>())
                )
                .Returns((String tableName,
                    Dictionary<String, object> keyFieldValues,
                    Dictionary<String, object> newFieldValues) =>
                {
                    return new Mock<IParametrizedQuery>().Object;
                });

            QueryExecutorMock.Setup(t => t.ExecuteNonQuery(It.IsAny<IParametrizedQuery>()))
                .Returns(() =>
                {
                    return 0;
                });

            IVersionedStorage storage = new GenericVersionedStorage(QueryGeneratorMock.Object, QueryExecutorMock.Object, UserId);
            storage.Delete("TEST_TABLE", 1);
        }


        #endregion
    }
}
