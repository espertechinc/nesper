///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    public class ExecEventMapNestedConfigRuntime : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("NestedMap", TestDefinition);
            RunAssertion(epService);
        }
    
        public static void RunAssertion(EPServiceProvider epService) {
            string statementText = "select nested as a, " +
                    "nested.n1 as b," +
                    "nested.n2 as c," +
                    "nested.n2.n1n1 as d " +
                    "from NestedMap#length(5)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            IDictionary<string, Object> mapEvent = TestData;
            epService.EPRuntime.SendEvent(mapEvent, "NestedMap");
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreSame(mapEvent.Get("nested"), theEvent.Get("a"));
            Assert.AreSame("abc", theEvent.Get("b"));
            Assert.AreSame(((IDictionary<string, object>) mapEvent.Get("nested")).Get("n2"), theEvent.Get("c"));
            Assert.AreSame("def", theEvent.Get("d"));
            statement.Stop();
        }

        public static IDictionary<string, object> TestDefinition {
            get {
                var propertiesNestedNested = new Dictionary<string, Object>();
                propertiesNestedNested.Put("n1n1", typeof(string));

                var propertiesNested = new Dictionary<string, Object>();
                propertiesNested.Put("n1", typeof(string));
                propertiesNested.Put("n2", propertiesNestedNested);

                var root = new Dictionary<string, Object>();
                root.Put("nested", propertiesNested);

                return root;
            }
        }

        public static IDictionary<string, object> TestData {
            get {
                var nestedNested = new Dictionary<string, Object>();
                nestedNested.Put("n1n1", "def");

                var nested = new Dictionary<string, Object>();
                nested.Put("n1", "abc");
                nested.Put("n2", nestedNested);

                var map = new Dictionary<string, Object>();
                map.Put("nested", nested);

                return map;
            }
        }
    }
} // end of namespace
