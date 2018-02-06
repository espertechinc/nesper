///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.subscriber
{
    public abstract class SupportSubscriberNoParamsBase : SupportSubscriberBase
    {
        private bool _called;

        protected SupportSubscriberNoParamsBase(bool requiresStatementDelivery)
            : base(requiresStatementDelivery)
        {
        }

        public void AddCalled()
        {
            _called = true;
        }

        public void AddCalled(EPStatement stmt)
        {
            _called = true;
            AddStmtIndication(stmt);
        }

        public void AssertCalledAndReset(EPStatement stmt)
        {
            AssertStmtOneReceived(stmt);
            Assert.IsTrue(_called);
            _called = false;
            ResetStmts();
        }
    }
} // end of namespace
