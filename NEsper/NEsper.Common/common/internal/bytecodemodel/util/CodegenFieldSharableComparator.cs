///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
	public class CodegenFieldSharableComparator : CodegenFieldSharable {
	    private readonly CodegenSharableSerdeName name;
	    private readonly Type[] types;
	    private readonly bool isSortUsingCollator;
	    private readonly bool[] descending;

	    public CodegenFieldSharableComparator(CodegenSharableSerdeName name, Type[] types, bool isSortUsingCollator, bool[] descending) {
	        this.name = name;
	        this.types = types;
	        this.isSortUsingCollator = isSortUsingCollator;
	        this.descending = descending;
	    }

	    public Type Type() {
	        return typeof(IComparer);
	    }

	    public CodegenExpression InitCtorScoped() {
	        return StaticMethod(typeof(ExprNodeUtilityMake), name.MethodName, Constant(types), Constant(isSortUsingCollator), Constant(descending));
	    }

	    public class CodegenSharableSerdeName
	    {
	        public static readonly CodegenSharableSerdeName COMPARATORHASHABLEMULTIKEYS =
	            new CodegenSharableSerdeName("getComparatorHashableMultiKeys");
	        public static readonly CodegenSharableSerdeName COMPARATOROBJECTARRAYNONHASHABLE =
	            new CodegenSharableSerdeName("getComparatorObjectArrayNonHashable");

	        CodegenSharableSerdeName(string methodName) {
	            this.MethodName = methodName;
	        }

	        public string MethodName { get; }
	    }

	}
} // end of namespace