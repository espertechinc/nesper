///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.assign;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.variable;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
	/// <summary>
	/// A convenience class for dealing with reading and updating multiple variable values.
	/// </summary>
	public class VariableReadWritePackageForge
	{
		private readonly ExprAssignment[] _assignments;
		private readonly VariableMetaData[] _variables;
		private readonly bool[] _mustCoerce;
		private readonly VariableTriggerWriteForge[] _writers;
		private readonly IDictionary<EventTypeSPI, EventBeanCopyMethodForge> _copyMethods;
		private readonly IDictionary<string, object> _variableTypes;

		public VariableReadWritePackageForge(
			IList<OnTriggerSetAssignment> assignments,
			string statementName,
			StatementCompileTimeServices services)
		{
			_variables = new VariableMetaData[assignments.Count];
			_mustCoerce = new bool[assignments.Count];
			_writers = new VariableTriggerWriteForge[assignments.Count];
			_variableTypes = new Dictionary<string, object>();

			IDictionary<EventTypeSPI, CopyMethodDesc> eventTypeWrittenProps = new Dictionary<EventTypeSPI, CopyMethodDesc>();
			var count = 0;
			IList<ExprAssignment> assignmentList = new List<ExprAssignment>();

			foreach (var spec in assignments) {
				var assignmentDesc = spec.Validated;
				assignmentList.Add(assignmentDesc);

				try {
					if (assignmentDesc is ExprAssignmentStraight) {
						var assignment = (ExprAssignmentStraight) assignmentDesc;
						var identAssignment = assignment.Lhs;

						var variableName = identAssignment.Ident;
						var variableMetadata = services.VariableCompileTimeResolver.Resolve(variableName);
						if (variableMetadata == null) {
							throw new ExprValidationException("Variable by name '" + variableName + "' has not been created or configured");
						}

						_variables[count] = variableMetadata;
						var expressionType = assignment.Rhs.Forge.EvaluationType;

						if (assignment.Lhs is ExprAssignmentLHSIdent) {
							// determine types
							if (variableMetadata.EventType != null) {
								if ((expressionType != null) &&
								    (!TypeHelper.IsSubclassOrImplementsInterface(expressionType, variableMetadata.EventType.UnderlyingType))) {
									throw new ExprValidationException(
										"Variable '" +
										variableName +
										"' of declared event type '" +
										variableMetadata.EventType.Name +
										"' underlying type '" +
										variableMetadata.EventType.UnderlyingType.Name +
										"' cannot be assigned a value of type '" +
										expressionType.Name +
										"'");
								}

								_variableTypes.Put(variableName, variableMetadata.EventType.UnderlyingType);
							}
							else {

								var variableType = variableMetadata.Type;
								_variableTypes.Put(variableName, variableType);

								// determine if the expression type can be assigned
								if (variableType != typeof(object)) {
									if ((expressionType.GetBoxedType() != variableType.GetBoxedType()) &&
									    (expressionType != null)) {
										if ((!TypeHelper.IsNumeric(variableType)) ||
										    (!TypeHelper.IsNumeric(expressionType))) {
											throw new ExprValidationException(VariableUtil.GetAssigmentExMessage(variableName, variableType, expressionType));
										}

										if (!(TypeHelper.CanCoerce(expressionType, variableType))) {
											throw new ExprValidationException(VariableUtil.GetAssigmentExMessage(variableName, variableType, expressionType));
										}

										_mustCoerce[count] = true;
									}
								}
							}
						}
						else if (assignment.Lhs is ExprAssignmentLHSIdentWSubprop) {
							var subpropAssignment = (ExprAssignmentLHSIdentWSubprop) assignment.Lhs;
							var subPropertyName = subpropAssignment.SubpropertyName;
							if (variableMetadata.EventType == null) {
								throw new ExprValidationException(
									"Variable by name '" + variableName + "' does not have a property named '" + subPropertyName + "'");
							}

							var type = variableMetadata.EventType;
							if (!(type is EventTypeSPI)) {
								throw new ExprValidationException("Variable by name '" + variableName + "' event type '" + type.Name + "' not writable");
							}

							var spi = (EventTypeSPI) type;
							var writer = spi.GetWriter(subPropertyName);
							var getter = spi.GetGetterSPI(subPropertyName);
							var getterType = spi.GetPropertyType(subPropertyName);
							if (writer == null) {
								throw new ExprValidationException(
									"Variable by name '" + variableName + "' the property '" + subPropertyName + "' is not writable");
							}

							var fullVariableName = variableName + "." + subPropertyName;
							_variableTypes.Put(fullVariableName, spi.GetPropertyType(subPropertyName));
							var writtenProps = eventTypeWrittenProps.Get(spi);
							if (writtenProps == null) {
								writtenProps = new CopyMethodDesc(variableName, new List<string>());
								eventTypeWrittenProps.Put(spi, writtenProps);
							}

							writtenProps.PropertiesCopied.Add(subPropertyName);

							_writers[count] = new VariableTriggerWriteDescForge(
								spi,
								variableName,
								writer,
								getter,
								getterType,
								assignment.Rhs.Forge.EvaluationType);
						}
						else if (assignment.Lhs is ExprAssignmentLHSArrayElement) {
							var arrayAssign = (ExprAssignmentLHSArrayElement) assignment.Lhs;
							var variableType = variableMetadata.Type;
							if (!variableType.IsArray) {
								throw new ExprValidationException("Variable '" + variableMetadata.VariableName + "' is not an array");
							}

							TypeWidenerSPI widener;
							try {
								widener = TypeWidenerFactory.GetCheckPropertyAssignType(
									ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(assignment.Rhs),
									expressionType,
									variableType.GetElementType(),
									variableMetadata.VariableName,
									false,
									null,
									statementName);
							}
							catch (TypeWidenerException ex) {
								throw new ExprValidationException(ex.Message, ex);
							}

							_writers[count] = new VariableTriggerWriteArrayElementForge(variableName, arrayAssign.IndexExpression.Forge, widener);
						}
						else {
							throw new IllegalStateException("Unrecognized left hand side assignment " + assignment.Lhs);
						}
					}
					else if (assignmentDesc is ExprAssignmentCurly) {
						var curly = (ExprAssignmentCurly) assignmentDesc;
						if (curly.Expression is ExprVariableNode) {
							throw new ExprValidationException("Missing variable assignment expression in assignment number " + count);
						}

						var variableVisitor = new ExprNodeVariableVisitor(services.VariableCompileTimeResolver);
						curly.Expression.Accept(variableVisitor);
						if (variableVisitor.VariableNames == null || variableVisitor.VariableNames.Count != 1) {
							throw new ExprValidationException("Assignment expression must receive a single variable value");
						}

						var variable = variableVisitor.VariableNames.First();
						_variables[count] = variable.Value;
						_writers[count] = new VariableTriggerWriteCurlyForge(variable.Key, curly.Expression.Forge);
					}
					else {
						throw new IllegalStateException("Unrecognized assignment expression " + assignmentDesc);
					}

					if (_variables[count].IsConstant) {
						throw new ExprValidationException("Variable by name '" + _variables[count].VariableName + "' is declared constant and may not be set");
					}

					count++;
				}
				catch (ExprValidationException ex) {
					throw new ExprValidationException(
						"Failed to validate assignment expression '" +
						ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(assignmentDesc.OriginalExpression) +
						"': " +
						ex.Message,
						ex);
				}
			}

			_assignments = assignmentList.ToArray();

			if (eventTypeWrittenProps.IsEmpty()) {
				_copyMethods = EmptyDictionary<EventTypeSPI, EventBeanCopyMethodForge>.Instance;
				return;
			}

			_copyMethods = new Dictionary<EventTypeSPI, EventBeanCopyMethodForge>();
			foreach (var entry in eventTypeWrittenProps) {
				var propsWritten = entry.Value.PropertiesCopied;
				var props = propsWritten.ToArray();
				var copyMethod = entry.Key.GetCopyMethodForge(props);
				if (copyMethod == null) {
					throw new ExprValidationException(
						"Variable '" +
						entry.Value.VariableName +
						"' of declared type " +
						entry.Key.UnderlyingType.CleanName() +
						"' cannot be assigned to");
				}

				_copyMethods.Put(entry.Key, copyMethod);
			}
		}

		/// <summary>
		/// Returns a map of variable names and type of variable.
		/// </summary>
		/// <value>variables</value>
		public IDictionary<string, object> VariableTypes => _variableTypes;

		public CodegenExpression Make(
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			var method = parent.MakeChild(typeof(VariableReadWritePackage), GetType(), classScope);
			var @ref = Ref("rw");
			method.Block
				.DeclareVar<VariableReadWritePackage>(@ref.Ref, NewInstance<VariableReadWritePackage>())
				.SetProperty(@ref, "CopyMethods", MakeCopyMethods(_copyMethods, method, symbols, classScope))
				.SetProperty(@ref, "Assignments", MakeAssignments(_assignments, _variables, method, symbols, classScope))
				.SetProperty(@ref, "Variables", MakeVariables(_variables, method, symbols, classScope))
				.SetProperty(@ref, "Writers", MakeWriters(_writers, method, symbols, classScope))
				.SetProperty(@ref, "ReadersForGlobalVars", MakeReadersForGlobalVars(_variables, method, symbols, classScope))
				.SetProperty(@ref, "MustCoerce", Constant(_mustCoerce))
				.MethodReturn(@ref);
			return LocalMethod(method);
		}

		private static CodegenExpression MakeReadersForGlobalVars(
			VariableMetaData[] variables,
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			var method = parent.MakeChild(typeof(VariableReader[]), typeof(VariableReadWritePackageForge), classScope);
			method.Block.DeclareVar<VariableReader[]>("readers", NewArrayByLength(typeof(VariableReader), Constant(variables.Length)));
			for (var i = 0; i < variables.Length; i++) {
				if (variables[i].OptionalContextName == null) {
					var resolve = StaticMethod(
						typeof(VariableDeployTimeResolver),
						"ResolveVariableReader",
						Constant(variables[i].VariableName),
						Constant(variables[i].VariableVisibility),
						Constant(variables[i].VariableModuleName),
						ConstantNull(),
						symbols.GetAddInitSvc(method));
					method.Block.AssignArrayElement("readers", Constant(i), resolve);
				}
			}

			method.Block.MethodReturn(Ref("readers"));
			return LocalMethod(method);
		}

		private static CodegenExpression MakeWriters(
			VariableTriggerWriteForge[] writers,
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			var method = parent.MakeChild(typeof(VariableTriggerWrite[]), typeof(VariableReadWritePackageForge), classScope);
			method.Block.DeclareVar<VariableTriggerWrite[]>("writers", NewArrayByLength(typeof(VariableTriggerWrite), Constant(writers.Length)));
			for (var i = 0; i < writers.Length; i++) {
				var writer = writers[i] == null ? ConstantNull() : writers[i].Make(method, symbols, classScope);
				method.Block.AssignArrayElement("writers", Constant(i), writer);
			}

			method.Block.MethodReturn(Ref("writers"));
			return LocalMethod(method);
		}

		private static CodegenExpression MakeVariables(
			VariableMetaData[] variables,
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			var method = parent.MakeChild(typeof(Variable[]), typeof(VariableReadWritePackageForge), classScope);
			method.Block.DeclareVar<Variable[]>("vars", NewArrayByLength(typeof(Variable), Constant(variables.Length)));
			for (var i = 0; i < variables.Length; i++) {
				var resolve = VariableDeployTimeResolver.MakeResolveVariable(variables[i], symbols.GetAddInitSvc(method));
				method.Block.AssignArrayElement("vars", Constant(i), resolve);
			}

			method.Block.MethodReturn(Ref("vars"));
			return LocalMethod(method);
		}

		private static CodegenExpression MakeAssignments(
			ExprAssignment[] assignments,
			VariableMetaData[] variables,
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			var method = parent.MakeChild(typeof(VariableTriggerSetDesc[]), typeof(VariableReadWritePackageForge), classScope);
			method.Block.DeclareVar<VariableTriggerSetDesc[]>("sets", NewArrayByLength(typeof(VariableTriggerSetDesc), Constant(assignments.Length)));
			for (var i = 0; i < assignments.Length; i++) {
				CodegenExpression set;
				if (assignments[i] is ExprAssignmentStraight) {
					var straightAssignment = (ExprAssignmentStraight) assignments[i];
					set = NewInstance<VariableTriggerSetDesc>(
						Constant(straightAssignment.Lhs.FullIdentifier),
						ExprNodeUtilityCodegen.CodegenEvaluator(straightAssignment.Rhs.Forge, method, typeof(VariableReadWritePackageForge), classScope));
				}
				else {
					set = NewInstance<VariableTriggerSetDesc>(Constant(variables[i].VariableName), ConstantNull());
				}

				method.Block.AssignArrayElement("sets", Constant(i), set);
			}

			method.Block.MethodReturn(Ref("sets"));
			return LocalMethod(method);
		}

		private static CodegenExpression MakeCopyMethods(
			IDictionary<EventTypeSPI, EventBeanCopyMethodForge> copyMethods,
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			if (copyMethods.IsEmpty()) {
				return EnumValue(typeof(EmptyDictionary<EventTypeSPI, EventBeanCopyMethod>), "Instance");
			}

			var method = parent.MakeChild(
				typeof(IDictionary<EventTypeSPI, EventBeanCopyMethod>),
				typeof(VariableReadWritePackageForge),
				classScope);
			method.Block.DeclareVar<IDictionary<EventTypeSPI, EventBeanCopyMethod>>(
				"methods",
				NewInstance<Dictionary<EventTypeSPI, EventBeanCopyMethod>>(Constant(copyMethods.Count)));
			foreach (var entry in copyMethods) {
				var type = EventTypeUtility.ResolveTypeCodegen(entry.Key, symbols.GetAddInitSvc(method));
				var copyMethod = entry.Value.MakeCopyMethodClassScoped(classScope);
				method.Block.ExprDotMethod(Ref("methods"), "Put", type, copyMethod);
			}

			method.Block.MethodReturn(Ref("methods"));
			return LocalMethod(method);
		}

		private class CopyMethodDesc
		{
			public CopyMethodDesc(
				string variableName,
				IList<string> propertiesCopied)
			{
				VariableName = variableName;
				PropertiesCopied = propertiesCopied;
			}

			public string VariableName { get; }

			public IList<string> PropertiesCopied { get; }
		}
	}
} // end of namespace
