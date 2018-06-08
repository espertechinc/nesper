///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.variable
{
    /// <summary>
    /// A convenience class for dealing with reading and updating multiple variable values.
    /// </summary>
    public class VariableReadWritePackage
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly VariableTriggerSetDesc[] _assignments;
        private readonly VariableMetaData[] _metaData;
        private readonly VariableReader[] _readersForGlobalVars;
        private readonly bool[] _mustCoerce;
        private readonly WriteDesc[] _writers;
        private readonly IDictionary<EventTypeSPI, EventBeanCopyMethod> _copyMethods;

        private readonly EventAdapterService _eventAdapterService;
        private readonly IDictionary<String, Object> _variableTypes;
        private readonly VariableService _variableService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="assignments">the list of variable assignments</param>
        /// <param name="variableService">variable service</param>
        /// <param name="eventAdapterService">event adapters</param>
        /// <throws><seealso cref="ExprValidationException" /> when variables cannot be found</throws>
        public VariableReadWritePackage(IList<OnTriggerSetAssignment> assignments, VariableService variableService, EventAdapterService eventAdapterService)
        {
            _metaData = new VariableMetaData[assignments.Count];
            _readersForGlobalVars = new VariableReader[assignments.Count];
            _mustCoerce = new bool[assignments.Count];
            _writers = new WriteDesc[assignments.Count];

            _variableTypes = new Dictionary<String, Object>();
            _eventAdapterService = eventAdapterService;
            _variableService = variableService;

            IDictionary<EventTypeSPI, CopyMethodDesc> eventTypeWrittenProps = new Dictionary<EventTypeSPI, CopyMethodDesc>();
            var count = 0;
            IList<VariableTriggerSetDesc> assignmentList = new List<VariableTriggerSetDesc>();

            foreach (var expressionWithAssignments in assignments)
            {
                var possibleVariableAssignment = ExprNodeUtility.CheckGetAssignmentToVariableOrProp(expressionWithAssignments.Expression);
                if (possibleVariableAssignment == null)
                {
                    throw new ExprValidationException("Missing variable assignment expression in assignment number " + count);
                }
                assignmentList.Add(new VariableTriggerSetDesc(possibleVariableAssignment.First, possibleVariableAssignment.Second.ExprEvaluator));

                var fullVariableName = possibleVariableAssignment.First;
                var variableName = fullVariableName;
                String subPropertyName = null;

                var indexOfDot = variableName.IndexOf('.');
                if (indexOfDot != -1)
                {
                    subPropertyName = variableName.Substring(indexOfDot + 1);
                    variableName = variableName.Substring(0, indexOfDot);
                }

                VariableMetaData variableMetadata = variableService.GetVariableMetaData(variableName);
                _metaData[count] = variableMetadata;
                if (variableMetadata == null)
                {
                    throw new ExprValidationException("Variable by name '" + variableName + "' has not been created or configured");
                }
                if (variableMetadata.IsConstant)
                {
                    throw new ExprValidationException("Variable by name '" + variableName + "' is declared constant and may not be set");
                }
                if (variableMetadata.ContextPartitionName == null)
                {
                    _readersForGlobalVars[count] = variableService.GetReader(variableName, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID);
                }

                if (subPropertyName != null)
                {
                    if (variableMetadata.EventType == null)
                    {
                        throw new ExprValidationException("Variable by name '" + variableName + "' does not have a property named '" + subPropertyName + "'");
                    }
                    var type = variableMetadata.EventType;
                    if (!(type is EventTypeSPI))
                    {
                        throw new ExprValidationException("Variable by name '" + variableName + "' event type '" + type.Name + "' not writable");
                    }
                    var spi = (EventTypeSPI)type;
                    var writer = spi.GetWriter(subPropertyName);
                    var getter = spi.GetGetter(subPropertyName);
                    if (writer == null)
                    {
                        throw new ExprValidationException("Variable by name '" + variableName + "' the property '" + subPropertyName + "' is not writable");
                    }

                    _variableTypes.Put(fullVariableName, spi.GetPropertyType(subPropertyName));
                    var writtenProps = eventTypeWrittenProps.Get(spi);
                    if (writtenProps == null)
                    {
                        writtenProps = new CopyMethodDesc(variableName, new List<String>());
                        eventTypeWrittenProps.Put(spi, writtenProps);
                    }
                    writtenProps.PropertiesCopied.Add(subPropertyName);

                    _writers[count] = new WriteDesc(spi, variableName, writer, getter);
                }
                else
                {

                    // determine types
                    var expressionType = possibleVariableAssignment.Second.ExprEvaluator.ReturnType;

                    if (variableMetadata.EventType != null)
                    {
                        if ((expressionType != null) && (!TypeHelper.IsSubclassOrImplementsInterface(expressionType, variableMetadata.EventType.UnderlyingType)))
                        {
                            throw new VariableValueException("Variable '" + variableName
                                + "' of declared event type '" + variableMetadata.EventType.Name + "' underlying type '" + variableMetadata.EventType.UnderlyingType.GetCleanName() +
                                    "' cannot be assigned a value of type '" + expressionType.GetCleanName() + "'");
                        }
                        _variableTypes.Put(variableName, variableMetadata.EventType.UnderlyingType);
                    }
                    else
                    {

                        var variableType = variableMetadata.VariableType;
                        _variableTypes.Put(variableName, variableType);

                        // determine if the expression type can be assigned
                        if (variableType != typeof(object))
                        {
                            if ((TypeHelper.GetBoxedType(expressionType) != variableType) &&
                                (expressionType != null))
                            {
                                if ((!TypeHelper.IsNumeric(variableType)) ||
                                    (!TypeHelper.IsNumeric(expressionType)))
                                {
                                    throw new ExprValidationException(VariableServiceUtil.GetAssigmentExMessage(variableName, variableType, expressionType));
                                }

                                if (!(TypeHelper.CanCoerce(expressionType, variableType)))
                                {
                                    throw new ExprValidationException(VariableServiceUtil.GetAssigmentExMessage(variableName, variableType, expressionType));
                                }

                                _mustCoerce[count] = true;
                            }
                        }
                    }
                }

                count++;
            }

            _assignments = assignmentList.ToArray();

            if (eventTypeWrittenProps.IsEmpty())
            {
                _copyMethods = new Dictionary<EventTypeSPI, EventBeanCopyMethod>();
                return;
            }

            _copyMethods = new Dictionary<EventTypeSPI, EventBeanCopyMethod>();
            foreach (var entry in eventTypeWrittenProps)
            {
                var propsWritten = entry.Value.PropertiesCopied;
                var props = propsWritten.ToArray();
                var copyMethod = entry.Key.GetCopyMethod(props);
                if (copyMethod == null)
                {
                    throw new ExprValidationException("Variable '" + entry.Value.VariableName
                        + "' of declared type " + entry.Key.UnderlyingType.GetCleanName() +
                            "' cannot be assigned to");
                }
                _copyMethods.Put(entry.Key, copyMethod);
            }
        }

        /// <summary>
        /// Write new variable values and commit, evaluating assignment expressions using the given
        /// events per stream.
        /// <para />Populates an optional map of new values if a non-null map is passed.
        /// </summary>
        /// <param name="variableService">variable service</param>
        /// <param name="eventsPerStream">events per stream</param>
        /// <param name="valuesWritten">null or an empty map to populate with written values</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        public void WriteVariables(VariableService variableService,
                                     EventBean[] eventsPerStream,
                                     IDictionary<String, Object> valuesWritten,
                                     ExprEvaluatorContext exprEvaluatorContext)
        {
            ISet<String> variablesBeansCopied = null;
            if (!_copyMethods.IsEmpty())
            {
                variablesBeansCopied = new HashSet<String>();
            }

            // We obtain a write lock global to the variable space
            // Since expressions can contain variables themselves, these need to be unchangeable for the duration
            // as there could be multiple statements that do "var1 = var1 + 1".
            using (variableService.ReadWriteLock.AcquireWriteLock())
            {
                try
                {
                    variableService.SetLocalVersion();

                    var count = 0;
                    foreach (var assignment in _assignments)
                    {
                        var variableMetaData = _metaData[count];
                        int agentInstanceId = variableMetaData.ContextPartitionName == null ? EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID : exprEvaluatorContext.AgentInstanceId;
                        var value = assignment.Evaluator.Evaluate(
                            new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));

                        if (_writers[count] != null)
                        {
                            var reader = variableService.GetReader(
                                variableMetaData.VariableName, exprEvaluatorContext.AgentInstanceId);
                            var current = (EventBean)reader.Value;
                            if (current == null)
                            {
                                value = null;
                            }
                            else
                            {
                                var writeDesc = _writers[count];
                                var copy = variablesBeansCopied.Add(writeDesc.VariableName);
                                if (copy)
                                {
                                    var copied = _copyMethods.Get(writeDesc.Type).Copy(current);
                                    current = copied;
                                }
                                variableService.Write(variableMetaData.VariableNumber, agentInstanceId, current);
                                writeDesc.Writer.Write(value, current);
                            }
                        }
                        else if (variableMetaData.EventType != null)
                        {
                            var eventBean = _eventAdapterService.AdapterForType(value, variableMetaData.EventType);
                            variableService.Write(variableMetaData.VariableNumber, agentInstanceId, eventBean);
                        }
                        else
                        {
                            if ((value != null) && (_mustCoerce[count]))
                            {
                                value = CoercerFactory.CoerceBoxed(value, variableMetaData.VariableType);
                            }
                            variableService.Write(variableMetaData.VariableNumber, agentInstanceId, value);
                        }

                        count++;

                        if (valuesWritten != null)
                        {
                            valuesWritten.Put(assignment.VariableName, value);
                        }
                    }

                    variableService.Commit();
                }
                catch (Exception ex)
                {
                    Log.Error("Error evaluating on-set variable expressions: " + ex.Message, ex);
                    variableService.Rollback();
                }
            }
        }

        /// <summary>
        /// Returns a map of variable names and type of variable.
        /// </summary>
        /// <value>variables</value>
        public IDictionary<string, object> VariableTypes
        {
            get { return _variableTypes; }
        }

        /// <summary>
        /// Iterate returning all values.
        /// </summary>
        /// <returns>map of values</returns>
        public IDictionary<String, Object> Iterate(int agentInstanceId)
        {
            IDictionary<String, Object> values = new Dictionary<String, Object>();

            var count = 0;
            foreach (var assignment in _assignments)
            {
                Object value;
                if (_readersForGlobalVars[count] == null)
                {
                    var reader = _variableService.GetReader(assignment.VariableName, agentInstanceId);
                    if (reader == null)
                    {
                        continue;
                    }
                    value = reader.Value;
                }
                else
                {
                    value = _readersForGlobalVars[count].Value;
                }

                if (value == null)
                {
                    values.Put(assignment.VariableName, null);
                }
                else if (_writers[count] != null)
                {
                    var current = (EventBean)value;
                    values.Put(assignment.VariableName, _writers[count].Getter.Get(current));
                }
                else if (value is EventBean)
                {
                    values.Put(assignment.VariableName, ((EventBean)value).Underlying);
                }
                else
                {
                    values.Put(assignment.VariableName, value);
                }
                count++;
            }
            return values;
        }

        private class CopyMethodDesc
        {
            internal readonly String VariableName;
            internal readonly IList<String> PropertiesCopied;

            public CopyMethodDesc(String variableName, IList<String> propertiesCopied)
            {
                VariableName = variableName;
                PropertiesCopied = propertiesCopied;
            }
        }

        private class WriteDesc
        {
            public WriteDesc(EventTypeSPI type, String variableName, EventPropertyWriter writer, EventPropertyGetter getter)
            {
                Type = type;
                VariableName = variableName;
                Writer = writer;
                Getter = getter;
            }

            internal readonly EventTypeSPI Type;
            internal readonly string VariableName;
            internal readonly EventPropertyWriter Writer;
            internal readonly EventPropertyGetter Getter;
        }

        private class VariableTriggerSetDesc
        {
            internal readonly String VariableName;
            internal readonly ExprEvaluator Evaluator;

            public VariableTriggerSetDesc(String variableName, ExprEvaluator evaluator)
            {
                VariableName = variableName;
                Evaluator = evaluator;
            }
        }
    }
}
