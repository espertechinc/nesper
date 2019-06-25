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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.client.option;

namespace com.espertech.esper.compiler.@internal.util
{
	public class ModuleAccessModifierServiceImpl : ModuleAccessModifierService {
	    private readonly CompilerOptions options;
	    private readonly ConfigurationCompilerByteCode config;

        internal ModuleAccessModifierServiceImpl(CompilerOptions options, ConfigurationCompilerByteCode config) {
	        this.options = options;
	        this.config = config;
	    }

	    public NameAccessModifier GetAccessModifierEventType(StatementRawInfo raw, string eventTypeName) {
	        return GetModifier(raw.Annotations,
	            opts => opts.AccessModifierEventType?.GetValue(new AccessModifierEventTypeContext(raw, eventTypeName)),
                conf => conf.AccessModifierEventType);
	    }

	    public NameAccessModifier GetAccessModifierVariable(StatementBaseInfo @base, string variableName) {
	        return GetModifier(@base.StatementRawInfo.Annotations,
	            opts => opts.AccessModifierVariable?.GetValue(new AccessModifierVariableContext(@base, variableName)),
                conf => conf.AccessModifierVariable);
	    }

	    public NameAccessModifier GetAccessModifierContext(StatementBaseInfo @base, string contextName) {
	        return GetModifier(@base.StatementRawInfo.Annotations,
	            opts => opts.AccessModifierContext?.GetValue(new AccessModifierContextContext(@base, contextName)),
                conf => conf.AccessModifierContext);
	    }

	    public NameAccessModifier GetAccessModifierExpression(StatementBaseInfo @base, string expressionName) {
	        return GetModifier(@base.StatementRawInfo.Annotations,
	            opts => opts.AccessModifierExpression?.GetValue(new AccessModifierExpressionContext(@base, expressionName)),
                conf => conf.AccessModifierExpression);
	    }

	    public NameAccessModifier GetAccessModifierTable(StatementBaseInfo @base, string tableName) {
	        return GetModifier(@base.StatementRawInfo.Annotations,
	            opts => opts.AccessModifierTable?.GetValue(new AccessModifierTableContext(@base, tableName)),
                conf => conf.AccessModifierTable);
	    }

	    public NameAccessModifier GetAccessModifierNamedWindow(StatementBaseInfo @base, string namedWindowName) {
	        return GetModifier(@base.StatementRawInfo.Annotations,
	            opts => opts.AccessModifierNamedWindow?.GetValue(new AccessModifierNamedWindowContext(@base, namedWindowName)),
                conf => conf.AccessModifierNamedWindow);
	    }

	    public NameAccessModifier GetAccessModifierScript(StatementBaseInfo @base, string scriptName, int numParameters) {
	        return GetModifier(@base.StatementRawInfo.Annotations,
	            opts => opts.AccessModifierScript?.GetValue(new AccessModifierScriptContext(@base, scriptName, numParameters)),
	            conf => conf.AccessModifierScript);
	    }

	    public EventTypeBusModifier GetBusModifierEventType(StatementRawInfo raw, string eventTypeName) {
	        if (options.BusModifierEventType != null) {
	            var result = options.BusModifierEventType.GetValue(new BusModifierEventTypeContext(raw, eventTypeName));
	            if (result != null) {
	                return result.Value;
	            }
	        }
	        bool busEventType = AnnotationUtil.FindAnnotation(raw.Annotations, typeof(BusEventTypeAttribute)) != null;
	        if (busEventType) {
	            return EventTypeBusModifier.BUS;
	        }
	        return config.BusModifierEventType;
	    }

        private NameAccessModifier GetModifier(
            Attribute[] annotations,
            Func<CompilerOptions, NameAccessModifier> optionsGet,
            Func<ConfigurationCompilerByteCode, NameAccessModifier> configGet)
        {

            if (options != null) {
                NameAccessModifier result = optionsGet.Invoke(options);
                if (result != null) {
                    return result;
                }
            }

            bool isPrivate = AnnotationUtil.FindAnnotation(annotations, typeof(PrivateAttribute)) != null;
            bool isProtected = AnnotationUtil.FindAnnotation(annotations, typeof(ProtectedAttribute)) != null;
            bool isPublic = AnnotationUtil.FindAnnotation(annotations, typeof(PublicAttribute)) != null;
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
                return NameAccessModifier.PROTECTED;
            }

            if (isPublic) {
                return NameAccessModifier.PUBLIC;
            }

            return configGet.Invoke(config);
        }
    }
} // end of namespace