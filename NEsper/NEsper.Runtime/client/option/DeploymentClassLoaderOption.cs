///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.client.option
{
	/// <summary>
	/// Implement this interface to provide a custom class loader for a deployment.
	/// </summary>
	public interface DeploymentClassLoaderOption
	{
		/// <summary>
		/// Returns the classloader to use for the deployment.
		/// <para />Implementations can use the runtime's parent class loader
		/// or can use the configuration transient values that are provided by the context.
		/// </summary>
		/// <param name="env">the deployment context</param>
		/// <returns>class loader (null is not supported)</returns>
		ClassLoader GetClassLoader(DeploymentClassLoaderContext env);
	}
} // end of namespace
