///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.dispatch;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.dispatch
{
    [TestFixture]
    public class TestDispatchServiceImpl 
    {
        private DispatchServiceImpl _service;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _service = new DispatchServiceImpl(_container.ThreadLocalManager());
        }
    
        [Test]
        public void TestAddAndDispatch()
        {
            // Dispatch without work to do, should complete
            _service.Dispatch();
    
            SupportDispatchable disOne = new SupportDispatchable();
            SupportDispatchable disTwo = new SupportDispatchable();
            _service.AddExternal(disOne);
            _service.AddExternal(disTwo);
    
            Assert.AreEqual(0, disOne.GetAndResetNumExecuted());
            Assert.AreEqual(0, disTwo.GetAndResetNumExecuted());
    
            _service.Dispatch();
    
            _service.AddExternal(disTwo);
            Assert.AreEqual(1, disOne.GetAndResetNumExecuted());
            Assert.AreEqual(1, disTwo.GetAndResetNumExecuted());
    
            _service.Dispatch();
            Assert.AreEqual(0, disOne.GetAndResetNumExecuted());
            Assert.AreEqual(1, disTwo.GetAndResetNumExecuted());
        }
    
        [Test]
        public void TestAddDispatchTwice()
        {
            SupportDispatchable disOne = new SupportDispatchable();
            _service.AddExternal(disOne);
    
            _service.Dispatch();
            Assert.AreEqual(1, disOne.GetAndResetNumExecuted());
    
            _service.Dispatch();
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
    
            _service.AddExternal(dispatchables[0]);
            _service.AddExternal(dispatchables[1]);
    
            _service.Dispatch();
    
            IList<SupportDispatchable> dispatchList = SupportDispatchable.GetAndResetInstanceList();
            Assert.AreSame(dispatchables[0], dispatchList[0]);
            Assert.AreSame(dispatchables[1], dispatchList[1]);
        }
    }
}
