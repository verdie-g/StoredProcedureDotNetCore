 # Execute stored procedures with Entity Framework Core

DbContext extension with *LoadStoredProc* method which creates
an `IStoredProcBuilder` to build a stored procedure with a custom
mapping strategy using the provided `DbDataReader` extension.

The method handles :
- Extra column in result set
- Extra property in model
- Null values in result set
- Underscores in result set column names ("column_name" is mapped to ColumnName property)
- Int (db) to enumeration (result model) mapping

## Example

```csharp
List<Model> rows = null;

ctx.LoadStoredProc("dbo.ListAll")
   .AddParam("limit", 300L)
   .AddParam("limitOut", out IOutParam<long> limitOut)
   .Exec(r => rows = r.ToList<Model>());

long limitOutValue = limitOut.Value;

ctx.LoadStoredProc("dbo.ReturnBoolean")
   .AddParam("boolean_to_return", true)
   .ReturnValue(out IOutParam<bool> retParam)
   .ExecNonQuery();

bool b = retParam.Value;

ctx.LoadStoredProc("dbo.ListAll")
   .AddParam("limit", 1L)
   .ExecScalar(out long l);
```

## API

### DbContext
```csharp
IStoredProcBuilder LoadStoredProc(string name)
```

### IStoredProcBuilder
```csharp
IStoredProcBuilder AddParam<T>(string name, T val); // Input parameter
IStoredProcBuilder AddParam<T>(string name, T val, out IOutParam<T> outParam, int size, byte precision, byte scale); // Input/Ouput parameter
IStoredProcBuilder AddParam<T>(string name, out IOutParam<T> outParam, int size, byte precision, byte scale); // Ouput parameter
IStoredProcBuilder ReturnValue<T>(out IOutParam<T> retParam, int size, byte precision, byte scale);
IStoredProcBuilder SetTimeout(int timeout);
Task               ExecAsync(Func<DbDataReader, Task> action, CancellationToken cancellationToken);
Task<int>          ExecNonQueryAsync(CancellationToken cancellationToken);
Task               ExecScalarAsync<T>(Action<T> action, CancellationToken cancellationToken);
```

### DbDataReader
```csharp
Task<List<T>>                        ToListAsync<T>();
Task<Dictionary<TKey, TValue>>       ToDictionaryAsync<TKey, TValue>(Func<TValue, TKey> keyProjection);
Task<Dictionary<TKey, List<TValue>>> ToLookupAsync<TKey, TValue>(Func<TValue, TKey> keyProjection);
Task<HashSet<T>>                     ToSetAsync<T>();
Task<List<T>>                        ColumnAsync<T>();
Task<List<T>>                        ColumnAsync<T>(string columnName);
Task<T>                              FirstAsync<T>();
Task<T>                              FirstOrDefaultAsync<T>();
Task<T>                              SingleAsync<T>();
Task<T>                              SingleOrDefaultAsync<T>();
```
All these methods have a corresponding async method : ToListAsync, ToDictionaryAsync, ...

## Installation

` Install-Package StoredProcedureEFCore `

## Why ?

Stored procedure execution was previously not supported in Entity Framework Core:
- [Raw store access APIs: Support for ad hoc mapping of arbitrary types #1862](https://github.com/aspnet/EntityFramework/issues/1862)
- [Stored procedure mapping support #245](https://github.com/aspnet/EntityFramework/issues/245)

It is now supported since EF Core 2.1 but this library has few advantages compared to *FromSql*:
- Extra property in the model won't throw an exception. The property keeps its default value
- The interface is easier to use. Output parameters and return values seem difficult to use with EF Core
- Mapping is 30% faster
