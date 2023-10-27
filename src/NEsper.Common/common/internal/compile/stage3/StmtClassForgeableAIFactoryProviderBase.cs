///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public abstract class StmtClassForgeableAIFactoryProviderBase : StmtClassForgeable
    {
        public const string MEMBERNAME_ID = "uid";
        public const string MEMBERNAME_STATEMENTAIFACTORY = "statementAIFactory";
        public const string MEMBERNAME_STATEMENT_FIELDS = "statementFields";
        
        private readonly string _className;
        private readonly CodegenNamespaceScope _namespaceScope;
        
        protected abstract Type TypeOfFactory();

        protected abstract CodegenMethod CodegenConstructorInit(
            CodegenMethodScope parent,
            CodegenClassScope classScope);

        public StmtClassForgeableAIFactoryProviderBase(
            string className,
            CodegenNamespaceScope namespaceScope)
        {
            _className = className;
            _namespaceScope = namespaceScope;
        }

        public CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget)
        {
            // REGION: Constructor
            var ctorParams = new List<CodegenTypedParam> {
                new CodegenTypedParam(
                    typeof(EPStatementInitServices),
                    EPStatementInitServicesConstants.REF.Ref,
                    false)
            };
            
            if (_namespaceScope.FieldsClassNameOptional != null) {
                ctorParams.Add(
                    new CodegenTypedParam(
                        _namespaceScope.FieldsClassNameOptional,
                        MEMBERNAME_STATEMENT_FIELDS));
            }
            
            var ctor = new CodegenCtor(GetType(), includeDebugSymbols, ctorParams);
            var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);
            
            if (_namespaceScope.FieldsClassNameOptional != null) {
                ctor.Block.ExprDotMethod(
                    Ref(MEMBERNAME_STATEMENT_FIELDS),
                    "Init",
                    EPStatementInitServicesConstants.REF);
            }

            ctor.Block.AssignMember(
                MEMBERNAME_STATEMENTAIFACTORY,
                LocalMethod(CodegenConstructorInit(ctor, classScope), SAIFFInitializeSymbol.REF_STMTINITSVC));

            // REGION: Members
            var members = new List<CodegenTypedParam>();
            members.Add(new CodegenTypedParam(typeof(Guid), MEMBERNAME_ID));
            members.Add(new CodegenTypedParam(TypeOfFactory(), MEMBERNAME_STATEMENTAIFACTORY));
            
            // REGION: Properties
            var properties = new CodegenClassProperties();
            var factoryProp = CodegenProperty.MakePropertyNode(
                typeof(StatementAgentInstanceFactory),
                GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            factoryProp.GetterBlock.BlockReturn(Ref(MEMBERNAME_STATEMENTAIFACTORY));
            
            // REGION: Methods
            var methods = new CodegenClassMethods();

            CodegenMethod assignMethod = null;
            CodegenMethod unassignMethod = null;
            
            if (_namespaceScope.FieldsClassNameOptional != null && _namespaceScope.HasAssignableStatementFields) {
                assignMethod = CodegenMethod
                    .MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                    .AddParam<StatementAIFactoryAssignments>("assignments");
                assignMethod.Block.ExprDotMethod(Ref(MEMBERNAME_STATEMENT_FIELDS), "Assign", Ref("assignments"));
                //assignMethod.Block.StaticMethod(_namespaceScope.FieldsClassNameOptional, "Assign", Ref("assignments"));
                
                unassignMethod = CodegenMethod
                    .MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
                unassignMethod.Block.ExprDotMethod(Ref(MEMBERNAME_STATEMENT_FIELDS), "Unassign");
                //unassignMethod.Block.StaticMethod(_namespaceScope.FieldsClassNameOptional, "Unassign");
            }

            CodegenMethod setValueMethod = null;
            if (classScope.NamespaceScope.HasSubstitution) {
                setValueMethod = CodegenMethod
                    .MakeParentNode(typeof(void), typeof(StmtClassForgeableStmtFields), classScope)
                    .AddParam<int>("index")
                    .AddParam<object>("value");
                CodegenSubstitutionParamEntry.CodegenSetterBody(
                    classScope, setValueMethod, setValueMethod.Block, Ref(MEMBERNAME_STATEMENT_FIELDS));
            }

            CodegenStackGenerator.RecursiveBuildStack(factoryProp, "Factory", methods, properties);

            if (assignMethod != null) {
                CodegenStackGenerator.RecursiveBuildStack(assignMethod, "Assign", methods, properties);
                CodegenStackGenerator.RecursiveBuildStack(unassignMethod, "Unassign", methods, properties);
            }

            if (setValueMethod != null) {
                CodegenStackGenerator.RecursiveBuildStack(setValueMethod, "SetValue", methods, properties);
            }

            CodegenStackGenerator.RecursiveBuildStack(ctor, "ctor", methods, properties);
            return new CodegenClass(
                CodegenClassType.STATEMENTAIFACTORYPROVIDER,
                typeof(StatementAIFactoryProvider),
                _className,
                classScope,
                members,
                ctor,
                methods,
                properties,
                EmptyList<CodegenInnerClass>.Instance);
        }

        public string ClassName => _className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.AIFACTORYPROVIDER;
    }
} // end of namespace