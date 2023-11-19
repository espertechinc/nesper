///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.container;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class SerializerUtil
    {
        public static byte[] ObjectToByteArr(
            IContainer container,
            object underlying)
        {
            return ObjectToByteArr(container.SerializerFactory(), underlying);
        }

        /// <summary>Serialize object to byte array. </summary>
        /// <param name="underlying">to serialize</param>
        /// <returns>byte array</returns>
        public static byte[] ObjectToByteArr(
            SerializerFactory serializerFactory,
            object underlying)
        {
            return serializerFactory.DefaultSerializer.SerializeAny(underlying);
        }

        /// <summary>Deserialize byte array to object. </summary>
        /// <param name="bytes">to read</param>
        /// <returns>object</returns>
        public static object ByteArrToObject(
            SerializerFactory serializerFactory,
            byte[] bytes)
        {
            if (bytes == null) {
                return null;
            }

            return serializerFactory.DefaultSerializer.DeserializeAny(bytes);
        }

        /// <summary>
        /// Deserializes a byte array in base 64 encoding to an object.  Assumes that the
        /// caller has no "container" and just wants to use a barebones serializer factory.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ByteArrBase64ToObject(
            string value)
        {
            return ByteArrBase64ToObject(SerializerFactory.Instance, value);
        }

        public static object ByteArrBase64ToObject(
            SerializerFactory serializerFactory,
            string value)
        {
            var bytes = Convert.FromBase64String(value);
            return ByteArrToObject(serializerFactory, bytes);
        }

        public static string ObjectToByteArrBase64(
            SerializerFactory serializerFactory,
            object userObject)
        {
            var bytes = ObjectToByteArr(serializerFactory, userObject);
            return Convert.ToBase64String(bytes);
        }

        public static CodegenExpression ExpressionForUserObject(
            SerializerFactory serializerFactory,
            object userObject)
        {
            if (userObject == null) {
                return ConstantNull();
            }

            var serialize = IsUseSerialize(userObject.GetType());
            if (!serialize) {
                return Constant(userObject);
            }

            var value = ObjectToByteArrBase64(serializerFactory, userObject);
            return StaticMethod(typeof(SerializerUtil), "ByteArrBase64ToObject", Constant(value));
        }

        private static bool IsUseSerialize(Type clazz)
        {
            if (clazz.IsBuiltinDataType()) {
                return false;
            }

            if (clazz.IsArray) {
                return IsUseSerialize(clazz.GetElementType());
            }

            return true;
        }
    }
}