///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Interface for a callback method to be called when an event matches a filter specification. Provided as a convenience 
    /// for use as a filter handle for registering with the <seealso cref="FilterService"/>.
    /// </summary>
    public interface FilterHandleCallback : FilterHandle
    {
        /// <summary>
        /// Indicate that an event was evaluated by the <seealso cref="com.espertech.esper.filter.FilterService"/> which
        /// matches the filter specification <seealso cref="com.espertech.esper.filter.FilterSpecCompiled"/> associated with this callback.
        /// </summary>
        /// <param name="theEvent">the event received that matches the filter specification</param>
        /// <param name="allStmtMatches">All STMT matches.</param>
        void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches);

        /// <summary>
        /// Returns true if the filter applies to subselects.
        /// </summary>
        /// <value>subselect filter</value>
        bool IsSubSelect { get; }
    }

    public sealed class ProxyFilterHandleCallback : FilterHandleCallback
    {
        public Action<EventBean, ICollection<FilterHandleCallback>> ProcMatchFound { get; set; }
        public Func<bool> ProcIsSubselect { get; set; }
        public Func<string> ProcStatementId { get; set; }

        public ProxyFilterHandleCallback()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyFilterHandleCallback"/> class.
        /// </summary>
        /// <param name="matchFound">The match found.</param>
        /// <param name="isSubSelect">The is sub select.</param>
        /// <param name="statementId">The statement id.</param>
        public ProxyFilterHandleCallback(Action<EventBean, ICollection<FilterHandleCallback>> matchFound,
                                         bool isSubSelect,
                                         string statementId)
        {
            ProcMatchFound = matchFound;
            ProcIsSubselect = () => isSubSelect;
            ProcStatementId = () => statementId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyFilterHandleCallback"/> class.
        /// </summary>
        /// <param name="matchFound">The match found.</param>
        /// <param name="isSubSelect">The is sub select.</param>
        /// <param name="statementId">The statement id.</param>
        public ProxyFilterHandleCallback(Action<EventBean, IEnumerable<FilterHandleCallback>> matchFound,
                                         Func<bool> isSubSelect,
                                         Func<string> statementId)
        {
            ProcMatchFound = matchFound;
            ProcIsSubselect = isSubSelect;
            ProcStatementId = statementId;
        }

        /// <summary>
        /// Gets the statement id.
        /// </summary>
        /// <value>The statement id.</value>
        public string StatementId
        {
            get { return ProcStatementId.Invoke(); }
        }

        /// <summary>
        /// Indicate that an event was evaluated by the <seealso cref="com.espertech.esper.filter.FilterService"/> which
        /// matches the filter specification <seealso cref="com.espertech.esper.filter.FilterSpecCompiled"/> associated with this callback.
        /// </summary>
        /// <param name="theEvent">the event received that matches the filter specification</param>
        /// <param name="allStmtMatches">All STMT matches.</param>
        public void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches)
        {
            ProcMatchFound.Invoke(theEvent, allStmtMatches);
        }

        /// <summary>
        /// Returns true if the filter applies to subselects.
        /// </summary>
        /// <value>subselect filter</value>
        public bool IsSubSelect
        {
            get { return ProcIsSubselect.Invoke(); }
        }
    }
}
