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
	public class CodegenFieldNameSubqueryAgg : CodegenFieldName {
	    private readonly int _subqueryNumber;

	    public CodegenFieldNameSubqueryAgg(int subqueryNumber) {
	        this._subqueryNumber = subqueryNumber;
	    }

	    public string Name {
	        get => CodegenPackageScopeNames.AggregationSubquery(_subqueryNumber);
	    }

	    public int SubqueryNumber {
	        get => _subqueryNumber;
	    }

	    public override bool Equals(object o) {
	        if (this == o) return true;
	        if (o == null || GetType() != o.GetType()) return false;

	        CodegenFieldNameSubqueryAgg that = (CodegenFieldNameSubqueryAgg) o;

	        return _subqueryNumber == that._subqueryNumber;
	    }

	    public override int GetHashCode() {
	        return _subqueryNumber;
	    }
	}
} // end of namespace