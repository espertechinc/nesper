///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.logger;

using NUnit.Framework;

namespace com.espertech.esper
{
    using Directory = System.IO.Directory;

    [SetUpFixture]
    public class NEsperSetup
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            var dir = TestContext.CurrentContext.TestDirectory;
            if (dir != null)
            {
                Environment.CurrentDirectory = dir;
                Directory.SetCurrentDirectory(dir);
            }

            var logConfig = LoggerNLog.BasicConfig();
            LoggerNLog.ResetConfig(logConfig);
            LoggerNLog.Register();
        }
    }
}
