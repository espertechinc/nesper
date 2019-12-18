///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.type;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.basic
{
    public class ClientBasicAnnotation : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('abc') @Tag(Name='a', Value='b') @Priority(1) @Drop select * from SupportBean";
            env.CompileDeployAddListenerMileZero(epl, "abc");

            var annotations = env.Statement("abc").Annotations;

            Assert.AreEqual(typeof(AnnotationName), annotations[0].GetType());
            Assert.AreEqual("abc", ((AnnotationName) annotations[0]).Value);

            env.UndeployAll();
        }
    }
} // end of namespace