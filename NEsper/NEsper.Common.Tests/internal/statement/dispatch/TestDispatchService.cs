///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.statement.dispatch
{
    [TestFixture]
    public class TestDispatchService : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            service = new DispatchService();
        }

        private DispatchService service;

        [Test, RunInApplicationDomain]
        public void TestAdd()
        {
            var dispatchables = new SupportDispatchable[2];
            for (var i = 0; i < dispatchables.Length; i++)
            {
                dispatchables[i] = new SupportDispatchable();
            }

            SupportDispatchable.GetAndResetInstanceList();

            service.AddExternal(dispatchables[0]);
            service.AddExternal(dispatchables[1]);

            service.Dispatch();

            var dispatchList = SupportDispatchable.GetAndResetInstanceList();
            Assert.AreSame(dispatchables[0], dispatchList[0]);
            Assert.AreSame(dispatchables[1], dispatchList[1]);
        }

        [Test, RunInApplicationDomain]
        public void TestAddAndDispatch()
        {
            // Dispatch without work to do, should complete
            service.Dispatch();

            var disOne = new SupportDispatchable();
            var disTwo = new SupportDispatchable();
            service.AddExternal(disOne);
            service.AddExternal(disTwo);

            Assert.AreEqual(0, disOne.GetAndResetNumExecuted());
            Assert.AreEqual(0, disTwo.GetAndResetNumExecuted());

            service.Dispatch();

            service.AddExternal(disTwo);
            Assert.AreEqual(1, disOne.GetAndResetNumExecuted());
            Assert.AreEqual(1, disTwo.GetAndResetNumExecuted());

            service.Dispatch();
            Assert.AreEqual(0, disOne.GetAndResetNumExecuted());
            Assert.AreEqual(1, disTwo.GetAndResetNumExecuted());
        }

        [Test, RunInApplicationDomain]
        public void TestAddDispatchTwice()
        {
            var disOne = new SupportDispatchable();
            service.AddExternal(disOne);

            service.Dispatch();
            Assert.AreEqual(1, disOne.GetAndResetNumExecuted());

            service.Dispatch();
            Assert.AreEqual(0, disOne.GetAndResetNumExecuted());
        }

        public class SupportDispatchable : Dispatchable
        {
            private static IList<SupportDispatchable> instanceList = new List<SupportDispatchable>();
            private int numExecuted;

            public void Execute()
            {
                numExecuted++;
                instanceList.Add(this);
            }

            public int GetAndResetNumExecuted()
            {
                var val = numExecuted;
                numExecuted = 0;
                return val;
            }

            public static IList<SupportDispatchable> GetAndResetInstanceList()
            {
                var instances = instanceList;
                instanceList = new List<SupportDispatchable>();
                return instances;
            }

            public UpdateDispatchView View => throw new NotSupportedException();

            public void Cancelled()
            {
                throw new NotSupportedException();
            }
        }
    }
} // end of namespace
