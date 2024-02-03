///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public enum ResultSetProcessorOutputConditionType
    {
        SNAPSHOT,
        POLICY_FIRST,
        POLICY_LASTALL_UNORDERED,
        POLICY_NONFIRST
    }

    public static class ResultSetProcessorOutputConditionTypeExtensions
    {
        public static ResultSetProcessorOutputConditionType GetConditionType(
            OutputLimitLimitType displayLimit,
            bool isAggregated,
            bool hasOrderBy,
            bool hasOptHint,
            bool isGrouped)
        {
            if (displayLimit == OutputLimitLimitType.SNAPSHOT) {
                return ResultSetProcessorOutputConditionType.SNAPSHOT;
            }
            else if (displayLimit == OutputLimitLimitType.FIRST && !isGrouped) {
                // For FIRST without groups we are using a special logic that integrates the first-flag, in order to still conveniently use all sorts of output conditions.
                // FIRST with group-by is handled by setting the output condition to null (OutputConditionNull) and letting the ResultSetProcessor handle first-per-group.
                // Without having-clause there is no required order of processing, thus also use regular policy.
                return ResultSetProcessorOutputConditionType.POLICY_FIRST;
            }
            else if (!isAggregated && !isGrouped && displayLimit == OutputLimitLimitType.LAST) {
                return ResultSetProcessorOutputConditionType.POLICY_LASTALL_UNORDERED;
            }
            else if (hasOptHint && displayLimit == OutputLimitLimitType.ALL && !hasOrderBy) {
                return ResultSetProcessorOutputConditionType.POLICY_LASTALL_UNORDERED;
            }
            else if (hasOptHint && displayLimit == OutputLimitLimitType.LAST && !hasOrderBy) {
                return ResultSetProcessorOutputConditionType.POLICY_LASTALL_UNORDERED;
            }
            else {
                return ResultSetProcessorOutputConditionType.POLICY_NONFIRST;
            }
        }

        public static bool GetOutputLimitOpt(
            Attribute[] annotations,
            Configuration configuration,
            bool hasOrderBy)
        {
            if (hasOrderBy) {
                if (HasOptHintEnable(annotations)) {
                    throw new ExprValidationException(
                        "The " + HintEnum.ENABLE_OUTPUTLIMIT_OPT + " hint is not supported with order-by");
                }

                return false;
            }

            var opt = configuration.Compiler.ViewResources.IsOutputLimitOpt;
            if (annotations == null) {
                return opt;
            }

            return opt ? HintEnum.DISABLE_OUTPUTLIMIT_OPT.GetHint(annotations) == null : HasOptHintEnable(annotations);
        }

        private static bool HasOptHintEnable(Attribute[] annotations)
        {
            return HintEnum.ENABLE_OUTPUTLIMIT_OPT.GetHint(annotations) != null;
        }
    }
} // end of namespace