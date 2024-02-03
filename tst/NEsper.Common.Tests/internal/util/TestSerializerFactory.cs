///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.Json.Serialization;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.util.serde;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestSerializerFactory : AbstractCommonTest
    {
        public class MyBean
        {
            private readonly string id;

            [JsonConstructor]
            public MyBean(string id)
            {
                this.id = id;
            }

            public string Id => id;

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }

                if (o == null || GetType() != o.GetType())
                {
                    return false;
                }

                var myBean = (MyBean) o;

                if (!id.Equals(myBean.id))
                {
                    return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                return id.GetHashCode();
            }
        }

        [Test]
        public void TestTypes()
        {
            object[] expected = { 2, 3L, 4f, 5.0d, "abc", new byte[] { 10, 20 }, (byte) 20, (short) 21, true, new MyBean("E1") };
            var classes = new Type[expected.Length];
            for (var i = 0; i < expected.Length; i++)
            {
                classes[i] = expected.GetType();
            }

            var serializerFactory = new SerializerFactory(container);
            var serializers = serializerFactory.GetSerializers(classes);
            var bytes = serializerFactory.Serialize(serializers, expected);

            var result = serializerFactory.Deserialize(expected.Length, bytes, serializers);
            EPAssertionUtil.AssertEqualsExactOrder(expected, result);

            // null values are simply not serialized
            bytes = serializerFactory.Serialize(new[] { serializerFactory.GetSerializer(typeof(int?)) }, new object[] { null });
            ClassicAssert.AreEqual(0, bytes[0].Length);
        }
    }
} // end of namespace
