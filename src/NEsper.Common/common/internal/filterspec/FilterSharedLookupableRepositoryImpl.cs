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
    public class FilterSharedLookupableRepositoryImpl : FilterSharedLookupableRepository
    {
        public static readonly FilterSharedLookupableRepositoryImpl INSTANCE =
            new FilterSharedLookupableRepositoryImpl();

        private FilterSharedLookupableRepositoryImpl()
        {
        }

        public void RegisterLookupable(
            int statementId,
            EventType eventType,
            ExprFilterSpecLookupable lookupable)
        {
            // not required
        }

        public void RemoveReferencesStatement(int statementId)
        {
            // not required
        }

        public void ApplyLookupableFromType(
            EventType asEventType,
            EventType eventType,
            int statementId)
        {
            // not required
        }
    }
} // end of namespace