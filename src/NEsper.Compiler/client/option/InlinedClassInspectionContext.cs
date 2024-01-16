///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.client.option
{
	/// <summary>
	/// Provides the environment to <seealso cref="InlinedClassInspectionOption" />.
	/// </summary>
	public class InlinedClassInspectionContext
	{
		private readonly IArtifact _artifact;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="artifact">artifact</param>
		public InlinedClassInspectionContext(IArtifact artifact)
		{
			_artifact = artifact;
		}

		public IArtifact Artifact => _artifact;
	}
} // end of namespace
