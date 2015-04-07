///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.subselect
{
    /// <summary>Holds stream information for subqueries. </summary>
    public class SubSelectActivationCollection
    {
        private readonly IDictionary<ExprSubselectNode, SubSelectActivationHolder> _subqueries;

        /// <summary>Ctor. </summary>
        public SubSelectActivationCollection()
        {
            _subqueries = new Dictionary<ExprSubselectNode, SubSelectActivationHolder>();
        }

        /// <summary>
        /// Add lookup.
        /// </summary>
        /// <param name="subselectNode">is the subselect expression node</param>
        /// <param name="holder">The holder.</param>
        public void Add(ExprSubselectNode subselectNode, SubSelectActivationHolder holder)
        {
            _subqueries.Put(subselectNode, holder);
        }

        /// <summary>
        /// Gets the sub select holder.
        /// </summary>
        /// <param name="subselectNode">The subselect node.</param>
        /// <returns></returns>
        public SubSelectActivationHolder GetSubSelectHolder(ExprSubselectNode subselectNode)
        {
            return _subqueries.Get(subselectNode);
        }

        /// <summary>Returns stream number. </summary>
        /// <param name="subqueryNode">is the lookup node's stream number</param>
        /// <returns>number of stream</returns>
        public int GetStreamNumber(ExprSubselectNode subqueryNode)
        {
            return _subqueries.Get(subqueryNode).StreamNumber;
        }

        /// <summary>Returns the lookup viewable, child-most view. </summary>
        /// <param name="subqueryNode">is the expression node to get this for</param>
        /// <returns>child viewable</returns>
        public EventType GetRootViewableType(ExprSubselectNode subqueryNode)
        {
            return _subqueries.Get(subqueryNode).ViewableType;
        }

        /// <summary>Returns the lookup's view factory chain. </summary>
        /// <param name="subqueryNode">is the node to look for</param>
        /// <returns>view factory chain</returns>
        public ViewFactoryChain GetViewFactoryChain(ExprSubselectNode subqueryNode)
        {
            return _subqueries.Get(subqueryNode).ViewFactoryChain;
        }

        public IDictionary<ExprSubselectNode, SubSelectActivationHolder> Subqueries
        {
            get { return _subqueries; }
        }
    }
}
