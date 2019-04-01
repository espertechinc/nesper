///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
	public class EnumTakeWhileIndexEventsForge : EnumForge {

	    internal ExprForge innerExpression;
	    internal int streamNumLambda;
	    internal ObjectArrayEventType indexEventType;

	    public EnumTakeWhileIndexEventsForge(ExprForge innerExpression, int streamNumLambda, ObjectArrayEventType indexEventType) {
	        this.innerExpression = innerExpression;
	        this.streamNumLambda = streamNumLambda;
	        this.indexEventType = indexEventType;
	    }

	    public int StreamNumSize
	    {
	        get => streamNumLambda + 2;
	    }

	    public virtual EnumEval EnumEvaluator
	    {
	        get => new EnumTakeWhileIndexEventsForgeEval(this, innerExpression.ExprEvaluator);
	    }

	    public virtual CodegenExpression Codegen(EnumForgeCodegenParams premade, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        return EnumTakeWhileIndexEventsForgeEval.Codegen(this, premade, codegenMethodScope, codegenClassScope);
	    }
	}
} // end of namespace