///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;

namespace com.espertech.esper.pattern
{
    public class PatternConsumptionUtil
    {
        public static bool ContainsEvent(ICollection<EventBean> matchEvent, MatchedEventMap beginState)
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
                else if (aPartial is EventBean) {
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
    
        public static void ChildNodeRemoveMatches<TK>(ISet<EventBean> matchEvent, ICollection<TK> evalStateNodes)
            where TK : EvalStateNode
        {
            var nodesArray = evalStateNodes.ToArray();
            foreach (var child in nodesArray)
            {
                child.RemoveMatch(matchEvent);
            }
        }
    }
}
