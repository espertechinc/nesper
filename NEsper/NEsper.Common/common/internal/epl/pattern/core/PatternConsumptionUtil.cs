///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public class PatternConsumptionUtil
    {
        public static bool ContainsEvent(
            ISet<EventBean> matchEvent,
            MatchedEventMap beginState)
        {
            if (beginState == null) {
                return false;
            }

            var partial = beginState.MatchingEvents;
            var quit = false;
            foreach (var aPartial in partial) {
                if (aPartial == null) {
                    continue;
                }

                if (aPartial is EventBean) {
                    if (matchEvent.Contains(aPartial)) {
                        quit = true;
                        break;
                    }
                }
                else if (aPartial is EventBean[]) {
                    var events = (EventBean[]) aPartial;
                    foreach (var @event in events) {
                        if (matchEvent.Contains(@event)) {
                            quit = true;
                            break;
                        }
                    }
                }

                if (quit) {
                    break;
                }
            }

            return quit;
        }

        public static void ChildNodeRemoveMatches<T>(
            ISet<EventBean> matchEvent,
            ICollection<T> evalStateNodes)
            where T : EvalStateNode
        {
            EvalStateNode[] nodesArray = evalStateNodes.ToArray();
            foreach (var child in nodesArray) {
                child.RemoveMatch(matchEvent);
            }
        }
    }
} // end of namespace