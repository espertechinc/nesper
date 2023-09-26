///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.faf
{
    public class StmtClassForgeableQueryMethodProvider : StmtClassForgeable
    {
        private const string MEMBERNAME_QUERYMETHOD = "queryMethod";
        private readonly string className;
        private readonly CodegenNamespaceScope namespaceScope;
        private readonly FAFQueryMethodForge forge;

        public StmtClassForgeableQueryMethodProvider(
            string className,
            CodegenNamespaceScope namespaceScope,
            FAFQueryMethodForge forge)
        {
            this.className = className;
            this.namespaceScope = namespaceScope;
            this.forge = forge;
        }

        public CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget)
        {
            Supplier<string> debugInformationProvider = () => {
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
                var classScope = new CodegenClassScope(includeDebugSymbols, namespaceScope, className);
                // add query method member
                IList<CodegenTypedParam> providerExplicitMembers = new List<CodegenTypedParam>(2);
                providerExplicitMembers.Add(new CodegenTypedParam(typeof(FAFQueryMethod), MEMBERNAME_QUERYMETHOD));
                var symbols = new SAIFFInitializeSymbol();
                var makeMethod = providerCtor.MakeChildWithScope(typeof(FAFQueryMethod), GetType(), symbols, classScope)
                    .AddParam<EPStatementInitServices>(EPStatementInitServicesConstants.REF.Ref);
                providerCtor.Block
                    .StaticMethod(namespaceScope.FieldsClassNameOptional, "init", EPStatementInitServicesConstants.REF)
                    .AssignMember(MEMBERNAME_QUERYMETHOD, LocalMethod(makeMethod, EPStatementInitServicesConstants.REF));
                forge.MakeMethod(makeMethod, symbols, classScope);
                // make provider methods
                var getQueryMethod = CodegenMethod.MakeParentNode(
                    typeof(FAFQueryMethod),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                getQueryMethod.Block.MethodReturn(Ref(MEMBERNAME_QUERYMETHOD));
                // add get-informational methods
                var getQueryInformationals = CodegenMethod.MakeParentNode(
                    typeof(FAFQueryInformationals),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                var queryInformationals = FAFQueryInformationals.From(
                    namespaceScope.SubstitutionParamsByNumber,
                    namespaceScope.SubstitutionParamsByName);
                getQueryInformationals.Block.MethodReturn(queryInformationals.Make(getQueryInformationals, classScope));
                // add get-statement-fields method
                var getSubstitutionFieldSetter = CodegenMethod.MakeParentNode(
                    typeof(FAFQueryMethodAssignerSetter),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                StmtClassForgeableStmtFields.MakeSubstitutionSetter(
                    namespaceScope,
                    getSubstitutionFieldSetter,
                    classScope);
                // make provider methods
                var methods = new CodegenClassMethods();
                var properties = new CodegenClassProperties();
                CodegenStackGenerator.RecursiveBuildStack(
                    providerCtor,
                    "ctor",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    getQueryMethod,
                    "getQueryMethod",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    getQueryInformationals,
                    "getQueryInformationals",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    getSubstitutionFieldSetter,
                    "getSubstitutionFieldSetter",
                    methods,
                    properties);
                // render and compile
                return new CodegenClass(
                    CodegenClassType.FAFQUERYMETHODPROVIDER,
                    typeof(FAFQueryMethodProvider),
                    className,
                    classScope,
                    providerExplicitMembers,
                    providerCtor,
                    methods,
                    properties,
                    innerClasses);
            }
            catch (Exception ex) {
                throw new EPException(
                    "Fatal exception during code-generation for " + debugInformationProvider.Invoke() + " : " + ex.Message,
                    ex);
            }
        }

        public string ClassName => className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.FAF;
    }
} // end of namespace