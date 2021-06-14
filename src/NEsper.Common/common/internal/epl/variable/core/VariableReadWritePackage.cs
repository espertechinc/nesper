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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.context.util.StatementCPCacheService; //DEFAULT_AGENT_INSTANCE_ID

namespace com.espertech.esper.common.@internal.epl.variable.core
{
	/// <summary>
	/// A convenience class for dealing with reading and updating multiple variable values.
	/// </summary>
	public class VariableReadWritePackage
	{

		private IDictionary<EventTypeSPI, EventBeanCopyMethod> _copyMethods;
		private VariableTriggerSetDesc[] _assignments;
		private VariableTriggerWrite[] _writers;
		private Variable[] _variables;
		private bool[] _mustCoerce;
		private VariableReader[] _readersForGlobalVars;

		public IDictionary<EventTypeSPI, EventBeanCopyMethod> CopyMethods {
			get => _copyMethods;
			set => _copyMethods = value;
		}

		public VariableTriggerSetDesc[] Assignments {
			get => _assignments;
			set => _assignments = value;
		}

		public VariableTriggerWrite[] Writers {
			get => _writers;
			set => _writers = value;
		}

		public Variable[] Variables {
			get => _variables;
			set => _variables = value;
		}

		public bool[] MustCoerce {
			get => _mustCoerce;
			set => _mustCoerce = value;
		}

		public VariableReader[] ReadersForGlobalVars {
			get => _readersForGlobalVars;
			set => _readersForGlobalVars = value;
		}

		/// <summary>
		/// Write new variable values and commit, evaluating assignment expressions using the given
		/// events per stream.
		/// <para />Populates an optional map of new values if a non-null map is passed.
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
			if (!_copyMethods.IsEmpty()) {
				variablesBeansCopied = new HashSet<string>();
			}

			// We obtain a write lock global to the variable space
			// Since expressions can contain variables themselves, these need to be unchangeable for the duration
			// as there could be multiple statements that do "var1 = var1 + 1".
			using (variableService.ReadWriteLock.WriteLock.Acquire()) {
				try {
					variableService.SetLocalVersion();

					var count = 0;
					foreach (var assignment in _assignments) {
						var variable = _variables[count];
						var variableMetaData = variable.MetaData;
						var agentInstanceId = variableMetaData.OptionalContextName == null ? DEFAULT_AGENT_INSTANCE_ID : agentInstanceContext.AgentInstanceId;
						var variableNumber = variable.VariableNumber;
						var writeBase = _writers[count];
						object written;

						if (writeBase is VariableTriggerWriteDesc) {
							var writeDesc = (VariableTriggerWriteDesc) writeBase;
							var reader = variableService.GetReader(_variables[count].DeploymentId, variableMetaData.VariableName, agentInstanceId);
							var current = (EventBean) reader.Value;
							var value = assignment.Evaluator.Evaluate(eventsPerStream, true, agentInstanceContext);
							written = value;
							if (current != null) {
								var copy = variablesBeansCopied.Add(writeDesc.VariableName);
								if (copy) {
									current = _copyMethods
										.Get((EventTypeSPI) writeDesc.Type)
										.Copy(current);
								}

								variableService.Write(variableNumber, agentInstanceId, current);
								writeDesc.Writer.Write(value, current);
							}
						}
						else if (writeBase is VariableTriggerWriteArrayElement) {
							var writeDesc = (VariableTriggerWriteArrayElement) writeBase;
							var index = (int?) writeDesc.IndexExpression.Evaluate(eventsPerStream, true, agentInstanceContext);
							var reader = variableService.GetReader(_variables[count].DeploymentId, variableMetaData.VariableName, agentInstanceId);
							var arrayValue = (Array) reader.Value;
							written = arrayValue;
							if (index != null) {
								if (arrayValue != null) {
									var len = arrayValue.Length;
									if (index < len) {
										var value = assignment.Evaluator.Evaluate(eventsPerStream, true, agentInstanceContext);
										if (writeDesc.TypeWidener != null) {
											value = writeDesc.TypeWidener.Widen(value);
										}

										if (value != null || !arrayValue.GetType().GetElementType().IsPrimitive) {
											arrayValue.SetValue(value, index.Value);
										}

										variableService.Write(variableNumber, agentInstanceId, arrayValue);
									}
									else {
										throw new EPException(
											"Array length " + len + " less than index " + index + " for variable '" + writeDesc.VariableName + "'");
									}
								}
							}
						}
						else if (writeBase is VariableTriggerWriteCurly) {
							var writeDesc = (VariableTriggerWriteCurly) writeBase;
							var reader = variableService.GetReader(_variables[count].DeploymentId, variableMetaData.VariableName, agentInstanceId);
							var value = reader.Value;
							writeDesc.Expression.Evaluate(eventsPerStream, true, agentInstanceContext);
							variableService.Write(variableNumber, agentInstanceId, value);
							written = value;
						}
						else if (variableMetaData.EventType != null) {
							var value = assignment.Evaluator.Evaluate(eventsPerStream, true, agentInstanceContext);
							var eventBean = agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedObject(value, variableMetaData.EventType);
							variableService.Write(variableNumber, agentInstanceId, eventBean);
							written = value;
						}
						else {
							var value = assignment.Evaluator.Evaluate(eventsPerStream, true, agentInstanceContext);
							if ((value != null) && (_mustCoerce[count])) {
								value = TypeHelper.CoerceBoxed(value, variableMetaData.Type);
							}

							variableService.Write(variableNumber, agentInstanceId, value);
							written = value;
						}

						count++;

						valuesWritten?.Put(assignment.VariableName, written);
					}

					variableService.Commit();
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
		/// <param name="variableManagementService">variable management</param>
		/// <param name="agentInstanceId">context partition id</param>
		/// <returns>map of values</returns>
		public IDictionary<string, object> Iterate(
			VariableManagementService variableManagementService,
			int agentInstanceId)
		{
			var values = new Dictionary<string, object>();
			var count = 0;
			foreach (var assignment in _assignments) {
				object value;
				if (_readersForGlobalVars[count] == null) {
					var reader = variableManagementService.GetReader(_variables[count].DeploymentId, assignment.VariableName, agentInstanceId);
					if (reader == null) {
						continue;
					}

					value = reader.Value;
				}
				else {
					value = _readersForGlobalVars[count].Value;
				}

				if (value == null) {
					values.Put(assignment.VariableName, null);
				}
				else if (_writers[count] is VariableTriggerWriteDesc) {
					var desc = (VariableTriggerWriteDesc) _writers[count];
					var current = (EventBean) value;
					values.Put(assignment.VariableName, desc.Getter.Get(current));
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
