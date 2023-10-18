///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateFilteredWMathContext : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@name('s0') select avg(DecimalOne) as c0 from SupportBeanNumeric";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBeanNumeric(null, MakeDecimal(0, 2, MidpointRounding.AwayFromZero)));
            env.SendEventBean(new SupportBeanNumeric(null, MakeDecimal(0, 2, MidpointRounding.AwayFromZero)));
            env.SendEventBean(new SupportBeanNumeric(null, MakeDecimal(1, 2, MidpointRounding.AwayFromZero)));
            env.AssertListener("s0", listener => Assert.AreEqual(0.33m, listener.GetAndResetLastNewData()[0].Get("c0").AsDecimal()));

            env.UndeployAll();
        }

        private decimal MakeDecimal(
            int value,
            int scale,
            MidpointRounding rounding)
        {
            return Math.Round(new decimal(value), scale, rounding);
        }
    }
} // end of namespace