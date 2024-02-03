///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.recompile
{
    /// <summary>
    /// Provider for a re-compiler that acts on existing deployment to either re-compile or re-load from an external source
    /// </summary>
    public interface EPRecompileProvider
    {
        /// <summary>
        /// Provide compiler output
        /// </summary>
        /// <param name="context">deployment information</param>
        /// <returns>compiler output</returns>
        /// <throws>EPRecompileProviderException to indicate that compiler output cannot be obtained</throws>
        EPCompiled Provide(EPRecompileProviderContext context);
    }
} // end of namespace