using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace StoredProcedureEFCore.ITest
{
    [TestFixture(DbProvider.PgSql)]
    [TestFixture(DbProvider.SqlServer)]
    public class StoredProcBuilderTest
    {
        private static readonly Table1 T1 = new Table1
        {
            Id = 1,
            Name = "a",
            Date = DateTime.Parse("2001-01-01T01:01:01"),
            Active = true,
            NameWithUnderscore = 0
        };

        private static readonly Table1 T2 = new Table1
        {
            Id = 2,
            Name = null,
            Date = DateTime.Parse("2002-02-02T02:02:02"),
            Active = true,
            NameWithUnderscore = 0
        };

        private static readonly Table1 T3 = new Table1
        {
            Id = 3,
            Name = "c",
            Date = DateTime.Parse("2003-03-03T03:03:03"),
            Active = true,
            NameWithUnderscore = 1
        };

        private readonly DbProvider _dbProvider;
        private TestContext _db;

        public StoredProcBuilderTest(DbProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        [SetUp]
        public void SetUp()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
            switch (_dbProvider)
            {
                case DbProvider.PgSql:
                    optionsBuilder.UseNpgsql("Host=localhost;Database=test;Username=postgres;Password=root");
                    break;
                case DbProvider.SqlServer:
                    optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=test;Trusted_Connection=True;");
                    break;
            }

            _db = new TestContext(optionsBuilder.Options);
            _db.Table1.RemoveRange(_db.Table1);
            _db.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        [Test]
        public async Task MappingWorksCorrectlyWhenResultEmpty()
        {
            List<Model> rows = null;
            await _db.LoadStoredProc("empty").ExecAsync(async r => rows = await r.ToListAsync<Model>());

            Assert.AreEqual(0, rows.Count);
        }

        [Test]
        public async Task MappingWorksCorrectly()
        {
            await _db.Table1.AddRangeAsync(T1, T2, T3);
            await _db.SaveChangesAsync();

            List<Model> rows = null;
            await _db.LoadStoredProc("list_all").ExecAsync(async r => rows = await r.ToListAsync<Model>());

            Assert.AreEqual(3, rows.Count);
            CompareTableAndModel(T1, rows[0]);
            CompareTableAndModel(T2, rows[1]);
            CompareTableAndModel(T3, rows[2]);
        }

        [Test]
        public async Task PartialMappingWorksCorrectly()
        {
            await _db.Table1.AddRangeAsync(T1, T2, T3);
            await _db.SaveChangesAsync();

            List<ModelSlim> rows = null;
            await _db.LoadStoredProc("list_not_all").ExecAsync(async r => rows = await r.ToListAsync<ModelSlim>());

            Assert.AreEqual(3, rows.Count);
            CompareTableAndModel(T1, rows[0]);
            CompareTableAndModel(T2, rows[1]);
            CompareTableAndModel(T3, rows[2]);
        }

        [Test]
        public async Task ParameterIn()
        {
             await _db.Table1.AddRangeAsync(T1, T2, T3);
             await _db.SaveChangesAsync();

             List<Model> rows = null;
             await _db.LoadStoredProc("list_all")
                 .AddParam("lim", 2L)
                 .ExecAsync(async r => rows = await r.ToListAsync<Model>());

             Assert.AreEqual(2, rows.Count);
             CompareTableAndModel(T1, rows[0]);
             CompareTableAndModel(T2, rows[1]);
        }

        [Test]
        public async Task ParameterOut()
        {
             await _db.Table1.AddRangeAsync(T1, T2, T3);
             await _db.SaveChangesAsync();

             await _db.LoadStoredProc("output_int")
                 .AddParam("int_to_return", 2, out IOutParam<int> intToReturn)
                 .ExecNonQueryAsync();

             Assert.AreEqual(7, intToReturn.Value);
        }

        [Test]
        public async Task FixedSizeParameterOut()
        {
            await _db.LoadStoredProc("output_fixed_size")
              .AddParam("fixed_size", out IOutParam<string> fixedSizeParam, size: 255)
              .ExecNonQueryAsync();

             Assert.AreEqual("Jambon Beurre", fixedSizeParam.Value);
        }

        [Test]
        public async Task NullableParameterOut()
        {
            await _db.LoadStoredProc("output_nullable")
                .AddParam("nullable", out IOutParam<int?> nullable)
                .ExecNonQueryAsync();

            Assert.AreEqual(null, nullable.Value);
        }

        [Test]
        public async Task NullableParameterOutWithNonNullableType()
        {
            await _db.LoadStoredProc("output_nullable")
                .AddParam("nullable", out IOutParam<int> nullable)
                .ExecNonQueryAsync();

            Assert.Throws<InvalidOperationException>(() => _ = nullable.Value);
        }

        [Test]
        public async Task Scalar()
        {
             await _db.LoadStoredProc("select_param")
                 .AddParam("n", 10)
                 .ExecScalarAsync((int res) => Assert.AreEqual(10, res));
        }

        [Test]
        public async Task ScalarNull()
        {
             await _db.LoadStoredProc("select_param")
                 .AddParam<int?>("n", null)
                 .ExecScalarAsync((int? res) => Assert.AreEqual(null, res));
        }

        [Test]
        public async Task TransactionDoesntThrow()
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            await _db.LoadStoredProc("list_all").ExecAsync(r => r.ToListAsync<Model>());
        }

        [Test]
        public void ExecAsyncThrowsWithCancelledToken()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.ThrowsAsync<TaskCanceledException>(() =>
                _db.LoadStoredProc("list_all").ExecAsync(r => r.ToListAsync<Model>(), cts.Token)
            );
        }

        private void CompareTableAndModel(Table1 expected, ModelSlim actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Name, actual.Name);
        }

        private void CompareTableAndModel(Table1 expected, Model actual)
        {
            CompareTableAndModel(expected, actual as ModelSlim);
            Assert.AreEqual(expected.Date, actual.Date);
            Assert.AreEqual(expected.Active, actual.Active);
            Assert.AreEqual(expected.NameWithUnderscore, (int) actual.NameWithUnderscore);
        }
    }

    public enum DbProvider
    {
        SqlServer,
        PgSql,
    }
}