///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// A view that handles the "output snapshot" keyword in output rate stabilizing.
    /// </summary>
    public class OutputProcessViewConditionFactory : OutputProcessViewDirectDistinctOrAfterFactory
    {
        #region ConditionType enum

        public enum ConditionType
        {
            SNAPSHOT,
            POLICY_FIRST,
            POLICY_NONFIRST
        }

        #endregion

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConditionType _conditionType;
        private readonly OutputConditionFactory _outputConditionFactory;
        private readonly OutputLimitLimitType _outputLimitLimitType;
        private readonly int _streamCount;
        private readonly bool _terminable;

        public OutputProcessViewConditionFactory(
            StatementContext statementContext,
            OutputStrategyPostProcessFactory postProcessFactory,
            bool distinct,
            ExprTimePeriod afterTimePeriod,
            int? afterConditionNumberOfEvents,
            EventType resultEventType,
            OutputConditionFactory outputConditionFactory,
            int streamCount,
            ConditionType conditionType,
            OutputLimitLimitType outputLimitLimitType,
            bool terminable)
            : base(statementContext, postProcessFactory, distinct, afterTimePeriod, afterConditionNumberOfEvents, resultEventType)
        {
            _outputConditionFactory = outputConditionFactory;
            _streamCount = streamCount;
            _conditionType = conditionType;
            _outputLimitLimitType = outputLimitLimitType;
            _terminable = terminable;
        }

        public OutputConditionFactory OutputConditionFactory
        {
            get { return _outputConditionFactory; }
        }

        public int StreamCount
        {
            get { return _streamCount; }
        }

        public OutputLimitLimitType OutputLimitLimitType
        {
            get { return _outputLimitLimitType; }
        }

        public bool IsTerminable
        {
            get { return _terminable; }
        }

        public override OutputProcessViewBase MakeView(ResultSetProcessor resultSetProcessor,
                                                       AgentInstanceContext agentInstanceContext)
        {
            // determine after-stuff
            bool isAfterConditionSatisfied = true;
            long? afterConditionTime = null;
            if (AfterConditionNumberOfEvents != null)
            {
                isAfterConditionSatisfied = false;
            }
            else if (AfterTimePeriod != null)
            {
                isAfterConditionSatisfied = false;
                long delta = AfterTimePeriod.NonconstEvaluator().DeltaMillisecondsUseEngineTime(null, agentInstanceContext);
                afterConditionTime = agentInstanceContext.StatementContext.TimeProvider.Time + delta;
            }

            if (_conditionType == ConditionType.SNAPSHOT)
            {
                if (PostProcessFactory == null)
                {
                    return new OutputProcessViewConditionSnapshot(resultSetProcessor, afterConditionTime,
                                                                  AfterConditionNumberOfEvents,
                                                                  isAfterConditionSatisfied, this, agentInstanceContext);
                }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionSnapshotPostProcess(resultSetProcessor, afterConditionTime,
                                                                         AfterConditionNumberOfEvents,
                                                                         isAfterConditionSatisfied, this,
                                                                         agentInstanceContext, postProcess);
            }
            if (_conditionType == ConditionType.POLICY_FIRST)
            {
                if (PostProcessFactory == null)
                {
                    return new OutputProcessViewConditionFirst(resultSetProcessor, afterConditionTime,
                                                               AfterConditionNumberOfEvents, isAfterConditionSatisfied,
                                                               this, agentInstanceContext);
                }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionFirstPostProcess(resultSetProcessor, afterConditionTime,
                                                                      AfterConditionNumberOfEvents,
                                                                      isAfterConditionSatisfied, this,
                                                                      agentInstanceContext, postProcess);
            }
            else
            {
                if (PostProcessFactory == null)
                {
                    return new OutputProcessViewConditionDefault(resultSetProcessor, afterConditionTime,
                                                                 AfterConditionNumberOfEvents, isAfterConditionSatisfied,
                                                                 this, agentInstanceContext);
                }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionDefaultPostProcess(resultSetProcessor, afterConditionTime,
                                                                        AfterConditionNumberOfEvents,
                                                                        isAfterConditionSatisfied, this,
                                                                        agentInstanceContext, postProcess);
            }
        }
    }
}