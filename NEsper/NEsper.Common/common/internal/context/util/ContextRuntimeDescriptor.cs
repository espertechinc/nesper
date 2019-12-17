///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.context.util
{
    public class ContextRuntimeDescriptor
    {
        public ContextRuntimeDescriptor(
            string contextName,
            string contextDeploymentId,
            ContextIteratorHandler enumeratorHandler)
        {
            ContextName = contextName;
            ContextDeploymentId = contextDeploymentId;
            EnumeratorHandler = enumeratorHandler;
        }

        public ContextIteratorHandler EnumeratorHandler { get; }

        public string ContextName { get; }

        public string ContextDeploymentId { get; }
    }
} // end of namespace