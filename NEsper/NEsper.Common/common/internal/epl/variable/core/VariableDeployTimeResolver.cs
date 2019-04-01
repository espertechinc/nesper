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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.context.util.StatementCPCacheService;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableDeployTimeResolver
    {
        public static CodegenExpressionField MakeVariableField(
            VariableMetaData variableMetaData, CodegenClassScope classScope, Type generator)
        {
            var symbols = new SAIFFInitializeSymbol();
            var variableInit = classScope.PackageScope.InitMethod
                .MakeChildWithScope(typeof(Variable), generator, symbols, classScope).AddParam(
                    typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
            variableInit.Block.MethodReturn(MakeResolveVariable(variableMetaData, EPStatementInitServicesConstants.REF));
            return classScope.PackageScope.AddFieldUnshared(
                true, typeof(Variable), LocalMethod(variableInit, EPStatementInitServicesConstants.REF));
        }

        public static CodegenExpression MakeResolveVariable(VariableMetaData variable, CodegenExpression initSvc)
        {
            return StaticMethod(
                typeof(VariableDeployTimeResolver), "resolveVariable",
                Constant(variable.VariableName),
                Constant(variable.VariableVisibility),
                Constant(variable.VariableModuleName),
                initSvc);
        }

        public static CodegenExpression MakeResolveVariables(
            ICollection<VariableMetaData> variables, CodegenExpression initSvc)
        {
            var expressions = new CodegenExpression[variables.Count];
            var count = 0;
            foreach (var variable in variables) {
                expressions[count++] = MakeResolveVariable(variable, initSvc);
            }

            return NewArrayWithInit(typeof(Variable), expressions);
        }

        public static Variable ResolveVariable(
            string variableName,
            NameAccessModifier visibility,
            string optionalModuleName,
            EPStatementInitServices services)
        {
            var deploymentId = ResolveDeploymentId(variableName, visibility, optionalModuleName, services);
            var variable = services.VariableManagementService.GetVariableMetaData(deploymentId, variableName);
            if (variable == null) {
                throw new EPException("Failed to resolve variable '" + variableName + "'");
            }

            return variable;
        }

        public static VariableReader ResolveVariableReader(
            string variableName,
            NameAccessModifier visibility,
            string optionalModuleName,
            string optionalContextName,
            EPStatementInitServices services)
        {
            if (optionalContextName != null) {
                throw new ArgumentException("Expected null context name");
            }

            var deploymentId = ResolveDeploymentId(variableName, visibility, optionalModuleName, services);
            var reader = services.VariableManagementService.GetReader(
                deploymentId, variableName, DEFAULT_AGENT_INSTANCE_ID);
            if (reader == null) {
                throw new EPException("Failed to resolve variable '" + variableName + "'");
            }

            return reader;
        }

        public static IDictionary<int, VariableReader> ResolveVariableReaderPerCP(
            string variableName,
            NameAccessModifier visibility,
            string optionalModuleName,
            string optionalContextName,
            EPStatementInitServices services)
        {
            if (optionalContextName == null) {
                throw new ArgumentException("No context name");
            }

            var deploymentId = ResolveDeploymentId(variableName, visibility, optionalModuleName, services);
            IDictionary<int, VariableReader> reader =
                services.VariableManagementService.GetReadersPerCP(deploymentId, variableName);
            if (reader == null) {
                throw new EPException("Failed to resolve variable '" + variableName + "'");
            }

            return reader;
        }

        private static string ResolveDeploymentId(
            string variableName,
            NameAccessModifier visibility,
            string optionalModuleName,
            EPStatementInitServices services)
        {
            string deploymentId;
            if (visibility == NameAccessModifier.PRECONFIGURED) {
                deploymentId = null;
            }
            else if (visibility == NameAccessModifier.PRIVATE) {
                deploymentId = services.DeploymentId;
            }
            else if (visibility == NameAccessModifier.PUBLIC) {
                deploymentId = services.VariablePathRegistry.GetDeploymentId(variableName, optionalModuleName);
                if (deploymentId == null) {
                    throw new EPException("Failed to resolve path variable '" + variableName + "'");
                }
            }
            else {
                throw new ArgumentException("Unrecognized visibility " + visibility);
            }

            return deploymentId;
        }
    }
} // end of namespace