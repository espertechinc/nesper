///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.client.scopetest.ScopeTestHelper;

namespace com.espertech.esper.common.@internal.support
{
    public class SupportEventPropUtil
    {
        public static void AssertPropsEquals(
            EventPropertyDescriptor[] received,
            params SupportEventPropDesc[] expected)
        {
            AssertEquals(received.Length, expected.Length);

            IDictionary<string, EventPropertyDescriptor> receivedProps =
                new Dictionary<string, EventPropertyDescriptor>();
            foreach (var descReceived in received) {
                if (receivedProps.ContainsKey(descReceived.PropertyName)) {
                    Fail("duplicate '" + descReceived.PropertyName + "'");
                }

                receivedProps.Put(descReceived.PropertyName, descReceived);
            }

            IDictionary<string, SupportEventPropDesc> expectedProps = new Dictionary<string, SupportEventPropDesc>();
            foreach (var expectedDesc in expected) {
                if (expectedProps.ContainsKey(expectedDesc.PropertyName)) {
                    Fail("duplicate '" + expectedDesc.PropertyName + "'");
                }

                expectedProps.Put(expectedDesc.PropertyName, expectedDesc);
            }

            foreach (var receivedDesc in received) {
                var expectedDesc = expectedProps.Get(receivedDesc.PropertyName);
                if (expectedDesc == null) {
                    Fail("could not find in expected the name '" + receivedDesc.PropertyName + "'");
                }

                AssertPropEquals(expectedDesc, receivedDesc);
            }
        }

        public static void AssertPropEquals(
            SupportEventPropDesc expected,
            EventPropertyDescriptor received)
        {
            var message = "comparing '" + expected.PropertyName + "'";
            AssertEquals(message, expected.PropertyName, received.PropertyName);
            AssertEquals(message, expected.PropertyType, received.PropertyType);
            AssertEquals(message, expected.ComponentType, received.PropertyComponentType);
            AssertEquals(message, expected.PropertyType, received.PropertyType);
            AssertEquals(message, expected.IsFragment, received.IsFragment);
            AssertEquals(message, expected.IsIndexed, received.IsIndexed);
            AssertEquals(message, expected.IsRequiresIndex, received.IsRequiresIndex);
            AssertEquals(message, expected.IsMapped, received.IsMapped);
            AssertEquals(message, expected.IsRequiresMapkey, received.IsRequiresMapkey);
        }

        public static void AssertTypes(
            EventType type,
            string[] fields,
            Type[] classes)
        {
            var count = 0;
            foreach (var field in fields) {
                AssertEquals("position " + count, classes[count++], type.GetPropertyType(field));
            }
        }

        public static void AssertTypes(
            EventType type,
            string field,
            Type clazz)
        {
            AssertTypes(type, new[] { field }, new[] { clazz });
        }

        public static void AssertTypesAllSame(
            EventType type,
            string[] fields,
            Type clazz)
        {
            var count = 0;
            foreach (var field in fields) {
                AssertEquals("position " + count, clazz, type.GetPropertyType(field));
            }
        }
    }
} // end of namespace