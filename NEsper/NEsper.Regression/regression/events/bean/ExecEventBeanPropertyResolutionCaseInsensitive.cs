///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.bean
{
    public class ExecEventBeanPropertyResolutionCaseInsensitive : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
        }
    
        public override void Run(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select MYPROPERTY, myproperty, myProperty, MyProperty from " + typeof(SupportBeanDupProperty).FullName);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower"));
            EventBean result = listener.AssertOneGetNewAndReset();

            Assert.AreEqual(result.EventType.PropertyNames.Length, 4);
            Assert.AreEqual(result.Get("MYPROPERTY"), "upper");
            Assert.AreEqual(result.Get("MyProperty"), "uppercamel");
            Assert.AreEqual(result.Get("myProperty"), "lowercamel");
            Assert.AreEqual(result.Get("myproperty"), "lower");

            stmt = epService.EPAdministrator.CreateEPL("select " +
                    "NESTED.NESTEDVALUE as val1, " +
                    "ARRAYPROPERTY[0] as val2, " +
                    "MAPPED('keyOne') as val3, " +
                    "INDEXED[0] as val4 " +
                    " from " + typeof(SupportBeanComplexProps).FullName);
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("NestedValue", theEvent.Get("val1"));
            Assert.AreEqual(10, theEvent.Get("val2"));
            Assert.AreEqual("valueOne", theEvent.Get("val3"));
            Assert.AreEqual(1, theEvent.Get("val4"));
        }
    }
} // end of namespace
