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

        private readonly string _className;
        private readonly FAFQueryMethodForge _forge;
        private readonly CodegenNamespaceScope _namespaceScope;

        public StmtClassForgeableQueryMethodProvider(
            string className,
            CodegenNamespaceScope namespaceScope,
            FAFQueryMethodForge forge)
        {
            _className = className;
            _namespaceScope = namespaceScope;
            _forge = forge;
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
                    new CodegenTypedParam(
                        typeof(EPStatementInitServices),
                        EPStatementInitServicesConstants.REF.Ref,
                        false));
                ctorParms.Add(
                    new CodegenTypedParam(
                        _namespaceScope.FieldsClassNameOptional,
                        null,
                        "statementFields",
                        true,
                        false));
                
                var providerCtor = new CodegenCtor(GetType(), includeDebugSymbols, ctorParms);
                var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);
                // add query method member
                IList<CodegenTypedParam> providerExplicitMembers = new List<CodegenTypedParam>(2);
                providerExplicitMembers.Add(new CodegenTypedParam(typeof(FAFQueryMethod), MEMBERNAME_QUERYMETHOD));
                var symbols = new SAIFFInitializeSymbol();
                var makeMethod = providerCtor
                    .MakeChildWithScope(typeof(FAFQueryMethod), GetType(), symbols, classScope)
                    .AddParam<EPStatementInitServices>(EPStatementInitServicesConstants.REF.Ref);
                
                if (_namespaceScope.FieldsClassNameOptional != null) {
                    providerCtor.Block.ExprDotMethod(
                        Ref("statementFields"),
                        "Init",
                        EPStatementInitServicesConstants.REF);
                }
                
                providerCtor.Block
                    .AssignMember(MEMBERNAME_QUERYMETHOD, LocalMethod(makeMethod, EPStatementInitServicesConstants.REF));

                _forge.MakeMethod(makeMethod, symbols, classScope);
                
                // make provider methods
                var propQueryMethod = CodegenProperty.MakePropertyNode(
                    typeof(FAFQueryMethod),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                propQueryMethod.GetterBlock.BlockReturn(Ref(MEMBERNAME_QUERYMETHOD));
                
                // add get-informational methods
                var propQueryInformationals = CodegenProperty.MakePropertyNode(
                    typeof(FAFQueryInformationals),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                var queryInformationals = FAFQueryInformationals.From(
                    _namespaceScope.SubstitutionParamsByNumber,
                    _namespaceScope.SubstitutionParamsByName);

                queryInformationals.Make(
                    propQueryInformationals.GetterBlock,
                    classScope);
                //queryInformationals.Make(queryInformationals, classScope);
                
                // add get-statement-fields method
                var propSubstitutionFieldSetter = CodegenProperty.MakePropertyNode(
                    typeof(FAFQueryMethodAssignerSetter),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                
                // create an intermediary method (since they can have child methods and properties cannot)
                var getSubstitutionFieldSetter = propSubstitutionFieldSetter.MakeChildMethodWithScope(
                    typeof(FAFQueryMethodAssignerSetter),
                    GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope);
                StmtClassForgeableStmtFields.MakeSubstitutionSetter(
                    _namespaceScope,
                    getSubstitutionFieldSetter,
                    classScope);
                
                propSubstitutionFieldSetter.GetterBlock.BlockReturn(
                    LocalMethod(getSubstitutionFieldSetter));
                
                // make provider methods
                var methods = new CodegenClassMethods();
                var properties = new CodegenClassProperties();
                CodegenStackGenerator.RecursiveBuildStack(
                    providerCtor,
                    "ctor",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    propQueryMethod,
                    "QueryMethod",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    propQueryInformationals,
                    "QueryInformationals",
                    methods,
                    properties);
                CodegenStackGenerator.RecursiveBuildStack(
                    propSubstitutionFieldSetter,
                    "SubstitutionFieldSetter",
                    methods,
                    properties);
                // render and compile
                return new CodegenClass(
                    CodegenClassType.FAFQUERYMETHODPROVIDER,
                    typeof(FAFQueryMethodProvider),
                    _className,
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

        public string ClassName => _className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.FAF;
    }
} // end of namespace