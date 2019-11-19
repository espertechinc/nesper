///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// For use in data flow instantiation, may provide operator instances.
    /// </summary>
    public interface EPDataFlowOperatorProvider
    {
        /// <summary>
        /// Called to see if the provider would like to provide the operator instance as described in the context.
        /// </summary>
        /// <param name="context">operator instance requested</param>
        /// <returns>
        /// operator instance, or null if the default empty construct instantiation for the operator class should be used
        /// </returns>
        object Provide(EPDataFlowOperatorProviderContext context);
    }
}