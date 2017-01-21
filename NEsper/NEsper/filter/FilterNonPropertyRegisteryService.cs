///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.filter
{
    /// <summary>
    ///     Service to provide engine-wide access to filter expressions that do not originate from
    ///     event property values, i.e. expressions that cannot be reproduced by obtaining a getter from the event type.
    /// </summary>
    public interface FilterNonPropertyRegisteryService
    {
        /// <summary>
        ///     Register expression.
        /// </summary>
        /// <param name="statementName">statement name</param>
        /// <param name="eventType">event type</param>
        /// <param name="lookupable">filter expression</param>
        void RegisterNonPropertyExpression(string statementName, EventType eventType, FilterSpecLookupable lookupable);

        /// <summary>
        ///     Obtain expression
        /// </summary>
        /// <param name="eventTypeName">event type name</param>
        /// <param name="expression">expression text</param>
        /// <returns>lookupable</returns>
        FilterSpecLookupable GetNonPropertyExpression(string eventTypeName, string expression);

        /// <summary>
        ///     Remove references to expression
        /// </summary>
        /// <param name="statementName">statement name</param>
        void RemoveReferencesStatement(string statementName);
    }
} // end of namespace