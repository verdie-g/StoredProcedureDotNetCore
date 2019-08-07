using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace StoredProcedureEFCore
{
  /// <summary>
  /// Stored procedure builder
  /// </summary>
  public interface IStoredProcBuilder : IDisposable
  {
    /// <summary>
    /// Add input parameter
    /// </summary>
    /// <typeparam name="T">Type of the parameter. Can be nullable</typeparam>
    /// <param name="name">Name of the parameter</param>
    /// <param name="val">Value of the parameter</param>
    /// <returns></returns>
    IStoredProcBuilder AddParam<T>(string name, T val);

    /// <summary>
    /// Add input/output parameter
    /// </summary>
    /// <typeparam name="T">Type of the parameter. Can be nullable</typeparam>
    /// <param name="name">Name of the parameter</param>
    /// <param name="val">Value of the parameter</param>
    /// <param name="outParam">Created parameter. Value will be populated after calling <see cref="Exec(Action{DbDataReader})"/></param>
    /// <returns></returns>
    IStoredProcBuilder AddParam<T>(string name, T val, out IOutParam<T> outParam);

    /// <summary>
    /// Add input/output parameter
    /// </summary>
    /// <typeparam name="T">Type of the parameter. Can be nullable</typeparam>
    /// <param name="name">Name of the parameter</param>
    /// <param name="val">Value of the parameter</param>
    /// <param name="outParam">Created parameter. Value will be populated after calling <see cref="Exec(Action{DbDataReader})"/></param>
    /// <param name="extra">Parameter extra informations</param>
    /// <returns></returns>
    IStoredProcBuilder AddParam<T>(string name, T val, out IOutParam<T> outParam, ParamExtra extra);

    /// <summary>
    /// Add output parameter
    /// </summary>
    /// <typeparam name="T">Type of the parameter. Can be nullable</typeparam>
    /// <param name="name">Name of the parameter</param>
    /// <param name="outParam">Created parameter. Value will be populated after calling <see cref="Exec(Action{DbDataReader})"/></param>
    /// <returns></returns>
    IStoredProcBuilder AddParam<T>(string name, out IOutParam<T> outParam);

    /// <summary>
    /// Add output parameter
    /// </summary>
    /// <typeparam name="T">Type of the parameter. Can be nullable</typeparam>
    /// <param name="name">Name of the parameter</param>
    /// <param name="outParam">Created parameter. Value will be populated after calling <see cref="Exec(Action{DbDataReader})"/></param>
    /// <param name="extra">Parameter extra informations</param>
    /// <returns></returns>
    IStoredProcBuilder AddParam<T>(string name, out IOutParam<T> outParam, ParamExtra extra);

    /// <summary>
    /// Add pre configured DB query execution parameter directly command
    /// </summary>
    /// <param name="parameter">DB query execution parameter <see cref="DbParameter"/></param>
    /// <returns></returns>
    IStoredProcBuilder AddParam(DbParameter parameter);

    /// <summary>
    /// Add return value parameter
    /// </summary>
    /// <typeparam name="T">Type of the parameter. Can be nullable</typeparam>
    /// <param name="retParam">Created parameter. Value will be populated after calling <see cref="Exec(Action{DbDataReader})"/></param>
    /// <returns></returns>
    IStoredProcBuilder ReturnValue<T>(out IOutParam<T> retParam);
    
    /// <summary>
    /// Add return value parameter
    /// </summary>
    /// <typeparam name="T">Type of the parameter. Can be nullable</typeparam>
    /// <param name="retParam">Created parameter. Value will be populated after calling <see cref="Exec(Action{DbDataReader})"/></param>
    /// <param name="extra">Parameter extra informations</param>
    /// <returns></returns>
    IStoredProcBuilder ReturnValue<T>(out IOutParam<T> retParam, ParamExtra extra);

    /// <summary>
    /// Set the wait time before terminating the attempt to execute the stored procedure and generating an error
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    IStoredProcBuilder SetTimeout(int timeout);

    /// <summary>
    /// Execute the stored procedure
    /// </summary>
    /// <param name="action">Actions to do with the result sets</param>
    void Exec(Action<DbDataReader> action);

    /// <summary>
    /// Execute the stored procedure
    /// </summary>
    /// <param name="action">Actions to do with the result sets</param>
    Task ExecAsync(Func<DbDataReader, Task> action);

    /// <summary>
    /// Execute the stored procedure
    /// </summary>
    void ExecNonQuery();

    /// <summary>
    /// Execute the stored procedure
    /// </summary>
    Task ExecNonQueryAsync();

    /// <summary>
    /// Execute the stored procedure and return the first column of the first row
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    void ExecScalar<T>(out T val);

    /// <summary>
    /// Execute the stored procedure and return the first column of the first row
    /// </summary>
    /// <typeparam name="T">Type of the scalar value</param>
    /// <param name="action">Action with the scalar value</param>
    Task ExecScalarAsync<T>(Action<T> action);
  }
}
