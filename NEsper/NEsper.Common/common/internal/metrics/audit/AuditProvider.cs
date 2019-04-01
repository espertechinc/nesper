///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.metrics.audit
{
    public interface AuditProvider : AuditProviderView,
        AuditProviderStream,
        AuditProviderSchedule,
        AuditProviderProperty,
        AuditProviderInsert,
        AuditProviderExpression,
        AuditProviderPattern,
        AuditProviderPatternInstances,
        AuditProviderExprDef,
        AuditProviderDataflowTransition,
        AuditProviderDataflowSource,
        AuditProviderDataflowOp,
        AuditProviderContextPartition
    {
        bool Activated();
    }
} // end of namespace