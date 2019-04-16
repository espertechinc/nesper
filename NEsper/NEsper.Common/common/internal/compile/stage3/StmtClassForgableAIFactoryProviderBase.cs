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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public abstract class StmtClassForgableAIFactoryProviderBase : StmtClassForgable
    {
        protected const string MEMBERNAME_STATEMENTAIFACTORY = "statementAIFactory";

        private readonly string className;
        private readonly CodegenPackageScope packageScope;

        protected abstract Type TypeOfFactory();

        protected abstract CodegenMethod CodegenConstructorInit(
            CodegenMethodScope parent,
            CodegenClassScope classScope);

        public StmtClassForgableAIFactoryProviderBase(
            string className,
            CodegenPackageScope packageScope)
        {
            this.className = className;
            this.packageScope = packageScope;
        }

        public CodegenClass Forge(bool includeDebugSymbols)
        {
            IList<CodegenTypedParam> ctorParms = new List<CodegenTypedParam>();
            ctorParms.Add(new CodegenTypedParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref, false));
            CodegenCtor codegenCtor = new CodegenCtor(this.GetType(), includeDebugSymbols, ctorParms);
            CodegenClassScope classScope = new CodegenClassScope(includeDebugSymbols, packageScope, className);

            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            members.Add(new CodegenTypedParam(TypeOfFactory(), MEMBERNAME_STATEMENTAIFACTORY));

            if (packageScope.FieldsClassNameOptional != null) {
                codegenCtor.Block.StaticMethod(packageScope.FieldsClassNameOptional, "init", EPStatementInitServicesConstants.REF);
            }

            codegenCtor.Block.AssignRef(
                MEMBERNAME_STATEMENTAIFACTORY, LocalMethod(CodegenConstructorInit(codegenCtor, classScope), SAIFFInitializeSymbol.REF_STMTINITSVC));

            CodegenMethod getFactoryMethod = CodegenMethod.MakeParentNode(
                typeof(StatementAgentInstanceFactory), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            getFactoryMethod.Block.MethodReturn(@Ref(MEMBERNAME_STATEMENTAIFACTORY));

            CodegenMethod assignMethod = CodegenMethod.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(StatementAIFactoryAssignments), "assignments");
            if (packageScope.FieldsClassNameOptional != null) {
                assignMethod.Block.StaticMethod(packageScope.FieldsClassNameOptional, "assign", @Ref("assignments"));
            }

            CodegenMethod unassignMethod = CodegenMethod.MakeParentNode(
                typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
            if (packageScope.FieldsClassNameOptional != null) {
                unassignMethod.Block.StaticMethod(packageScope.FieldsClassNameOptional, "unassign");
            }

            CodegenMethod setValueMethod = CodegenMethod.MakeParentNode(typeof(void), typeof(StmtClassForgableStmtFields), classScope)
                .AddParam(typeof(int), "index").AddParam(typeof(object), "value");
            CodegenSubstitutionParamEntry.CodegenSetterMethod(classScope, setValueMethod);

            CodegenClassMethods methods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(getFactoryMethod, "getFactory", methods);
            CodegenStackGenerator.RecursiveBuildStack(assignMethod, "assign", methods);
            CodegenStackGenerator.RecursiveBuildStack(unassignMethod, "unassign", methods);
            CodegenStackGenerator.RecursiveBuildStack(setValueMethod, "setValue", methods);
            CodegenStackGenerator.RecursiveBuildStack(codegenCtor, "ctor", methods);

            return new CodegenClass(
                typeof(StatementAIFactoryProvider), packageScope.PackageName, className, classScope, members, codegenCtor, methods,
                new EmptyList<CodegenInnerClass>());
        }

        public string ClassName {
            get { return className; }
        }

        public StmtClassForgableType ForgableType {
            get { return StmtClassForgableType.AIFACTORYPROVIDER; }
        }
    }
} // end of namespace