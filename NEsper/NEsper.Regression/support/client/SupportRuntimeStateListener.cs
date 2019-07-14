///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.client
{
    public class SupportRuntimeStateListener : EPRuntimeStateListener
    {
        public IList<EPRuntime> DestroyedEvents { get; } = new List<EPRuntime>();

        public IList<EPRuntime> InitializedEvents { get; } = new List<EPRuntime>();

        public void OnEPRuntimeDestroyRequested(EPRuntime runtime)
        {
            DestroyedEvents.Add(runtime);
        }

        public void OnEPRuntimeInitialized(EPRuntime runtime)
        {
            InitializedEvents.Add(runtime);
        }

        public EPRuntime AssertOneGetAndResetDestroyedEvents()
        {
            Assert.AreEqual(1, DestroyedEvents.Count);
            var item = DestroyedEvents[0];
            DestroyedEvents.Clear();
            return item;
        }

        public EPRuntime AssertOneGetAndResetInitializedEvents()
        {
            Assert.AreEqual(1, InitializedEvents.Count);
            var item = InitializedEvents[0];
            InitializedEvents.Clear();
            return item;
        }
    }
} // end of namespace