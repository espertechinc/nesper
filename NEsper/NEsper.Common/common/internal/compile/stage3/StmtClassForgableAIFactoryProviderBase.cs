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
        public const string MEMBERNAME_ID = "uid";
        public const string MEMBERNAME_STATEMENTAIFACTORY = "statementAIFactory";
        public const string MEMBERNAME_STATEMENT_FIELDS = "statementFields";

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
            var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);

            // REGION: Constructor
            var ctorParams = new List<CodegenTypedParam>() {
                new CodegenTypedParam(
                    typeof(EPStatementInitServices),
                    EPStatementInitServicesConstants.REF.Ref,
                    false)
            };

            if (_namespaceScope.FieldsClassName != null) {
                ctorParams.Add(
                    new CodegenTypedParam(
                        _namespaceScope.FieldsClassName,
                        MEMBERNAME_STATEMENT_FIELDS));
            }

            var ctor = new CodegenCtor(GetType(), _className, includeDebugSymbols, ctorParams);

            ctor.Block.AssignRef(
                MEMBERNAME_ID,
                StaticMethod(typeof(Guid), "NewGuid"));

            if (_namespaceScope.FieldsClassName != null) {
                ctor.Block.ExprDotMethod(
                    Ref(MEMBERNAME_STATEMENT_FIELDS),
                    "Init",
                    EPStatementInitServicesConstants.REF);
            }

            ctor.Block.AssignRef(
                MEMBERNAME_STATEMENTAIFACTORY,
                LocalMethod(
                    CodegenConstructorInit(ctor, classScope),
                    SAIFFInitializeSymbol.REF_STMTINITSVC));

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

            var assignMethod = CodegenMethod
                .MakeMethod(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(StatementAIFactoryAssignments), "assignments");
            if (_namespaceScope.FieldsClassName != null) {
                assignMethod.Block.ExprDotMethod(Ref(MEMBERNAME_STATEMENT_FIELDS), "Assign", @Ref("assignments"));
                //assignMethod.Block.StaticMethod(_namespaceScope.FieldsClassNameOptional, "Assign", @Ref("assignments"));
            }

            var unassignMethod = CodegenMethod
                .MakeMethod(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            if (_namespaceScope.FieldsClassName != null) {
                unassignMethod.Block.ExprDotMethod(Ref(MEMBERNAME_STATEMENT_FIELDS), "Unassign");
                //unassignMethod.Block.StaticMethod(_namespaceScope.FieldsClassNameOptional, "Unassign");
            }

            var setValueMethod = CodegenMethod
                .MakeMethod(typeof(void), typeof(StmtClassForgableStmtFields), classScope)
                .AddParam(typeof(int), "index")
                .AddParam(typeof(object), "value");
            CodegenSubstitutionParamEntry.CodegenSetterBody(
                classScope, setValueMethod.Block, Ref(MEMBERNAME_STATEMENT_FIELDS));

            // Assignment, not sure why this is being done... TBD - Burn this

            CodegenStackGenerator.RecursiveBuildStack(factoryProp, "Factory", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(assignMethod, "Assign", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(unassignMethod, "Unassign", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(setValueMethod, "SetValue", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(ctor, "Ctor", methods, properties);

            return new CodegenClass(
                typeof(StatementAIFactoryProvider),
                _namespaceScope.Namespace,
                _className,
                classScope,
                members,
                ctor,
                methods,
                properties,
                new EmptyList<CodegenInnerClass>());
        }

        public string ClassName => _className;

        public StmtClassForgableType ForgableType => StmtClassForgableType.AIFACTORYPROVIDER;
    }
} // end of namespace