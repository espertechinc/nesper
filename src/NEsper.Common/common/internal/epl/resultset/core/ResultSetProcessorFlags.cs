///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public class ResultSetProcessorFlags
    {
        private readonly OutputLimitSpec spec;

        public ResultSetProcessorFlags(
            bool join,
            OutputLimitSpec spec,
            ResultSetProcessorOutputConditionType outputConditionType)
        {
            IsJoin = join;
            this.spec = spec;
            OutputConditionType = outputConditionType;
        }

        public bool IsJoin { get; }

        public bool HasOutputLimit => spec != null;

        public ResultSetProcessorOutputConditionType OutputConditionType { get; }

        public bool IsOutputLimitWSnapshot => spec != null && spec.DisplayLimit == OutputLimitLimitType.SNAPSHOT;

        public bool IsOutputLimitNoSnapshot => spec != null && spec.DisplayLimit != OutputLimitLimitType.SNAPSHOT;
    }
} // end of namespace