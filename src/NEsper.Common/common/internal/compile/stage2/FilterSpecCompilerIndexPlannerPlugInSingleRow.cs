///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexPlannerPlugInSingleRow
    {
        internal static FilterSpecParamForge HandlePlugInSingleRow(ExprPlugInSingleRowNode constituent)
        {
            var forgeEvaluationType = constituent.Forge.EvaluationType;
            if (forgeEvaluationType == null) {
                return null;
            }

            if (!forgeEvaluationType.IsTypeBoolean()) {
                return null;
            }

            if (!constituent.IsFilterLookupEligible) {
                return null;
            }

            var lookupable = constituent.FilterLookupable;
            return new FilterSpecParamConstantForge(lookupable, FilterOperator.EQUAL, true);
        }
    }
} // end of namespace