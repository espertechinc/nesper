///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// A listener interface for callbacks regarding <seealso cref="EPRuntime" /> state changes.
    /// </summary>
    public interface EPRuntimeStateListener
    {
        /// <summary>
        /// Invoked before an <seealso cref="EPRuntime" /> is destroyed.
        /// </summary>
        /// <param name="runtime">runtime to be destroyed</param>
        void OnEPRuntimeDestroyRequested(EPRuntime runtime);

        /// <summary>
        /// Invoked after an existing <seealso cref="EPRuntime" /> is initialized upon completion of a call to initialize.
        /// </summary>
        /// <param name="runtime">runtime that has been successfully initialized</param>
        void OnEPRuntimeInitialized(EPRuntime runtime);
    }
} // end of namespace