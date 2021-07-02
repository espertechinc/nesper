///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compiler.client.option
{
	/// <summary>
	///     Implement this interface to receive Janino-specific class detail for inlined-classes.
	///     <para>
	///         The compiler invokes the callback for each inlined-class that it compiles.
	///     </para>
	/// </summary>
	public interface InlinedClassInspectionOption
    {
	    /// <summary>
	    ///     Provides Janino-specific class detail for inlined-classes
	    /// </summary>
	    /// <param name="env">compiler-specific information for inlined classes</param>
	    void Visit(InlinedClassInspectionContext env);
    }
} // end of namespace