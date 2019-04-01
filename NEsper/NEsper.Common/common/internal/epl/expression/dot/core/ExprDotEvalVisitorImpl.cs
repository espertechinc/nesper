using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
	///////////////////////////////////////////////////////////////////////////////////////
	// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
	// http://esper.codehaus.org                                                          /
	// ---------------------------------------------------------------------------------- /
	// The software in this package is published under the terms of the GPL license       /
	// a copy of which has been included with this distribution in the license.txt file.  /
	///////////////////////////////////////////////////////////////////////////////////////

	/*
	 ***************************************************************************************
	 *  Copyright (C) 2006 EsperTech, Inc. All rights reserved.                            *
	 *  http://www.espertech.com/esper                                                     *
	 *  http://www.espertech.com                                                           *
	 *  ---------------------------------------------------------------------------------- *
	 *  The software in this package is published under the terms of the GPL license       *
	 *  a copy of which has been included with this distribution in the license.txt file.  *
	 ***************************************************************************************
	 */

	public class ExprDotEvalVisitorImpl : ExprDotEvalVisitor {
	    private string methodType;
	    private string methodName;

	    public void VisitPropertySource() {
	        Set("property value", null);
	    }

	    public void VisitEnumeration(string name) {
	        Set("enumeration method", name);
	    }

	    public void VisitMethod(string methodName) {
	        Set("jvm method", methodName);
	    }

	    public void VisitDateTime() {
	        Set("datetime method", null);
	    }

	    public void VisitUnderlyingEvent() {
	        Set("underlying event", null);
	    }

	    public void VisitUnderlyingEventColl() {
	        Set("underlying event collection", null);
	    }

	    public void VisitArraySingleItemSource() {
	        Set("array item", null);
	    }

	    public void VisitArrayLength() {
	        Set("array length", null);
	    }

	    public string GetMethodType() {
	        return methodType;
	    }

	    public string GetMethodName() {
	        return methodName;
	    }

	    private void Set(string methodType, string methodName) {
	        this.methodType = methodType;
	        this.methodName = methodName;
	    }
	}

} // end of namespace
