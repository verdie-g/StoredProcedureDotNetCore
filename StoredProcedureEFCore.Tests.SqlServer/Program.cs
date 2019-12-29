using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace StoredProcedureEFCore.Tests.SqlServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DbContext ctx = new TestContext();

            List<Model> rows = null;

            // EXEC dbo.ListAll @limit = 300, @limitOut OUT
            await ctx.LoadStoredProc("dbo.ListAll")
              .SetTimeout(1)
              .AddParam("limit", 300L)
              .AddParam("limitOut", out IOutParam<long> limitOut)
              .ExecAsync(async r => rows = await r.ToListAsync<Model>());

            long limitOutValue = limitOut.Value;

            List<Model2> rows2 = null;

            // EXEC dbo.ListNotAll @limit = 200
            await ctx.LoadStoredProc("dbo.ListNotAll")
              .AddParam("limit", 200L)
              .ExecAsync(async r => rows2 = await r.ToListAsync<Model2>());

            // EXEC dbo.ListNotAll @limit = 400
            await ctx.LoadStoredProc("dbo.ListNotAll")
              .AddParam("limit", 400L)
              .ExecAsync(async r => rows2 = await r.ToListAsync<Model2>());

            // EXEC @_retParam = dbo.ReturnBoolean @boolean_to_return = true
            await ctx.LoadStoredProc("dbo.ReturnBoolean")
               .AddParam("boolean_to_return", true)
               .ReturnValue(out IOutParam<bool> retParam)
               .ExecNonQueryAsync();

            bool b = retParam.Value;

            // EXEC dbo.ListAll @limit = 1
            await ctx.LoadStoredProc("dbo.ListAll")
               .AddParam("limit", 1L)
               .ExecScalarAsync<long>(l => Console.WriteLine(l));

            // Limit is omitted, it takes default value specified in the stored procedure
            ctx.LoadStoredProc("dbo.ListAll")
               .Exec(r => rows = r.ToList<Model>());

            // EXEC dbo.SelectParam @n = NULL
            await ctx.LoadStoredProc("dbo.SelectParam")
              .AddParam<int?>("n", null)
              .ExecScalarAsync<int?>(i => Console.WriteLine(i));

            await ctx.LoadStoredProc("dbo.OutputFixedSize")
              .AddParam("fixed_size", out IOutParam<string> fixedSizeParam, size: 255)
              .ExecNonQueryAsync();

            string s = fixedSizeParam.Value;

            try
            {
                await ctx.LoadStoredProc("dbo.OutputNullable")
                  .AddParam("nullable", out IOutParam<int> nullableWithExceptionParameter)
                  .ExecNonQueryAsync();

                var nullableWithException = nullableWithExceptionParameter.Value;
            }
            catch (InvalidOperationException)
            {
                // Should throw InvalidOperationException as int is not nullable
            }

            await ctx.LoadStoredProc("dbo.OutputNullable")
                .AddParam("nullable", out IOutParam<int?> nullableParameter)
                .ExecNonQueryAsync();

            var nullable = nullableParameter.Value;

            using (var transaction = await ctx.Database.BeginTransactionAsync())
            {
                await ctx.LoadStoredProc("dbo.ListAll").ExecAsync(async r => rows = await r.ToListAsync<Model>());
            }

            // **************************
            // Async cancellation tests.
            // **************************


            // Test ExecAsync with a pre-cancelled token.
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    cts.Cancel();
                    await ctx.LoadStoredProc("dbo.ListAll").ExecAsync(async r => rows = await r.ToListAsync<Model>(cts.Token), cts.Token);

                    // If the task was cancelled properly, we should not be here.
                    throw new InvalidOperationException("Task was supposed to be cancelled.");
                }
            }
            catch (TaskCanceledException)
            {
            }

            // Test DbDataReader extension method cacellations.

            await TestCancelBeforeResultSet(ctx, 1, (r, ct) => r.ToListAsync<Model>(ct));
            await TestCancelAfterResultSet(ctx, 2, (r, ct) => r.ToListAsync<Model>(ct));

            await TestCancelBeforeResultSet(ctx, 3, (r, ct) => r.ColumnAsync<long>("id", ct));
            await TestCancelAfterResultSet(ctx, 4, (r, ct) => r.ColumnAsync<long>("id", ct));

            await TestCancelBeforeResultSet(ctx, 5, (r, ct) => r.ToDictionaryAsync<long, Model>(m => m.Id, ct));
            await TestCancelAfterResultSet(ctx, 6, (r, ct) => r.ToDictionaryAsync<long, Model>(m => m.Id, ct));

            await TestCancelBeforeResultSet(ctx, 7, (r, ct) => r.ToLookupAsync<long, Model>(m => m.Id, ct));
            await TestCancelAfterResultSet(ctx, 8, (r, ct) => r.ToLookupAsync<long, Model>(m => m.Id, ct));

            await TestCancelBeforeResultSet(ctx, 9, (r, ct) => r.ToSetAsync<long>(ct));
            await TestCancelAfterResultSet(ctx, 10, (r, ct) => r.ToSetAsync<long>(ct));

            await TestCancelBeforeResultSet(ctx, 11, (r, ct) => r.FirstAsync<Model>(ct));
            await TestCancelAfterResultSet(ctx, 12, (r, ct) => r.FirstAsync<Model>(ct));

            await TestCancelBeforeResultSet(ctx, 13, (r, ct) => r.SingleAsync<Model>(ct), 1);
            await TestCancelAfterResultSet(ctx, 14, (r, ct) => r.SingleAsync<Model>(ct), 1);

            await TestCancelBeforeResultSet(ctx, 15, (r, ct) => r.SingleOrDefaultAsync<Model>(ct), 1);
            await TestCancelAfterResultSet(ctx, 16, (r, ct) => r.SingleOrDefaultAsync<Model>(ct), 1);
        }

