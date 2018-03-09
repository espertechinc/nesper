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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateFilteredWMathContext : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Expression.MathContext = new MathContext(MidpointRounding.AwayFromZero, 2);
            configuration.AddEventType(typeof(SupportBeanNumeric));
        }
    
        public override void Run(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select avg(DecimalOne) as c0 from SupportBeanNumeric").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, MakeDecimal(0, 2, MidpointRounding.AwayFromZero)));
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, MakeDecimal(0, 2, MidpointRounding.AwayFromZero)));
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, MakeDecimal(1, 2, MidpointRounding.AwayFromZero)));
            Assert.AreEqual(0.33m, listener.GetAndResetLastNewData()[0].Get("c0").AsDecimal());
        }
    
        private decimal MakeDecimal(int value, int scale, MidpointRounding rounding)
        {
            return Math.Round(new decimal(value), scale, MidpointRounding.AwayFromZero);
        }
    }
} // end of namespace
