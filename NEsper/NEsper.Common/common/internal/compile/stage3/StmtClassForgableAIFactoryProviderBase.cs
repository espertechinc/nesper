///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public abstract class StmtClassForgableAIFactoryProviderBase : StmtClassForgable
    {
        protected const string MEMBERNAME_STATEMENTAIFACTORY = "statementAIFactory";

        private readonly string _className;
        private readonly CodegenNamespaceScope _namespaceScope;

        protected abstract Type TypeOfFactory();

        protected abstract CodegenMethod CodegenConstructorInit(
            CodegenMethodScope parent,
            CodegenClassScope classScope);

        public StmtClassForgableAIFactoryProviderBase(
            string className,
            CodegenNamespaceScope namespaceScope)
        {
            _className = className;
            _namespaceScope = namespaceScope;
        }

        public CodegenClass Forge(bool includeDebugSymbols)
        {
            IList<CodegenTypedParam> ctorParms = new List<CodegenTypedParam>();
            ctorParms.Add(
                new CodegenTypedParam(
                    typeof(EPStatementInitServices),
                    EPStatementInitServicesConstants.REF.Ref,
                    false));
            var codegenCtor = new CodegenCtor(GetType(), includeDebugSymbols, ctorParms);
            var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);

            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            members.Add(new CodegenTypedParam(TypeOfFactory(), MEMBERNAME_STATEMENTAIFACTORY));

            if (_namespaceScope.FieldsClassNameOptional != null) {
                codegenCtor.Block.StaticMethod(
                    _namespaceScope.FieldsClassNameOptional,
                    "Init",
                    EPStatementInitServicesConstants.REF);
            }

            codegenCtor.Block.AssignRef(
                MEMBERNAME_STATEMENTAIFACTORY,
                LocalMethod(CodegenConstructorInit(codegenCtor, classScope), SAIFFInitializeSymbol.REF_STMTINITSVC));

            var factoryProp = CodegenProperty.MakeParentNode(
                typeof(StatementAgentInstanceFactory),
                GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            factoryProp.GetterBlock.BlockReturn(Ref(MEMBERNAME_STATEMENTAIFACTORY));

            var assignMethod = CodegenMethod
                .MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(StatementAIFactoryAssignments), "assignments");
            if (_namespaceScope.FieldsClassNameOptional != null) {
                assignMethod.Block.StaticMethod(_namespaceScope.FieldsClassNameOptional, "Assign", @Ref("assignments"));
            }

            var unassignMethod = CodegenMethod.MakeParentNode(
                typeof(void),
                GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            if (_namespaceScope.FieldsClassNameOptional != null) {
                unassignMethod.Block.StaticMethod(_namespaceScope.FieldsClassNameOptional, "Unassign");
            }

            var setValueMethod = CodegenMethod
                .MakeParentNode(typeof(void), typeof(StmtClassForgableStmtFields), classScope)
                .AddParam(typeof(int), "index")
                .AddParam(typeof(object), "value");
            CodegenSubstitutionParamEntry.CodegenSetterMethod(classScope, setValueMethod);

            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();

            CodegenStackGenerator.RecursiveBuildStack(factoryProp, "Factory", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(assignMethod, "Assign", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(unassignMethod, "Unassign", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(setValueMethod, "SetValue", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(codegenCtor, "Ctor", methods, properties);

            return new CodegenClass(
                typeof(StatementAIFactoryProvider),
                _namespaceScope.Namespace,
                _className,
                classScope,
                members,
                codegenCtor,
                methods,
                properties,
                new EmptyList<CodegenInnerClass>());
        }

        public string ClassName => _className;

        public StmtClassForgableType ForgableType => StmtClassForgableType.AIFACTORYPROVIDER;
    }
} // end of namespace