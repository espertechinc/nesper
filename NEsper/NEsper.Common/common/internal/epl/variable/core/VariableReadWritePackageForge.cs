///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    ///     A convenience class for dealing with reading and updating multiple variable values.
    /// </summary>
    public class VariableReadWritePackageForge
    {
        private readonly VariableTriggerSetForge[] assignments;
        private readonly IDictionary<EventTypeSPI, EventBeanCopyMethodForge> copyMethods;
        private readonly bool[] mustCoerce;
        private readonly VariableMetaData[] variables;
        private readonly VariableTriggerWriteDescForge[] writers;

        public VariableReadWritePackageForge(
            IList<OnTriggerSetAssignment> assignments,
            StatementCompileTimeServices services)
        {
            variables = new VariableMetaData[assignments.Count];
            mustCoerce = new bool[assignments.Count];
            writers = new VariableTriggerWriteDescForge[assignments.Count];
            VariableTypes = new Dictionary<string, object>();

            IDictionary<EventTypeSPI, CopyMethodDesc> eventTypeWrittenProps =
                new Dictionary<EventTypeSPI, CopyMethodDesc>();
            var count = 0;
            IList<VariableTriggerSetForge> assignmentList = new List<VariableTriggerSetForge>();

            foreach (var expressionWithAssignments in assignments) {
                var possibleVariableAssignment =
                    ExprNodeUtilityValidate.CheckGetAssignmentToVariableOrProp(expressionWithAssignments.Expression);
                if (possibleVariableAssignment == null) {
                    throw new ExprValidationException(
                        "Missing variable assignment expression in assignment number " + count);
                }

                var evaluator = possibleVariableAssignment.Second.Forge;
                assignmentList.Add(new VariableTriggerSetForge(possibleVariableAssignment.First, evaluator));

                var fullVariableName = possibleVariableAssignment.First;
                var variableName = fullVariableName;
                string subPropertyName = null;

                var indexOfDot = variableName.IndexOf('.');
                if (indexOfDot != -1) {
                    subPropertyName = variableName.Substring(indexOfDot + 1);
                    variableName = variableName.Substring(0, indexOfDot);
                }

                var variableMetadata = services.VariableCompileTimeResolver.Resolve(variableName);
                if (variableMetadata == null) {
                    throw new ExprValidationException(
                        "Variable by name '" + variableName + "' has not been created or configured");
                }

                variables[count] = variableMetadata;

                if (variableMetadata.IsConstant) {
                    throw new ExprValidationException(
                        "Variable by name '" + variableName + "' is declared constant and may not be set");
                }

                if (subPropertyName != null) {
                    if (variableMetadata.EventType == null) {
                        throw new ExprValidationException(
                            "Variable by name '" +
                            variableName +
                            "' does not have a property named '" +
                            subPropertyName +
                            "'");
                    }

                    var type = variableMetadata.EventType;
                    if (!(type is EventTypeSPI)) {
                        throw new ExprValidationException(
                            "Variable by name '" + variableName + "' event type '" + type.Name + "' not writable");
                    }

                    var spi = (EventTypeSPI) type;
                    var writer = spi.GetWriter(subPropertyName);
                    var getter = spi.GetGetterSPI(subPropertyName);
                    var getterType = spi.GetPropertyType(subPropertyName);
                    if (writer == null) {
                        throw new ExprValidationException(
                            "Variable by name '" +
                            variableName +
                            "' the property '" +
                            subPropertyName +
                            "' is not writable");
                    }

                    VariableTypes.Put(fullVariableName, spi.GetPropertyType(subPropertyName));
                    var writtenProps = eventTypeWrittenProps.Get(spi);
                    if (writtenProps == null) {
                        writtenProps = new CopyMethodDesc(variableName, new List<string>());
                        eventTypeWrittenProps.Put(spi, writtenProps);
                    }

                    writtenProps.PropertiesCopied.Add(subPropertyName);

                    writers[count] = new VariableTriggerWriteDescForge(
                        spi,
                        variableName,
                        writer,
                        getter,
                        getterType,
                        evaluator.EvaluationType);
                }
                else {
                    // determine types
                    var expressionType = possibleVariableAssignment.Second.Forge.EvaluationType;

                    if (variableMetadata.EventType != null) {
                        if (expressionType != null &&
                            !TypeHelper.IsSubclassOrImplementsInterface(
                                expressionType,
                                variableMetadata.EventType.UnderlyingType)) {
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

                        VariableTypes.Put(variableName, variableMetadata.EventType.UnderlyingType);
                    }
                    else {
                        var variableType = variableMetadata.Type;
                        VariableTypes.Put(variableName, variableType);

                        // determine if the expression type can be assigned
                        if (variableType != typeof(object)) {
                            if (expressionType.GetBoxedType() != variableType &&
                                expressionType != null) {
                                if (!variableType.IsNumeric() ||
                                    !expressionType.IsNumeric()) {
                                    throw new ExprValidationException(
                                        VariableUtil.GetAssigmentExMessage(variableName, variableType, expressionType));
                                }

                                if (!expressionType.CanCoerce(variableType)) {
                                    throw new ExprValidationException(
                                        VariableUtil.GetAssigmentExMessage(variableName, variableType, expressionType));
                                }

                                mustCoerce[count] = true;
                            }
                        }
                    }
                }

                count++;
            }

            this.assignments = assignmentList.ToArray();

            if (eventTypeWrittenProps.IsEmpty()) {
                copyMethods = Collections.GetEmptyMap<EventTypeSPI, EventBeanCopyMethodForge>();
                return;
            }

            copyMethods = new Dictionary<EventTypeSPI, EventBeanCopyMethodForge>();
            foreach (var entry in eventTypeWrittenProps) {
                var propsWritten = entry.Value.PropertiesCopied;
                var props = propsWritten.ToArray();
                var copyMethod = entry.Key.GetCopyMethodForge(props);
                if (copyMethod == null) {
                    throw new ExprValidationException(
                        "Variable '" +
                        entry.Value.VariableName +
                        "' of declared type " +
                        entry.Key.UnderlyingType.GetCleanName() +
                        "' cannot be assigned to");
                }

                copyMethods.Put(entry.Key, copyMethod);
            }
        }

        /// <summary>
        ///     Returns a map of variable names and type of variable.
        /// </summary>
        /// <value>variables</value>
        public IDictionary<string, object> VariableTypes { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(VariableReadWritePackage), GetType(), classScope);
            var @ref = Ref("rw");
            method.Block
                .DeclareVar<VariableReadWritePackage>(@ref.Ref, NewInstance(typeof(VariableReadWritePackage)))
                .SetProperty(@ref, "CopyMethods", MakeCopyMethods(copyMethods, method, symbols, classScope))
                .SetProperty(@ref, "Assignments", MakeAssignments(assignments, method, symbols, classScope))
                .SetProperty(@ref, "Variables", MakeVariables(variables, method, symbols, classScope))
                .SetProperty(@ref, "Writers", MakeWriters(writers, method, symbols, classScope))
                .SetProperty(
                    @ref,
                    "ReadersForGlobalVars",
                    MakeReadersForGlobalVars(variables, method, symbols, classScope))
                .SetProperty(@ref, "MustCoerce", Constant(mustCoerce))
                .MethodReturn(@ref);
            return LocalMethod(method);
        }

        private static CodegenExpression MakeReadersForGlobalVars(
            VariableMetaData[] variables,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(VariableReader[]),
                typeof(VariableReadWritePackageForge),
                classScope);
            method.Block.DeclareVar<VariableReader[]>(
                "readers",
                NewArrayByLength(typeof(VariableReader), Constant(variables.Length)));
            for (var i = 0; i < variables.Length; i++) {
                if (variables[i].OptionalContextName == null) {
                    var resolve = StaticMethod(
                        typeof(VariableDeployTimeResolver),
                        "resolveVariableReader",
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
            VariableTriggerWriteDescForge[] writers,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(VariableTriggerWriteDesc[]),
                typeof(VariableReadWritePackageForge),
                classScope);
            method.Block.DeclareVar<VariableTriggerWriteDesc[]>(
                "writers",
                NewArrayByLength(typeof(VariableTriggerWriteDesc), Constant(writers.Length)));
            for (var i = 0; i < writers.Length; i++) {
                var writer =
                    writers[i] == null ? ConstantNull() : writers[i].Make(method, symbols, classScope);
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
            var method = parent.MakeChild(
                typeof(Variable[]),
                typeof(VariableReadWritePackageForge),
                classScope);
            method.Block.DeclareVar<Variable[]>(
                "vars",
                NewArrayByLength(typeof(Variable), Constant(variables.Length)));
            for (var i = 0; i < variables.Length; i++) {
                var resolve = VariableDeployTimeResolver.MakeResolveVariable(
                    variables[i],
                    symbols.GetAddInitSvc(method));
                method.Block.AssignArrayElement("vars", Constant(i), resolve);
            }

            method.Block.MethodReturn(Ref("vars"));
            return LocalMethod(method);
        }

        private static CodegenExpression MakeAssignments(
            VariableTriggerSetForge[] assignments,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(VariableTriggerSetDesc[]),
                typeof(VariableReadWritePackageForge),
                classScope);
            method.Block.DeclareVar<VariableTriggerSetDesc[]>(
                "sets",
                NewArrayByLength(typeof(VariableTriggerSetDesc), Constant(assignments.Length)));
            for (var i = 0; i < assignments.Length; i++) {
                var set = NewInstance<VariableTriggerSetDesc>(
                    Constant(assignments[i].VariableName),
                    ExprNodeUtilityCodegen.CodegenEvaluator(
                        assignments[i].Forge,
                        method,
                        typeof(VariableReadWritePackageForge),
                        classScope));
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
                return StaticMethod(typeof(Collections), "emptyMap");
            }

            var method = parent.MakeChild(
                typeof(IDictionary<string, object>),
                typeof(VariableReadWritePackageForge),
                classScope);
            method.Block.DeclareVar<IDictionary<string, object>>(
                "methods",
                NewInstance(typeof(Dictionary<string, object>), Constant(copyMethods.Count)));
            foreach (var entry in copyMethods) {
                var type = EventTypeUtility.ResolveTypeCodegen(entry.Key, symbols.GetAddInitSvc(method));
                var copyMethod = entry.Value.MakeCopyMethodClassScoped(classScope);
                method.Block.ExprDotMethod(Ref("methods"), "put", type, copyMethod);
            }

            method.Block.MethodReturn(Ref("methods"));
            return LocalMethod(method);
        }

        private class CopyMethodDesc
        {
            internal readonly IList<string> PropertiesCopied;

            internal readonly string VariableName;

            public CopyMethodDesc(
                string variableName,
                IList<string> propertiesCopied)
            {
                VariableName = variableName;
                PropertiesCopied = propertiesCopied;
            }
        }

        private class VariableTriggerSetForge
        {
            internal readonly ExprForge Forge;
            internal readonly string VariableName;

            public VariableTriggerSetForge(
                string variableName,
                ExprForge forge)
            {
                VariableName = variableName;
                Forge = forge;
            }
        }
    }
} // end of namespace