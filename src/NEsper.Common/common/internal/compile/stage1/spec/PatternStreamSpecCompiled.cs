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
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Specification for building an event stream out of a pattern statement and views staggered onto the
    ///     pattern statement.
    ///     <para />
    ///     The pattern statement is represented by the top EvalNode evaluation node.
    ///     A pattern statement contains tagged events (i.e. a=A -&gt; b=B).
    ///     Thus the resulting event type is has properties "a" and "b" of the type of A and B.
    /// </summary>
    public class PatternStreamSpecCompiled : StreamSpecBase,
        StreamSpecCompiled
    {
        public PatternStreamSpecCompiled(
            EvalRootForgeNode root,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTags,
            ViewSpec[] viewSpecs,
            string optionalStreamName,
            StreamSpecOptions streamSpecOptions,
            bool suppressSameEventMatches,
            bool discardPartialsOnMatch)
            : base(optionalStreamName, viewSpecs, streamSpecOptions)
        {
            IsSuppressSameEventMatches = suppressSameEventMatches;
            IsDiscardPartialsOnMatch = discardPartialsOnMatch;
            Root = root;
            AllTags = allTags;

            var copy = new LinkedHashMap<string, Pair<EventType, string>>();
            copy.PutAll(taggedEventTypes);
            TaggedEventTypes = copy;

            copy = new LinkedHashMap<string, Pair<EventType, string>>();
            copy.PutAll(arrayEventTypes);
            ArrayEventTypes = copy;
        }

        /// <summary>
        ///     Returns the pattern expression evaluation node for the top pattern operator.
        /// </summary>
        /// <returns>parent pattern expression node</returns>
        public EvalRootForgeNode Root { get; }

        public bool IsConsumingFilters => IsConsumingFiltersRecursive(Root);

        public ISet<string> AllTags { get; }

        public bool IsSuppressSameEventMatches { get; }

        public bool IsDiscardPartialsOnMatch { get; }

        /// <summary>
        ///     Returns event types tagged in the pattern expression.
        /// </summary>
        /// <value>map of tag and event type tagged in pattern expression</value>
        public IDictionary<string, Pair<EventType, string>> TaggedEventTypes { get; }

        /// <summary>
        ///     Returns event types tagged in the pattern expression under a repeat-operator.
        /// </summary>
        /// <value>map of tag and event type tagged in pattern expression, repeated an thus producing array events</value>
        public IDictionary<string, Pair<EventType, string>> ArrayEventTypes { get; }

        public MatchedEventMapMeta MatchedEventMapMeta {
            get {
                var tags = new string[AllTags.Count];
                var eventTypes = new EventType[AllTags.Count];
                var count = 0;
                foreach (var tag in AllTags) {
                    tags[count] = tag;
                    EventType eventType = null;
                    var nonArray = TaggedEventTypes.Get(tag);
                    if (nonArray != null) {
                        eventType = nonArray.First;
                    }
                    else {
                        var array = ArrayEventTypes.Get(tag);
                        if (array != null) {
                            eventType = array.First;
                        }
                    }

                    if (eventType == null) {
                        throw new IllegalStateException("Failed to find tag '" + tag + "' among type information");
                    }

                    eventTypes[count++] = eventType;
                }

                var arrayTags = ArrayEventTypes.IsEmpty() ? null : ArrayEventTypes.Keys.ToArray();
                return new MatchedEventMapMeta(tags, eventTypes, arrayTags);
            }
        }

        private bool IsConsumingFiltersRecursive(EvalForgeNode evalNode)
        {
            if (evalNode is EvalFilterForgeNode node) {
                return node.ConsumptionLevel != null;
            }

            var consumption = false;
            foreach (var child in evalNode.ChildNodes) {
                consumption = consumption || IsConsumingFiltersRecursive(child);
            }

            return consumption;
        }
    }
} // end of namespace