///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.support.dispatch;

using NUnit.Framework;

namespace com.espertech.esper.dispatch
{
    [TestFixture]
    public class TestDispatchServiceImpl 
    {
        private DispatchServiceImpl service;
    
        [SetUp]
        public void SetUp()
        {
            service = new DispatchServiceImpl();
        }
    
        [Test]
        public void TestAddAndDispatch()
        {
            // Dispatch without work to do, should complete
            service.Dispatch();
    
            SupportDispatchable disOne = new SupportDispatchable();
            SupportDispatchable disTwo = new SupportDispatchable();
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
    
        [Test]
        public void TestAddDispatchTwice()
        {
            SupportDispatchable disOne = new SupportDispatchable();
            service.AddExternal(disOne);
    
            service.Dispatch();
            Assert.AreEqual(1, disOne.GetAndResetNumExecuted());
    
            service.Dispatch();
            Assert.AreEqual(0, disOne.GetAndResetNumExecuted());
        }
    
        [Test]
        public void TestAdd()
        {
            SupportDispatchable[] dispatchables = new SupportDispatchable[2];
            for (int i = 0; i < dispatchables.Length; i++)
            {
                dispatchables[i] = new SupportDispatchable();
            }
            SupportDispatchable.GetAndResetInstanceList();
    
            service.AddExternal(dispatchables[0]);
            service.AddExternal(dispatchables[1]);
    
            service.Dispatch();
    
            IList<SupportDispatchable> dispatchList = SupportDispatchable.GetAndResetInstanceList();
            Assert.AreSame(dispatchables[0], dispatchList[0]);
            Assert.AreSame(dispatchables[1], dispatchList[1]);
        }
    }
}
