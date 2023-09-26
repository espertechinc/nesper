///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.table.compiletime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableDeployTimeResolver
    {
        public static CodegenExpressionInstanceField MakeTableEventToPublicField(
            TableMetaData table,
            CodegenClassScope classScope,
            Type generator)
        {
            var symbols = new SAIFFInitializeSymbol();
            var tableInit = classScope.NamespaceScope.InitMethod
                .MakeChildWithScope(typeof(TableMetadataInternalEventToPublic), generator, symbols, classScope)
                .AddParam<EPStatementInitServices>(EPStatementInitServicesConstants.REF.Ref);
            var tableResolve = MakeResolveTable(table, EPStatementInitServicesConstants.REF);
            tableInit.Block.MethodReturn(ExprDotName(tableResolve, "EventToPublic"));
            return classScope.NamespaceScope.AddDefaultFieldUnshared(
                true,
                typeof(TableMetadataInternalEventToPublic),
                LocalMethod(tableInit, EPStatementInitServicesConstants.REF));
        }

        public static CodegenExpression MakeResolveTable(
            TableMetaData table,
            CodegenExpression initSvc)
        {
            return StaticMethod(
                typeof(TableDeployTimeResolver),
                "ResolveTable",
                Constant(table.TableName),
                Constant(table.TableVisibility),
                Constant(table.TableModuleName),
                initSvc);
        }

        public static Table ResolveTable(
            string tableName,
            NameAccessModifier visibility,
            string optionalModuleName,
            EPStatementInitServices services)
        {
            var deploymentId = ResolveDeploymentId(tableName, visibility, optionalModuleName, services);
            var table = services.TableManagementService.GetTable(deploymentId, tableName);
            if (table == null) {
                throw new EPException("Failed to resolve table '" + tableName + "'");
            }

            return table;
        }

        private static string ResolveDeploymentId(
            string tableName,
            NameAccessModifier visibility,
            string optionalModuleName,
            EPStatementInitServices services)
        {
            string deploymentId;
            if (visibility == NameAccessModifier.PRIVATE) {
                deploymentId = services.DeploymentId;
            }
            else if (visibility == NameAccessModifier.PUBLIC || visibility == NameAccessModifier.INTERNAL) {
                deploymentId = services.TablePathRegistry.GetDeploymentId(tableName, optionalModuleName);
                if (deploymentId == null) {
                    throw new EPException("Failed to resolve path table '" + tableName + "'");
                }
            }
            else {
                throw new ArgumentException("Unrecognized visibility " + visibility);
            }

            return deploymentId;
        }
    }
} // end of namespace