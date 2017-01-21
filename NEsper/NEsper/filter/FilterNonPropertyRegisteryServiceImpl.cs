///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.filter
{
    public class FilterNonPropertyRegisteryServiceImpl : FilterNonPropertyRegisteryService
    {
        public void RegisterNonPropertyExpression(
            string statementName,
            EventType eventType,
            FilterSpecLookupable lookupable)
        {
            // default implementation, no action required
        }

        public FilterSpecLookupable GetNonPropertyExpression(string eventTypeName, string expression)
        {
            // default implementation, no action required
            throw new UnsupportedOperationException();
        }

        public void RemoveReferencesStatement(string statementName)
        {
            // default implementation, no action required
        }
    }
} // end of namespace