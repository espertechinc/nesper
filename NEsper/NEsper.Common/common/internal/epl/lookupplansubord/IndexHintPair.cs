///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.@join.hint;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class IndexHintPair
    {
        public IndexHintPair(
            IndexHint indexHint,
            ExcludePlanHint excludePlanHint)
        {
            IndexHint = indexHint;
            ExcludePlanHint = excludePlanHint;
        }

        public IndexHint IndexHint { get; }

        public ExcludePlanHint ExcludePlanHint { get; }

        public static IndexHintPair GetIndexHintPair(
            OnTriggerDesc onTriggerDesc,
            string streamZeroAsName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            IndexHint indexHint = IndexHint.GetIndexHint(statementRawInfo.Annotations);
            ExcludePlanHint excludePlanHint = null;
            if (onTriggerDesc is OnTriggerWindowDesc) {
                var onTriggerWindowDesc = (OnTriggerWindowDesc) onTriggerDesc;
                string[] streamNames = {onTriggerWindowDesc.OptionalAsName, streamZeroAsName};
                excludePlanHint = ExcludePlanHint.GetHint(streamNames, statementRawInfo, services);
            }

            return new IndexHintPair(indexHint, excludePlanHint);
        }
    }
} // end of namespace