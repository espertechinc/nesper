///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using NEsper.Examples.MatchMaker.eventbean;
using NEsper.Examples.MatchMaker.monitor;

using NUnit.Framework;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

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
            configuration.Common.EventMeta.ClassPropertyResolutionStyle =
                PropertyResolutionStyle.CASE_INSENSITIVE;

            _listener = new MatchAlertListener();
            EPRuntimeProvider
                .GetDefaultRuntime()
                .DeploymentService
                .UndeployAll();

            _runtime = EPRuntimeProvider.GetDefaultRuntime(configuration);
            _runtime.Initialize();

            new MatchMakingMonitor(_runtime, _listener);
        }

        public void TearDown()
        {
            _runtime.Destroy();
        }

        #endregion

        private const string EVENTTYPE = "MobileUserBean";
        private const int USER_ID_1 = 1;
        private const int USER_ID_2 = 2;

        private MatchAlertListener _listener;
        private EPRuntime _runtime;


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

        [Test]
        public void TestLocationChanges()
        {
            var sender = _runtime.EventService.GetEventSender(typeof(MobileUserBean).Name);
            
            var user1 = new MobileUserBean(USER_ID_1, 10, 10,
                                            Gender.MALE, HairColor.BLONDE, AgeRange.AGE_4,
                                            Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_1);
            sender.SendEvent(user1);

            var user2 = new MobileUserBean(USER_ID_2, 10, 10,
                                            Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_1,
                                            Gender.MALE, HairColor.BLONDE, AgeRange.AGE_4);
            sender.SendEvent(user2);

            Assert.AreEqual(1, _listener.GetAndClearEmittedCount());
            _runtime.EventService.SendEventBean(user1.Copy().WithLocation(8.99999, 10), EVENTTYPE);
            Assert.AreEqual(0, _listener.GetAndClearEmittedCount());

            _runtime.EventService.SendEventBean(user1.Copy().WithLocation(9, 10), EVENTTYPE);
            Assert.AreEqual(1, _listener.GetAndClearEmittedCount());

            _runtime.EventService.SendEventBean(user1.Copy().WithLocation(11, 10), EVENTTYPE);
            Assert.AreEqual(1, _listener.GetAndClearEmittedCount());

            _runtime.EventService.SendEventBean(user1.Copy().WithLocation(11.0000001, 10), EVENTTYPE);
            Assert.AreEqual(0, _listener.GetAndClearEmittedCount());

            _runtime.EventService.SendEventBean(user2.Copy().WithLocation(10.0000001, 9), EVENTTYPE);
            
            Assert.AreEqual(1, _listener.GetAndClearEmittedCount());
        }

        [Test]
        public void TestPreferredMatching()
        {
            var sender = _runtime.EventService.GetEventSender(typeof(MobileUserBean).Name);
            var user1 = new MobileUserBean(USER_ID_1, 10, 10,
                                            Gender.MALE, HairColor.RED, AgeRange.AGE_6,
                                            Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_5);
            sender.SendEvent(user1);

            // Test all combinations
            foreach (Gender gender in Enum.GetValues(typeof (Gender))) {
                foreach (HairColor color in Enum.GetValues(typeof (HairColor))) {
                    foreach (AgeRange age in AgeRange.Values) {
                        // Try user preferences
                        var userA = new MobileUserBean(USER_ID_2, 10, 10,
                                                       Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_5,
                                                       gender, color, age);
                        sender.SendEvent(userA);

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
            var sender = _runtime.EventService.GetEventSender(typeof(MobileUserBean).Name);
            var user1 = new MobileUserBean(USER_ID_1, 10, 10,
                                            Gender.MALE, HairColor.RED, AgeRange.AGE_6,
                                            Gender.FEMALE, HairColor.BLACK, AgeRange.AGE_5);
            sender.SendEvent(user1);

            // Test all combinations
            foreach (Gender gender in Enum.GetValues(typeof (Gender))) {
                foreach (HairColor color in Enum.GetValues(typeof (HairColor))) {
                    foreach (AgeRange age in AgeRange.Values) {
                        // Try user preferences backwards
                        var userB = new MobileUserBean(USER_ID_2, 10, 10,
                                                       gender, color, age,
                                                       Gender.MALE, HairColor.RED, AgeRange.AGE_6);
                        sender.SendEvent(userB);

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
