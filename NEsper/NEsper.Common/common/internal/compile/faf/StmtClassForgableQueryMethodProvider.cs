///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public class StmtClassForgableQueryMethodProvider : StmtClassForgable
    {
        private const string MEMBERNAME_QUERYMETHOD = "queryMethod";

        private readonly FAFQueryMethodForge forge;
        private readonly CodegenNamespaceScope _namespaceScope;

        public StmtClassForgableQueryMethodProvider(
            string className,
            CodegenNamespaceScope namespaceScope,
            FAFQueryMethodForge forge)
        {
            ClassName = className;
            this._namespaceScope = namespaceScope;
            this.forge = forge;
        }

        public CodegenClass Forge(bool includeDebugSymbols)
        {
            var debugInformationProvider = new Supplier<string>(
                () => {
                    var writer = new StringWriter();
                    writer.Write("FAF query");
                    return writer.ToString();
                });

            try {
                IList<CodegenInnerClass> innerClasses = new List<CodegenInnerClass>();

                // build ctor
                IList<CodegenTypedParam> ctorParms = new List<CodegenTypedParam>();
                ctorParms.Add(
                    new CodegenTypedParam(
                        typeof(EPStatementInitServices),
                        EPStatementInitServicesConstants.REF.Ref,
                        false));
                var providerCtor = new CodegenCtor(GetType(), ClassName, includeDebugSymbols, ctorParms);
                var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, ClassName);

                // add query method member
                IList<CodegenTypedParam> providerExplicitMembers = new List<CodegenTypedParam>(2);
                providerExplicitMembers.Add(new CodegenTypedParam(typeof(FAFQueryMethod), MEMBERNAME_QUERYMETHOD));

                var symbols = new SAIFFInitializeSymbol();
                var makeMethod = providerCtor.MakeChildWithScope(typeof(FAFQueryMethod), GetType(), symbols, classScope)
                    .AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
                providerCtor.Block
                    .StaticMethod(_namespaceScope.FieldsClassNameOptional, "Init", EPStatementInitServicesConstants.REF)
                    .AssignRef(MEMBERNAME_QUERYMETHOD, LocalMethod(makeMethod, EPStatementInitServicesConstants.REF));
                forge.MakeMethod(makeMethod, symbols, classScope);

                // make provider methods
                var propQueryMethod = CodegenProperty.MakePropertyNode(
                    typeof(FAFQueryMethod),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                propQueryMethod
                    .GetterBlock
                    .BlockReturn(Ref(MEMBERNAME_QUERYMETHOD));

                // add get-informational methods
                var propQueryInformationals = CodegenProperty.MakePropertyNode(
                    typeof(FAFQueryInformationals),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                var queryInformationals = FAFQueryInformationals.From(
                    _namespaceScope.SubstitutionParamsByNumber,
                    _namespaceScope.SubstitutionParamsByName);
                propQueryInformationals
                    .GetterBlock
                    .BlockReturn(queryInformationals.Make(
                        propQueryInformationals, classScope));

                // add get-statement-fields method
                var propSubstitutionFieldSetter = CodegenProperty.MakePropertyNode(
                    typeof(FAFQueryMethodAssignerSetter),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                StmtClassForgableStmtFields.MakeSubstitutionSetter(
                    _namespaceScope,
                    propSubstitutionFieldSetter.GetterBlock,
                    classScope);

                // make provider methods
                var methods = new CodegenClassMethods();
                var properties = new CodegenClassProperties();
                CodegenStackGenerator.RecursiveBuildStack(providerCtor, "ctor", methods, properties);
                CodegenStackGenerator.RecursiveBuildStack(propQueryMethod, "QueryMethod", methods, properties);
                CodegenStackGenerator.RecursiveBuildStack(propQueryInformationals, "QueryInformationals", methods, properties);
                CodegenStackGenerator.RecursiveBuildStack(propSubstitutionFieldSetter, "SubstitutionFieldSetter", methods, properties);

                // render and compile
                return new CodegenClass(
                    typeof(FAFQueryMethodProvider),
                    _namespaceScope.Namespace,
                    ClassName,
                    classScope,
                    providerExplicitMembers,
                    providerCtor,
                    methods,
                    properties,
                    innerClasses);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException(
                    "Fatal exception during code-generation for " +
                    debugInformationProvider.Invoke() +
                    " : " +
                    e.Message,
                    e);
            }
        }

        public string ClassName { get; }

        public StmtClassForgableType ForgableType => StmtClassForgableType.FAF;
    }
} // end of namespace