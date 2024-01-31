///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestSimpleTypeCasterAnyType : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            caster = new SimpleTypeCasterAnyType(typeof(ISupportA));
        }

        private SimpleTypeCasterAnyType caster;

        [Test]
        public void TestCast()
        {
            ClassicAssert.IsNull(caster.Cast(new object()));
            ClassicAssert.IsNull(caster.Cast(new SupportBean()));
            ClassicAssert.IsNotNull(caster.Cast(new ISupportABCImpl("", "", "", "")));
            ClassicAssert.IsNotNull(caster.Cast(new ISupportABCImpl("", "", "", "")));
            ClassicAssert.IsNull(caster.Cast(new ISupportBCImpl("", "", "")));
        }
    }
} // end of namespace
