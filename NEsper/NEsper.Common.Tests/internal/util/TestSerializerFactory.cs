///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestSerializerFactory : AbstractCommonTest
    {
        [Serializable]
        public class MyBean
        {
            private readonly string id;

            public MyBean(string id)
            {
                this.id = id;
            }

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

            var serializers = SerializerFactory.GetSerializers(classes);
            var bytes = SerializerFactory.Serialize(serializers, expected);

            var result = SerializerFactory.Deserialize(expected.Length, bytes, serializers);
            EPAssertionUtil.AssertEqualsExactOrder(expected, result);

            // null values are simply not serialized
            bytes = SerializerFactory.Serialize(new[] { SerializerFactory.GetSerializer(typeof(int?)) }, new object[] { null });
            Assert.AreEqual(0, bytes.Length);
        }
    }
} // end of namespace
