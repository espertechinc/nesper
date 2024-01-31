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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.namedwindow.path;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    public class NamedWindowDeployTimeResolver
    {
        public static CodegenExpression MakeResolveNamedWindow(
            NamedWindowMetaData namedWindow,
            CodegenExpression initSvc)
        {
            return StaticMethod(
                typeof(NamedWindowDeployTimeResolver),
                "ResolveNamedWindow",
                Constant(namedWindow.EventType.Name),
                Constant(namedWindow.EventType.Metadata.AccessModifier),
                Constant(namedWindow.NamedWindowModuleName),
                initSvc);
        }

        public static NamedWindow ResolveNamedWindow(
            string namedWindowName,
            NameAccessModifier visibility,
            string optionalModuleName,
            EPStatementInitServices services)
        {
            var deploymentId = ResolveDeploymentId(namedWindowName, visibility, optionalModuleName, services);
            var namedWindow =
                services.NamedWindowManagementService.GetNamedWindow(deploymentId, namedWindowName);
            if (namedWindow == null) {
                throw new EPException("Failed to resolve named window '" + namedWindowName + "'");
            }

            return namedWindow;
        }

        private static string ResolveDeploymentId(
            string tableName,
            NameAccessModifier visibility,
            string optionalModuleName,
            EPStatementInitServices services)
        {
            string deploymentId;
            switch (visibility) {
                case NameAccessModifier.PRIVATE:
                    deploymentId = services.DeploymentId;
                    break;

                case NameAccessModifier.PUBLIC:
                case NameAccessModifier.INTERNAL: {
                    deploymentId = services.NamedWindowPathRegistry.GetDeploymentId(tableName, optionalModuleName);
                    if (deploymentId == null) {
                        throw new EPException("Failed to resolve path named window '" + tableName + "'");
                    }

                    break;
                }

                default:
                    throw new ArgumentException("Unrecognized visibility " + visibility);
            }

            return deploymentId;
        }
    }
} // end of namespace