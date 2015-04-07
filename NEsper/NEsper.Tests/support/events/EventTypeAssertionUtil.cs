///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.support.events
{
    public class EventTypeAssertionUtil
    {
        public static void AssertConsistency(EventBean eventBean)
        {
            AssertConsistencyRecusive(eventBean, new HashSet<EventType>());
        }
    
        public static void AssertConsistency(EventType eventType)
        {
            AssertConsistencyRecursive(eventType, new HashSet<EventType>());
        }
    
        public static String Print(EventBean theEvent)
        {
            var writer = new StringWriter();
            Print(theEvent, writer, 0, new Stack<String>());
            return writer.ToString();
        }

        private static void Print(EventBean theEvent, TextWriter writer, int indent, Stack<String> propertyStack)
        {
            WriteIndent(writer, indent);
            writer.Write("Properties : \n");
            PrintProperties(theEvent, writer, indent + 2, propertyStack);
    
            // count fragments
            var countFragments = 0;
            foreach (var desc in theEvent.EventType.PropertyDescriptors)
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
            foreach (var desc in theEvent.EventType.PropertyDescriptors)
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
                    var count = 0;
                    while(true){
                        try
                        {
                            WriteIndent(writer, indent + 4);
                            writer.Write("bean #");
                            writer.Write(Convert.ToString(count));
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
                    var fragment = theEvent.GetFragment(desc.PropertyName);
                    if (fragment == null)
                    {
                        writer.Write("(null)\n");
                        continue;
                    }
    
                    if (fragment is EventBean)
                    {
                        var fragmentBean = (EventBean)fragment;
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
                        var fragmentBeans = (EventBean[])fragment;
                        writer.Write("EventBean[] type ");
                        if (fragmentBeans.Length == 0)
                        {
                            writer.Write("(empty array)\n");
                        }
                        else
                        {
                            writer.Write(fragmentBeans[0].EventType.Name);
                            writer.Write("...\n");
                            for (var i = 0; i < fragmentBeans.Length; i++)
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

        private static void PrintProperties(EventBean eventBean, TextWriter writer, int indent, Stack<String> propertyStack)
        {
            var properties = eventBean.EventType.PropertyDescriptors;
    
            // write simple properties
            for (var i = 0; i < properties.Count; i++)
            {
                var propertyName = properties[i].PropertyName;
    
                if (properties[i].IsIndexed || properties[i].IsMapped)
                {
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
            for (var i = 0; i < properties.Count; i++)
            {
                var propertyName = properties[i].PropertyName;
    
                if (!properties[i].IsIndexed)
                {
                    continue;
                }
    
                WriteIndent(writer, indent);
                writer.Write(propertyName);
                var type = "array";
                if (properties[i].RequiresIndex)
                {
                    type = type + " requires-index";
                }
                writer.Write(" (" + type + ") : ");
    
                if (properties[i].RequiresIndex)
                {
                    var count = 0;
                    writer.Write("\n");
                    while(true){
                        try
                        {
                            WriteIndent(writer, indent + 2);
                            writer.Write("#");
                            writer.Write(Convert.ToString(count));
                            writer.Write(" ");
                            var result = eventBean.Get(propertyName + "[" + count + "]");
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
                    var result = eventBean.Get(propertyName);
                    WriteValue(writer, result);
                    writer.Write("\n");
                }
            }
    
            // write mapped properties
            for (var i = 0; i < properties.Count; i++)
            {
                var propertyName = properties[i].PropertyName;
    
                if (!properties[i].IsMapped)
                {
                    continue;
                }
    
                WriteIndent(writer, indent);
                writer.Write(propertyName);
                var type = "mapped";
                if (properties[i].RequiresMapKey)
                {
                    type = type + " requires-mapkey";
                }
                writer.Write(" (" + type + ") : ");

                if (!properties[i].RequiresMapKey)
                {
                    var result = eventBean.Get(propertyName);
                    WriteValue(writer, result);
                    writer.Write("\n");
                }
                else
                {
                    writer.Write("??map key unknown??\n");                
                }
            }
        }
        
        private static void AssertConsistencyRecusive(EventBean eventBean, ICollection<EventType> alreadySeenTypes)
        {
            AssertConsistencyRecursive(eventBean.EventType, alreadySeenTypes);
    
            var properties = eventBean.EventType.PropertyDescriptors;
            for (var i = 0; i < properties.Count; i++)
            {
                var failedMessage = "failed assertion for property '" + properties[i].PropertyName + "' ";
                var propertyName = properties[i].PropertyName;
    
                // assert getter
                if ((!properties[i].RequiresIndex) && (!properties[i].RequiresMapKey))
                {
                    var getter = eventBean.EventType.GetGetter(propertyName);
                    var resultGetter = getter.Get(eventBean);
                    var resultGet = eventBean.Get(propertyName);
    
                    if ((resultGetter == null) && (resultGet == null))
                    {
                        // fine
                    }
                    else if (resultGet is XmlNodeList)
                    {
                        Assert.AreEqual(((XmlNodeList)resultGet).Count, ((XmlNodeList)resultGetter).Count, failedMessage);
                    }
                    else if (resultGet.GetType().IsArray)
                    {
                        var asArray = (Array) resultGet;
                        var asArrayGetter = (Array) resultGetter;

                        Assert.AreEqual(asArray.Length, asArrayGetter.Length, failedMessage);
                    }
                    else
                    {
                        Assert.AreEqual(resultGet, resultGetter, failedMessage);
                    }
    
                    if (resultGet != null)
                    {
                        if (resultGet is EventBean[] || resultGet is EventBean)
                        {
                            Assert.IsTrue(properties[i].IsFragment);
                        }
                        else
                        {
                            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(resultGet.GetType(), properties[i].PropertyType), failedMessage);
                        }
                    }
                }
    
                // fragment
                if (!properties[i].IsFragment)
                {
                    Assert.IsNull(eventBean.GetFragment(propertyName), failedMessage);
                    continue;
                }
    
                var fragment = eventBean.GetFragment(propertyName);
                Assert.NotNull(fragment, failedMessage);
    
                var fragmentType = eventBean.EventType.GetFragmentType(propertyName);
                Assert.NotNull(fragmentType, failedMessage);
    
                if (!fragmentType.IsIndexed)
                {
                    Assert.IsTrue(fragment is EventBean, failedMessage);
                    var fragmentEvent = (EventBean) fragment;
                    AssertConsistencyRecusive(fragmentEvent, alreadySeenTypes);
                }
                else
                {
                    Assert.IsTrue(fragment is EventBean[], failedMessage);
                    var events = (EventBean[]) fragment;
                    Assert.IsTrue(events.Length > 0, failedMessage);
                    foreach (var theEvent in events)
                    {
                        AssertConsistencyRecusive(theEvent, alreadySeenTypes);
                    }
                }
            }
        }
    
        private static void AssertConsistencyRecursive(EventType eventType, ICollection<EventType> alreadySeenTypes)
        {
            if (alreadySeenTypes.Contains(eventType))
            {
                return;
            }
            alreadySeenTypes.Add(eventType);
    
            AssertConsistencyProperties(eventType);
    
            // test fragments
            foreach (var descriptor in eventType.PropertyDescriptors)
            {
                var failedMessage = "failed assertion for property '" + descriptor.PropertyName + "' ";
                if (!descriptor.IsFragment)
                {
                    Assert.IsNull(eventType.GetFragmentType(descriptor.PropertyName), failedMessage);
                    continue;
                }
    
                var fragment = eventType.GetFragmentType(descriptor.PropertyName);
                if (!descriptor.RequiresIndex)
                {
                    Assert.NotNull(fragment, failedMessage);
                    if (fragment.IsIndexed)
                    {
                        Assert.IsTrue(descriptor.IsIndexed);
                    }
                    AssertConsistencyRecursive(fragment.FragmentType, alreadySeenTypes);
                }
                else
                {
                    fragment = eventType.GetFragmentType(descriptor.PropertyName + "[0]");
                    Assert.NotNull(fragment, failedMessage);
                    Assert.IsTrue(descriptor.IsIndexed);
                    AssertConsistencyRecursive(fragment.FragmentType, alreadySeenTypes);
                }
            }
        }
    
        private static void AssertConsistencyProperties(EventType eventType)
        {
            IList<String> propertyNames = new List<String>();
    
            var properties = eventType.PropertyDescriptors;
            for (var i = 0; i < properties.Count; i++)
            {
                var propertyName = properties[i].PropertyName;
                propertyNames.Add(propertyName);
                var failedMessage = "failed assertion for property '" + propertyName + "' ";
    
                // assert presence of descriptor
                Assert.AreSame(properties[i], eventType.GetPropertyDescriptor(propertyName));
    
                // test properties that can simply be in a property expression
                if ((!properties[i].RequiresIndex) && (!properties[i].RequiresMapKey))
                {
                    Assert.IsTrue(eventType.IsProperty(propertyName), failedMessage);
                    Assert.AreSame(eventType.GetPropertyType(propertyName), properties[i].PropertyType, failedMessage);
                    Assert.NotNull(eventType.GetGetter(propertyName), failedMessage);
                }
                
                // test indexed property
                if (properties[i].IsIndexed)
                {
                    var propertyNameIndexed = propertyName + "[0]";
                    Assert.IsTrue(eventType.IsProperty(propertyNameIndexed), failedMessage);
                    Assert.NotNull(eventType.GetPropertyType(propertyNameIndexed), failedMessage);
                    Assert.NotNull(eventType.GetGetter(propertyNameIndexed), failedMessage);
                }
    
                // test mapped property
                if (properties[i].RequiresMapKey)
                {
                    var propertyNameMapped = propertyName + "('a')";
                    Assert.IsTrue(eventType.IsProperty(propertyNameMapped), failedMessage);
                    Assert.NotNull(eventType.GetPropertyType(propertyNameMapped), failedMessage);
                    Assert.NotNull(eventType.GetGetter(propertyNameMapped), failedMessage);
                }
    
                // consistent flags
                Assert.IsFalse(properties[i].IsIndexed && properties[i].IsMapped, failedMessage);
                if (properties[i].RequiresIndex)
                {
                    Assert.IsTrue(properties[i].IsIndexed, failedMessage);
                }
                if (properties[i].RequiresMapKey)
                {
                    Assert.IsTrue(properties[i].IsMapped, failedMessage);
                }
            }
    
            // assert same property names
            EPAssertionUtil.AssertEqualsAnyOrder(eventType.PropertyNames, propertyNames.ToArray());
        }
    
        private static void WriteIndent(TextWriter writer, int indent)
        {
            for (var i = 0; i < indent; i++)
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
                writer.Write(Convert.ToString(asArray.Length));
                writer.Write("{");
                var delimiter = "";
                for (var i = 0; i < asArray.Length; i++)
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
    
        public static void AssertEventTypeProperties(Object[][] expectedArr, EventType eventType, params EventTypeAssertionEnum[] assertions)
        {
            for (var propNum = 0; propNum < expectedArr.Length; propNum++) {
                var message = "Failed assertion for property " + propNum;
                var prop = eventType.PropertyDescriptors[propNum];
    
                for (var i = 0; i < assertions.Length; i++) {
                    var assertion = assertions[i];
                    var expected = expectedArr[propNum][i];
                    var value = assertion.GetExtractor().Invoke(prop, eventType);
                    Assert.AreEqual(expected, value, message + " at assertion " + assertion);
                }
            }
        }
    }
}
