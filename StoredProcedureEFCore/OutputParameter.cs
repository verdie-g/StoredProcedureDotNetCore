using System;
using System.Data.Common;

namespace StoredProcedureEFCore
{
    internal class OutputParam<T> : IOutParam<T>
    {
        public OutputParam(DbParameter param)
        {
            _dbParam = param;
        }

        public T Value
        {
            get
            {
                if (_dbParam.Value is DBNull)
                {
                    if (default(T) == null)
                    {
                        return default;
                    }
                    else
                    {
                        throw new InvalidOperationException($"{_dbParam.ParameterName} is null and can't be assigned to a non-nullable type");
                    }
                }

                return GetValue();
            }
        }

        /// <summary>
        /// Convert parameter value to type T, accounting for the special case of nullable generic types.
        /// </summary>
        private T GetValue()
        {
            var t = typeof(T);

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (_dbParam.Value == null)
                {
                    return default;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return (T)Convert.ChangeType(_dbParam.Value, t);
        }

        public override string ToString() => _dbParam.Value.ToString();

        private readonly DbParameter _dbParam;
    }
}
