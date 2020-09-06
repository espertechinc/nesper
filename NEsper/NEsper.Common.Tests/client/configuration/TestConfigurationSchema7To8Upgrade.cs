///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Net;

using com.espertech.esper.container;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace com.espertech.esper.common.client.configuration
{
    // this test is not applicable as we never release NEsper 7

    //[TestFixture]
    public class TestConfigurationSchema7To8Upgrade : AbstractCommonTest
    {
        private static readonly string FILE_PREFIX = "regression/esper_version_7_old_configuration_file_";
        private static readonly string FILE_ONE = FILE_PREFIX + "one.xml";
        private static readonly string FILE_TWO = FILE_PREFIX + "two.xml";
        private static readonly string FILE_THREE = FILE_PREFIX + "three.xml";

        private void RunAssertion(string file)
        {
            var url = container.ResourceManager().ResolveResourceURL(file);
            using (var client = new WebClient())
            {
                using (var stream = client.OpenRead(url)) {
                    var result = ConfigurationSchema7To8Upgrade.Upgrade(stream, file);
                    Assert.That(result, Is.Not.Null);
                    Console.WriteLine(result);
                }
            }
        }

        //[Test, RunInApplicationDomain]
        public void TestIt()
        {
            RunAssertion(FILE_ONE);
            RunAssertion(FILE_TWO);
            RunAssertion(FILE_THREE);
        }
    }
} // end of namespace
