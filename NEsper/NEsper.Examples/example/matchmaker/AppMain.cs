///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logger;
using com.espertech.esper.compat.logging;

using NEsper.Examples.MatchMaker.eventbean;
using NEsper.Examples.MatchMaker.monitor;

namespace NEsper.Examples.MatchMaker
{
    public class AppMain
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _engineURI;
        private readonly bool _continuousSimulation;

        public static void Main(string[] args)
        {
            LoggerNLog.BasicConfig();
            LoggerNLog.Register();

            new AppMain("MatchMaker", false).Run();
        }

        public AppMain(string engineURI, bool continuousSimulation)
        {
            _engineURI = engineURI;
            _continuousSimulation = continuousSimulation;
        }

        public void Run()
        {
            Log.Info("Setting up EPL");

            var container = ContainerExtensions.CreateDefaultContainer()
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            // This code runs as part of the automated regression test suite; Therefore disable internal timer theading to safe resources
            var config = new Configuration(container);
            config.EngineDefaults.Threading.IsInternalTimerEnabled = false;

            var listener = new MatchAlertListener();
            var epService = EPServiceProviderManager.GetProvider(container, _engineURI, config);
            epService.Initialize();

            new MatchMakingMonitor(epService, listener);

            Log.Info("Sending user information");
            var user1 = new MobileUserBean(1, 10, 10,
                    Gender.MALE, HairColor.BLONDE, AgeRange.AGE_4,
                    Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_1);
            epService.EPRuntime.SendEvent(user1);

            var user2 = new MobileUserBean(2, 10, 10,
                    Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_1,
                    Gender.MALE, HairColor.BLONDE, AgeRange.AGE_4);
            epService.EPRuntime.SendEvent(user2);

            Log.Info("Sending some near locations");
            user1.SetLocation(8.99999, 10);
            epService.EPRuntime.SendEvent(user1);

            user1.SetLocation(9, 10);
            epService.EPRuntime.SendEvent(user1);

            user1.SetLocation(11, 10);
            epService.EPRuntime.SendEvent(user1);

            user1.SetLocation(11.0000001, 10);
            epService.EPRuntime.SendEvent(user1);

            user2.SetLocation(10.0000001, 9);
            epService.EPRuntime.SendEvent(user2);

            user1 = new MobileUserBean(1, 10, 10,
                    Gender.MALE, HairColor.RED, AgeRange.AGE_6,
                    Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_5);
            epService.EPRuntime.SendEvent(user1);

            // Test all combinations
            foreach (var gender in EnumHelper.GetValues<Gender>())
            {
                foreach (var color in EnumHelper.GetValues<HairColor>())
                {
                    foreach (var age in EnumHelper.GetValues<AgeRange>())
                    {
                        // Try user preferences
                        var userA = new MobileUserBean(2, 10, 10,
                                Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_5,
                                gender, color, age);
                        epService.EPRuntime.SendEvent(userA);

                    }
                }
            }
        
            var random = new Random();
            int maxEvents;
            if (_continuousSimulation) {
                maxEvents = int.MaxValue;
            }
            else {
                maxEvents = 100000;
                Log.Info("Sending 100k of random locations");
            }

            for (var i = 1; i < maxEvents; i++)
            {
                var x = 10 + random.Next(i) / 100000;
                var y = 10 + random.Next(i) / 100000;

                user2.SetLocation(x, y);
                epService.EPRuntime.SendEvent(user2);

                if (_continuousSimulation) {
                    try {
                        Thread.Sleep(200);
                    } catch (ThreadInterruptedException e) {
                        Log.Debug("Interrupted", e);
                    }
                }
            }        

            Log.Info("Done.");
        }
    }
}
