///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util
{
    public class SerializerUtil
    {
        /// <summary>Serialize object to byte array. </summary>
        /// <param name="underlying">to serialize</param>
        /// <returns>byte array</returns>
        public static byte[] ObjectToByteArr(object underlying)
        {
            return SerializerFactory.Instance.Serialize(
                new[] {SerializerFactory.Instance.OBJECT_SERIALIZER},
                new[] {underlying});
        }

        /// <summary>Deserialize byte array to object. </summary>
        /// <param name="bytes">to read</param>
        /// <returns>object</returns>
        public static object ByteArrToObject(byte[] bytes)
        {
            if (bytes == null) {
                return null;
            }

            return SerializerFactory.Instance.Deserialize(
                1,
                bytes,
                new[] {SerializerFactory.Instance.OBJECT_SERIALIZER})[0];
        }

        public static object ByteArrBase64ToObject(string s)
        {
            byte[] bytes = Convert.FromBase64String(s);
            return ByteArrToObject(bytes);
        }
        
        public static string ObjectToByteArrBase64(object userObject)
        {
            byte[] bytes = ObjectToByteArr(userObject);
            return Convert.ToBase64String(bytes);
        }

        public static CodegenExpression ExpressionForUserObject(object userObject)
        {
            if (userObject == null) {
                return ConstantNull();
            }

            var serialize = IsUseSerialize(userObject.GetType());
            if (!serialize) {
                return Constant(userObject);
            }

            var value = SerializerUtil.ObjectToByteArrBase64(userObject);
            return StaticMethod(typeof(SerializerUtil), "ByteArrBase64ToObject", Constant(value));
        }

        private static bool IsUseSerialize(Type clazz)
        {
            if (TypeHelper.IsBuiltinDataType(clazz)) {
                return false;
            }

            if (clazz.IsArray) {
                return IsUseSerialize(clazz.GetElementType());
            }

            return true;
        }
    }
}