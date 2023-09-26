///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;

namespace com.espertech.esper.common.@internal.bytecodemodel.name
{
    public class CodegenFieldNameSubqueryPrior : CodegenFieldName
    {
        private readonly int _subqueryNumber;

        public CodegenFieldNameSubqueryPrior(int subqueryNumber)
        {
            _subqueryNumber = subqueryNumber;
        }

        public string Name => CodegenNamespaceScopeNames.PriorSubquery(_subqueryNumber);

        public int SubqueryNumber => _subqueryNumber;

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (CodegenFieldNameSubqueryPrior)o;

            return _subqueryNumber == that._subqueryNumber;
        }

        public override int GetHashCode()
        {
            return _subqueryNumber;
        }
    }
} // end of namespace