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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
	public abstract class EnumForgeBase : EnumForge {
	    internal ExprForge innerExpression;
	    internal int streamNumLambda;

	    public EnumForgeBase(ExprForge innerExpression, int streamCountIncoming)
	        : this(streamCountIncoming)
	    {
	        this.innerExpression = innerExpression;
	    }

	    public EnumForgeBase(int streamCountIncoming) {
	        this.streamNumLambda = streamCountIncoming;
	    }

	    public ExprForge InnerExpression
	    {
	        get => innerExpression;
	    }

	    public int StreamNumSize
	    {
	        get => streamNumLambda + 1;
	    }

	    public abstract EnumEval EnumEvaluator { get; }

	    public abstract CodegenExpression Codegen(
	        EnumForgeCodegenParams premade, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope);
	}
} // end of namespace