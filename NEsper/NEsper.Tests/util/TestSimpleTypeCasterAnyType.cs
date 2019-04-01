///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.supportunit.bean;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestSimpleTypeCasterAnyType 
    {
        private SimpleTypeCaster caster;
    
        [SetUp]
        public void SetUp()
        {
            caster = SimpleTypeCasterFactory.GetCaster(typeof(ISupportA));
            //caster = new SimpleTypeCasterAnyType(typeof(ISupportA));
        }
    
        [Test]
        public void TestCast()
        {
            Assert.IsNull(caster.Invoke(new Object()));
            Assert.IsNull(caster.Invoke(new SupportBean()));
            Assert.IsNotNull(caster.Invoke(new ISupportABCImpl("","","","")));
            Assert.IsNotNull(caster.Invoke(new ISupportABCImpl("","","","")));
            Assert.IsNull(caster.Invoke(new ISupportBCImpl("","","")));
        }
    }
}
