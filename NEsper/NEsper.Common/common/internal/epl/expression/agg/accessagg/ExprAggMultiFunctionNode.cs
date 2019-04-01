///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;

namespace com.espertech.esper.common.@internal.epl.expression.agg.accessagg
{
    public interface ExprAggMultiFunctionNode : ExprEnumerationForge
    {
        void ValidatePositionals(ExprValidationContext validationContext);

        AggregationTableReadDesc ValidateAggregationTableRead(
            ExprValidationContext context, TableMetadataColumnAggregation tableAccessColumn, TableMetaData table);
    }
} // end of namespace