///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.condition
{
    /// <summary>
    /// Factory for engine condition handler Instance(s).
    /// <para/>
    /// Receives CEP engine contextual information and should return an implementation 
    /// of the <see cref="ConditionHandler" /> interface.
    /// </summary>
    public interface ConditionHandlerFactory
    {
        /// <summary>
        /// Returns an exception handler instances, or null if the factory decided not 
        /// to contribute an exception handler.
        /// </summary>
        /// <param name="context">contains the engine URI</param>
        /// <returns>exception handler</returns>
        ConditionHandler GetHandler(ConditionHandlerFactoryContext context);
    }
}