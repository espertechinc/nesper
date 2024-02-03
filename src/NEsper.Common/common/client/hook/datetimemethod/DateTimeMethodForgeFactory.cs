///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.datetimemethod
{
    /// <summary>
    ///     Date-time method extension API for adding date-time methods.
    /// </summary>
    public interface DateTimeMethodForgeFactory
    {
        /// <summary>
        ///     Called by the compiler to receive the list of footprints.
        /// </summary>
        /// <param name="context">contextual information</param>
        /// <returns>footprints</returns>
        DateTimeMethodDescriptor Initialize(DateTimeMethodInitializeContext context);

        /// <summary>
        ///     Called by the compiler to allow validation of actual parameters beyond validation of the footprint information
        ///     that the compiler does automatically.
        ///     <para />
        ///     Can be used to pre-evaluate parameter expressions.
        /// </summary>
        /// <param name="context">contextual information</param>
        /// <returns>operations descriptor</returns>
        DateTimeMethodOps Validate(DateTimeMethodValidateContext context);
    }
} // end of namespace