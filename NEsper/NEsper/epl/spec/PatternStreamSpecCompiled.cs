///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.pattern;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification for building an event stream out of a pattern statement and views 
    /// staggered onto the pattern statement.
    /// <para/>
    /// The pattern statement is represented by the top EvalNode evaluation node. A pattern 
    /// statement contains tagged events (i.e. a=A -&gt; b=B). Thus the resulting event 
    /// type is has properties "a" and "b" of the type of A and B.
    /// </summary>
    public class PatternStreamSpecCompiled : StreamSpecBase, StreamSpecCompiled
    {
        private readonly EvalFactoryNode _evalFactoryNode;
        private readonly IDictionary<String, Pair<EventType, String>> _taggedEventTypes;      // Stores types for filters with tags, single event
        private readonly IDictionary<String, Pair<EventType, String>> _arrayEventTypes;       // Stores types for filters with tags, array event
        private readonly ISet<String> _allTags;
        private readonly bool _suppressSameEventMatches;
        private readonly bool _discardPartialsOnMatch;
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evalFactoryNode">pattern evaluation node representing pattern statement</param>
        /// <param name="taggedEventTypes">event tags and their types as specified in the pattern, copied to allow original collection to change</param>
        /// <param name="arrayEventTypes">event tags and their types as specified in the pattern for any repeat-expressions that generate an array of events</param>
        /// <param name="allTags">All tags.</param>
        /// <param name="viewSpecs">specifies what view to use to derive data</param>
        /// <param name="optionalStreamName">stream name, or null if none supplied</param>
        /// <param name="streamSpecOptions">additional stream options such as unidirectional stream in a join, applicable for joins</param>
        /// <param name="suppressSameEventMatches">if set to <c>true</c> [suppress same event matches].</param>
        /// <param name="discardPartialsOnMatch">if set to <c>true</c> [discard partials on match].</param>
        public PatternStreamSpecCompiled(EvalFactoryNode evalFactoryNode, IDictionary<string, Pair<EventType, string>> taggedEventTypes, IDictionary<string, Pair<EventType, string>> arrayEventTypes, ISet<String> allTags, ViewSpec[] viewSpecs, String optionalStreamName, StreamSpecOptions streamSpecOptions, bool suppressSameEventMatches, bool discardPartialsOnMatch)
            : base(optionalStreamName, viewSpecs, streamSpecOptions)
        {
            _suppressSameEventMatches = suppressSameEventMatches;
            _discardPartialsOnMatch = discardPartialsOnMatch;
            _evalFactoryNode = evalFactoryNode;
            _allTags = allTags;
    
            var copy = new LinkedHashMap<String, Pair<EventType, String>>();
            copy.PutAll(taggedEventTypes);
            _taggedEventTypes = copy;
    
            copy = new LinkedHashMap<String, Pair<EventType, String>>();
            copy.PutAll(arrayEventTypes);
            _arrayEventTypes = copy;
        }

        /// <summary>Returns the pattern expression evaluation node for the top pattern operator. </summary>
        /// <value>parent pattern expression node</value>
        public EvalFactoryNode EvalFactoryNode
        {
            get { return _evalFactoryNode; }
        }

        /// <summary>Returns event types tagged in the pattern expression. </summary>
        /// <value>map of tag and event type tagged in pattern expression</value>
        public IDictionary<string, Pair<EventType, string>> TaggedEventTypes
        {
            get { return _taggedEventTypes; }
        }

        /// <summary>Returns event types tagged in the pattern expression under a repeat-operator. </summary>
        /// <value>map of tag and event type tagged in pattern expression, repeated an thus producing array events</value>
        public IDictionary<string, Pair<EventType, string>> ArrayEventTypes
        {
            get { return _arrayEventTypes; }
        }

        public MatchedEventMapMeta MatchedEventMapMeta
        {
            get
            {
                String[] tags = _allTags.ToArray();
                return new MatchedEventMapMeta(tags, !_arrayEventTypes.IsEmpty());
            }
        }

        public ISet<string> AllTags
        {
            get { return _allTags; }
        }

        public bool IsSuppressSameEventMatches
        {
            get { return _suppressSameEventMatches; }
        }

        public bool IsDiscardPartialsOnMatch
        {
            get { return _discardPartialsOnMatch; }
        }
    }
}
