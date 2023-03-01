///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.context.aifactory.createclass
{
    public class StmtForgeMethodCreateClass : StmtForgeMethodCreateSimpleBase
    {
        private readonly string _className;

        private readonly ClassProvidedPrecompileResult _classProvidedPrecompileResult;

        public StmtForgeMethodCreateClass(
            StatementBaseInfo @base,
            ClassProvidedPrecompileResult classProvidedPrecompileResult,
            string className)
            : base(@base)
        {
            _classProvidedPrecompileResult = classProvidedPrecompileResult;
            _className = className;
        }

        protected override string Register(StatementCompileTimeServices services)
        {
            if (services.ClassProvidedCompileTimeResolver.ResolveClass(_className) != null) {
                throw new ExprValidationException("Class '" + _className + "' has already been declared");
            }

            var classProvided = new ClassProvided(_classProvidedPrecompileResult.Artifact, _className);
            var visibility = services.ModuleVisibilityRules.GetAccessModifierExpression(Base, _className);
            classProvided.ModuleName = Base.ModuleName;
            classProvided.Visibility = visibility;
            classProvided.LoadClasses(services.ParentTypeResolver);
            services.ClassProvidedCompileTimeRegistry.NewClass(classProvided);
            return _className;
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