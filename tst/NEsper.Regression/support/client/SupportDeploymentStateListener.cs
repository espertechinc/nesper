///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.client
{
    public class SupportDeploymentStateListener : DeploymentStateListener
    {
        public static IList<DeploymentStateEvent> Events { get; private set; } = new List<DeploymentStateEvent>();

        public void OnDeployment(DeploymentStateEventDeployed @event)
        {
            Events.Add(@event);
        }

        public void OnUndeployment(DeploymentStateEventUndeployed @event)
        {
            Events.Add(@event);
        }

        public static IList<DeploymentStateEvent> GetEventsAndReset()
        {
            var copy = Events;
            Events = new List<DeploymentStateEvent>();
            return copy;
        }

        public static void Reset()
        {
            Events = new List<DeploymentStateEvent>();
        }

        public static DeploymentStateEvent GetSingleEventAndReset()
        {
            var copy = GetEventsAndReset();
            if (copy.Count != 1) {
                throw new IllegalStateException("Expected single event");
            }

            return copy[0];
        }

        public static IList<DeploymentStateEvent> GetNEventsAndReset(int numExpected)
        {
            var copy = GetEventsAndReset();
            if (copy.Count != numExpected) {
                throw new IllegalStateException("Expected " + numExpected + " events but received " + copy.Count);
            }

            return copy;
        }
    }
} // end of namespace