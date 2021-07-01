///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.@internal.filtersvc
{
    public sealed class ProxyFilterHandleCallback : FilterHandleCallback
    {
        public ProxyFilterHandleCallback()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyFilterHandleCallback" /> class.
        /// </summary>
        /// <param name="matchFound">The match found.</param>
        /// <param name="isSubSelect">The is sub select.</param>
        public ProxyFilterHandleCallback(
            Action<EventBean, ICollection<FilterHandleCallback>> matchFound,
            bool isSubSelect)
        {
            ProcMatchFound = matchFound;
            ProcIsSubselect = () => isSubSelect;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyFilterHandleCallback" /> class.
        /// </summary>
        /// <param name="matchFound">The match found.</param>
        /// <param name="isSubSelect">The is sub select.</param>
        public ProxyFilterHandleCallback(
            Action<EventBean, IEnumerable<FilterHandleCallback>> matchFound,
            Func<bool> isSubSelect)
        {
            ProcMatchFound = matchFound;
            ProcIsSubselect = isSubSelect;
        }

        public Action<EventBean, ICollection<FilterHandleCallback>> ProcMatchFound { get; set; }
        public Func<bool> ProcIsSubselect { get; set; }

        /// <summary>
        ///     Indicate that an event was evaluated by the <seealso cref="FilterService" /> which
        ///     matches the filter specification <seealso cref="FilterSpecCompiled" /> associated with this callback.
        /// </summary>
        /// <param name="theEvent">the event received that matches the filter specification</param>
        /// <param name="allStmtMatches">All STMT matches.</param>
        public void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            ProcMatchFound.Invoke(theEvent, allStmtMatches);
        }

        /// <summary>
        ///     Returns true if the filter applies to subselects.
        /// </summary>
        /// <value>subselect filter</value>
        public bool IsSubSelect => ProcIsSubselect.Invoke();
    }
}