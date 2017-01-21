///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    ///     This class represents one filter parameter in an <seealso cref="FilterSpecCompiled" /> filter specification.
    ///     <para />
    ///     Each filerting parameter has an attribute name and operator type.
    /// </summary>
    [Serializable]
    public abstract class FilterSpecParam
        : MetaDefItem
    {
        public static readonly FilterSpecParam[] EMPTY_PARAM_ARRAY = new FilterSpecParam[0];

        protected FilterSpecParam(FilterSpecLookupable lookupable, FilterOperator filterOperator)
        {
            Lookupable = lookupable;
            FilterOperator = filterOperator;
        }

        public FilterSpecLookupable Lookupable { get; private set; }

        /// <summary>Returns the filter operator type. </summary>
        /// <value>filter operator type</value>
        public FilterOperator FilterOperator { get; private set; }

        /// <summary>
        /// Return the filter parameter constant to filter for.
        /// </summary>
        /// <param name="matchedEvents">is the prior results that can be used to determine filter parameters</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <returns>
        /// filter parameter constant's value
        /// </returns>
        public abstract object GetFilterValue(MatchedEventMap matchedEvents, AgentInstanceContext agentInstanceContext);

        public override String ToString()
        {
            return "FilterSpecParam" +
                   " lookupable=" + Lookupable +
                   " filterOp=" + FilterOperator;
        }

        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is FilterSpecParam))
            {
                return false;
            }

            var other = (FilterSpecParam) obj;
            if (!(Lookupable.Equals(other.Lookupable)))
            {
                return false;
            }
            if (FilterOperator != other.FilterOperator)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int result;
            result = Lookupable.GetHashCode();
            result = 31*result + FilterOperator.GetHashCode();
            return result;
        }

        public static FilterSpecParam[] ToArray(ICollection<FilterSpecParam> coll)
        {
            if (coll.IsEmpty())
            {
                return EMPTY_PARAM_ARRAY;
            }
            return coll.ToArray();
        }
    }
}