///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.faf
{
    public class StmtClassForgableQueryMethodProvider : StmtClassForgable
    {
        private const string MEMBERNAME_QUERYMETHOD = "queryMethod";

        private readonly FAFQueryMethodForge forge;
        private readonly CodegenPackageScope packageScope;

        public StmtClassForgableQueryMethodProvider(
            string className, CodegenPackageScope packageScope, FAFQueryMethodForge forge)
        {
            ClassName = className;
            this.packageScope = packageScope;
            this.forge = forge;
        }

        public CodegenClass Forge(bool includeDebugSymbols)
        {
            var debugInformationProvider = () => {
                var writer = new StringWriter();
                writer.Write("FAF query");
                return writer.ToString();
            };

            try {
                IList<CodegenInnerClass> innerClasses = new List<CodegenInnerClass>();

                // build ctor
                IList<CodegenTypedParam> ctorParms = new List<CodegenTypedParam>();
                ctorParms.Add(
                    new CodegenTypedParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref, false));
                var providerCtor = new CodegenCtor(GetType(), includeDebugSymbols, ctorParms);
                var classScope = new CodegenClassScope(includeDebugSymbols, packageScope, ClassName);

                // add query method member
                IList<CodegenTypedParam> providerExplicitMembers = new List<CodegenTypedParam>(2);
                providerExplicitMembers.Add(new CodegenTypedParam(typeof(FAFQueryMethod), MEMBERNAME_QUERYMETHOD));

                var symbols = new SAIFFInitializeSymbol();
                var makeMethod = providerCtor.MakeChildWithScope(typeof(FAFQueryMethod), GetType(), symbols, classScope)
                    .AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
                providerCtor.Block
                    .StaticMethod(packageScope.FieldsClassNameOptional, "init", EPStatementInitServicesConstants.REF)
                    .AssignRef(MEMBERNAME_QUERYMETHOD, LocalMethod(makeMethod, EPStatementInitServicesConstants.REF));
                forge.MakeMethod(makeMethod, symbols, classScope);

                // make provider methods
                var getQueryMethod = CodegenMethod.MakeParentNode(
                    typeof(FAFQueryMethod), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
                getQueryMethod.Block.MethodReturn(Ref(MEMBERNAME_QUERYMETHOD));

                // add get-informational methods
                var getQueryInformationals = CodegenMethod.MakeParentNode(
                    typeof(FAFQueryInformationals), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
                FAFQueryInformationals queryInformationals = FAFQueryInformationals.From(
                    packageScope.SubstitutionParamsByNumber, packageScope.SubstitutionParamsByName);
                getQueryInformationals.Block.MethodReturn(queryInformationals.Make(getQueryInformationals, classScope));

                // add get-statement-fields method
                var getSubstitutionFieldSetter = CodegenMethod.MakeParentNode(
                    typeof(FAFQueryMethodAssignerSetter), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
                StmtClassForgableStmtFields.MakeSubstitutionSetter(
                    packageScope, getSubstitutionFieldSetter, classScope);

                // make provider methods
                var methods = new CodegenClassMethods();
                CodegenStackGenerator.RecursiveBuildStack(providerCtor, "ctor", methods);
                CodegenStackGenerator.RecursiveBuildStack(getQueryMethod, "getQueryMethod", methods);
                CodegenStackGenerator.RecursiveBuildStack(getQueryInformationals, "getQueryInformationals", methods);
                CodegenStackGenerator.RecursiveBuildStack(
                    getSubstitutionFieldSetter, "getSubstitutionFieldSetter", methods);

                // render and compile
                return new CodegenClass(
                    typeof(FAFQueryMethodProvider), packageScope.PackageName, ClassName, classScope,
                    providerExplicitMembers, providerCtor, methods, innerClasses);
            }
            catch (Throwable t) {
                throw new EPException(
                    "Fatal exception during code-generation for " + debugInformationProvider.Get() + " : " + t.Message,
                    t);
            }
        }

        public string ClassName { get; }

        public StmtClassForgableType ForgableType => StmtClassForgableType.FAF;
    }
} // end of namespace