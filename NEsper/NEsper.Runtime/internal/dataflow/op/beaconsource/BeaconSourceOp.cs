///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.runtime.@internal.dataflow.op.beaconsource
{
    public class BeaconSourceOp : DataFlowSourceOperator
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Pair<EventPropertyWriter, object>[] additionalProperties;
        private readonly BeaconSourceFactory factory;
        private readonly long initialDelayMSec;
        private readonly long iterations;
        private readonly long periodDelayMSec;

#pragma warning disable 649
        [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore 649

        private long iterationNumber;

        private long lastSendTime;

        public BeaconSourceOp(
            BeaconSourceFactory factory,
            long iterations,
            long initialDelayMSec,
            long periodDelayMSec,
            IDictionary<string, object> additionalParameters)
        {
            this.factory = factory;
            this.iterations = iterations;
            this.initialDelayMSec = initialDelayMSec;
            this.periodDelayMSec = periodDelayMSec;

            if (additionalParameters != null) {
                additionalProperties = new Pair<EventPropertyWriter, object>[additionalParameters.Count];
                var count = 0;
                foreach (var param in additionalParameters) {
                    EventPropertyWriter writer = ((EventTypeSPI) factory.OutputEventType).GetWriter(param.Key);
                    if (writer == null) {
                        throw new EPException(
                            "Failed to find writer for property '" + param.Key + "' for event type '" + factory.OutputEventType.Name + "'");
                    }

                    additionalProperties[count++] = new Pair<EventPropertyWriter, object>(writer, param.Value);
                }
            }
        }

        public void Next()
        {
            if (iterationNumber == 0 && initialDelayMSec > 0) {
                try {
                    Thread.Sleep((int) initialDelayMSec);
                }
                catch (ThreadInterruptedException) {
                    graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
                }
            }

            if (iterationNumber > 0 && periodDelayMSec > 0) {
                var nsecDelta = lastSendTime - PerformanceObserver.NanoTime;
                var sleepTime = periodDelayMSec - nsecDelta / 1000000;
                if (sleepTime > 0) {
                    try {
                        Thread.Sleep((int) sleepTime);
                    }
                    catch (ThreadInterruptedException) {
                        graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
                    }
                }
            }

            if (iterations > 0 && iterationNumber >= iterations) {
                graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
            }
            else {
                iterationNumber++;
                ExprEvaluator[] evaluators = factory.PropertyEvaluators;
                if (evaluators != null) {
                    var row = new object[evaluators.Length];
                    for (var i = 0; i < row.Length; i++) {
                        if (evaluators[i] != null) {
                            row[i] = evaluators[i].Evaluate(null, true, null);
                        }
                    }

                    if (log.IsDebugEnabled) {
                        log.Debug("BeaconSource submitting row " + row.RenderAny());
                    }

                    EventBeanManufacturer manufacturer = factory.Manufacturer;
                    if (manufacturer == null) {
                        SubmitAndDone(row);
                        return;
                    }

                    if (!factory.IsProduceEventBean && additionalProperties == null) {
                        var outputEvent = manufacturer.MakeUnderlying(row);
                        SubmitAndDone(outputEvent);
                        return;
                    }

                    var @event = manufacturer.Make(row);
                    if (additionalProperties != null) {
                        foreach (var pair in additionalProperties) {
                            pair.First.Write(pair.Second, @event);
                        }
                    }

                    if (!factory.IsProduceEventBean) {
                        SubmitAndDone(@event.Underlying);
                        return;
                    }

                    SubmitAndDone(@event);
                }
                else {
                    if (log.IsDebugEnabled) {
                        log.Debug("BeaconSource submitting empty row");
                    }

                    SubmitAndDone(new object[0]);
                }
            }
        }


        private void SubmitAndDone(object row)
        {
            graphContext.Submit(row);
            if (periodDelayMSec > 0)
            {
                lastSendTime = PerformanceObserver.NanoTime;
            }
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
            // no action
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
            // no action
        }
    }
} // end of namespace