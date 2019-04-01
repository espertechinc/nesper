///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
	public class DefaultSupportCaptureOpForge<T> : DataFlowOperatorForge {
	    [DataFlowOpParameter]
	    string name;

	    public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context) {
	        return null;
	    }

	    public CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        return new SAIFFInitializeBuilder(typeof(DefaultSupportCaptureOpFactory), this.GetType(), "so", parent, symbols, classScope)
	                .Constant("name", name)
	                .Build();
	    }
	}

} // end of namespace