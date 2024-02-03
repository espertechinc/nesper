///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class FilterItem
    {
        public FilterItem(
            string name,
            FilterOperator op,
            object optionalValue = null,
            object index = null)
        {
            Name = name;
            Op = op;
            OptionalValue = optionalValue;
            Index = index;
        }

        public FilterItem(
            string name,
            FilterOperator op,
            object index)
            : this(name, op, null, index)
        {
        }

        public string Name { get; }

        public FilterOperator Op { get; }

        public object OptionalValue { get; }

        public object Index { get; set; }

        public static FilterItem BoolExprFilterItem => new FilterItem(
            FilterSpecCompilerIndexPlanner.PROPERTY_NAME_BOOLEAN_EXPRESSION,
            FilterOperator.BOOLEAN_EXPRESSION, null);

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Op)}: {Op}, {nameof(OptionalValue)}: {OptionalValue}, {nameof(Index)}: {Index}";
        }

        protected bool Equals(FilterItem other)
        {
            return string.Equals(Name, other.Name) && Op == other.Op;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((FilterItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (int) Op;
            }
        }
    }
} // end of namespace