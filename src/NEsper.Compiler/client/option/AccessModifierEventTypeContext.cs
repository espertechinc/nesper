///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Provides the environment to <seealso cref="AccessModifierEventTypeOption" />.
    /// </summary>
    public class AccessModifierEventTypeContext : StatementOptionContextBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="raw">statement name</param>
        /// <param name="eventTypeName">event type name</param>
        public AccessModifierEventTypeContext(
            StatementRawInfo raw,
            string eventTypeName)
            : base(raw)
        {
            EventTypeName = eventTypeName;
        }

        /// <summary>
        ///     Returns the event type name
        /// </summary>
        /// <returns>event type name</returns>
        public string EventTypeName { get; }
    }
} // end of namespace