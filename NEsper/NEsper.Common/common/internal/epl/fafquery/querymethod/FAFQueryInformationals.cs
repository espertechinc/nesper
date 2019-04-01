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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
	public class FAFQueryInformationals {
	    private readonly Type[] substitutionParamsTypes;
	    private readonly IDictionary<string, int> substitutionParamsNames;

	    public FAFQueryInformationals(Type[] substitutionParamsTypes, IDictionary<string, int> substitutionParamsNames) {
	        this.substitutionParamsTypes = substitutionParamsTypes;
	        this.substitutionParamsNames = substitutionParamsNames;
	    }

	    public static FAFQueryInformationals From(IList<CodegenSubstitutionParamEntry> paramsByNumber, LinkedHashMap<string, CodegenSubstitutionParamEntry> paramsByName) {
	        Type[] types;
	        IDictionary<string, int> names;
	        if (!paramsByNumber.IsEmpty()) {
	            types = new Type[paramsByNumber.Count];
	            for (int i = 0; i < paramsByNumber.Count; i++) {
	                types[i] = paramsByNumber.Get(i).Type;
	            }
	            names = null;
	        } else if (!paramsByName.IsEmpty()) {
	            types = new Type[paramsByName.Count];
	            names = new Dictionary<>();
	            int index = 0;
	            foreach (KeyValuePair<string, CodegenSubstitutionParamEntry> entry in paramsByName) {
	                types[index] = entry.Value.Type;
	                names.Put(entry.Key, index + 1);
	                index++;
	            }
	        } else {
	            types = null;
	            names = null;
	        }
	        return new FAFQueryInformationals(types, names);
	    }

	    public Type[] GetSubstitutionParamsTypes() {
	        return substitutionParamsTypes;
	    }

	    public IDictionary<string, int> GetSubstitutionParamsNames() {
	        return substitutionParamsNames;
	    }

	    public CodegenExpression Make(CodegenMethodScope parent, CodegenClassScope classScope) {
	        return NewInstance(typeof(FAFQueryInformationals), Constant(substitutionParamsTypes), MakeNames(parent, classScope));
	    }

	    private CodegenExpression MakeNames(CodegenMethodScope parent, CodegenClassScope classScope) {
	        if (substitutionParamsNames == null) {
	            return ConstantNull();
	        }
	        CodegenMethod method = parent.MakeChild(typeof(IDictionary<object, object>), this.GetType(), classScope);
	        method.Block.DeclareVar(typeof(IDictionary<object, object>), "names", NewInstance(typeof(Dictionary<object, object>), Constant(CollectionUtil.CapacityHashMap(substitutionParamsNames.Count))));
	        foreach (KeyValuePair<string, int> entry in substitutionParamsNames) {
	            method.Block.ExprDotMethod(@Ref("names"), "put", Constant(entry.Key), Constant(entry.Value));
	        }
	        method.Block.MethodReturn(@Ref("names"));
	        return LocalMethod(method);
	    }
	}
} // end of namespace