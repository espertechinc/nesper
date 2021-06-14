///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.eventtyperepo
{
    public class TestEventTypeRepositoryUtil : AbstractCommonTest
    {
        [Test]
        public void TestSort()
        {
            var setOne = new LinkedHashSet<string>();
            setOne.Add("a");
            setOne.Add("b_sub");
            var setTwo = new LinkedHashSet<string>();
            setOne.Add("b_super");
            setOne.Add("y");

            var configs = new Dictionary<string, ConfigurationCommonEventTypeWithSupertype>();
            ConfigurationCommonEventTypeWithSupertype config = new ConfigurationCommonEventTypeWithSupertype();
            config.SuperTypes = Collections.SingletonSet("b_super");
            configs["b_sub"] = config;

            var result = EventTypeRepositoryUtil.GetCreationOrder(setOne, setTwo, configs);
            Assert.AreEqual("[\"a\", \"b_super\", \"y\", \"b_sub\"]", result.RenderAny());
        }
    }
} // end of namespace
