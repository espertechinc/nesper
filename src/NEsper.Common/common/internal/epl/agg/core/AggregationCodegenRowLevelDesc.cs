///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationCodegenRowLevelDesc
    {
        public static readonly AggregationCodegenRowLevelDesc EMPTY = new AggregationCodegenRowLevelDesc(null, null);

        public AggregationCodegenRowLevelDesc(
            AggregationCodegenRowDetailDesc optionalTopRow,
            AggregationCodegenRowDetailDesc[] optionalAdditionalRows)
        {
            OptionalTopRow = optionalTopRow;
            OptionalAdditionalRows = optionalAdditionalRows;
        }

        public AggregationCodegenRowDetailDesc OptionalTopRow { get; }

        public AggregationCodegenRowDetailDesc[] OptionalAdditionalRows { get; }

        public static AggregationCodegenRowLevelDesc FromTopOnly(AggregationRowStateForgeDesc rowStateDesc)
        {
            var state = new AggregationCodegenRowDetailStateDesc(
                rowStateDesc.MethodForges,
                rowStateDesc.OptionalMethodFactories,
                rowStateDesc.AccessFactoriesForges);
            var top = new AggregationCodegenRowDetailDesc(state, rowStateDesc.AccessAccessorsForges, null);
            return new AggregationCodegenRowLevelDesc(top, null);
        }
    }
} // end of namespace