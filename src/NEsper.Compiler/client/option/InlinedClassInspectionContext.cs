///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.client.option
{
	/// <summary>
	/// Provides the environment to <seealso cref="InlinedClassInspectionOption" />.
	/// </summary>
	public class InlinedClassInspectionContext
	{
		private readonly object[] classFiles;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="classFiles">class files</param>
		public InlinedClassInspectionContext(object[] classFiles)
		{
			this.classFiles = classFiles;
		}

		/// <summary>
		/// Returns the class files
		/// </summary>
		/// <returns>class files</returns>
		public object[] GetClassFiles()
		{
			return classFiles;
		}
	}
} // end of namespace
