///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.compiler;

namespace com.espertech.esper.compiler.client.option
{
	/// <summary>
	/// Implement this interface to provide a compiler to use
	/// </summary>
	public interface CompilerHookOption
	{
		/// <summary>
		/// Returns the compiler to use, or null for the default compiler
		/// </summary>
		/// <param name="env">the compiler tool context</param>
		/// <returns>compiler or null for the default compiler.</returns>
		CompilerAbstraction GetValue(CompilerHookContext env);
	}

} // end of namespace
