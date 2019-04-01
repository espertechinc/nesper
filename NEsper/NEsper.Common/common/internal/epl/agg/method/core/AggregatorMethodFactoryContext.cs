///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
	public class AggregatorMethodFactoryContext {
	    private readonly int col;
	    private readonly CodegenCtor rowCtor;
	    private readonly CodegenMemberCol membersColumnized;
	    private readonly CodegenClassScope classScope;

	    public AggregatorMethodFactoryContext(int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized, CodegenClassScope classScope) {
	        this.col = col;
	        this.rowCtor = rowCtor;
	        this.membersColumnized = membersColumnized;
	        this.classScope = classScope;
	    }

	    public int Col {
	        get => col;
	    }

	    public CodegenCtor RowCtor {
	        get => rowCtor;
	    }

	    public CodegenMemberCol MembersColumnized {
	        get => membersColumnized;
	    }

	    public CodegenClassScope ClassScope {
	        get => classScope;
	    }
	}
} // end of namespace