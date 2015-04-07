///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Provides efficient cast methods for converting from object to
    /// primitive types.  The cast method provided here-in is consistent
    /// with the cast mechanics of C#.  These cast mechanics are not
    /// the same as those provided by the IConvertible interface.
    /// </summary>

    public class CastHelper
    {
        public static GenericTypeCaster<T> GetCastConverter<T>()
        {
            TypeCaster typeCaster = GetCastConverter(typeof(T));
            return o => (T)typeCaster.Invoke(o);
        }

        /// <summary>
        /// Gets the cast converter.
        /// </summary>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static TypeCaster GetTypeCaster(Type sourceType, Type targetType)
        {
            return GetCastConverter(targetType);
            //return typeCasterFactory.GetTypeCaster(sourceType, targetType);
        }

        /// <summary>
        /// Gets the cast converter for the specified type.  If none is
        /// found, this method returns null.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static TypeCaster GetCastConverter(Type t)
        {
            Type baseT = Nullable.GetUnderlyingType(t);
            if (baseT != null)
            {
                t = baseT;
            }

            if (t == typeof(Int32))
            {
                return PrimitiveCastInt32;
            }
            if (t == typeof(Int64))
            {
                return PrimitiveCastInt64;
            }
            if (t == typeof(Int16))
            {
                return PrimitiveCastInt16;
            }
            if (t == typeof(SByte))
            {
                return PrimitiveCastSByte;
            }
            if (t == typeof(Single))
            {
                return PrimitiveCastSingle;
            }
            if (t == typeof(Double))
            {
                return PrimitiveCastDouble;
            }
            if (t == typeof(Decimal))
            {
                return PrimitiveCastDecimal;
            }
            if (t == typeof(UInt32))
            {
                return PrimitiveCastUInt32;
            }
            if (t == typeof(UInt64))
            {
                return PrimitiveCastUInt64;
            }
            if (t == typeof(UInt16))
            {
                return PrimitiveCastUInt16;
            }
            if (t == typeof(Char))
            {
                return PrimitiveCastChar;
            }
            if (t == typeof(Byte))
            {
                return PrimitiveCastByte;
            }
            if (t.IsEnum) {
                return sourceObj => PrimitiveCastEnum(t, sourceObj);
            }

            return delegate(Object sourceObj)
            {
                Type sourceObjType = sourceObj.GetType();
                if (t.IsAssignableFrom(sourceObjType))
                {
                    return sourceObj;
                }
                
                return null;
            };
        }

        /// <summary>
        /// Casts the object to a enumerated type
        /// </summary>
        /// <param name="enumType">The type.</param>
        /// <param name="sourceObj">The source object</param>
        /// <returns></returns>

        public static Object PrimitiveCastEnum(Type enumType, Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == enumType)
            {
                return sourceObj;
            }
            if (sourceObjType == typeof(SByte))
            {
                return Enum.ToObject(enumType, (SByte) sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return Enum.ToObject(enumType, ((SByte?) sourceObj).Value);
            }

            if (sourceObjType == typeof(Byte))
            {
                return Enum.ToObject(enumType, (Byte) sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return Enum.ToObject(enumType, ((Byte?) sourceObj).Value);
            }

            if (sourceObjType == typeof(Char))
            {
                return Enum.ToObject(enumType, ((Char)sourceObj));
            }

            if (sourceObjType == typeof(Char?))
            {
                return Enum.ToObject(enumType, ((Char?)sourceObj).Value);
            }

            if (sourceObjType == typeof(Int16))
            {
                return Enum.ToObject(enumType, ((Int16)sourceObj));
            }

            if (sourceObjType == typeof(Int16?))
            {
                return Enum.ToObject(enumType, ((Int16?)sourceObj).Value);
            }

            if (sourceObjType == typeof(Int32))
            {
                return Enum.ToObject(enumType, ((Int32)sourceObj));
            }

            if (sourceObjType == typeof(Int32?))
            {
                return Enum.ToObject(enumType, ((Int32?) sourceObj).Value);
            }

            if (sourceObjType == typeof(Int64))
            {
                return Enum.ToObject(enumType, ((Int64)sourceObj));
            }

            if (sourceObjType == typeof(Int64?))
            {
                return Enum.ToObject(enumType, ((Int64?)sourceObj).Value);
            }

            if (sourceObjType == typeof(UInt16))
            {
                return Enum.ToObject(enumType, ((UInt16)sourceObj));
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return Enum.ToObject(enumType, ((UInt16?)sourceObj).Value);
            }

            if (sourceObjType == typeof(UInt32))
            {
                return Enum.ToObject(enumType, ((UInt32)sourceObj));
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return Enum.ToObject(enumType, ((UInt32?)sourceObj).Value);
            }

            if (sourceObjType == typeof(UInt64))
            {
                return Enum.ToObject(enumType, ((UInt64)sourceObj));
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return Enum.ToObject(enumType, ((UInt64?)sourceObj).Value);
            }

            if (sourceObjType == typeof(Single))
            {
                return Enum.ToObject(enumType, ((Single)sourceObj));
            }

            if (sourceObjType == typeof(Single?))
            {
                return Enum.ToObject(enumType, ((Single?)sourceObj).Value);
            }

            if (sourceObjType == typeof(Double))
            {
                return Enum.ToObject(enumType, ((Double)sourceObj));
            }

            if (sourceObjType == typeof(Double?))
            {
                return Enum.ToObject(enumType, ((Double?)sourceObj).Value);
            }

            if (sourceObjType == typeof(Decimal))
            {
                return Enum.ToObject(enumType, ((Decimal)sourceObj));
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return Enum.ToObject(enumType, ((Decimal?)sourceObj).Value);
            }

            if (sourceObjType == typeof(string))
            {
                return Enum.ToObject(enumType, (((string)sourceObj)[0]));
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.SByte
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastSByte(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (SByte)sourceObj;
            }

            if (sourceObjType == typeof(SByte?))
            {
                return ((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (SByte)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (SByte)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (SByte)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (SByte)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (SByte)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (SByte)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (SByte)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (SByte)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (SByte)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (SByte)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (SByte)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (SByte)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (SByte)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (SByte)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (SByte)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (SByte)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (SByte)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (SByte)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (SByte)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (SByte)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (SByte)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (SByte)((Decimal?)sourceObj).Value;
            }

            if (sourceObjType == typeof(string))
            {
                return (SByte)(((string)sourceObj)[0]);
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Byte
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastByte(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (Byte)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (Byte)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (Byte)sourceObj;
            }

            if (sourceObjType == typeof(Byte?))
            {
                return ((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (Byte)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (Byte)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (Byte)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (Byte)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (Byte)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (Byte)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (Byte)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (Byte)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (Byte)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (Byte)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (Byte)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (Byte)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (Byte)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (Byte)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (Byte)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (Byte)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (Byte)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (Byte)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (Byte)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (Byte)((Decimal?)sourceObj).Value;
            }

            if (sourceObjType == typeof(string))
            {
                return (Byte) (((string) sourceObj)[0]);
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Char
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastChar(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (Char)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (Char)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (Char)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (Char)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (Char)sourceObj;
            }

            if (sourceObjType == typeof(Char?))
            {
                return ((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (Char)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (Char)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (Char)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (Char)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (Char)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (Char)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (Char)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (Char)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (Char)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (Char)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (Char)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (Char)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (Char)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (Char)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (Char)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (Char)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (Char)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (Char)((Decimal?)sourceObj).Value;
            }

            if (sourceObjType == typeof(string))
            {
                return ((string) sourceObj)[0];
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Int16
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastInt16(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (Int16)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (Int16)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (Int16)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (Int16)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (Int16)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (Int16)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (Int16)sourceObj;
            }

            if (sourceObjType == typeof(Int16?))
            {
                return ((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (Int16)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (Int16)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (Int16)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (Int16)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (Int16)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (Int16)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (Int16)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (Int16)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (Int16)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (Int16)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (Int16)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (Int16)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (Int16)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (Int16)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (Int16)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (Int16)((Decimal?)sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Int32
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastInt32(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (Int32)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (Int32)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (Int32)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (Int32)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (Int32)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (Int32)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (Int32)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (Int32)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (Int32)sourceObj;
            }

            if (sourceObjType == typeof(Int32?))
            {
                return ((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (Int32)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (Int32)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (Int32)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (Int32)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (Int32)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (Int32)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (Int32)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (Int32)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (Int32)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (Int32)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (Int32)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (Int32)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (Int32)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (Int32)((Decimal?)sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Int64
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastInt64(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (Int64)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (Int64)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (Int64)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (Int64)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (Int64)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (Int64)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (Int64)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (Int64)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (Int64)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (Int64)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (Int64)sourceObj;
            }

            if (sourceObjType == typeof(Int64?))
            {
                return ((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (Int64)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (Int64)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (Int64)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (Int64)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (Int64)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (Int64)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (Int64)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (Int64)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (Int64)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (Int64)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (Int64)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (Int64)((Decimal?)sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.UInt16
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastUInt16(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (UInt16)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (UInt16)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (UInt16)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (UInt16)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (UInt16)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (UInt16)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (UInt16)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (UInt16)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (UInt16)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (UInt16)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (UInt16)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (UInt16)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (UInt16)sourceObj;
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return ((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (UInt16)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (UInt16)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (UInt16)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (UInt16)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (UInt16)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (UInt16)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (UInt16)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (UInt16)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (UInt16)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (UInt16)((Decimal?)sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.UInt32
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastUInt32(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (UInt32)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (UInt32)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (UInt32)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (UInt32)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (UInt32)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (UInt32)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (UInt32)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (UInt32)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (UInt32)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (UInt32)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (UInt32)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (UInt32)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (UInt32)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (UInt32)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (UInt32)sourceObj;
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return ((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (UInt32)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (UInt32)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (UInt32)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (UInt32)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (UInt32)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (UInt32)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (UInt32)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (UInt32)((Decimal?)sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.UInt64
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastUInt64(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (UInt64)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (UInt64)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (UInt64)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (UInt64)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (UInt64)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (UInt64)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (UInt64)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (UInt64)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (UInt64)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (UInt64)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (UInt64)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (UInt64)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (UInt64)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (UInt64)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (UInt64)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (UInt64)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (UInt64)sourceObj;
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return ((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (UInt64)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (UInt64)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (UInt64)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (UInt64)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (UInt64)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (UInt64)((Decimal?)sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Single
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastSingle(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (Single)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (Single)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (Single)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (Single)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (Single)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (Single)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (Single)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (Single)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (Single)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (Single)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (Single)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (Single)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (Single)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (Single)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (Single)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (Single)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (Single)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (Single)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (Single)sourceObj;
            }

            if (sourceObjType == typeof(Single?))
            {
                return ((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (Single)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (Single)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (Single)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (Single)((Decimal?)sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Double
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastDouble(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (Double)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (Double)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (Double)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (Double)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (Double)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (Double)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (Double)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (Double)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (Double)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (Double)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (Double)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (Double)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (Double)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (Double)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (Double)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (Double)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (Double)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (Double)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (Double)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (Double)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (Double)sourceObj;
            }

            if (sourceObjType == typeof(Double?))
            {
                return ((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (Double)((Decimal)sourceObj);
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return (Double)((Decimal?)sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Decimal
        /// </summary>
        /// <param name="sourceObj">The source object</param>

        public static Object PrimitiveCastDecimal(Object sourceObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            Type sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(SByte))
            {
                return (Decimal)((SByte)sourceObj);
            }

            if (sourceObjType == typeof(SByte?))
            {
                return (Decimal)((SByte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Byte))
            {
                return (Decimal)((Byte)sourceObj);
            }

            if (sourceObjType == typeof(Byte?))
            {
                return (Decimal)((Byte?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Char))
            {
                return (Decimal)((Char)sourceObj);
            }

            if (sourceObjType == typeof(Char?))
            {
                return (Decimal)((Char?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int16))
            {
                return (Decimal)((Int16)sourceObj);
            }

            if (sourceObjType == typeof(Int16?))
            {
                return (Decimal)((Int16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int32))
            {
                return (Decimal)((Int32)sourceObj);
            }

            if (sourceObjType == typeof(Int32?))
            {
                return (Decimal)((Int32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Int64))
            {
                return (Decimal)((Int64)sourceObj);
            }

            if (sourceObjType == typeof(Int64?))
            {
                return (Decimal)((Int64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt16))
            {
                return (Decimal)((UInt16)sourceObj);
            }

            if (sourceObjType == typeof(UInt16?))
            {
                return (Decimal)((UInt16?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt32))
            {
                return (Decimal)((UInt32)sourceObj);
            }

            if (sourceObjType == typeof(UInt32?))
            {
                return (Decimal)((UInt32?)sourceObj).Value;
            }

            if (sourceObjType == typeof(UInt64))
            {
                return (Decimal)((UInt64)sourceObj);
            }

            if (sourceObjType == typeof(UInt64?))
            {
                return (Decimal)((UInt64?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Single))
            {
                return (Decimal)((Single)sourceObj);
            }

            if (sourceObjType == typeof(Single?))
            {
                return (Decimal)((Single?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Double))
            {
                return (Decimal)((Double)sourceObj);
            }

            if (sourceObjType == typeof(Double?))
            {
                return (Decimal)((Double?)sourceObj).Value;
            }

            if (sourceObjType == typeof(Decimal))
            {
                return (Decimal)sourceObj;
            }

            if (sourceObjType == typeof(Decimal?))
            {
                return ((Decimal?)sourceObj).Value;
            }

            return null;
        }
    }
}
