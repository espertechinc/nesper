///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.client.configuration
{
    [TestFixture]
    public class TestConfigurationSchema7To8Upgrade : AbstractTestBase
    {
        private static readonly string FILE_PREFIX = "regression/esper_version_7_old_configuration_file_";
        private static readonly string FILE_ONE = FILE_PREFIX + "one.xml";
        private static readonly string FILE_TWO = FILE_PREFIX + "two.xml";
        private static readonly string FILE_THREE = FILE_PREFIX + "three.xml";

        private void RunAssertion(string file)
        {
            using (var stream = container.ResourceManager().GetResourceAsStream(file)) {
                var result = ConfigurationSchema7To8Upgrade.Upgrade(stream, file);
                Assert.That(result, Is.Not.Null);
                Console.WriteLine(result);
            }
        }

        [Test]
        public void TestIt()
        {
            RunAssertion(FILE_ONE);
            RunAssertion(FILE_TWO);
            RunAssertion(FILE_THREE);
        }
    }
} // end of namespace