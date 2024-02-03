///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.declared.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.declared.runtime
{
    public class ExpressionDeployTimeResolver
    {
        public static CodegenExpressionInstanceField MakeRuntimeCacheKeyField(
            CodegenExpression instance,
            ExpressionDeclItem expression,
            CodegenClassScope classScope,
            Type generator)
        {
            if (expression.Visibility == NameAccessModifier.TRANSIENT) {
                // for private expression that cache key is simply an Object shared by the name of the
                // expression (fields are per-statement already so its safe)
                return classScope.NamespaceScope.AddOrGetInstanceFieldSharable(
                    instance,
                    new ExprDeclaredCacheKeyLocalCodegenField(expression.Name));
            }

            // global expressions need a cache key that derives from the deployment id of the expression and the expression name
            var keyInit = classScope.NamespaceScope.InitMethod
                .MakeChild(typeof(ExprDeclaredCacheKeyGlobal), generator, classScope)
                .AddParam(
                    typeof(EPStatementInitServices),
                    EPStatementInitServicesConstants.REF.Ref);
            keyInit.Block.DeclareVar<string>(
                    "deploymentId",
                    StaticMethod(
                        typeof(ExpressionDeployTimeResolver),
                        "ResolveDeploymentId",
                        Constant(expression.Name),
                        Constant(expression.Visibility),
                        Constant(expression.ModuleName),
                        EPStatementInitServicesConstants.REF))
                .MethodReturn(
                    NewInstance<ExprDeclaredCacheKeyGlobal>(
                        Ref("deploymentId"),
                        Constant(expression.Name)));

            return classScope.NamespaceScope.AddInstanceFieldUnshared(
                instance,
                true,
                typeof(ExprDeclaredCacheKeyGlobal),
                LocalMethod(keyInit, EPStatementInitServicesConstants.REF));
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="expressionName">name</param>
        /// <param name="visibility">visibility</param>
        /// <param name="optionalModuleName">module name</param>
        /// <param name="services">services</param>
        /// <returns>deployment id</returns>
        public static string ResolveDeploymentId(
            string expressionName,
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
                deploymentId = services.ExprDeclaredPathRegistry.GetDeploymentId(expressionName, optionalModuleName);
                if (deploymentId == null) {
                    throw new EPException("Failed to resolve path expression '" + expressionName + "'");
                }
            }
            else {
                throw new ArgumentException("Unrecognized visibility " + visibility);
            }

            return deploymentId;
        }
    }
} // end of namespace