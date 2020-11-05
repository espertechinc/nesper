///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.streamtype;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public interface ExprEnumerationForgeProvider
    {
	    /// <summary>
	    ///     Returns the enumeration forge provider, or null if not applicable
	    /// </summary>
	    /// <returns>forge provider</returns>
	    /// <param name="streamTypeService">stream type service</param>
	    /// <param name="contextDescriptor">context descriptor</param>
	    ExprEnumerationForgeDesc GetEnumerationForge(
            StreamTypeService streamTypeService,
            ContextCompileTimeDescriptor contextDescriptor);
    }
} // end of namespace