#pragma warning disable AsyncFixer04 // A disposable object used in a fire & forget async call
        /// <summary>
        /// Test async cancellation where the sproc is cancelled before it has a chance to produce a resultset.
        /// </summary>
        static async Task TestCancelBeforeResultSet(DbContext ctx, int callId, Func<DbDataReader, CancellationToken, Task> action, long limit = long.MaxValue)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                Console.WriteLine("\n====================================");
                Console.WriteLine($"Starting long-running call #{callId}...");

                using (var cts = new CancellationTokenSource())
                {
                    var sprocTask = ctx.LoadStoredProc("dbo.ListAll")
                        .AddParam("limit", limit)
                        .AddParam("delay_in_seconds_before_result_set", 10)
                        .ExecAsync(r => action(r, cts.Token), cts.Token);

                    // Cancel the task after one second.
                    var cancelTask = CancelTokenAsync(callId, 1000, cts);

                    // Wait for the sproc and the cancellation task to complete.
                    await Task.WhenAll(sprocTask, cancelTask);

                    // If the task was cancelled properly, we should not be here.
                    throw new InvalidOperationException("Task was supposed to be cancelled.");
                }
            }
            catch (SqlException e)
            {
                // If the task was cancelled properly, SqlClient's cmd.ExecuteAsync should throw SqlException with "Operation cancelled by user" in message.
                if (!e.Message.Contains("Operation cancelled by user"))
                {
                    throw new InvalidOperationException("Task was supposed to be cancelled.");
                }
                Console.WriteLine($"Call #{callId} cancelled");
            }
            finally
            {
                stopWatch.Stop();
                Console.WriteLine($"Call #{callId} duration: {stopWatch.Elapsed}");
            }

        }
#pragma warning restore AsyncFixer04 // A disposable object used in a fire & forget async call


#pragma warning disable AsyncFixer04 // A disposable object used in a fire & forget async call

        /// <summary>
        /// Test async cancellation where the sproc is cancelled after it has produced some resultset, but it's still actively working.
        /// </summary>
        static async Task TestCancelAfterResultSet(DbContext ctx, int callId, Func<DbDataReader, CancellationToken, Task> action, long limit = 9223372036854775807)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                Console.WriteLine("\n====================================");
                Console.WriteLine($"Starting long-running call #{callId}...");

                using (var cts = new CancellationTokenSource())
                {
                    var sprocTask = ctx.LoadStoredProc("dbo.ListAll")
                        .AddParam("limit", limit)
                        .AddParam("delay_in_seconds_after_result_set", 10)
                        .ExecAsync(async r => await action(r, cts.Token), cts.Token);

                    // Cancel the task after one second.
                    var cancelTask = CancelTokenAsync(callId, 1000, cts);

                    // Wait for the sproc and the cancellation task to complete.
                    await Task.WhenAll(sprocTask, cancelTask);

                    // If the task was cancelled properly, we should not be here.
                    throw new InvalidOperationException("Task was supposed to be cancelled.");
                }
            }
            catch (TaskCanceledException)
            {
                // If the task was cancelled properly, reader.ReadAsync should throw TaskCanceledException.
                Console.WriteLine($"Call #{callId} cancelled");
            }
            finally
            {
                stopWatch.Stop();
                Console.WriteLine($"Call #{callId} duration: {stopWatch.Elapsed}");
            }

        }
#pragma warning restore AsyncFixer04 // A disposable object used in a fire & forget async call


        /// <summary>
        /// Given a delay interval and a cancellation token source, wait the appropriate amount of time and then cancel the cancellation token.
        /// </summary>
        static async Task CancelTokenAsync(int callId, int delayMilliseconds, CancellationTokenSource cts)
        {
            await Task.Delay(delayMilliseconds);

            Console.WriteLine($"Cancelling call #{callId}...");
            cts.Cancel();
        }
    }
}
