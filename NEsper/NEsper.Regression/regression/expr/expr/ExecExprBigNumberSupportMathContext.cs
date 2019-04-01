///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprBigNumberSupportMathContext : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.Expression.MathContext = MathContext.DECIMAL32;
        }

        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            RunAssertionMathContextDivide(epService);
        }

        private void RunAssertionMathContextDivide(EPServiceProvider epService)
        {
            // cast and divide
            var stmtDivide = epService.EPAdministrator.CreateEPL("select cast(1.6, decimal) / cast(9.2, decimal) from SupportBean");
            stmtDivide.Subscriber = new Action<decimal>(
                value => Assert.AreEqual(0.1739130d, value));

            epService.EPRuntime.SendEvent(new SupportBean());
        }
    }
} // end of namespace
