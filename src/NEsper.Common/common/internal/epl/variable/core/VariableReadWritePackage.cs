///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.context.util.StatementCPCacheService;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    /// A convenience class for dealing with reading and updating multiple variable values.
    /// </summary>
    public class VariableReadWritePackage
    {
        private IDictionary<EventTypeSPI, EventBeanCopyMethod> copyMethods;
        private VariableTriggerSetDesc[] assignments;
        private VariableTriggerWrite[] writers;
        private Variable[] variables;
        private bool[] mustCoerce;
        private VariableReader[] readersForGlobalVars;

        /// <summary>
        /// Write new variable values and commit, evaluating assignment expressions using the given
        /// events per stream.
        /// <para/>Populates an optional map of new values if a non-null map is passed.
        /// </summary>
        /// <param name = "eventsPerStream">events per stream</param>
        /// <param name = "valuesWritten">null or an empty map to populate with written values</param>
        /// <param name = "exprEvaluatorContext">expression evaluation context</param>
        public void WriteVariables(
            EventBean[] eventsPerStream,
            IDictionary<string, object> valuesWritten,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ISet<string> variablesBeansCopied = null;
            var variableService = exprEvaluatorContext.VariableManagementService;
            if (!copyMethods.IsEmpty()) {
                variablesBeansCopied = new HashSet<string>();
            }

            // We obtain a write lock global to the variable space
            // Since expressions can contain variables themselves, these need to be unchangeable for the duration
            // as there could be multiple statements that do "var1 = var1 + 1".

            using (variableService.ReadWriteLock.AcquireWriteLock()) {
                try {
                    variableService.SetLocalVersion();
                    var count = 0;
                    foreach (var assignment in assignments) {
                        var variable = variables[count];
                        var variableMetaData = variable.MetaData;
                        var agentInstanceId = variableMetaData.OptionalContextName == null
                            ? DEFAULT_AGENT_INSTANCE_ID
                            : exprEvaluatorContext.AgentInstanceId;
                        var variableNumber = variable.VariableNumber;
                        var writeBase = writers[count];
                        object written;
                        if (writeBase is VariableTriggerWriteDesc desc) {
                            var reader = variableService.GetReader(
                                variables[count].DeploymentId,
                                variableMetaData.VariableName,
                                agentInstanceId);
                            var current = (EventBean)reader.Value;
                            var value = assignment.Evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                            written = value;
                            if (current != null) {
                                var copy = variablesBeansCopied.Add(desc.VariableName);
                                if (copy) {
                                    current = copyMethods
                                        .Get((EventTypeSPI) desc.Type)
                                        .Copy(current);
                                }

                                variableService.Write(variableNumber, agentInstanceId, current);
                                desc.Writer.Write(value, current);
                            }
                        }
                        else if (writeBase is VariableTriggerWriteArrayElement @base) {
                            var index = @base.IndexExpression
                                .Evaluate(eventsPerStream, true, exprEvaluatorContext)
                                .AsBoxedInt32();
                            var reader = variableService.GetReader(
                                variables[count].DeploymentId,
                                variableMetaData.VariableName,
                                agentInstanceId);
                            
                            var arrayValue = (Array) reader.Value;
                            written = arrayValue;
                            if (index != null) {
                                if (arrayValue != null) {
                                    var len = arrayValue.Length;
                                    if (index < len) {
                                        var value = assignment.Evaluator.Evaluate(
                                            eventsPerStream,
                                            true,
                                            exprEvaluatorContext);
                                        if (@base.TypeWidener != null) {
                                            value = @base.TypeWidener.Widen(value);
                                        }

                                        var arrayType = arrayValue.GetType();
                                        var arrayTypeElement = arrayType.GetElementType();
                                        if (value != null || !arrayTypeElement!.IsPrimitive) {
                                            arrayValue.SetValue(value, index.Value);
                                        }

                                        variableService.Write(variableNumber, agentInstanceId, arrayValue);
                                    }
                                    else {
                                        throw new EPException(
                                            "Array length " +
                                            len +
                                            " less than index " +
                                            index +
                                            " for variable '" +
                                            @base.VariableName +
                                            "'");
                                    }
                                }
                            }
                        }
                        else if (writeBase is VariableTriggerWriteCurly writeDesc) {
                            var reader = variableService.GetReader(
                                variables[count].DeploymentId,
                                variableMetaData.VariableName,
                                agentInstanceId);
                            var value = reader.Value;
                            writeDesc.Expression.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                            variableService.Write(variableNumber, agentInstanceId, value);
                            written = value;
                        }
                        else if (variableMetaData.EventType != null) {
                            var eventType = variableMetaData.EventType;
                            var value = assignment.Evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                            var eventBean =
                                exprEvaluatorContext.EventBeanTypedEventFactory.AdapterForGivenType(
                                    value,
                                    variableMetaData.EventType);
                            variableService.Write(variableNumber, agentInstanceId, eventBean);
                            written = value;
                        }
                        else {
                            var value = assignment.Evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                            if (value != null && mustCoerce[count]) {
                                value = TypeHelper.CoerceBoxed(value, variableMetaData.Type);
                            }

                            variableService.Write(variableNumber, agentInstanceId, value);
                            written = value;
                        }

                        count++;
                        if (valuesWritten != null) {
                            valuesWritten.Put(assignment.VariableName, written);
                        }
                    }

                    variableService.Commit();
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception ex) {
                    variableService.Rollback();
                    throw new EPException("Failed variable write: " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Iterate returning all values.
        /// </summary>
        /// <param name = "variableManagementService">variable management</param>
        /// <param name = "agentInstanceId">context partition id</param>
        /// <returns>map of values</returns>
        public IDictionary<string, object> Iterate(
            VariableManagementService variableManagementService,
            int agentInstanceId)
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            var count = 0;
            foreach (var assignment in assignments) {
                object value;
                if (readersForGlobalVars[count] == null) {
                    var reader = variableManagementService.GetReader(
                        variables[count].DeploymentId,
                        assignment.VariableName,
                        agentInstanceId);
                    if (reader == null) {
                        continue;
                    }

                    value = reader.Value;
                }
                else {
                    value = readersForGlobalVars[count].Value;
                }

                if (value == null) {
                    values.Put(assignment.VariableName, null);
                }
                else if (writers[count] is VariableTriggerWriteDesc) {
                    var desc = (VariableTriggerWriteDesc)writers[count];
                    var current = (EventBean)value;
                    values.Put(assignment.VariableName, desc.Getter.Get(current));
                }
                else if (value is EventBean bean) {
                    values.Put(assignment.VariableName, bean.Underlying);
                }
                else {
                    values.Put(assignment.VariableName, value);
                }

                count++;
            }

            return values;
        }

        public IDictionary<EventTypeSPI, EventBeanCopyMethod> CopyMethods {
            set => copyMethods = value;
        }

        public VariableTriggerSetDesc[] Assignments {
            set => assignments = value;
        }

        public VariableTriggerWrite[] Writers {
            set => writers = value;
        }

        public Variable[] Variables {
            set => variables = value;
        }

        public bool[] MustCoerce {
            set => mustCoerce = value;
        }

        public VariableReader[] ReadersForGlobalVars {
            set => readersForGlobalVars = value;
        }
    }
} // end of namespace