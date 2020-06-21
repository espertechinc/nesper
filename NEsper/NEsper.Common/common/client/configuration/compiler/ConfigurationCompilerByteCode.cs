///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Code generation settings.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerByteCode
    {
        private NameAccessModifier accessModifierContext = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierEventType = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierExpression = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierNamedWindow = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierScript = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierTable = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierVariable = NameAccessModifier.PRIVATE;
        private EventTypeBusModifier busModifierEventType = EventTypeBusModifier.NONBUS;
        
        /// <summary>
        /// Returns the number of threads available for parallel compilation of multiple EPL statements.
        /// The default is 8 threads.
        /// </summary>
        public int ThreadPoolCompilerNumThreads { get; set; } = 8;

        /// <summary>
        /// Returns the capacity of the parallel compiler semaphore, or null if none defined
        /// (null is the default and is the unbounded case).
        /// </summary>
        public int? ThreadPoolCompilerCapacity { get; set; } = null;

        /// <summary>
        /// Returns the maximum number of methods per class, which defaults to 16k. The lower limit
        /// for this number is 1000.
        /// </summary>
        public int MaxMethodsPerClass { get; set; } = 16 * 1024;
        
        /// <summary>
        /// Returns the flag whether the compiler allows inlined classes.
        /// </summary>
        public bool IsAllowInlinedClass { get; set; } = true;

        /// <summary>
        ///     Returns indicator whether the binary class code should include debug symbols
        /// </summary>
        /// <value>indicator</value>
        public bool IsIncludeDebugSymbols { get; private set; }

        /// <summary>
        ///     Sets indicator whether the binary class code should include debug symbols
        /// </summary>
        /// <value>indicator</value>
        public bool IncludeDebugSymbols {
            get => IsIncludeDebugSymbols;
            set => IsIncludeDebugSymbols = value;
        }

        /// <summary>
        ///     Returns indicator whether the generated source code should include comments for tracing back
        /// </summary>
        /// <value>indicator</value>
        public bool IsIncludeComments { get; private set; }

        /// <summary>
        ///     Sets indicator whether the generated source code should include comments for tracing back
        /// </summary>
        /// <value>indicator</value>
        public bool IncludeComments {
            get => IsIncludeComments;
            set => IsIncludeComments = value;
        }

        /// <summary>
        ///     Returns the indicator whether the EPL text will be available as a statement property.
        ///     The default is true and the compiler provides the EPL as a statement property.
        ///     When set to false the compiler does not retain the EPL in the compiler output.
        /// </summary>
        /// <value>indicator</value>
        public bool IsAttachEPL { get; private set; } = true;

        /// <summary>
        ///     Sets the indicator whether the EPL text will be available as a statement property.
        ///     The default is true and the compiler provides the EPL as a statement property.
        ///     When set to false the compiler does not retain the EPL in the compiler output.
        /// </summary>
        /// <value>indicator</value>
        public bool AttachEPL {
            get => IsAttachEPL;
            set => IsAttachEPL = value;
        }

        /// <summary>
        ///     Returns the indicator whether the EPL module text will be available as a module property.
        ///     The default is false and the compiler does not provide the module EPL as a module property.
        ///     When set to true the compiler retains the module EPL in the compiler output.
        /// </summary>
        /// <value>indicator</value>
        public bool IsAttachModuleEPL { get; private set; }

        /// <summary>
        ///     Sets the indicator whether the EPL module text will be available as a module property.
        ///     The default is false and the compiler does not provide the module EPL as a module property.
        ///     When set to true the compiler retains the module EPL in the compiler output.
        /// </summary>
        /// <value>indicator</value>
        public bool AttachModuleEPL {
            get => IsAttachModuleEPL;
            set => IsAttachModuleEPL = value;
        }

        /// <summary>
        /// Returns the indicator whether, for tools with access to pattern factories, the pattern subexpression text
        /// will be available for the pattern.
        /// The default is false and the compiler does not produce text for patterns for tooling.
        /// When set to true the compiler does generate pattern subexpression text for pattern for use by tools.
        /// </summary>
        public bool IsAttachPatternEPL { get; private set; }

        /// <summary>
        /// Gets or sets the indicator whether, for tools with access to pattern factories, the pattern subexpression text
        /// will be available for the pattern.
        /// The default is false and the compiler does not produce text for patterns for tooling.
        /// When set to true the compiler does generate pattern subexpression text for pattern for use by tools.
        /// </summary>
        public bool AttachPatternEPL {
            get => IsAttachPatternEPL;
            set => IsAttachPatternEPL = value;
        }
        
        /// <summary>
        ///     Returns indicator whether any statements allow subscribers or not (false by default).
        ///     The default is false which results in the runtime throwing an exception when an application calls {@code
        ///     setSubscriber}
        ///     on a statement.
        /// </summary>
        /// <value>indicator</value>
        public bool IsAllowSubscriber { get; private set; }

        /// <summary>
        ///     Sets indicator whether any statements allow subscribers or not (false by default).
        ///     The default is false which results in the runtime throwing an exception when an application calls {@code
        ///     setSubscriber}
        ///     on a statement.
        /// </summary>
        /// <value>indicator</value>
        public bool AllowSubscriber {
            get => IsAllowSubscriber;
            set => IsAllowSubscriber = value;
        }

        /// <summary>
        ///     Returns the indicator whether the compiler generates instrumented byte code for use with the debugger.
        /// </summary>
        /// <value>indicator</value>
        public bool IsInstrumented { get; private set; }

        /// <summary>
        ///     Sets the indicator whether the compiler generates instrumented byte code for use with the debugger.
        /// </summary>
        /// <value>indicator</value>
        public bool Instrumented {
            get => IsInstrumented;
            set => IsInstrumented = value;
        }

        /// <summary>
        ///     Returns the default access modifier for event types
        /// </summary>
        /// <value>access modifier</value>
        public NameAccessModifier AccessModifierEventType {
            get => accessModifierEventType;
            set {
                CheckModifier(value);
                accessModifierEventType = value;
            }
        }

        /// <summary>
        ///     Returns the default access modifier for named windows
        /// </summary>
        /// <value>access modifier</value>
        public NameAccessModifier AccessModifierNamedWindow {
            get => accessModifierNamedWindow;
            set {
                CheckModifier(value);
                accessModifierNamedWindow = value;
            }
        }

        /// <summary>
        ///     Returns the default access modifier for contexts
        /// </summary>
        /// <value>access modifier</value>
        public NameAccessModifier AccessModifierContext {
            get => accessModifierContext;
            set {
                CheckModifier(value);
                accessModifierContext = value;
            }
        }

        /// <summary>
        ///     Returns the default access modifier for variables
        /// </summary>
        /// <value>access modifier</value>
        public NameAccessModifier AccessModifierVariable {
            get => accessModifierVariable;
            set {
                CheckModifier(value);
                accessModifierVariable = value;
            }
        }

        /// <summary>
        ///     Returns the default access modifier for declared expressions
        /// </summary>
        /// <value>access modifier</value>
        public NameAccessModifier AccessModifierExpression {
            get => accessModifierExpression;
            set {
                CheckModifier(value);
                accessModifierExpression = value;
            }
        }

        /// <summary>
        ///     Returns the default access modifier for scripts
        /// </summary>
        /// <value>access modifier</value>
        public NameAccessModifier AccessModifierScript {
            get => accessModifierScript;
            set {
                CheckModifier(value);
                accessModifierScript = value;
            }
        }

        /// <summary>
        ///     Returns the default access modifier for tables
        /// </summary>
        /// <value>access modifier</value>
        public NameAccessModifier AccessModifierTable {
            get => accessModifierTable;
            set => accessModifierTable = value;
        }

        /// <summary>
        ///     Returns the default bus modifier for event types
        /// </summary>
        /// <value>access modifier</value>
        public EventTypeBusModifier BusModifierEventType {
            get => busModifierEventType;
            set => busModifierEventType = value;
        }

        /// <summary>
        ///     Set all access modifiers to public.
        /// </summary>
        public void SetAccessModifiersPublic()
        {
            accessModifierEventType = NameAccessModifier.PUBLIC;
            accessModifierNamedWindow = NameAccessModifier.PUBLIC;
            accessModifierContext = NameAccessModifier.PUBLIC;
            accessModifierVariable = NameAccessModifier.PUBLIC;
            accessModifierExpression = NameAccessModifier.PUBLIC;
            accessModifierScript = NameAccessModifier.PUBLIC;
            accessModifierTable = NameAccessModifier.PUBLIC;
        }

        private void CheckModifier(NameAccessModifier modifier)
        {
            if (!modifier.IsModuleProvidedAccessModifier()) {
                throw new ConfigurationException("Access modifier configuration allows private, protected or public");
            }
        }
    }
} // end of namespace