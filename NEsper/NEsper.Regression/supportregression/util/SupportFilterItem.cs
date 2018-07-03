///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.filter;

namespace com.espertech.esper.supportregression.util {
    public class SupportFilterItem {
        public SupportFilterItem(string name, FilterOperator op) {
            Name = name;
            Op = op;
        }

        public string Name { get; }

        public FilterOperator Op { get; }

        public static SupportFilterItem BoolExprFilterItem => new SupportFilterItem(
            FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION, FilterOperator.BOOLEAN_EXPRESSION);

        public override string ToString() {
            return "FilterItem{" +
                   "name='" + Name + '\'' +
                   ", op=" + Op +
                   '}';
        }

        public override bool Equals(object o) {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (SupportFilterItem) o;

            if (Op != that.Op) {
                return false;
            }

            if ((Name != null) ? (Name != that.Name) : that.Name != null) {
                return false;
            }

            return true;
        }

        public override int GetHashCode() {
            var result = Name != null ? Name.GetHashCode() : 0;
            result = 31 * result + Op.GetHashCode();
            return result;
        }
    }
} // end of namespace