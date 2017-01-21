///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestSerializerFactory  {
    
        [Test]
        public void TestTypes() {
            Object[] expected = new Object[]{2, 3L, 4f, 5.0d, "abc", new byte[]{10, 20}, (byte) 20, (short) 21, true, new MyBean("E1")};
            Type[] classes = new Type[expected.Length];
            for (int i = 0; i < expected.Length; i++) {
                classes[i] = expected.GetType();
            }
    
            Serializer[] serializers = SerializerFactory.GetSerializers(classes);
            byte[] bytes = SerializerFactory.Serialize(serializers, expected);
    
            Object[] result = SerializerFactory.Deserialize(expected.Length, bytes, serializers);
            EPAssertionUtil.AssertEqualsExactOrder(expected, result);
    
            // null values are simply not serialized
            bytes = SerializerFactory.Serialize(new Serializer[]{SerializerFactory.GetSerializer(typeof(int))}, new Object[]{null});
            Assert.AreEqual(0, bytes.Length);
        }
    
        [Serializable]
        public class MyBean
        {
            private readonly String _id;
    
            public MyBean(String id) {
                this._id = id;
            }

            public bool Equals(MyBean other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other._id, _id);
            }

            /// <summary>
            /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(MyBean)) return false;
                return Equals((MyBean) obj);
            }

            /// <summary>
            /// Serves as a hash function for a particular type. 
            /// </summary>
            /// <returns>
            /// A hash code for the current <see cref="T:System.Object"/>.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override int GetHashCode()
            {
                return (_id != null ? _id.GetHashCode() : 0);
            }
        }
    }
}
