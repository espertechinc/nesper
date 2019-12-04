///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.support
{
    public class SupportEventTypeAssertionUtil
    {
        public static void AssertFragments(
            EventBean @event,
            bool isNative,
            bool array,
            string propertyExpressions)
        {
            var names = propertyExpressions.SplitCsv();
            foreach (var name in names) {
                if (!array) {
                    AssertFragmentNonArray(@event, isNative, name);
                }
                else {
                    AssertFragmentArray(@event, isNative, name);
                }
            }
        }

        public static void AssertConsistency(EventBean eventBean)
        {
            AssertConsistencyRecursive(eventBean, new HashSet<EventType>());
        }

        public static void AssertConsistency(EventType eventType)
        {
            AssertConsistencyRecursive(eventType, new HashSet<EventType>());
        }

        public static string Print(EventBean theEvent)
        {
            var writer = new StringWriter();
            Print(theEvent, writer, 0, new Stack<string>());
            return writer.ToString();
        }

        private static void Print(
            EventBean theEvent,
            TextWriter writer,
            int indent,
            Stack<string> propertyStack)
        {
            WriteIndent(writer, indent);
            writer.Write("Properties : \n");
            PrintProperties(theEvent, writer, indent + 2, propertyStack);

            // count fragments
            var countFragments = 0;
            foreach (var desc in theEvent.EventType.PropertyDescriptors) {
                if (desc.IsFragment) {
                    countFragments++;
                }
            }

            if (countFragments == 0) {
                return;
            }

            WriteIndent(writer, indent);
            writer.Write("Fragments : (" + countFragments + ") \n");
            foreach (var desc in theEvent.EventType.PropertyDescriptors) {
                if (!desc.IsFragment) {
                    continue;
                }

                WriteIndent(writer, indent + 2);
                writer.Write(desc.PropertyName);
                writer.Write(" : ");

                if (desc.IsRequiresIndex) {
                    writer.Write("\n");
                    var count = 0;
                    while (true) {
                        try {
                            WriteIndent(writer, indent + 4);
                            writer.Write("bean #");
                            writer.Write(Convert.ToString(count));
                            var result = (EventBean) theEvent.GetFragment(desc.PropertyName + "[" + count + "]");
                            if (result == null) {
                                writer.Write("(null EventBean)\n");
                            }
                            else {
                                writer.Write("\n");
                                propertyStack.Push(desc.PropertyName);
                                Print(result, writer, indent + 6, propertyStack);
                                propertyStack.Pop();
                            }

                            count++;
                        }
                        catch (PropertyAccessException) {
                            writer.Write("-- no access --\n");
                            break;
                        }
                    }
                }
                else {
                    var fragment = theEvent.GetFragment(desc.PropertyName);
                    if (fragment == null) {
                        writer.Write("(null)\n");
                        continue;
                    }

                    if (fragment is EventBean) {
                        var fragmentBean = (EventBean) fragment;
                        writer.Write("EventBean type ");
                        writer.Write(fragmentBean.EventType.Name);
                        writer.Write("...\n");

                        // prevent getThis() loops
                        if (fragmentBean.EventType == theEvent.EventType) {
                            WriteIndent(writer, indent + 2);
                            writer.Write("Skipping");
                        }
                        else {
                            propertyStack.Push(desc.PropertyName);
                            Print(fragmentBean, writer, indent + 4, propertyStack);
                            propertyStack.Pop();
                        }
                    }
                    else {
                        var fragmentBeans = (EventBean[]) fragment;
                        writer.Write("EventBean[] type ");
                        if (fragmentBeans.Length == 0) {
                            writer.Write("(empty array)\n");
                        }
                        else {
                            writer.Write(fragmentBeans[0].EventType.Name);
                            writer.Write("...\n");
                            for (var i = 0; i < fragmentBeans.Length; i++) {
                                WriteIndent(writer, indent + 4);
                                writer.Write("bean #" + i + "...\n");

                                propertyStack.Push(desc.PropertyName);
                                Print(fragmentBeans[i], writer, indent + 6, propertyStack);
                                propertyStack.Pop();
                            }
                        }
                    }
                }
            }
        }

        private static void PrintProperties(
            EventBean eventBean,
            TextWriter writer,
            int indent,
            Stack<string> propertyStack)
        {
            var properties = eventBean.EventType.PropertyDescriptors;

            // write simple properties
            for (var i = 0; i < properties.Count; i++) {
                var propertyName = properties[i].PropertyName;

                if (properties[i].IsIndexed || properties[i].IsMapped) {
                    continue;
                }

                WriteIndent(writer, indent);
                writer.Write(propertyName);
                writer.Write(" : ");

                var resultGet = eventBean.Get(propertyName);
                WriteValue(writer, resultGet);
                writer.Write("\n");
            }

            // write indexed properties
            for (var i = 0; i < properties.Count; i++) {
                var propertyName = properties[i].PropertyName;

                if (!properties[i].IsIndexed) {
                    continue;
                }

                WriteIndent(writer, indent);
                writer.Write(propertyName);
                var type = "array";
                if (properties[i].IsRequiresIndex) {
                    type = type + " requires-index";
                }

                writer.Write(" (" + type + ") : ");

                if (properties[i].IsRequiresIndex) {
                    var count = 0;
                    writer.Write("\n");
                    while (true) {
                        try {
                            WriteIndent(writer, indent + 2);
                            writer.Write("#");
                            writer.Write(Convert.ToString(count));
                            writer.Write(" ");
                            var result = eventBean.Get(propertyName + "[" + count + "]");
                            WriteValue(writer, result);
                            writer.Write("\n");
                            count++;
                        }
                        catch (PropertyAccessException) {
                            writer.Write("-- no access --\n");
                            break;
                        }
                    }
                }
                else {
                    var result = eventBean.Get(propertyName);
                    WriteValue(writer, result);
                    writer.Write("\n");
                }
            }

            // write mapped properties
            for (var i = 0; i < properties.Count; i++) {
                var propertyName = properties[i].PropertyName;

                if (!properties[i].IsMapped) {
                    continue;
                }

                WriteIndent(writer, indent);
                writer.Write(propertyName);
                var type = "mapped";
                if (properties[i].IsRequiresMapKey) {
                    type = type + " requires-mapkey";
                }

                writer.Write(" (" + type + ") : ");

                if (!properties[i].IsRequiresMapKey) {
                    var result = eventBean.Get(propertyName);
                    WriteValue(writer, result);
                    writer.Write("\n");
                }
                else {
                    writer.Write("??map key unknown??\n");
                }
            }
        }

        private static void AssertConsistencyRecursive(
            EventBean eventBean,
            ISet<EventType> alreadySeenTypes)
        {
            AssertConsistencyRecursive(eventBean.EventType, alreadySeenTypes);

            var properties = eventBean.EventType.PropertyDescriptors;
            for (var i = 0; i < properties.Count; i++) {
                var failedMessage = "failed assertion for property '" + properties[i].PropertyName + "' ";
                var propertyName = properties[i].PropertyName;

                // assert getter
                if (!properties[i].IsRequiresIndex && !properties[i].IsRequiresMapKey) {
                    var getter = eventBean.EventType.GetGetter(propertyName);
                    var resultGetter = getter.Get(eventBean);
                    var resultGet = eventBean.Get(propertyName);

                    if (resultGetter == null && resultGet == null) {
                        // fine
                    }
                    else if (resultGet is XmlNodeList) {
                        ScopeTestHelper.AssertEquals(
                            failedMessage,
                            ((XmlNodeList) resultGet).Count,
                            ((XmlNodeList) resultGetter).Count);
                    }
                    else if (resultGet.GetType().IsArray) {
                        ScopeTestHelper.AssertEquals(
                            failedMessage,
                            ((Array) resultGet).Length,
                            ((Array) resultGetter).Length);
                    }
                    else {
                        ScopeTestHelper.AssertEquals(failedMessage, resultGet, resultGetter);
                    }

                    if (resultGet != null) {
                        if (resultGet is EventBean[] || resultGet is EventBean) {
                            ScopeTestHelper.AssertTrue(properties[i].IsFragment);
                        }
                        else {
                            var resultType = resultGet.GetType();
                            var propertyType = properties[i].PropertyType.GetBoxedType();
                            if ((resultType != propertyType) &&
                                !TypeHelper.IsSubclassOrImplementsInterface(resultType, propertyType)) {
                                ScopeTestHelper.Fail(failedMessage);
                            }
                        }
                    }
                }

                // fragment
                if (!properties[i].IsFragment) {
                    ScopeTestHelper.AssertNull(failedMessage, eventBean.GetFragment(propertyName));
                    continue;
                }

                var fragment = eventBean.GetFragment(propertyName);
                ScopeTestHelper.AssertNotNull(failedMessage, fragment);

                var fragmentType = eventBean.EventType.GetFragmentType(propertyName);
                ScopeTestHelper.AssertNotNull(failedMessage, fragmentType);

                if (!fragmentType.IsIndexed) {
                    ScopeTestHelper.AssertTrue(failedMessage, fragment is EventBean);
                    var fragmentEvent = (EventBean) fragment;
                    AssertConsistencyRecursive(fragmentEvent, alreadySeenTypes);
                }
                else {
                    ScopeTestHelper.AssertTrue(failedMessage, fragment is EventBean[]);
                    var events = (EventBean[]) fragment;
                    ScopeTestHelper.AssertTrue(failedMessage, events.Length > 0);
                    foreach (var theEvent in events) {
                        AssertConsistencyRecursive(theEvent, alreadySeenTypes);
                    }
                }
            }
        }

        private static void AssertConsistencyRecursive(
            EventType eventType,
            ISet<EventType> alreadySeenTypes)
        {
            if (alreadySeenTypes.Contains(eventType)) {
                return;
            }

            alreadySeenTypes.Add(eventType);

            AssertConsistencyProperties(eventType);

            // test fragments
            foreach (var descriptor in eventType.PropertyDescriptors) {
                var failedMessage = "failed assertion for property '" + descriptor.PropertyName + "' ";
                if (!descriptor.IsFragment) {
                    ScopeTestHelper.AssertNull(failedMessage, eventType.GetFragmentType(descriptor.PropertyName));
                    continue;
                }

                var fragment = eventType.GetFragmentType(descriptor.PropertyName);
                if (!descriptor.IsRequiresIndex) {
                    ScopeTestHelper.AssertNotNull(failedMessage, fragment);
                    if (fragment.IsIndexed) {
                        ScopeTestHelper.AssertTrue(descriptor.IsIndexed);
                    }

                    AssertConsistencyRecursive(fragment.FragmentType, alreadySeenTypes);
                }
                else {
                    fragment = eventType.GetFragmentType(descriptor.PropertyName + "[0]");
                    ScopeTestHelper.AssertNotNull(failedMessage, fragment);
                    ScopeTestHelper.AssertTrue(descriptor.IsIndexed);
                    AssertConsistencyRecursive(fragment.FragmentType, alreadySeenTypes);
                }
            }
        }

        private static void AssertConsistencyProperties(EventType eventType)
        {
            IList<string> propertyNames = new List<string>();

            var properties = eventType.PropertyDescriptors;
            for (var i = 0; i < properties.Count; i++) {
                var propertyName = properties[i].PropertyName;
                propertyNames.Add(propertyName);
                var failedMessage = "failed assertion for property '" + propertyName + "' ";

                // assert presence of descriptor
                ScopeTestHelper.AssertSame(properties[i], eventType.GetPropertyDescriptor(propertyName));

                // test properties that can simply be in a property expression
                if (!properties[i].IsRequiresIndex && !properties[i].IsRequiresMapKey) {
                    ScopeTestHelper.AssertTrue(failedMessage, eventType.IsProperty(propertyName));
                    ScopeTestHelper.AssertSame(
                        failedMessage,
                        eventType.GetPropertyType(propertyName),
                        properties[i].PropertyType);
                    ScopeTestHelper.AssertNotNull(failedMessage, eventType.GetGetter(propertyName));
                }

                // test indexed property
                if (properties[i].IsIndexed) {
                    var propertyNameIndexed = propertyName + "[0]";
                    ScopeTestHelper.AssertTrue(failedMessage, eventType.IsProperty(propertyNameIndexed));
                    ScopeTestHelper.AssertNotNull(failedMessage, eventType.GetPropertyType(propertyNameIndexed));
                    ScopeTestHelper.AssertNotNull(failedMessage, eventType.GetGetter(propertyNameIndexed));
                }

                // test mapped property
                if (properties[i].IsRequiresMapKey) {
                    var propertyNameMapped = propertyName + "('a')";
                    ScopeTestHelper.AssertTrue(failedMessage, eventType.IsProperty(propertyNameMapped));
                    ScopeTestHelper.AssertNotNull(failedMessage, eventType.GetPropertyType(propertyNameMapped));
                    ScopeTestHelper.AssertNotNull(failedMessage, eventType.GetGetter(propertyNameMapped));
                }

                // consistent flags
                ScopeTestHelper.AssertFalse(failedMessage, properties[i].IsIndexed && properties[i].IsMapped);
                if (properties[i].IsRequiresIndex) {
                    ScopeTestHelper.AssertTrue(failedMessage, properties[i].IsIndexed);
                }

                if (properties[i].IsRequiresMapKey) {
                    ScopeTestHelper.AssertTrue(failedMessage, properties[i].IsMapped);
                }
            }

            // assert same property names
            EPAssertionUtil.AssertEqualsAnyOrder(eventType.PropertyNames, propertyNames.ToArray());
        }

        private static void WriteIndent(
            TextWriter writer,
            int indent)
        {
            for (var i = 0; i < indent; i++) {
                writer.Write(' ');
            }
        }

        private static void WriteValue(
            TextWriter writer,
            object result)
        {
            if (result == null) {
                writer.Write("(null)");
                return;
            }

            if (result is Array resultArray) {
                writer.Write("Array len=");
                writer.Write(Convert.ToString(resultArray.Length));
                writer.Write("{");
                var delimiter = "";
                for (var i = 0; i < resultArray.Length; i++) {
                    writer.Write(delimiter);
                    WriteValue(writer, resultArray.GetValue(i));
                    delimiter = ", ";
                }

                writer.Write("}");
            }
            else {
                writer.Write(result.ToString());
            }
        }

        public static void AssertEventTypeProperties(
            object[][] expectedValuesArr,
            EventType eventType,
            params SupportEventTypeAssertionEnum[] assertions)
        {
            var propertyDescriptors = eventType.PropertyDescriptors
                .OrderBy(p => p.PropertyName)
                .ToList();
 
            for (var propNum = 0; propNum < expectedValuesArr.Length; propNum++) {
                var message = "Failed assertion for property " + propNum;
                var prop = propertyDescriptors[propNum];
                var expectedArr = expectedValuesArr[propNum];

                for (var i = 0; i < assertions.Length; i++) {
                    var assertion = assertions[i];
                    var expected = expectedArr[i];
                    var value = assertion.GetExtractor().Invoke(prop, eventType);
                    ScopeTestHelper.AssertEquals(
                        message + " at assertion " + assertion, expected, value);
                }
            }
        }

        private static void AssertFragmentNonArray(
            EventBean @event,
            bool isNative,
            string propertyExpression)
        {
            var fragmentBean = (EventBean) @event.GetFragment(propertyExpression);
            var fragmentType = @event.EventType.GetFragmentType(propertyExpression);
            ScopeTestHelper.AssertFalse("failed for " + propertyExpression, fragmentType.IsIndexed);
            ScopeTestHelper.AssertEquals("failed for " + propertyExpression, isNative, fragmentType.IsNative);
            ScopeTestHelper.AssertSame(
                "failed for " + propertyExpression,
                fragmentBean.EventType,
                fragmentType.FragmentType);
            AssertConsistency(fragmentBean);
        }

        private static void AssertFragmentArray(
            EventBean @event,
            bool isNative,
            string propertyExpression)
        {
            var fragmentBean = (EventBean[]) @event.GetFragment(propertyExpression);
            var fragmentType = @event.EventType.GetFragmentType(propertyExpression);
            ScopeTestHelper.AssertTrue("failed for " + propertyExpression, fragmentType.IsIndexed);
            ScopeTestHelper.AssertEquals("failed for " + propertyExpression, isNative, fragmentType.IsNative);
            ScopeTestHelper.AssertSame(
                "failed for " + propertyExpression,
                fragmentBean[0].EventType,
                fragmentType.FragmentType);
            AssertConsistency(fragmentBean[0]);
        }
    }
} // end of namespace