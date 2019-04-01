using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
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

	public class ReformatFormatForgeDesc {
	    private readonly bool java8;
	    private readonly Type formatterType;

	    public ReformatFormatForgeDesc(bool java8, Type formatterType) {
	        this.java8 = java8;
	        this.formatterType = formatterType;
	    }

	    public bool IsJava8() {
	        return java8;
	    }

	    public Type FormatterType {
	        get { return formatterType; }
	    }
	}
} // end of namespace
