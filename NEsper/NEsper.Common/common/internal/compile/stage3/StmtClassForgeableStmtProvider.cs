///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // newInstance;
using static com.espertech.esper.common.@internal.context.aifactory.core.SAIFFInitializeSymbol; // REF_STMTINITSVC;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StmtClassForgeableStmtProvider : StmtClassForgeable
    {
        private const string MEMBERNAME_STATEMENT_FIELDS = "statementFields";
        private const string MEMBERNAME_INFORMATION = "statementInformationals";
        private const string MEMBERNAME_FACTORY_PROVIDER = "factoryProvider";
        
        private readonly CodegenNamespaceScope _namespaceScope;
        private readonly string _statementAiFactoryClassName;
        private readonly StatementInformationalsCompileTime _statementInformationals;
        private readonly string _className;

        public StmtClassForgeableStmtProvider(
            string statementAIFactoryClassName,
            string statementProviderClassName,
            StatementInformationalsCompileTime statementInformationals,
            CodegenNamespaceScope namespaceScope)
        {
            _statementAiFactoryClassName = statementAIFactoryClassName;
            _className = statementProviderClassName;
            _statementInformationals = statementInformationals;
            _namespaceScope = namespaceScope;
        }

        public CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget)
        {
            // write code to create an implementation of StatementResource
            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            
            // members
            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            members.Add(new CodegenTypedParam(_namespaceScope.FieldsClassName, MEMBERNAME_STATEMENT_FIELDS));
            members.Add(new CodegenTypedParam(typeof(StatementInformationalsRuntime), MEMBERNAME_INFORMATION));
            members.Add(new CodegenTypedParam(typeof(StatementAIFactoryProvider), MEMBERNAME_FACTORY_PROVIDER).WithFinal(false));

            // ctor
            var ctor = new CodegenCtor(GetType(), ClassName, includeDebugSymbols, Collections.GetEmptyList<CodegenTypedParam>());
            var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, ClassName);
            ctor.Block.AssignMember(MEMBERNAME_INFORMATION, _statementInformationals.Make(ctor, classScope));

            var initializeMethod = MakeInitialize(classScope);
            var statementAIFactoryProviderProp = MakeGetStatementAIFactoryProvider(classScope);
            var statementInformationalsProp = CodegenProperty.MakePropertyNode(
                    typeof(StatementInformationalsRuntime),
                    typeof(StmtClassForgeableStmtProvider),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
            statementInformationalsProp
                .GetterBlock.BlockReturn(Ref(MEMBERNAME_INFORMATION));

            CodegenStackGenerator.RecursiveBuildStack(
                statementInformationalsProp, 
                "Informationals", 
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                initializeMethod, 
                "Initialize", 
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                statementAIFactoryProviderProp,
                "StatementAIFactoryProvider",
                methods,
                properties);
            CodegenStackGenerator.RecursiveBuildStack(
                ctor,
                "ctor",
                methods,
                properties);

            return new CodegenClass(
                CodegenClassType.STATEMENTPROVIDER,
                typeof(StatementProvider),
                ClassName,
                classScope,
                members,
                ctor,
                methods,
                properties,
                EmptyList<CodegenInnerClass>.Instance);
        }

        public string ClassName => _className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.STMTPROVIDER;

        private CodegenMethod MakeInitialize(CodegenClassScope classScope)
        {
            var method = CodegenMethod
                .MakeMethod(typeof(void), typeof(StmtClassForgeableStmtProvider), classScope)
                .AddParam(typeof(EPStatementInitServices), REF_STMTINITSVC.Ref);
            
            if (_namespaceScope.FieldsClassName != null) {
                method.Block.AssignRef(
                    MEMBERNAME_STATEMENT_FIELDS,
                    NewInstanceInner(_namespaceScope.FieldsClassName));
            }

            method.Block.AssignMember(
                MEMBERNAME_FACTORY_PROVIDER,
                NewInstanceInner(_statementAiFactoryClassName, REF_STMTINITSVC, Ref(MEMBERNAME_STATEMENT_FIELDS)));
            return method;
        }

        private static CodegenProperty  MakeGetStatementAIFactoryProvider(CodegenClassScope classScope)
        {
            var property = CodegenProperty.MakePropertyNode(
                typeof(StatementAIFactoryProvider),
                typeof(StmtClassForgeableStmtProvider),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            property.GetterBlock.BlockReturn(Ref(MEMBERNAME_FACTORY_PROVIDER));
            return property;
        }
    }
} // end of namespace