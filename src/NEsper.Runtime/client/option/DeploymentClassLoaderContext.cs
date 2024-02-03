///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.runtime.client.option
{
	/// <summary>
	///     Provides the environment to <seealso cref="DeploymentTypeResolverOption" />.
	/// </summary>
	public class DeploymentClassLoaderContext
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="runtimeParentTypeResolver">runtime parent class loader</param>
	    /// <param name="configuration">configuration</param>
	    public DeploymentClassLoaderContext(
		    ParentTypeResolver runtimeParentTypeResolver,
            Configuration configuration)
        {
            RuntimeParentTypeResolver = runtimeParentTypeResolver;
            Configuration = configuration;
        }

	    /// <summary>
	    ///     Returns the classloader that is the parent class loader for the runtime.
	    /// </summary>
	    /// <value>parent class loader</value>
	    public ParentTypeResolver RuntimeParentTypeResolver { get; }

	    /// <summary>
	    ///     Returns the configuration.
	    /// </summary>
	    /// <value>configuration</value>
	    public Configuration Configuration { get; }
    }
} // end of namespace