///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.methodbase
{
	public class DotMethodFPProvidedParam {

	    private int lambdaParamNum; // 0 means not a lambda expression expected, 1 means "x=>", 2 means "(x,y)=>"
	    private Type returnType;
	    private ExprNode expression;

	    public DotMethodFPProvidedParam(int lambdaParamNum, Type returnType, ExprNode expression) {
	        this.lambdaParamNum = lambdaParamNum;
	        this.returnType = returnType;
	        this.expression = expression;
	    }

	    public int LambdaParamNum
	    {
	        get => lambdaParamNum;
	    }

	    public Type ReturnType
	    {
	        get => returnType;
	    }

	    public ExprNode Expression
	    {
	        get => expression;
	    }
	}
} // end of namespace