///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.logging;

namespace NEsper.Examples.RSI
{
    public class AppMain
    {
        public static void Main()
        {
            LoggerNLog.BasicConfig();
            LoggerNLog.Register();

            using (var testRSI = new TestRSI()) {
                testRSI.SetUp();
                testRSI.TestFlow();
            }
        }
    }
}
