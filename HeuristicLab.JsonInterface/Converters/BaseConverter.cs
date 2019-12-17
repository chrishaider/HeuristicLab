﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.Data;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.JsonInterface {
  public abstract class BaseConverter : IJsonItemConverter
  {
    public void Inject(IItem item, JsonItem data) {
      InjectData(item, data);
    }

    public JsonItem Extract(IItem value) {
      JsonItem data = ExtractData(value);
      data.Name = string.IsNullOrEmpty(data.Name) ? value.ItemName : data.Name;
      return data;
    }
    
    public abstract void InjectData(IItem item, JsonItem data);
    public abstract JsonItem ExtractData(IItem value);

    #region Helper
    protected ValueType CastValue<ValueType>(object obj) {
      if (obj is JToken)
        return obj.Cast<JToken>().ToObject<ValueType>();
      else if (obj is IConvertible)
        return Convert.ChangeType(obj, typeof(ValueType)).Cast<ValueType>();
      else return (ValueType)obj;
    }

    protected IItem Instantiate(Type type, params object[] args) =>
      (IItem)Activator.CreateInstance(type,args);

    protected T Instantiate<T>(params object[] args) => (T)Instantiate(typeof(T), args);

    protected object GetMaxValue(Type t) {
      TypeCode typeCode = Type.GetTypeCode(t);

      if (typeof(ValueType).IsEqualTo(typeof(PercentValue)))
        return 1.0d;

      switch (typeCode) {
        case TypeCode.Int16: return short.MaxValue;
        case TypeCode.Int32: return int.MaxValue;
        case TypeCode.Int64: return long.MaxValue;
        case TypeCode.UInt16: return ushort.MaxValue;
        case TypeCode.UInt32: return uint.MaxValue;
        case TypeCode.UInt64: return ulong.MaxValue;
        case TypeCode.Single: return float.MaxValue;
        case TypeCode.Double: return double.MaxValue;
        case TypeCode.Decimal: return decimal.MaxValue;
        case TypeCode.Byte: return byte.MaxValue;
        case TypeCode.Boolean: return true;
        default: return GetDefaultValue(t);
      }
    }

    protected object GetMinValue(Type t) {
      TypeCode typeCode = Type.GetTypeCode(t);

      if (typeof(ValueType).IsEqualTo(typeof(PercentValue)))
        return 0.0d;

      switch (typeCode) {
        case TypeCode.Int16: return short.MinValue;
        case TypeCode.Int32: return int.MinValue;
        case TypeCode.Int64: return long.MinValue;
        case TypeCode.UInt16: return ushort.MinValue;
        case TypeCode.UInt32: return uint.MinValue;
        case TypeCode.UInt64: return ulong.MinValue;
        case TypeCode.Single: return float.MinValue;
        case TypeCode.Double: return double.MinValue;
        case TypeCode.Decimal: return decimal.MinValue;
        case TypeCode.Byte: return byte.MinValue;
        case TypeCode.Boolean: return false;
        default: return GetDefaultValue(t);
      }
    }

    protected object GetDefaultValue(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
    #endregion
  }
}
