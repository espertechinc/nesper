///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyResolutionCaseInsensitive : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy(
                "@name('s0') select MYPROPERTY, myproperty, myProperty, MyProperty from SupportBeanDupProperty");
            env.AddListener("s0");

            env.SendEventBean(new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower"));
            env.AssertEventNew(
                "s0",
                result => {
                    Assert.AreEqual("upper", result.Get("MYPROPERTY"));
                    Assert.AreEqual("lower", result.Get("myproperty"));
                    Assert.AreEqual("lowercamel", result.Get("myProperty"));
                    Assert.AreEqual("uppercamel", result.Get("MyProperty"));
                });

            env.UndeployAll();

            env.CompileDeploy(
                    "@name('s0') select " +
                    "NESTED.NESTEDVALUE as val1, " +
                    "ARRAYPROPERTY[0] as val2, " +
                    "MAPPED('keyOne') as val3, " +
                    "INDEXED[0] as val4 " +
                    " from SupportBeanComplexProps")
                .AddListener("s0");

            env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
            env.AssertEventNew(
                "s0",
                theEvent => {
                    Assert.AreEqual("NestedValue", theEvent.Get("val1"));
                    Assert.AreEqual(10, theEvent.Get("val2"));
                    Assert.AreEqual("valueOne", theEvent.Get("val3"));
                    Assert.AreEqual(1, theEvent.Get("val4"));
                });

            env.UndeployAll();
        }
    }
} // end of namespace