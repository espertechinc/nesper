///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    public interface FilterSharedLookupableRepository
    {
        void RegisterLookupable(
            int statementId,
            EventType eventType,
            ExprFilterSpecLookupable lookupable);

        void RemoveReferencesStatement(int statementId);

        void ApplyLookupableFromType(
            EventType asEventType,
            EventType eventType,
            int statementId);
    }
} // end of namespace