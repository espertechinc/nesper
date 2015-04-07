///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.client.scopetest;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.support.epl.join;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.exec
{
    [TestFixture]
    public class TestLookupInstructionExec 
    {
        private LookupInstructionExec _exec;
        private SupportRepositoryImpl _rep;
        private JoinExecTableLookupStrategy[] _lookupStrategies;
    
        [SetUp]
        public void SetUp()
        {
            _lookupStrategies = new JoinExecTableLookupStrategy[4];
            for (int i = 0; i < _lookupStrategies.Length; i++)
            {
                _lookupStrategies[i] = new SupportTableLookupStrategy(1);
            }
    
            _exec = new LookupInstructionExec(0, "test",
                    new[] {1, 2, 3, 4}, _lookupStrategies, new[] {false, true, true, false, false});
    
            _rep = new SupportRepositoryImpl();
        }
    
        [Test]
        public void TestProcessAllResults()
        {
            bool result = _exec.Process(_rep, null);
    
            Assert.IsTrue(result);
            Assert.AreEqual(4, _rep.LookupResultsList.Count);
            EPAssertionUtil.AssertEqualsExactOrder(new int?[] { 1, 2, 3, 4 }, _rep.ResultStreamList.ToArray());
        }
    
        [Test]
        public void TestProcessNoRequiredResults()
        {
            _lookupStrategies[1] = new SupportTableLookupStrategy(0);
    
            bool result = _exec.Process(_rep, null);
    
            Assert.IsFalse(result);
            Assert.AreEqual(0, _rep.LookupResultsList.Count);
        }
    
        [Test]
        public void TestProcessPartialOptionalResults()
        {
            _lookupStrategies[3] = new SupportTableLookupStrategy(0);
    
            bool result = _exec.Process(_rep, null);
    
            Assert.IsTrue(result);
            Assert.AreEqual(3, _rep.LookupResultsList.Count);
        }
    }
}
