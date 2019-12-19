using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace StoredProcedureEFCore
{
    internal class StoredProcBuilder : IStoredProcBuilder
    {
        private const string _retParamName = "_retParam";
        private DbCommand _cmd;

        public StoredProcBuilder(DbContext ctx, string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            DbCommand cmd = ctx.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = name;
            cmd.Transaction = ctx.Database.CurrentTransaction?.GetDbTransaction();
            cmd.CommandTimeout = ctx.Database.GetCommandTimeout().GetValueOrDefault(cmd.CommandTimeout);

            _cmd = cmd;
        }

        public IStoredProcBuilder AddParam<T>(string name, T val)
        {
            AddParamInner(name, val, ParameterDirection.Input);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, out IOutParam<T> outParam)
        {
            outParam = AddOutputParamInner(name, default(T), ParameterDirection.Output);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, out IOutParam<T> outParam, ParamExtra extra)
        {
            outParam = AddOutputParamInner(name, default(T), ParameterDirection.Output, extra.Size, extra.Precision, extra.Scale);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, T val, out IOutParam<T> outParam, int size = 0, byte precision = 0, byte scale = 0)
        {
            outParam = AddOutputParamInner(name, val, ParameterDirection.Output, size, precision, scale);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, out IOutParam<T> outParam, int size = 0, byte precision = 0, byte scale = 0)
        {
            outParam = AddOutputParamInner(name, default(T), ParameterDirection.Output, size, precision, scale);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, T val, out IOutParam<T> outParam)
        {
            outParam = AddOutputParamInner(name, val, ParameterDirection.InputOutput);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, T val, out IOutParam<T> outParam, ParamExtra extra)
        {
            outParam = AddOutputParamInner(name, val, ParameterDirection.InputOutput, extra.Size, extra.Precision, extra.Scale);
            return this;
        }

        public IStoredProcBuilder AddParam(DbParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            _cmd.Parameters.Add(parameter);
            return this;
        }

        public IStoredProcBuilder ReturnValue<T>(out IOutParam<T> retParam)
        {
            retParam = AddOutputParamInner(_retParamName, default(T), ParameterDirection.ReturnValue);
            return this;
        }

        public IStoredProcBuilder ReturnValue<T>(out IOutParam<T> retParam, ParamExtra extra)
        {
            retParam = AddOutputParamInner(_retParamName, default(T), ParameterDirection.ReturnValue, extra.Size, extra.Precision, extra.Scale);
            return this;
        }

        public IStoredProcBuilder SetTimeout(int timeout)
        {
            _cmd.CommandTimeout = timeout;
            return this;
        }

        public void Exec(Action<DbDataReader> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                OpenConnection();
                using (DbDataReader r = _cmd.ExecuteReader())
                {
                    action(r);
                }
            }
            finally
            {
                Dispose();
            }
        }

        public Task ExecAsync(Func<DbDataReader, Task> action)
        {
            return ExecAsync(action, CancellationToken.None);
        }

        public async Task ExecAsync(Func<DbDataReader, Task> action, CancellationToken cancellationToken)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                await OpenConnectionAsync(cancellationToken);
                using (DbDataReader r = await _cmd.ExecuteReaderAsync(cancellationToken))
                {
                    try
                    {
                        await action(r);
                    }
                    catch(Exception)
                    {
                        // In case the action bombs out, cancel the command and rethrow to propagate the actual action exception.
                        // If we don't cancel the command, we will be stuck on disposing of the reader until the sproc completes, even though the action has already thrown an exception.
                        // This is also the case when the cancellation token is cancelled after the action exception but before the sproc completes:
                        // we will still be stuck on disposing of the reader until the sproc completes.
                        // This is caused by the fact that DbDataReader.Dispose does not react to cancellations and simply waits for the sproc to complete.
                        // The only way to cancel the execution when the reader has been engaged and the action has thrown, is to cancel the command.
                        _cmd.Cancel();
                        throw;
                    }
                }
            }
            finally
            {
                Dispose();
            }
        }

        public void ExecNonQuery()
        {
            try
            {
                OpenConnection();
                _cmd.ExecuteNonQuery();
            }
            finally
            {
                Dispose();
            }
        }

        public Task ExecNonQueryAsync()
        {
            return ExecNonQueryAsync(CancellationToken.None);
        }


        public async Task ExecNonQueryAsync(CancellationToken cancellationToken)
        {
            try
            {
                await OpenConnectionAsync(cancellationToken);
                await _cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                Dispose();
            }
        }

        public void ExecScalar<T>(out T val)
        {
            try
            {
                OpenConnection();
                object scalar = _cmd.ExecuteScalar();
                val = DefaultIfDBNull<T>(scalar);
            }
            finally
            {
                Dispose();
            }
        }

        public Task ExecScalarAsync<T>(Action<T> action)
        {
            return ExecScalarAsync(action, CancellationToken.None);
        }

        public async Task ExecScalarAsync<T>(Action<T> action, CancellationToken cancellationToken)
        {
            try
            {
                await OpenConnectionAsync(cancellationToken);
                object scalar = await _cmd.ExecuteScalarAsync(cancellationToken);
                T val = DefaultIfDBNull<T>(scalar);
                action(val);
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            _cmd.Connection.Close();
            _cmd.Dispose();
        }

        private OutputParam<T> AddOutputParamInner<T>(string name, T val, ParameterDirection direction, int size = 0, byte precision = 0, byte scale = 0)
        {
            DbParameter param = AddParamInner(name, val, direction, size, precision, scale);
            return new OutputParam<T>(param);
        }

        private DbParameter AddParamInner<T>(string name, T val, ParameterDirection direction, int size = 0, byte precision = 0, byte scale = 0)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            DbParameter param = _cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = (object) val ?? DBNull.Value;
            param.Direction = direction;
            param.DbType = DbTypeConverter.ConvertToDbType<T>();
            param.Size = size;
            param.Precision = precision;
            param.Scale = scale;

            _cmd.Parameters.Add(param);
            return param;
        }

        private void OpenConnection()
        {
            if (_cmd.Connection.State == ConnectionState.Closed)
            {
                _cmd.Connection.Open();
            }
        }

        private Task OpenConnectionAsync(CancellationToken cancellationToken)
        {
            if (_cmd.Connection.State == ConnectionState.Closed)
            {
                return _cmd.Connection.OpenAsync(cancellationToken);
            }
            return Task.CompletedTask;
        }

        private T DefaultIfDBNull<T>(object o)
        {
            return o == DBNull.Value ? default(T) : (T) o;
        }
    }
}
