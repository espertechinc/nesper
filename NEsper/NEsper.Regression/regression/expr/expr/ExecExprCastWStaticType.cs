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

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprCastWStaticType : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            var map = new Dictionary<string, Object>();
            map.Put("anInt", typeof(string));
            map.Put("anDouble", typeof(string));
            map.Put("anLong", typeof(string));
            map.Put("anFloat", typeof(string));
            map.Put("anByte", typeof(string));
            map.Put("anShort", typeof(string));
            map.Put("IntPrimitive", typeof(int));
            map.Put("IntBoxed", typeof(int?));
            configuration.AddEventType("TestEvent", map);
        }
    
        public override void Run(EPServiceProvider epService) {
    
            string stmt = "select cast(anInt, int) as intVal, " +
                    "cast(anDouble, double) as doubleVal, " +
                    "cast(anLong, long) as longVal, " +
                    "cast(anFloat, float) as floatVal, " +
                    "cast(anByte, byte) as byteVal, " +
                    "cast(anShort, short) as shortVal, " +
                    "cast(IntPrimitive, int) as intOne, " +
                    "cast(IntBoxed, int) as intTwo, " +
                    "cast(IntPrimitive, " + Name.Clean<long>(false) + ") as longOne, " +
                    "cast(IntBoxed, long) as longTwo " +
                    "from TestEvent";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmt);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            var map = new Dictionary<string, Object>();
            map.Put("anInt", "100");
            map.Put("anDouble", "1.4E-1");
            map.Put("anLong", "-10");
            map.Put("anFloat", "1.001");
            map.Put("anByte", "0x0A");
            map.Put("anShort", "223");
            map.Put("IntPrimitive", 10);
            map.Put("IntBoxed", 11);
    
            epService.EPRuntime.SendEvent(map, "TestEvent");
            EventBean row = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(100, row.Get("intVal"));
            Assert.AreEqual(0.14d, row.Get("doubleVal"));
            Assert.AreEqual(-10L, row.Get("longVal"));
            Assert.AreEqual(1.001f, row.Get("floatVal"));
            Assert.AreEqual((byte) 10, row.Get("byteVal"));
            Assert.AreEqual((short) 223, row.Get("shortVal"));
            Assert.AreEqual(10, row.Get("intOne"));
            Assert.AreEqual(11, row.Get("intTwo"));
            Assert.AreEqual(10L, row.Get("longOne"));
            Assert.AreEqual(11L, row.Get("longTwo"));
        }
    }
} // end of namespace
