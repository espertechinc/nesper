///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestExprAreSerializable
    {
        [Test]
        public void TestAllClassesAreSerializable()
        {
            var typeList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOrImplementsInterface<ExprNode>())
                .Where(type => type.IsClass)
                .Where(type => type.IsAbstract == false)
                .ToList();

            var nonSerializableTypes = typeList
                .Where(type => type.IsSerializable == false)
                .ToList();

            typeList.ForEach(type => Assert.That(type.IsSerializable, string.Format("{0} is not serializable / {1}", type.FullName, nonSerializableTypes.Render())));
        }
    }
}