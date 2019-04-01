///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StmtClassForgableStmtProvider : StmtClassForgable
    {
        private const string MEMBERNAME_INFORMATION = "statementInformationals";
        private const string MEMBERNAME_FACTORY_PROVIDER = "factoryProvider";
        private readonly CodegenPackageScope packageScope;

        private readonly string statementAIFactoryClassName;
        private readonly StatementInformationalsCompileTime statementInformationals;

        public StmtClassForgableStmtProvider(
            string statementAIFactoryClassName, string statementProviderClassName,
            StatementInformationalsCompileTime statementInformationals, CodegenPackageScope packageScope)
        {
            this.statementAIFactoryClassName = statementAIFactoryClassName;
            ClassName = statementProviderClassName;
            this.statementInformationals = statementInformationals;
            this.packageScope = packageScope;
        }

        public CodegenClass Forge(bool includeDebugSymbols)
        {
            // write code to create an implementation of StatementResource
            var methods = new CodegenClassMethods();

            // members
            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            members.Add(new CodegenTypedParam(typeof(StatementInformationalsRuntime), MEMBERNAME_INFORMATION));
            members.Add(
                new CodegenTypedParam(typeof(StatementAIFactoryProvider), MEMBERNAME_FACTORY_PROVIDER).WithFinal(false));

            // ctor
            var ctor = new CodegenCtor(GetType(), includeDebugSymbols, Collections.GetEmptyList<CodegenTypedParam>());
            var classScope = new CodegenClassScope(includeDebugSymbols, packageScope, ClassName);
            ctor.Block.AssignRef(MEMBERNAME_INFORMATION, statementInformationals.Make(ctor, classScope));

            var initializeMethod = MakeInitialize(classScope);
            var getStatementAIFactoryProviderMethod = MakeGetStatementAIFactoryProvider(classScope);
            CodegenMethod getStatementInformationalsMethod = CodegenMethod.MakeParentNode(
                    typeof(StatementInformationalsRuntime), typeof(StmtClassForgableStmtProvider),
                    CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .Block.MethodReturn(Ref(MEMBERNAME_INFORMATION));

            CodegenStackGenerator.RecursiveBuildStack(getStatementInformationalsMethod, "getInformationals", methods);
            CodegenStackGenerator.RecursiveBuildStack(initializeMethod, "initialize", methods);
            CodegenStackGenerator.RecursiveBuildStack(
                getStatementAIFactoryProviderMethod, "getStatementAIFactoryProvider", methods);
            CodegenStackGenerator.RecursiveBuildStack(ctor, "ctor", methods);

            return new CodegenClass(
                typeof(StatementProvider), packageScope.PackageName, ClassName, classScope, members, ctor, methods,
                Collections.GetEmptyList<CodegenInnerClass>());
        }

        public string ClassName { get; }

        public StmtClassForgableType ForgableType => StmtClassForgableType.STMTPROVIDER;

        private CodegenMethod MakeInitialize(CodegenClassScope classScope)
        {
            CodegenMethod method = CodegenMethod
                .MakeParentNode(typeof(void), typeof(StmtClassForgableStmtProvider), classScope).AddParam(
                    typeof(EPStatementInitServices), REF_STMTINITSVC.Ref);
            method.Block.AssignRef(
                MEMBERNAME_FACTORY_PROVIDER, NewInstance(statementAIFactoryClassName, REF_STMTINITSVC));
            return method;
        }

        private static CodegenMethod MakeGetStatementAIFactoryProvider(CodegenClassScope classScope)
        {
            CodegenMethod method = CodegenMethod.MakeParentNode(
                typeof(StatementAIFactoryProvider), typeof(StmtClassForgableStmtProvider),
                CodegenSymbolProviderEmpty.INSTANCE, classScope);
            method.Block.MethodReturn(Ref(MEMBERNAME_FACTORY_PROVIDER));
            return method;
        }
    }
} // end of namespace