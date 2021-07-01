///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multistmtassert
{
    public class MultiStmtAssertUtil
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void RunIsInvokedWTestdataUniform(
            RegressionEnvironment env,
            IList<string> epls,
            object[] testData,
            Consumer<object> sender,
            bool[] received,
            AtomicLong milestone)
        {
            Assert.AreEqual(testData.Length, received.Length);
            IList<EPLWithInvokedFlags> list = new List<EPLWithInvokedFlags>();
            foreach (var epl in epls) {
                list.Add(new EPLWithInvokedFlags(epl, received));
            }

            RunIsInvokedWTestdata(env, list, testData, sender, milestone);
        }

        public static void RunIsInvokedWTestdata(
            RegressionEnvironment env,
            IList<EPLWithInvokedFlags> descriptors,
            object[] testData,
            Consumer<object> sender,
            AtomicLong milestone)
        {
            ValidateDescriptors(descriptors, testData.Length);
            DeployAndMilestone(env, descriptors, milestone);

            log.Info("Running {} assertions", descriptors.Count * testData.Length);
            for (var @event = 0; @event < testData.Length; @event++) {
                sender.Invoke(testData[@event]);
                AssertDescriptors(env, descriptors, @event);
                env.Milestone(milestone.GetAndIncrement());
            }

            env.UndeployAll();
        }

        public static void RunIsInvokedWithEventSender(
            RegressionEnvironment env,
            IList<EPLWithInvokedFlags> descriptors,
            int numEvents,
            Consumer<int> sender,
            AtomicLong milestone)
        {
            ValidateDescriptors(descriptors, numEvents);
            DeployAndMilestone(env, descriptors, milestone);

            log.Info("Running {} assertions", descriptors.Count * numEvents);
            for (var @event = 0; @event < numEvents; @event++) {
                sender.Invoke(@event);
                AssertDescriptors(env, descriptors, @event);
                env.Milestone(milestone.GetAndIncrement());
            }

            env.UndeployAll();
        }

        public static void RunEPL(
            RegressionEnvironment env,
            IList<string> epls,
            object[] testData,
            Consumer<object> sender,
            AsserterPerObj<string> asserter,
            AtomicLong milestone)
        {
            for (var i = 0; i < epls.Count; i++) {
                var name = "s" + i;
                var epl = "@Name('" + name + "') " + epls[i];
                log.Info("Compiling and deploying ... {}", epl);
                env.CompileDeploy(epl).AddListener(name);
            }

            env.Milestone(milestone.GetAndIncrement());

            log.Info("Running {} assertions", epls.Count * testData.Length);
            for (var @event = 0; @event < testData.Length; @event++) {
                sender.Invoke(testData[@event]);

                for (var i = 0; i < epls.Count; i++) {
                    var name = "s" + i;
                    var epl = epls[i];
                    var message = "Failed at event " + @event + " statement + " + i + " epl ";
                    asserter.Invoke(@event, testData[@event], epl, name, message);
                }

                env.Milestone(milestone.GetAndIncrement());
            }

            env.UndeployAll();
        }

        /// <summary>
        ///     For use when:
        ///     - small number of events to send
        ///     - when data and expected-output is paired already
        /// </summary>
        public static void RunSendAssertPairs(
            RegressionEnvironment env,
            IList<string> epls,
            SendAssertPair[] pairs,
            AtomicLong milestone)
        {
            for (var i = 0; i < epls.Count; i++) {
                var name = "s" + i;
                var epl = "@Name('" + name + "') " + epls[i];
                log.Info("Compiling and deploying ... {}", epl);
                env.CompileDeploy(epl).AddListener(name);
            }

            env.Milestone(milestone.GetAndIncrement());

            log.Info("Running {} assertions", epls.Count * pairs.Length);
            for (var @event = 0; @event < pairs.Length; @event++) {
                pairs[@event].Sender.Invoke();

                for (var i = 0; i < epls.Count; i++) {
                    var name = "s" + i;
                    var message = "Failed at event " + @event + " statement + " + i + " epl " + epls[i];
                    pairs[@event].Asserter.Invoke(@event, name, message);
                }

                env.Milestone(milestone.GetAndIncrement());
            }

            env.UndeployAll();
        }

        private static void DeployAndMilestone(
            RegressionEnvironment env,
            IList<EPLWithInvokedFlags> descriptors,
            AtomicLong milestone)
        {
            for (var i = 0; i < descriptors.Count; i++) {
                var name = "s" + i;
                var desc = descriptors[i];
                var epl = "@Name('" + name + "') " + desc.Epl();
                log.Info("Compiling and deploying ... {}", epl);
                env.CompileDeploy(epl).AddListener(name);
            }

            env.Milestone(milestone.GetAndIncrement());
        }

        private static void AssertDescriptors(
            RegressionEnvironment env,
            IList<EPLWithInvokedFlags> descriptors,
            int @event)
        {
            for (var i = 0; i < descriptors.Count; i++) {
                var name = "s" + i;
                var desc = descriptors[i];
                var message = "Failed at event " + @event + " statement " + i + " epl [" + desc.Epl() + "]";
                Assert.AreEqual(desc.Received[@event], env.Listener(name).GetAndClearIsInvoked(), message);
            }
        }

        private static void ValidateDescriptors(
            IList<EPLWithInvokedFlags> descriptors,
            int length)
        {
            foreach (var desc in descriptors) {
                Assert.AreEqual(desc.Received.Length, length);
            }
        }
    }
} // end of namespace