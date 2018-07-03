///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;

using NEsper.Examples.MatchMaker.eventbean;
using NEsper.Examples.MatchMaker.monitor;

using NUnit.Framework;

namespace NEsper.Examples.MatchMaker
{
    [TestFixture]
    public class TestMatchMakingMonitor : IDisposable
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            var container = ContainerExtensions.CreateDefaultContainer()
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle =
                PropertyResolutionStyle.CASE_INSENSITIVE;

            _listener = new MatchAlertListener();
            EPServiceProviderManager.PurgeDefaultProvider();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();

            new MatchMakingMonitor(_epService, _listener);
        }

        public void TearDown()
        {
            _epService.Dispose();
        }

        #endregion

        private const int USER_ID_1 = 1;
        private const int USER_ID_2 = 2;

        private MatchAlertListener _listener;
        private EPServiceProvider _epService;


        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

        [Test]
        public void TestLocationChanges()
        {
            var user1 = new MobileUserBean(USER_ID_1, 10, 10,
                                            Gender.MALE, HairColor.BLONDE, AgeRange.AGE_4,
                                            Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_1);
            _epService.EPRuntime.SendEvent(user1);

            var user2 = new MobileUserBean(USER_ID_2, 10, 10,
                                            Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_1,
                                            Gender.MALE, HairColor.BLONDE, AgeRange.AGE_4);
            _epService.EPRuntime.SendEvent(user2);

            Assert.AreEqual(1, _listener.GetAndClearEmittedCount());

            user1.SetLocation(8.99999, 10);
            _epService.EPRuntime.SendEvent(user1);
            Assert.AreEqual(0, _listener.GetAndClearEmittedCount());

            user1.SetLocation(9, 10);
            _epService.EPRuntime.SendEvent(user1);
            Assert.AreEqual(1, _listener.GetAndClearEmittedCount());

            user1.SetLocation(11, 10);
            _epService.EPRuntime.SendEvent(user1);
            Assert.AreEqual(1, _listener.GetAndClearEmittedCount());

            user1.SetLocation(11.0000001, 10);
            _epService.EPRuntime.SendEvent(user1);
            Assert.AreEqual(0, _listener.GetAndClearEmittedCount());

            user2.SetLocation(10.0000001, 9);
            _epService.EPRuntime.SendEvent(user2);
            Assert.AreEqual(1, _listener.GetAndClearEmittedCount());
        }

        [Test]
        public void TestPreferredMatching()
        {
            var user1 = new MobileUserBean(USER_ID_1, 10, 10,
                                            Gender.MALE, HairColor.RED, AgeRange.AGE_6,
                                            Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_5);
            _epService.EPRuntime.SendEvent(user1);

            // Test all combinations
            foreach (Gender gender in Enum.GetValues(typeof (Gender))) {
                foreach (HairColor color in Enum.GetValues(typeof (HairColor))) {
                    foreach (AgeRange age in AgeRange.Values) {
                        // Try user preferences
                        var userA = new MobileUserBean(USER_ID_2, 10, 10,
                                                       Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_5,
                                                       gender, color, age);
                        _epService.EPRuntime.SendEvent(userA);

                        if (_listener.EmittedList.Count == 1) {
                            Assert.AreEqual(gender, Gender.MALE);
                            Assert.AreEqual(color, HairColor.RED);
                            Assert.AreEqual(age, AgeRange.AGE_6);
                            _listener.ClearEmitted();
                        }
                        else {
                            Assert.AreEqual(0, _listener.GetAndClearEmittedCount());
                        }
                    }
                }
            }
        }

        [Test]
        public void TestPreferredMatchingBackwards()
        {
            var user1 = new MobileUserBean(USER_ID_1, 10, 10,
                                            Gender.MALE, HairColor.RED, AgeRange.AGE_6,
                                            Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_5);
            _epService.EPRuntime.SendEvent(user1);

            // Test all combinations
            foreach (Gender gender in Enum.GetValues(typeof (Gender))) {
                foreach (HairColor color in Enum.GetValues(typeof (HairColor))) {
                    foreach (AgeRange age in AgeRange.Values) {
                        // Try user preferences backwards
                        var userB = new MobileUserBean(USER_ID_2, 10, 10,
                                                       gender, color, age,
                                                       Gender.MALE, HairColor.RED, AgeRange.AGE_6);
                        _epService.EPRuntime.SendEvent(userB);

                        if (_listener.EmittedList.Count == 1) {
                            Assert.AreEqual(gender, Gender.FEMALE);
                            Assert.AreEqual(color, HairColor.BLACK);
                            Assert.AreEqual(age, AgeRange.AGE_5);
                            _listener.ClearEmitted();
                        }
                        else {
                            Assert.AreEqual(0, _listener.GetAndClearEmittedCount());
                        }
                    }
                }
            }
        }
    }
}
