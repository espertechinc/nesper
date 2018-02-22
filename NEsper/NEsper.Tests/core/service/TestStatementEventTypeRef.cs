///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.core.service
{
    [TestFixture]
    public class TestStatementEventTypeRef 
    {
        private StatementEventTypeRefImpl _service;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _service = new StatementEventTypeRefImpl(_container.Resolve<IReaderWriterLockManager>());
        }
    
        [Test]
        public void TestFlowNoRemoveType()
        {
            AddReference("s0", "e1");
            Assert.IsTrue(_service.IsInUse("e1"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e1").ToArray(), new string[]{"s0"});
    
            AddReference("s0", "e2");
            Assert.IsTrue(_service.IsInUse("e2"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e2").ToArray(), new string[] { "s0" });
    
            AddReference("s1", "e1");
            Assert.IsTrue(_service.IsInUse("e1"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e1").ToArray(), new string[] { "s0", "s1" });
    
            AddReference("s1", "e1");
            Assert.IsTrue(_service.IsInUse("e1"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e1").ToArray(), new string[] { "s0", "s1" });
    
            Assert.IsFalse(_service.IsInUse("e3"));
            AddReference("s2", "e3");
            Assert.IsTrue(_service.IsInUse("e3"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e3").ToArray(), new string[] { "s2" });
    
            _service.RemoveReferencesStatement("s2");
            Assert.IsFalse(_service.IsInUse("e3"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e3").ToArray(), new string[0]);
    
            _service.RemoveReferencesStatement("s0");
            Assert.IsTrue(_service.IsInUse("e1"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e1").ToArray(), new string[] { "s1" });
    
            _service.RemoveReferencesStatement("s1");
            Assert.IsFalse(_service.IsInUse("e1"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e1").ToArray(), new string[0]);
    
            var values = new HashSet<String> { "e5", "e6" };
            _service.AddReferences("s4", CollectionUtil.ToArray(values));
    
            Assert.IsTrue(_service.IsInUse("e5"));
            Assert.IsTrue(_service.IsInUse("e6"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e5").ToArray(), new string[] { "s4" });
    
            _service.RemoveReferencesStatement("s4");
    
            Assert.IsFalse(_service.IsInUse("e5"));
            Assert.IsFalse(_service.IsInUse("e6"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e5").ToArray(), new string[0]);
    
            Assert.AreEqual(0, _service.TypeToStmt.Count);
            Assert.AreEqual(0, _service.TypeToStmt.Count);
        }
    
        [Test]
        public void TestFlowRemoveType()
        {
            AddReference("s0", "e1");
            AddReference("s1", "e1");
            AddReference("s2", "e2");
    
            Assert.IsTrue(_service.IsInUse("e1"));
            _service.RemoveReferencesType("e1");
            Assert.IsFalse(_service.IsInUse("e1"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e1").ToArray(), new string[0]);
    
            Assert.IsTrue(_service.IsInUse("e2"));
            _service.RemoveReferencesType("e2");
            Assert.IsFalse(_service.IsInUse("e2"));
    
            _service.RemoveReferencesType("e3");
    
            Assert.AreEqual(0, _service.TypeToStmt.Count);
            Assert.AreEqual(0, _service.TypeToStmt.Count);
        }
    
        [Test]
        public void TestInvalid()
        {
            _service.RemoveReferencesStatement("s1");
    
            AddReference("s2", "e2");
    
            Assert.IsTrue(_service.IsInUse("e2"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e2").ToArray(), new string[] { "s2" });
    
            _service.RemoveReferencesStatement("s2");
    
            Assert.IsFalse(_service.IsInUse("e2"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e2").ToArray(), new string[0]);
    
            _service.RemoveReferencesStatement("s2");
    
            Assert.IsFalse(_service.IsInUse("e2"));
            EPAssertionUtil.AssertEqualsAnyOrder(_service.GetStatementNamesForType("e2").ToArray(), new string[0]);
    
            Assert.AreEqual(0, _service.TypeToStmt.Count);
            Assert.AreEqual(0, _service.TypeToStmt.Count);
        }
    
        private void AddReference(String stmtName, String typeName)
        {
            var set = new HashSet<String> { typeName };
            _service.AddReferences(stmtName, CollectionUtil.ToArray(set));
        }
    }
}
