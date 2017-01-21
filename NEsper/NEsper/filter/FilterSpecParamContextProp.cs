///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// This class represents a filter parameter containing a reference to a context property.
    /// </summary>
    [Serializable]
    public sealed class FilterSpecParamContextProp : FilterSpecParam
    {
        private readonly string _contextPropertyName;
        [NonSerialized]
        private readonly EventPropertyGetter _getter;
        [NonSerialized]
        private readonly Coercer _numberCoercer;

        public FilterSpecParamContextProp(FilterSpecLookupable lookupable, FilterOperator filterOperator, String contextPropertyName, EventPropertyGetter getter, Coercer numberCoercer)
            : base(lookupable, filterOperator)
        {
            _contextPropertyName = contextPropertyName;
            _getter = getter;
            _numberCoercer = numberCoercer;
        }

        public override object GetFilterValue(MatchedEventMap matchedEvents, AgentInstanceContext agentInstanceContext)
        {
            if (agentInstanceContext.ContextProperties == null)
            {
                return null;
            }

            var result = _getter.Get(agentInstanceContext.ContextProperties);
            if (_numberCoercer == null)
            {
                return result;
            }
            return _numberCoercer.Invoke(result);
        }

        public bool Equals(FilterSpecParamContextProp other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(other._contextPropertyName, _contextPropertyName);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as FilterSpecParamContextProp);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (_contextPropertyName != null ? _contextPropertyName.GetHashCode() : 0);
            }
        }
    }
}