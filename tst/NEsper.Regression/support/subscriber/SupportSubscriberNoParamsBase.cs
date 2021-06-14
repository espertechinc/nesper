///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public abstract class SupportSubscriberNoParamsBase : SupportSubscriberBase
    {
        private bool called;

        protected SupportSubscriberNoParamsBase(bool requiresStatementDelivery) : base(requiresStatementDelivery)
        {
        }

        public void AddCalled()
        {
            called = true;
        }

        public void AddCalled(EPStatement stmt)
        {
            called = true;
            AddStmtIndication(stmt);
        }

        public void AssertCalledAndReset(EPStatement stmt)
        {
            AssertStmtOneReceived(stmt);
            Assert.IsTrue(called);
            called = false;
            ResetStmts();
        }
    }
} // end of namespace