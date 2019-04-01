///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.mgr
{
	public class ContextDeployTimeResolver {
	    public static string ResolveContextDeploymentId(string contextModuleName, NameAccessModifier contextVisibility, string contextName, string myDeploymentId, PathRegistry<string, ContextMetaData> pathContextRegistry) {
	        bool protectedVisibility = contextVisibility == NameAccessModifier.PRIVATE;
	        string contextDeploymentId;
	        if (protectedVisibility) {
	            contextDeploymentId = myDeploymentId;
	        } else {
	            contextDeploymentId = pathContextRegistry.GetDeploymentId(contextName, contextModuleName);
	        }
	        if (contextDeploymentId == null) {
	            throw FailedToFind(contextModuleName, contextVisibility, contextName);
	        }
	        return contextDeploymentId;
	    }

	    public static EPException FailedToFind(string contextModuleName, NameAccessModifier visibility, string contextName) {
	        bool protectedVisibility = visibility == NameAccessModifier.PRIVATE;
	        string message = "Failed find to context '" + contextName + "'";
	        if (!protectedVisibility) {
	            message += " module name '" + StringValue.UnnamedWhenNullOrEmpty(contextModuleName) + "'";
	        }
	        return new EPException(message);
	    }
	}
} // end of namespace