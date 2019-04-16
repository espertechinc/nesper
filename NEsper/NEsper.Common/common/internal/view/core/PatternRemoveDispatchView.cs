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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     View to handle pattern discarding for a single stream (no join).
    /// </summary>
    public class PatternRemoveDispatchView : ViewSupport,
        EPStatementDispatch
    {
        private readonly bool discardPartialsOnMatch;
        private readonly EvalRootMatchRemover matchRemoveCallback;
        private readonly bool suppressSameEventMatches;

        private bool hasData;
        private readonly FlushedEventBuffer newDataBuffer = new FlushedEventBuffer();

        public PatternRemoveDispatchView(
            EvalRootMatchRemover matchRemoveCallback,
            bool suppressSameEventMatches,
            bool discardPartialsOnMatch)
        {
            this.matchRemoveCallback = matchRemoveCallback;
            this.suppressSameEventMatches = suppressSameEventMatches;
            this.discardPartialsOnMatch = discardPartialsOnMatch;
        }

        public override EventType EventType => Parent.EventType;

        public void Execute()
        {
            if (hasData) {
                hasData = false;

                var matches = newDataBuffer.GetAndFlush();

                if (discardPartialsOnMatch) {
                    ISet<EventBean> events = new HashSet<EventBean>();
                    foreach (var match in matches) {
                        AddEventsFromMatch(match, events);
                    }

                    if (events.Count > 0) {
                        matchRemoveCallback.RemoveMatch(events);
                    }
                }

                if (suppressSameEventMatches && matches.Length > 1) {
                    ISet<EventBean> events = new HashSet<EventBean>();
                    AddEventsFromMatch(matches[0], events);
                    if (matches.Length == 2) {
                        var overlaps = AddEventsFromMatch(matches[1], events);
                        if (overlaps) {
                            matches = new[] {matches[0]};
                        }
                    }
                    else {
                        IList<EventBean> matchesNonOverlapping = new List<EventBean>(matches.Length);
                        matchesNonOverlapping.Add(matches[0]);
                        for (var i = 1; i < matches.Length; i++) {
                            ISet<EventBean> eventsThisMatch = new HashSet<EventBean>();
                            eventsThisMatch.AddAll(events);
                            var overlaps = AddEventsFromMatch(matches[i], eventsThisMatch);
                            if (!overlaps) {
                                events.AddAll(eventsThisMatch);
                                matchesNonOverlapping.Add(matches[i]);
                            }
                        }

                        matches = matchesNonOverlapping.ToArray();
                    }
                }

                Child.Update(matches, null);
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Parent.GetEnumerator();
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            newDataBuffer.Add(newData);
            hasData = true;
        }

        private bool AddEventsFromMatch(
            EventBean match,
            ISet<EventBean> events)
        {
            var properties = match.EventType.PropertyDescriptors;
            var overlaps = false;

            foreach (var desc in properties) {
                var prop = ((IDictionary<object, object>) match.Underlying).Get(desc.PropertyName);
                if (prop == null) {
                }
                else if (prop is EventBean) {
                    overlaps |= !events.Add((EventBean) prop);
                }
                else if (prop is EventBean[]) {
                    var arr = (EventBean[]) prop;
                    foreach (var ele in arr) {
                        overlaps |= !events.Add(ele);
                    }
                }
            }

            return overlaps;
        }
    }
} // end of namespace