///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.pattern;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// View to handle pattern discarding for a single stream (no join).
    /// </summary>
    public sealed class PatternRemoveDispatchView : ViewSupport, EPStatementDispatch
    {
        private readonly EvalRootState _patternRoot;
        private readonly bool _suppressSameEventMatches;
        private readonly bool _discardPartialsOnMatch;

        private bool _hasData = false;
        private readonly FlushedEventBuffer _newDataBuffer = new FlushedEventBuffer();

        public PatternRemoveDispatchView(EvalRootState patternRoot, bool suppressSameEventMatches, bool discardPartialsOnMatch) {
            _patternRoot = patternRoot;
            _suppressSameEventMatches = suppressSameEventMatches;
            _discardPartialsOnMatch = discardPartialsOnMatch;
        }

        public override EventType EventType
        {
            get { return Parent.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Parent.GetEnumerator();
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            _newDataBuffer.Add(newData);
            _hasData = true;
        }

        public void Execute()
        {
            if (_hasData) {
                _hasData = false;

                var matches = _newDataBuffer.GetAndFlush();

                if (_discardPartialsOnMatch) {
                    var events = new HashSet<EventBean>();
                    foreach (var match in matches) {
                        AddEventsFromMatch(match, events);
                    }
                    if (events.Count > 0) {
                        _patternRoot.RemoveMatch(events);
                    }
                }

                if (_suppressSameEventMatches && matches.Length > 1) {
                    var events = new HashSet<EventBean>();
                    AddEventsFromMatch(matches[0], events);
                    if (matches.Length == 2) {
                        var overlaps = AddEventsFromMatch(matches[1], events);
                        if (overlaps) {
                            matches = new EventBean[] {matches[0]};
                        }
                    }
                    else {
                        IList<EventBean> matchesNonOverlapping = new List<EventBean>(matches.Length);
                        matchesNonOverlapping.Add(matches[0]);
                        for (var i = 1; i < matches.Length; i++) {
                            var eventsThisMatch = new HashSet<EventBean>();
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

                UpdateChildren(matches, null);
            }
        }

        private bool AddEventsFromMatch(EventBean match, ISet<EventBean> events)
        {
            var properties = match.EventType.PropertyDescriptors;
            var overlaps = false;

            foreach (var desc in properties) {
                var prop = ((IDictionary<string, object>) match.Underlying).Get(desc.PropertyName);
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
}
