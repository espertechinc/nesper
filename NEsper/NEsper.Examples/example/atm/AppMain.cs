///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using log4net.Config;

namespace com.espertech.esper.example.atm
{
    public class AppMain
    {
        public static void Main()
        {
            XmlConfigurator.Configure();

            using (FraudMonitorTest fraudMonitorTest = new FraudMonitorTest()) {
                fraudMonitorTest.SetUp();
                fraudMonitorTest.TestJoin();
            }
        }
    }
}
