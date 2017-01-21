///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.service;

using NUnit.Framework;

namespace com.espertech.esper.epl.spec.util
{
    [TestFixture]
    public class TestStatementSpecAnalyzer
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            var config = new Configuration();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
            _engine = EPServiceProviderManager.GetDefaultProvider(config);
        }
    
        [Test]
        public void TestFilterWalker()
        {
            _engine.EPAdministrator.CreateEPL("create schema A1 as (col1 string)");
    
            Assert.AreEqual(1, GetFilters("select * from A1").Count);
            Assert.AreEqual(2, GetFilters("select * from A1.std:lastevent(), A1.std:lastevent()").Count);
            Assert.AreEqual(2, GetFilters("select (select col1 from A1.std:lastevent()), col1 from A1.std:lastevent()").Count);
            Assert.AreEqual(2, GetFilters("select * from pattern [A1 -> A1(col1='a')]").Count);
        }
    
        private IList<FilterSpecRaw> GetFilters(string epl)
        {
            var spi = (EPAdministratorSPI) _engine.EPAdministrator;
            var raw = spi.CompileEPLToRaw(epl);
            var filters = StatementSpecRawAnalyzer.AnalyzeFilters(raw);
            return filters;
        }
    
    }
}
