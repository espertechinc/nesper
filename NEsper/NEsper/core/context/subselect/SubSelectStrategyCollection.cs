///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.subquery;

namespace com.espertech.esper.core.context.subselect
{
    /// <summary>
    /// Holds stream information for subqueries.
    /// </summary>
    public class SubSelectStrategyCollection
    {
        private IDictionary<ExprSubselectNode, SubSelectStrategyFactoryDesc> _subqueries;

        /// <summary>
        /// Add lookup.
        /// </summary>
        /// <param name="subselectNode">is the subselect expression node</param>
        /// <param name="prototypeHolder">The Prototype holder.</param>
        public void Add(ExprSubselectNode subselectNode, SubSelectStrategyFactoryDesc prototypeHolder)
        {
            if (_subqueries == null)
            {
                _subqueries = new Dictionary<ExprSubselectNode, SubSelectStrategyFactoryDesc>();
            }
            _subqueries.Put(subselectNode, prototypeHolder);
        }

        public IDictionary<ExprSubselectNode, SubSelectStrategyFactoryDesc> Subqueries
        {
            get
            {
                if (_subqueries == null)
                {
                    return Collections.GetEmptyMap<ExprSubselectNode, SubSelectStrategyFactoryDesc>();
                }
                return _subqueries;
            }
        }
    }
}