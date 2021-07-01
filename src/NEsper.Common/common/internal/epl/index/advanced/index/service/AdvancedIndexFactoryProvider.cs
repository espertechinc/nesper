///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.service
{
    public interface AdvancedIndexFactoryProvider
    {
        EventAdvancedIndexProvisionCompileTime ValidateEventIndex(
            string indexName,
            string indexTypeName,
            ExprNode[] columns,
            ExprNode[] parameters);

        AdvancedIndexConfigContextPartition ValidateConfigureFilterIndex(
            string indexName,
            string indexTypeName,
            ExprNode[] parameters,
            ExprValidationContext validationContext);
    }
} // end of namespace