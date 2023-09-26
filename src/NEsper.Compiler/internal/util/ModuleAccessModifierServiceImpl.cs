///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.client.option;

namespace com.espertech.esper.compiler.@internal.util
{
    public class ModuleAccessModifierServiceImpl : ModuleAccessModifierService
    {
        private readonly CompilerOptions options;
        private readonly ConfigurationCompilerByteCode config;

        internal ModuleAccessModifierServiceImpl(
            CompilerOptions options,
            ConfigurationCompilerByteCode config)
        {
            this.options = options;
            this.config = config;
        }

        public NameAccessModifier GetAccessModifierEventType(
            StatementRawInfo raw,
            string eventTypeName)
        {
            return GetModifier(
                raw.Annotations,
                opts => opts.AccessModifierEventType?.Invoke(new AccessModifierEventTypeContext(raw, eventTypeName)),
                conf => conf.AccessModifierEventType);
        }

        public NameAccessModifier GetAccessModifierVariable(
            StatementBaseInfo @base,
            string variableName)
        {
            return GetModifier(
                @base.StatementRawInfo.Annotations,
                opts => opts.AccessModifierVariable?.Invoke(new AccessModifierVariableContext(@base, variableName)),
                conf => conf.AccessModifierVariable);
        }

        public NameAccessModifier GetAccessModifierContext(
            StatementBaseInfo @base,
            string contextName)
        {
            return GetModifier(
                @base.StatementRawInfo.Annotations,
                opts => opts.AccessModifierContext?.Invoke(new AccessModifierContextContext(@base, contextName)),
                conf => conf.AccessModifierContext);
        }

        public NameAccessModifier GetAccessModifierExpression(
            StatementBaseInfo @base,
            string expressionName)
        {
            return GetModifier(
                @base.StatementRawInfo.Annotations,
                opts => opts.AccessModifierExpression?.Invoke(
                    new AccessModifierExpressionContext(@base, expressionName)),
                conf => conf.AccessModifierExpression);
        }

        public NameAccessModifier GetAccessModifierTable(
            StatementBaseInfo @base,
            string tableName)
        {
            return GetModifier(
                @base.StatementRawInfo.Annotations,
                opts => opts.AccessModifierTable?.Invoke(new AccessModifierTableContext(@base, tableName)),
                conf => conf.AccessModifierTable);
        }

        public NameAccessModifier GetAccessModifierNamedWindow(
            StatementBaseInfo @base,
            string namedWindowName)
        {
            return GetModifier(
                @base.StatementRawInfo.Annotations,
                opts => opts.AccessModifierNamedWindow?.Invoke(
                    new AccessModifierNamedWindowContext(@base, namedWindowName)),
                conf => conf.AccessModifierNamedWindow);
        }

        public NameAccessModifier GetAccessModifierScript(
            StatementBaseInfo @base,
            string scriptName,
            int numParameters)
        {
            return GetModifier(
                @base.StatementRawInfo.Annotations,
                opts => opts.AccessModifierScript?.Invoke(
                    new AccessModifierScriptContext(@base, scriptName, numParameters)),
                conf => conf.AccessModifierScript);
        }

        public NameAccessModifier GetAccessModifierInlinedClass(
            StatementBaseInfo @base,
            string inlinedClassName)
        {
            return GetModifier(
                @base.StatementRawInfo.Annotations,
                opts => opts.AccessModifierInlinedClass?.Invoke(new AccessModifierInlinedClassContext(@base, inlinedClassName)),
                _ => _.AccessModifierInlinedClass);
        }

        public EventTypeBusModifier GetBusModifierEventType(
            StatementRawInfo raw,
            string eventTypeName)
        {
            var result = options.BusModifierEventType?.Invoke(new BusModifierEventTypeContext(raw, eventTypeName));
            if (result != null) {
                return result.Value;
            }

            var busEventType = AnnotationUtil.HasAnnotation(raw.Annotations, typeof(BusEventTypeAttribute));
            if (busEventType) {
                return EventTypeBusModifier.BUS;
            }

            return config.BusModifierEventType;
        }

        private NameAccessModifier GetModifier(
            Attribute[] annotations,
            Func<CompilerOptions, NameAccessModifier?> optionsGet,
            Func<ConfigurationCompilerByteCode, NameAccessModifier> configGet)
        {
            if (options != null) {
                var result = optionsGet.Invoke(options);
                if (result != null) {
                    return result.Value;
                }
            }

            var isPrivate = AnnotationUtil.HasAnnotation(annotations, typeof(PrivateAttribute));
            var isProtected = AnnotationUtil.HasAnnotation(annotations, typeof(ProtectedAttribute));
            var isPublic = AnnotationUtil.HasAnnotation(annotations, typeof(PublicAttribute));
            if (isPrivate) {
                if (isProtected) {
                    throw new EPException("Encountered both the @private and the @protected annotation");
                }

                if (isPublic) {
                    throw new EPException("Encountered both the @private and the @public annotation");
                }
            }
            else if (isProtected && isPublic) {
                throw new EPException("Encountered both the @protected and the @public annotation");
            }

            if (isPrivate) {
                return NameAccessModifier.PRIVATE;
            }

            if (isProtected) {
                return NameAccessModifier.INTERNAL;
            }

            if (isPublic) {
                return NameAccessModifier.PUBLIC;
            }

            return configGet.Invoke(config);
        }
    }
} // end of namespace