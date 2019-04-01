using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.@internal.bytecodemodel.util
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

	public class IdentifierUtil {
	    public static string GetIdentifierMayStartNumeric(string str) {
	        StringBuilder sb = new StringBuilder();
	        for (int i = 0; i < str.Length(); i++) {
	            if (Character.IsJavaIdentifierPart(str.CharAt(i)))
	                sb.Append(str.CharAt(i));
	            else
	                sb.Append((int) str.CharAt(i));
	        }
	        return sb.ToString();
	    }
	}
} // end of namespace
