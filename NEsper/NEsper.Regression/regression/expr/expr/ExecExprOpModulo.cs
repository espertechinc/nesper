///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprOpModulo : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            var epl = "select LongBoxed % IntBoxed as myMod " +
                      " from " + typeof(SupportBean).FullName + "#length(3) where Not(LongBoxed > IntBoxed)";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEvent(epService, 1, 1, 0);
            Assert.AreEqual(0L, listener.LastNewData[0].Get("myMod"));
            listener.Reset();

            SendEvent(epService, 2, 1, 0);
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendEvent(epService, 2, 3, 0);
            Assert.AreEqual(2L, listener.LastNewData[0].Get("myMod"));
            listener.Reset();
        }

        private void SendEvent(EPServiceProvider epService, long longBoxed, int intBoxed, short shortBoxed)
        {
            SendBoxedEvent(epService, longBoxed, intBoxed, shortBoxed);
        }

        private void SendBoxedEvent(EPServiceProvider epService, long longBoxed, int? intBoxed, short? shortBoxed)
        {
            var bean = new SupportBean();
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace