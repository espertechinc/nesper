///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.aggfunc
{
	/// <summary>
	///     Annotation for use in EPL statements to add a debug.
	/// </summary>
	public class ExtensionAggregationFunctionAttribute : Attribute
    {
        private string Name { get; set; }
    }
} // end of namespace