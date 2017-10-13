///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;

namespace com.espertech.esper.util.support
{
    public class SupportEventTypeAssertionUtil
    {
        public static void AssertFragments(EventBean @event, bool isNative, bool array, string propertyExpressions)
        {
            string[] names = propertyExpressions.SplitCsv();
            foreach (string name in names)
            {
                if (!array)
                {
                    AssertFragmentNonArray(@event, isNative, name);
                }
                else
                {
                    AssertFragmentArray(@event, isNative, name);
                }
            }
        }

        public static void AssertConsistency(EventBean eventBean)
        {
            AssertConsistencyRecusive(eventBean, new HashSet<EventType>());
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

        private static void Print(EventBean theEvent, StringWriter writer, int indent, Stack<string> propertyStack)
        {
            WriteIndent(writer, indent);
            writer.Write("Properties : \n");
            PrintProperties(theEvent, writer, indent + 2, propertyStack);

            // count fragments
            int countFragments = 0;
            foreach (EventPropertyDescriptor desc in theEvent.EventType.PropertyDescriptors)
            {
                if (desc.IsFragment)
                {
                    countFragments++;
                }
            }
            if (countFragments == 0)
            {
                return;
            }

            WriteIndent(writer, indent);
            writer.Write("Fragments : (" + countFragments + ") \n");
            foreach (EventPropertyDescriptor desc in theEvent.EventType.PropertyDescriptors)
            {
                if (!desc.IsFragment)
                {
                    continue;
                }

                WriteIndent(writer, indent + 2);
                writer.Write(desc.PropertyName);
                writer.Write(" : ");

                if (desc.RequiresIndex)
                {
                    writer.Write("\n");
                    int count = 0;
                    while (true)
                    {
                        try
                        {
                            WriteIndent(writer, indent + 4);
                            writer.Write("bean #");
                            writer.Write(count);
                            var result = (EventBean) theEvent.GetFragment(desc.PropertyName + "[" + count + "]");
                            if (result == null)
                            {
                                writer.Write("(null EventBean)\n");
                            }
                            else
                            {
                                writer.Write("\n");
                                propertyStack.Push(desc.PropertyName);
                                Print(result, writer, indent + 6, propertyStack);
                                propertyStack.Pop();
                            }
                            count++;
                        }
                        catch (PropertyAccessException)
                        {
                            writer.Write("-- no access --\n");
                            break;
                        }
                    }
                }
                else
                {
                    object fragment = theEvent.GetFragment(desc.PropertyName);
                    if (fragment == null)
                    {
                        writer.Write("(null)\n");
                        continue;
                    }

                    if (fragment is EventBean)
                    {
                        var fragmentBean = (EventBean) fragment;
                        writer.Write("EventBean type ");
                        writer.Write(fragmentBean.EventType.Name);
                        writer.Write("...\n");

                        // prevent GetThis() loops
                        if (fragmentBean.EventType == theEvent.EventType)
                        {
                            WriteIndent(writer, indent + 2);
                            writer.Write("Skipping");
                        }
                        else
                        {
                            propertyStack.Push(desc.PropertyName);
                            Print(fragmentBean, writer, indent + 4, propertyStack);
                            propertyStack.Pop();
                        }
                    }
                    else
                    {
                        var fragmentBeans = (EventBean[]) fragment;
                        writer.Write("EventBean[] type ");
                        if (fragmentBeans.Length == 0)
                        {
                            writer.Write("(empty array)\n");
                        }
                        else
                        {
                            writer.Write(fragmentBeans[0].EventType.Name);
                            writer.Write("...\n");
                            for (int i = 0; i < fragmentBeans.Length; i++)
                            {
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
            StringWriter writer,
            int indent,
            Stack<string> propertyStack)
        {
            IList<EventPropertyDescriptor> properties = eventBean.EventType.PropertyDescriptors;

            // write simple properties
            for (int i = 0; i < properties.Count; i++)
            {
                string propertyName = properties[i].PropertyName;

                if (properties[i].IsIndexed || properties[i].IsMapped)
                {
                    continue;
                }

                WriteIndent(writer, indent);
                writer.Write(propertyName);
                writer.Write(" : ");

                object resultGet = eventBean.Get(propertyName);
                WriteValue(writer, resultGet);
                writer.Write("\n");
            }

            // write indexed properties
            for (int i = 0; i < properties.Count; i++)
            {
                string propertyName = properties[i].PropertyName;

                if (!properties[i].IsIndexed)
                {
                    continue;
                }

                WriteIndent(writer, indent);
                writer.Write(propertyName);
                string type = "array";
                if (properties[i].RequiresIndex)
                {
                    type = type + " requires-index";
                }
                writer.Write(" (" + type + ") : ");

                if (properties[i].RequiresIndex)
                {
                    int count = 0;
                    writer.Write("\n");
                    while (true)
                    {
                        try
                        {
                            WriteIndent(writer, indent + 2);
                            writer.Write("#");
                            writer.Write(count);
                            writer.Write(" ");
                            object result = eventBean.Get(propertyName + "[" + count + "]");
                            WriteValue(writer, result);
                            writer.Write("\n");
                            count++;
                        }
                        catch (PropertyAccessException)
                        {
                            writer.Write("-- no access --\n");
                            break;
                        }
                    }
                }
                else
                {
                    object result = eventBean.Get(propertyName);
                    WriteValue(writer, result);
                    writer.Write("\n");
                }
            }

            // write mapped properties
            for (int i = 0; i < properties.Count; i++)
            {
                string propertyName = properties[i].PropertyName;

                if (!properties[i].IsMapped)
                {
                    continue;
                }

                WriteIndent(writer, indent);
                writer.Write(propertyName);
                string type = "mapped";
                if (properties[i].RequiresMapKey)
                {
                    type = type + " requires-mapkey";
                }
                writer.Write(" (" + type + ") : ");

                if (!properties[i].RequiresMapKey)
                {
                    object result = eventBean.Get(propertyName);
                    WriteValue(writer, result);
                    writer.Write("\n");
                }
                else
                {
                    writer.Write("??map key unknown??\n");
                }
            }
        }

        private static void AssertConsistencyRecusive(EventBean eventBean, ISet<EventType> alreadySeenTypes)
        {
            AssertConsistencyRecursive(eventBean.EventType, alreadySeenTypes);

            IList<EventPropertyDescriptor> properties = eventBean.EventType.PropertyDescriptors;
            for (int i = 0; i < properties.Count; i++)
            {
                string failedMessage = "failed assertion for property '" + properties[i].PropertyName + "' ";
                string propertyName = properties[i].PropertyName;

                // assert getter
                if ((!properties[i].RequiresIndex) && (!properties[i].RequiresMapKey))
                {
                    EventPropertyGetter getter = eventBean.EventType.GetGetter(propertyName);
                    object resultGetter = getter.Get(eventBean);
                    object resultGet = eventBean.Get(propertyName);

                    if ((resultGetter == null) && (resultGet == null))
                    {
                        // fine
                    }
                    else if (resultGet is XmlNodeList)
                    {
                        ScopeTestHelper.AssertEquals(
                            failedMessage, 
                            ((XmlNodeList) resultGet).Count, 
                            ((XmlNodeList) resultGetter).Count);
                    }
                    else if (resultGet is Array)
                    {
                        ScopeTestHelper.AssertEquals(
                            failedMessage,
                            ((Array) resultGet).Length,
                            ((Array) resultGetter).Length);
                    }
                    else
                    {
                        ScopeTestHelper.AssertEquals(failedMessage, resultGet, resultGetter);
                    }

                    if (resultGet != null)
                    {
                        if (resultGet is EventBean[] || resultGet is EventBean)
                        {
                            ScopeTestHelper.AssertTrue(properties[i].IsFragment);
                        }
                        else
                        {
                            ScopeTestHelper.AssertTrue(
                                failedMessage,
                                TypeHelper.IsSubclassOrImplementsInterface(
                                    resultGet.GetType(),
                                    properties[i].PropertyType.GetBoxedType()));
                        }
                    }
                }

                // fragment
                if (!properties[i].IsFragment)
                {
                    ScopeTestHelper.AssertNull(failedMessage, eventBean.GetFragment(propertyName));
                    continue;
                }

                object fragment = eventBean.GetFragment(propertyName);
                ScopeTestHelper.AssertNotNull(failedMessage, fragment);

                FragmentEventType fragmentType = eventBean.EventType.GetFragmentType(propertyName);
                ScopeTestHelper.AssertNotNull(failedMessage, fragmentType);

                if (!fragmentType.IsIndexed)
                {
                    ScopeTestHelper.AssertTrue(failedMessage, fragment is EventBean);
                    var fragmentEvent = (EventBean) fragment;
                    AssertConsistencyRecusive(fragmentEvent, alreadySeenTypes);
                }
                else
                {
                    ScopeTestHelper.AssertTrue(failedMessage, fragment is EventBean[]);
                    var events = (EventBean[]) fragment;
                    ScopeTestHelper.AssertTrue(failedMessage, events.Length > 0);
                    foreach (EventBean theEvent in events)
                    {
                        AssertConsistencyRecusive(theEvent, alreadySeenTypes);
                    }
                }
            }
        }

        private static void AssertConsistencyRecursive(EventType eventType, ISet<EventType> alreadySeenTypes)
        {
            if (alreadySeenTypes.Contains(eventType))
            {
                return;
            }
            alreadySeenTypes.Add(eventType);

            AssertConsistencyProperties(eventType);

            // test fragments
            foreach (EventPropertyDescriptor descriptor in eventType.PropertyDescriptors)
            {
                string failedMessage = "failed assertion for property '" + descriptor.PropertyName + "' ";
                if (!descriptor.IsFragment)
                {
                    ScopeTestHelper.AssertNull(failedMessage, eventType.GetFragmentType(descriptor.PropertyName));
                    continue;
                }

                FragmentEventType fragment = eventType.GetFragmentType(descriptor.PropertyName);
                if (!descriptor.RequiresIndex)
                {
                    ScopeTestHelper.AssertNotNull(failedMessage, fragment);
                    if (fragment.IsIndexed)
                    {
                        ScopeTestHelper.AssertTrue(descriptor.IsIndexed);
                    }
                    AssertConsistencyRecursive(fragment.FragmentType, alreadySeenTypes);
                }
                else
                {
                    fragment = eventType.GetFragmentType(descriptor.PropertyName + "[0]");
                    ScopeTestHelper.AssertNotNull(failedMessage, fragment);
                    ScopeTestHelper.AssertTrue(descriptor.IsIndexed);
                    AssertConsistencyRecursive(fragment.FragmentType, alreadySeenTypes);
                }
            }
        }

        private static void AssertConsistencyProperties(EventType eventType)
        {
            var propertyNames = new List<string>();

            IList<EventPropertyDescriptor> properties = eventType.PropertyDescriptors;
            for (int i = 0; i < properties.Count; i++)
            {
                string propertyName = properties[i].PropertyName;
                propertyNames.Add(propertyName);
                string failedMessage = "failed assertion for property '" + propertyName + "' ";

                // assert presence of descriptor
                ScopeTestHelper.AssertSame(properties[i], eventType.GetPropertyDescriptor(propertyName));

                // test properties that can simply be in a property expression
                if ((!properties[i].RequiresIndex) && (!properties[i].RequiresMapKey))
                {
                    ScopeTestHelper.AssertTrue(failedMessage, eventType.IsProperty(propertyName));
                    ScopeTestHelper.AssertSame(
                        failedMessage, eventType.GetPropertyType(propertyName), properties[i].PropertyType);
                    ScopeTestHelper.AssertNotNull(failedMessage, eventType.GetGetter(propertyName));
                }

                // test indexed property
                if (properties[i].IsIndexed)
                {
                    string propertyNameIndexed = propertyName + "[0]";
                    ScopeTestHelper.AssertTrue(failedMessage, eventType.IsProperty(propertyNameIndexed));
                    ScopeTestHelper.AssertNotNull(failedMessage, eventType.GetPropertyType(propertyNameIndexed));
                    ScopeTestHelper.AssertNotNull(failedMessage, eventType.GetGetter(propertyNameIndexed));
                }

                // test mapped property
                if (properties[i].RequiresMapKey)
                {
                    string propertyNameMapped = propertyName + "('a')";
                    ScopeTestHelper.AssertTrue(failedMessage, eventType.IsProperty(propertyNameMapped));
                    ScopeTestHelper.AssertNotNull(failedMessage, eventType.GetPropertyType(propertyNameMapped));
                    ScopeTestHelper.AssertNotNull(failedMessage, eventType.GetGetter(propertyNameMapped));
                }

                // consistent flags
                ScopeTestHelper.AssertFalse(failedMessage, properties[i].IsIndexed && properties[i].IsMapped);
                if (properties[i].RequiresIndex)
                {
                    ScopeTestHelper.AssertTrue(failedMessage, properties[i].IsIndexed);
                }
                if (properties[i].RequiresMapKey)
                {
                    ScopeTestHelper.AssertTrue(failedMessage, properties[i].IsMapped);
                }
            }

            // assert same property names
            EPAssertionUtil.AssertEqualsAnyOrder(eventType.PropertyNames, propertyNames.ToArray());
        }

        private static void WriteIndent(TextWriter writer, int indent)
        {
            for (int i = 0; i < indent; i++)
            {
                writer.Write(' ');
            }
        }

        private static void WriteValue(TextWriter writer, Object result)
        {
            if (result == null)
            {
                writer.Write("(null)");
                return;
            }

            var asArray = result as Array;
            if (asArray != null)
            {
                writer.Write("Array len=");
                writer.Write(asArray.Length);
                writer.Write("{");
                string delimiter = "";
                for (int i = 0; i < asArray.Length; i++)
                {
                    writer.Write(delimiter);
                    WriteValue(writer, asArray.GetValue(i));
                    delimiter = ", ";
                }
                writer.Write("}");
            }
            else
            {
                writer.Write(result.ToString());
            }
        }

        public static void AssertEventTypeProperties(
            Object[][] expectedArr,
            EventType eventType,
            params SupportEventTypeAssertionEnum[] assertions)
        {
            for (int propNum = 0; propNum < expectedArr.Length; propNum++)
            {
                string message = "Failed assertion for property " + propNum;
                EventPropertyDescriptor prop = eventType.PropertyDescriptors[propNum];

                for (int i = 0; i < assertions.Length; i++)
                {
                    SupportEventTypeAssertionEnum assertion = assertions[i];
                    object expected = expectedArr[propNum][i];
                    object value = assertion.GetExtractor().Invoke(prop, eventType);
                    ScopeTestHelper.AssertEquals(message + " at assertion " + assertion, expected, value);
                }
            }
        }

        private static void AssertFragmentNonArray(EventBean @event, bool isNative, string propertyExpression)
        {
            var fragmentBean = (EventBean) @event.GetFragment(propertyExpression);
            FragmentEventType fragmentType = @event.EventType.GetFragmentType(propertyExpression);
            ScopeTestHelper.AssertFalse("failed for " + propertyExpression, fragmentType.IsIndexed);
            ScopeTestHelper.AssertEquals("failed for " + propertyExpression, isNative, fragmentType.IsNative);
            ScopeTestHelper.AssertSame(
                "failed for " + propertyExpression, fragmentBean.EventType, fragmentType.FragmentType);
            AssertConsistency(fragmentBean);
        }

        private static void AssertFragmentArray(EventBean @event, bool isNative, string propertyExpression)
        {
            var fragmentBean = (EventBean[]) @event.GetFragment(propertyExpression);
            FragmentEventType fragmentType = @event.EventType.GetFragmentType(propertyExpression);
            ScopeTestHelper.AssertTrue("failed for " + propertyExpression, fragmentType.IsIndexed);
            ScopeTestHelper.AssertEquals("failed for " + propertyExpression, isNative, fragmentType.IsNative);
            ScopeTestHelper.AssertSame(
                "failed for " + propertyExpression, fragmentBean[0].EventType, fragmentType.FragmentType);
            AssertConsistency(fragmentBean[0]);
        }
    }
} // end of namespace