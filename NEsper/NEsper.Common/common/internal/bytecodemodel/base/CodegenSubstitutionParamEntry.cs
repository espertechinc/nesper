///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
	public class CodegenSubstitutionParamEntry {
	    private readonly CodegenField field;
	    private readonly string name;
	    private readonly Type type;

	    public CodegenSubstitutionParamEntry(CodegenField field, string name, Type type) {
	        this.field = field;
	        this.name = name;
	        this.type = type;
	    }

	    public CodegenField Field {
	        get => field;
	    }

	    public string Name {
	        get => name;
	    }

	    public Type Type {
	        get => type;
	    }

	    public static void CodegenSetterMethod(CodegenClassScope classScope, CodegenMethod method) {
	        IList<CodegenSubstitutionParamEntry> numbered = classScope.PackageScope.SubstitutionParamsByNumber;
	        LinkedHashMap<string, CodegenSubstitutionParamEntry> named = classScope.PackageScope.SubstitutionParamsByName;
	        if (numbered.IsEmpty() && named.IsEmpty()) {
	            return;
	        }
	        if (!numbered.IsEmpty() && !named.IsEmpty()) {
	            throw new IllegalStateException("Both named and numbered substitution parameters are non-empty");
	        }

	        IList<CodegenSubstitutionParamEntry> fields;
	        if (!numbered.IsEmpty()) {
	            fields = numbered;
	        } else {
	            fields = new List<>(named.Values());
	        }

	        method.Block.DeclareVar(typeof(int), "zidx", Op(@Ref("index"), "-", Constant(1)));
	        CodegenBlock[] blocks = method.Block.SwitchBlockOfLength("zidx", fields.Count, false);
	        for (int i = 0; i < blocks.Length; i++) {
	            CodegenSubstitutionParamEntry param = fields.Get(i);
	            blocks[i].AssignRef(Field(param.Field), Cast(Boxing.GetBoxedType(param.Type), @Ref("value")));
	        }
	    }
	}
} // end of namespace