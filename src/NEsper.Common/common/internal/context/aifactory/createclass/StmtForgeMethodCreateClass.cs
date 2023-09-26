///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;


namespace com.espertech.esper.common.@internal.context.aifactory.createclass
{
    public class StmtForgeMethodCreateClass : StmtForgeMethodCreateSimpleBase
    {
        private readonly ClassProvidedPrecompileResult _classProvidedPrecompileResult;
        private readonly string _className;

        public StmtForgeMethodCreateClass(
            StatementBaseInfo @base,
            ClassProvidedPrecompileResult classProvidedPrecompileResult,
            string className) : base(@base)
        {
            _classProvidedPrecompileResult = classProvidedPrecompileResult;
            _className = className;
        }

        protected override StmtForgeMethodRegisterResult Register(StatementCompileTimeServices services)
        {
            if (services.ClassProvidedCompileTimeResolver.ResolveClass(_className) != null) {
                throw new ExprValidationException("Class '" + _className + "' has already been declared");
            }

            var classProvided = new ClassProvided(_classProvidedPrecompileResult.Artifact, _className);
            var visibility =
                services.ModuleVisibilityRules.GetAccessModifierInlinedClass(_base, classProvided.ClassName);
            classProvided.ModuleName = _base.ModuleName;
            classProvided.Visibility = visibility;
            classProvided.LoadClasses(services.ParentTypeResolver);
            services.ClassProvidedCompileTimeRegistry.NewClass(classProvided);

            var fabricCharge = services.StateMgmtSettingsProvider.NewCharge();
            services.StateMgmtSettingsProvider.InlinedClasses(fabricCharge, classProvided);
            return new StmtForgeMethodRegisterResult(_className, fabricCharge);
        }

        protected override StmtClassForgeable AiFactoryForgable(
            string className,
            CodegenNamespaceScope namespaceScope,
            EventType statementEventType,
            string objectName)
        {
            var forge = new StatementAgentInstanceFactoryCreateClassForge(statementEventType, className);
            return new StmtClassForgeableAIFactoryProviderCreateClass(className, namespaceScope, forge);
        }
    }
} // end of namespace