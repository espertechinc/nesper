///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using System.Threading;

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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Pair<EventPropertyWriter, object>[] _additionalProperties;
        private readonly BeaconSourceFactory _factory;
        private readonly long _initialDelayMSec;
        private readonly long _iterations;
        private readonly long _periodDelayMSec;

#pragma warning disable 649
        [DataFlowContext] private EPDataFlowEmitter _graphContext;
#pragma warning restore 649

        private long _iterationNumber;

        private long _lastSendTime;

        public BeaconSourceOp(
            BeaconSourceFactory factory,
            long iterations,
            long initialDelayMSec,
            long periodDelayMSec,
            IDictionary<string, object> additionalParameters)
        {
            this._factory = factory;
            this._iterations = iterations;
            this._initialDelayMSec = initialDelayMSec;
            this._periodDelayMSec = periodDelayMSec;

            if (additionalParameters != null) {
                _additionalProperties = new Pair<EventPropertyWriter, object>[additionalParameters.Count];
                var count = 0;
                foreach (var param in additionalParameters) {
                    EventPropertyWriter writer = ((EventTypeSPI) factory.OutputEventType).GetWriter(param.Key);
                    if (writer == null) {
                        throw new EPException(
                            "Failed to find writer for property '" + param.Key + "' for event type '" + factory.OutputEventType.Name + "'");
                    }

                    _additionalProperties[count++] = new Pair<EventPropertyWriter, object>(writer, param.Value);
                }
            }
        }

        public void Next()
        {
            if (_iterationNumber == 0 && _initialDelayMSec > 0) {
                try {
                    Thread.Sleep((int) _initialDelayMSec);
                }
                catch (ThreadInterruptedException) {
                    _graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
                }
            }

            if (_iterationNumber > 0 && _periodDelayMSec > 0) {
                var nsecDelta = _lastSendTime - PerformanceObserver.NanoTime;
                var sleepTime = _periodDelayMSec - nsecDelta / 1000000;
                if (sleepTime > 0) {
                    try {
                        Thread.Sleep((int) sleepTime);
                    }
                    catch (ThreadInterruptedException) {
                        _graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
                    }
                }
            }

            if (_iterations > 0 && _iterationNumber >= _iterations) {
                _graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
            }
            else {
                _iterationNumber++;
                ExprEvaluator[] evaluators = _factory.PropertyEvaluators;
                if (evaluators != null) {
                    var row = new object[evaluators.Length];
                    for (var i = 0; i < row.Length; i++) {
                        if (evaluators[i] != null) {
                            row[i] = evaluators[i].Evaluate(null, true, null);
                        }
                    }

                    if (Log.IsDebugEnabled) {
                        Log.Debug("BeaconSource submitting row " + row.RenderAny());
                    }

                    EventBeanManufacturer manufacturer = _factory.Manufacturer;
                    if (manufacturer == null) {
                        SubmitAndDone(row);
                        return;
                    }

                    if (!_factory.IsProduceEventBean && _additionalProperties == null) {
                        var outputEvent = manufacturer.MakeUnderlying(row);
                        SubmitAndDone(outputEvent);
                        return;
                    }

                    var @event = manufacturer.Make(row);
                    if (_additionalProperties != null) {
                        foreach (var pair in _additionalProperties) {
                            pair.First.Write(pair.Second, @event);
                        }
                    }

                    if (!_factory.IsProduceEventBean) {
                        SubmitAndDone(@event.Underlying);
                        return;
                    }

                    SubmitAndDone(@event);
                }
                else {
                    if (Log.IsDebugEnabled) {
                        Log.Debug("BeaconSource submitting empty row");
                    }

                    SubmitAndDone(new object[0]);
                }
            }
        }


        private void SubmitAndDone(object row)
        {
            _graphContext.Submit(row);
            if (_periodDelayMSec > 0)
            {
                _lastSendTime = PerformanceObserver.NanoTime;
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