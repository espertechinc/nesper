///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.context.util.StatementCPCacheService;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    ///     A convenience class for dealing with reading and updating multiple variable values.
    /// </summary>
    public class VariableReadWritePackage
    {
        private VariableTriggerSetDesc[] assignments;

        private IDictionary<EventTypeSPI, EventBeanCopyMethod> copyMethods;
        private bool[] mustCoerce;
        private VariableReader[] readersForGlobalVars;
        private Variable[] variables;
        private VariableTriggerWriteDesc[] writers;

        public IDictionary<EventTypeSPI, EventBeanCopyMethod> CopyMethods {
            set => copyMethods = value;
        }

        public VariableTriggerSetDesc[] Assignments {
            set => assignments = value;
        }

        public VariableTriggerWriteDesc[] Writers {
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

        /// <summary>
        ///     Write new variable values and commit, evaluating assignment expressions using the given
        ///     events per stream.
        ///     <para />
        ///     Populates an optional map of new values if a non-null map is passed.
        /// </summary>
        /// <param name="eventsPerStream">events per stream</param>
        /// <param name="valuesWritten">null or an empty map to populate with written values</param>
        /// <param name="agentInstanceContext">expression evaluation context</param>
        public void WriteVariables(
            EventBean[] eventsPerStream,
            IDictionary<string, object> valuesWritten,
            AgentInstanceContext agentInstanceContext)
        {
            ISet<string> variablesBeansCopied = null;
            var variableService = agentInstanceContext.VariableManagementService;
            if (!copyMethods.IsEmpty()) {
                variablesBeansCopied = new HashSet<string>();
            }

            // We obtain a write lock global to the variable space
            // Since expressions can contain variables themselves, these need to be unchangeable for the duration
            // as there could be multiple statements that do "var1 = var1 + 1".
            using (variableService.ReadWriteLock.WriteLock.Acquire()) {
                try {
                    variableService.SetLocalVersion();

                    var count = 0;
                    foreach (var assignment in assignments) {
                        var variable = variables[count];
                        var variableMetaData = variable.MetaData;
                        var agentInstanceId = variableMetaData.OptionalContextName == null
                            ? DEFAULT_AGENT_INSTANCE_ID
                            : agentInstanceContext.AgentInstanceId;
                        var value = assignment.Evaluator.Evaluate(eventsPerStream, true, agentInstanceContext);
                        var variableNumber = variable.VariableNumber;

                        if (writers[count] != null) {
                            var reader = variableService.GetReader(
                                variables[count].DeploymentId, variableMetaData.VariableName, agentInstanceId);
                            var current = (EventBean) reader.Value;
                            if (current == null) {
                                value = null;
                            }
                            else {
                                var writeDesc = writers[count];
                                var copy = variablesBeansCopied.Add(writeDesc.VariableName);
                                if (copy) {
                                    current = copyMethods.Get(writeDesc.Type).Copy(current);
                                }

                                variableService.Write(variableNumber, agentInstanceId, current);
                                writeDesc.Writer.Write(value, current);
                            }
                        }
                        else if (variableMetaData.EventType != null) {
                            var eventBean =
                                agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedBean(
                                    value, variableMetaData.EventType);
                            variableService.Write(variableNumber, agentInstanceId, eventBean);
                        }
                        else {
                            if (value != null && mustCoerce[count]) {
                                value = TypeHelper.CoerceBoxed(value, variableMetaData.Type);
                            }

                            variableService.Write(variableNumber, agentInstanceId, value);
                        }

                        count++;

                        if (valuesWritten != null) {
                            valuesWritten.Put(assignment.VariableName, value);
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
        ///     Iterate returning all values.
        /// </summary>
        /// <param name="variableManagementService">variable management</param>
        /// <param name="agentInstanceId">context partition id</param>
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
                        variables[count].DeploymentId, assignment.VariableName, agentInstanceId);
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
                else if (writers[count] != null) {
                    var current = (EventBean) value;
                    values.Put(assignment.VariableName, writers[count].Getter.Get(current));
                }
                else if (value is EventBean) {
                    values.Put(assignment.VariableName, ((EventBean) value).Underlying);
                }
                else {
                    values.Put(assignment.VariableName, value);
                }

                count++;
            }

            return values;
        }
    }
} // end of namespace