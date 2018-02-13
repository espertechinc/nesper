///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.filter;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.util
{
    public class SupportFilterItem {
        private readonly string name;
        private readonly FilterOperator op;
    
        public SupportFilterItem(string name, FilterOperator op) {
            this.name = name;
            this.op = op;
        }
    
        public string GetName() {
            return name;
        }
    
        public FilterOperator GetOp() {
            return op;
        }
    
        public override String ToString() {
            return "FilterItem{" +
                    "name='" + name + '\'' +
                    ", op=" + op +
                    '}';
        }
    
        public override bool Equals(object o) {
            if (this == o) return true;
            if (o == null || GetClass() != o.Class) return false;
    
            SupportFilterItem that = (SupportFilterItem) o;
    
            if (op != that.op) return false;
            if (name != null ? !name.Equals(that.name) : that.name != null) return false;
    
            return true;
        }
    
        public override int GetHashCode() {
            int result = name != null ? Name.HashCode() : 0;
            result = 31 * result + (op != null ? Op.HashCode() : 0);
            return result;
        }
    
        public static SupportFilterItem GetBoolExprFilterItem() {
            return new SupportFilterItem(FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION, FilterOperator.BOOLEAN_EXPRESSION);
        }
    }
} // end of namespace
