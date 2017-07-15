///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.dataflow.ops
{
    [DataFlowOperator]
    public class Filter : DataFlowOpLifecycle {
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        [DataFlowOpParameter]
        private ExprNode filter;
    
        private ExprEvaluator evaluator;
        private EventBeanSPI theEvent;
        private EventBean[] eventsPerStream = new EventBean[1];
        private bool singleOutputPort;
    
        [DataFlowContext]
        private EPDataFlowEmitter graphContext;
    
        public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext prepareContext) {
    
            if (prepareContext.InputPorts.Count != 1) {
                throw new ExprValidationException("Filter requires single input port");
            }
            if (filter == null) {
                throw new ExprValidationException("Required parameter 'filter' providing the filter expression is not provided");
            }
            if (prepareContext.OutputPorts.IsEmpty() || prepareContext.OutputPorts.Count > 2) {
                throw new ArgumentException("Filter operator requires one or two output Stream(s) but produces " + prepareContext.OutputPorts.Count + " streams");
            }
    
            EventType eventType = prepareContext.InputPorts[0].TypeDesc.EventType;
            singleOutputPort = prepareContext.OutputPorts.Count == 1;
    
            ExprNode validated = ExprNodeUtility.ValidateSimpleGetSubtree(ExprNodeOrigin.DATAFLOWFILTER, filter, prepareContext.StatementContext, eventType, false);
            evaluator = validated.ExprEvaluator;
            theEvent = prepareContext.ServicesContext.EventAdapterService.GetShellForType(eventType);
            eventsPerStream[0] = theEvent;
    
            var typesPerPort = new GraphTypeDesc[prepareContext.OutputPorts.Count];
            for (int i = 0; i < typesPerPort.Length; i++) {
                typesPerPort[i] = new GraphTypeDesc(false, true, eventType);
            }
            return new DataFlowOpInitializeResult(typesPerPort);
        }
    
        public void OnInput(Object row) {
            if (Log.IsDebugEnabled) {
                Log.Debug("Received row for filtering: " + Arrays.ToString((Object[]) row));
            }
    
            if (!(row is EventBeanSPI)) {
                theEvent.Underlying = row;
            } else {
                theEvent = (EventBeanSPI) row;
            }
    
            bool? pass = (bool?) evaluator.Evaluate(eventsPerStream, true, null);
            if (pass != null && pass) {
                if (Log.IsDebugEnabled) {
                    Log.Debug("Submitting row " + Arrays.ToString((Object[]) row));
                }
    
                if (singleOutputPort) {
                    graphContext.Submit(row);
                } else {
                    graphContext.SubmitPort(0, row);
                }
            } else {
                if (!singleOutputPort) {
                    graphContext.SubmitPort(1, row);
                }
            }
        }
    
        public void Open(DataFlowOpOpenContext openContext) {
            // no action
        }
    
        public void Close(DataFlowOpCloseContext openContext) {
            // no action
        }
    }
} // end of namespace
