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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.context.aifactory.core.SAIFFInitializeSymbol;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StmtClassForgeableStmtProvider : StmtClassForgeable
    {
        public const string MEMBERNAME_STATEMENT_FIELDS = "statementFields";
        public const string MEMBERNAME_INFORMATION = "statementInformationals";
        public const string MEMBERNAME_FACTORY_PROVIDER = "factoryProvider";
        
        private readonly CodegenNamespaceScope _namespaceScope;
        private readonly string _statementAIFactoryClassName;
        private readonly string _statementProviderClassName;
        private readonly StatementInformationalsCompileTime _statementInformationals;

        public StmtClassForgeableStmtProvider(
            string statementAIFactoryClassName,
            string statementProviderClassName,
            StatementInformationalsCompileTime statementInformationals,
            CodegenNamespaceScope namespaceScope)
        {
            _statementAIFactoryClassName = statementAIFactoryClassName;
            _statementProviderClassName = statementProviderClassName;
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
            members.Add(new CodegenTypedParam(_namespaceScope.FieldsClassNameOptional, MEMBERNAME_STATEMENT_FIELDS).WithFinal(false));
            members.Add(new CodegenTypedParam(typeof(StatementInformationalsRuntime), MEMBERNAME_INFORMATION));
            members.Add(new CodegenTypedParam(typeof(StatementAIFactoryProvider), MEMBERNAME_FACTORY_PROVIDER).WithFinal(false));
            
            // ctor
            var ctor = new CodegenCtor(GetType(), includeDebugSymbols, EmptyList<CodegenTypedParam>.Instance);
            var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _statementProviderClassName);
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
                _statementProviderClassName,
                classScope,
                members,
                ctor,
                methods,
                properties,
                EmptyList<CodegenInnerClass>.Instance);
        }

        private CodegenMethod MakeInitialize(CodegenClassScope classScope)
        {
            var method = CodegenMethod
                .MakeParentNode(typeof(void), typeof(StmtClassForgeableStmtProvider), classScope)
                .AddParam<EPStatementInitServices>(REF_STMTINITSVC.Ref);
            
            if (_namespaceScope.FieldsClassNameOptional != null) {
                method.Block.AssignRef(
                    MEMBERNAME_STATEMENT_FIELDS,
                    NewInstanceInner(_namespaceScope.FieldsClassNameOptional));
            }
            
            method.Block.AssignMember(
                MEMBERNAME_FACTORY_PROVIDER,
                NewInstanceInner(_statementAIFactoryClassName, REF_STMTINITSVC, Ref(MEMBERNAME_STATEMENT_FIELDS)));
            return method;
        }

        private static CodegenProperty MakeGetStatementAIFactoryProvider(CodegenClassScope classScope)
        {
            var property = CodegenProperty.MakePropertyNode(
                typeof(StatementAIFactoryProvider),
                typeof(StmtClassForgeableStmtProvider),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            property.GetterBlock.BlockReturn(Ref(MEMBERNAME_FACTORY_PROVIDER));
            return property;
        }

        public string ClassName => _statementProviderClassName;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.STMTPROVIDER;
    }
} // end of namespace