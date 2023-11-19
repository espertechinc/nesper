///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class ConfigurationCompilerByteCode
    {
        private NameAccessModifier accessModifierContext = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierEventType = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierExpression = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierInlinedClass = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierNamedWindow = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierScript = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierTable = NameAccessModifier.PRIVATE;
        private NameAccessModifier accessModifierVariable = NameAccessModifier.PRIVATE;

        private bool allowInlinedClass = true;
        private bool allowSubscriber;
        private bool attachEPL = true;
        private bool attachModuleEPL;
        private bool attachPatternEPL;
        private EventTypeBusModifier busModifierEventType = EventTypeBusModifier.NONBUS;
        private bool includeComments;
        private bool includeDebugSymbols;
        private bool instrumented;
        private int internalUseOnlyMaxMembersPerClass = 2 * 1024;

        private int internalUseOnlyMaxMethodComplexity = 1024;

        private int maxMethodsPerClass = 1024;
        private int? threadPoolCompilerCapacity;
        private int threadPoolCompilerNumThreads = 8;

        /// <summary>
        ///     Returns indicator whether the binary class code should include debug symbols
        /// </summary>
        /// <value>indicator</value>
        public bool IsIncludeDebugSymbols {
            get => includeDebugSymbols;
            set => includeDebugSymbols = value;
        }

        /// <summary>
        ///     Returns indicator whether the generated source code should include comments for tracing back
        /// </summary>
        /// <value>indicator</value>
        public bool IsIncludeComments {
            get => includeComments;
            set => includeComments = value;
        }

        /// <summary>
        ///     Returns the indicator whether the EPL text will be available as a statement property.
        ///     The default is true and the compiler provides the EPL as a statement property.
        ///     When set to false the compiler does not retain the EPL in the compiler output.
        /// </summary>
        /// <value>indicator</value>
        public bool IsAttachEPL {
            get => attachEPL;
            set => attachEPL = value;
        }

        /// <summary>
        ///     Returns the indicator whether the EPL module text will be available as a module property.
        ///     The default is false and the compiler does not provide the module EPL as a module property.
        ///     When set to true the compiler retains the module EPL in the compiler output.
        /// </summary>
        /// <value>indicator</value>
        public bool IsAttachModuleEPL {
            get => attachModuleEPL;
            set => attachModuleEPL = value;
        }

        /// <summary>
        ///     Returns indicator whether any statements allow subscribers or not (false by default).
        ///     The default is false which results in the runtime throwing an exception when an application calls {@code
        ///     setSubscriber}
        ///     on a statement.
        /// </summary>
        /// <value>indicator</value>
        public bool IsAllowSubscriber {
            get => allowSubscriber;
            set => allowSubscriber = value;
        }

        /// <summary>
        ///     Returns the indicator whether the compiler generates instrumented byte code for use with the debugger.
        /// </summary>
        /// <value>indicator</value>
        public bool IsInstrumented {
            get => instrumented;
            set => instrumented = value;
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
        ///     Returns the default access modifier for inlined-classes
        /// </summary>
        /// <value>access modifier</value>
        public NameAccessModifier AccessModifierInlinedClass {
            get => accessModifierInlinedClass;
            set => accessModifierInlinedClass = value;
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
        ///     Returns the indicator whether, for tools with access to pattern factories, the pattern subexpression text
        ///     will be available for the pattern.
        ///     The default is false and the compiler does not produce text for patterns for tooling.
        ///     When set to true the compiler does generate pattern subexpression text for pattern for use by tools.
        /// </summary>
        /// <value>indicator</value>
        public bool IsAttachPatternEPL {
            get => attachPatternEPL;
            set => attachPatternEPL = value;
        }

        /// <summary>
        ///     Returns the number of threads available for parallel compilation of multiple EPL statements. The default is 8
        ///     threads.
        /// </summary>
        /// <value>number of threads</value>
        public int ThreadPoolCompilerNumThreads {
            get => threadPoolCompilerNumThreads;
            set => threadPoolCompilerNumThreads = value;
        }

        /// <summary>
        ///     Returns the capacity of the parallel compiler semaphore, or null if none defined (null is the default and is the
        ///     unbounded case).
        /// </summary>
        /// <value>capacity or null if none defined</value>
        public int? ThreadPoolCompilerCapacity {
            get => threadPoolCompilerCapacity;
            set => threadPoolCompilerCapacity = value;
        }

        /// <summary>
        ///     Returns the maximum number of methods per class, which defaults to 1k. The lower limit for this number is 1000.
        /// </summary>
        /// <value>max number methods per class</value>
        public int MaxMethodsPerClass {
            get => maxMethodsPerClass;
            set => maxMethodsPerClass = value;
        }

        /// <summary>
        ///     (Internal-use-only) Returns the maximum number of members per class, which defaults to 2k. The lower limit for this
        ///     number is 1.
        /// </summary>
        /// <value>max number of members per class</value>
        public int InternalUseOnlyMaxMembersPerClass {
            get => internalUseOnlyMaxMembersPerClass;
            set => internalUseOnlyMaxMembersPerClass = value;
        }

        /// <summary>
        ///     (Internal-use-only) Sets the maximum method complexity, which defaults to 1k. Applicable to methods that repeat
        ///     operations on elements.
        ///     This roughly corresponds to lines of code of a method. The lower limit is not defined.
        /// </summary>
        /// <value>max method complexity</value>
        public int InternalUseOnlyMaxMethodComplexity {
            get => internalUseOnlyMaxMethodComplexity;
            set => internalUseOnlyMaxMethodComplexity = value;
        }

        /// <summary>
        ///     Returns the flag whether the compiler allows inlined classes
        /// </summary>
        /// <value>flag</value>
        public bool IsAllowInlinedClass {
            get => allowInlinedClass;
            set => allowInlinedClass = value;
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
            accessModifierInlinedClass = NameAccessModifier.PUBLIC;
        }

        private void CheckModifier(NameAccessModifier modifier)
        {
            if (!modifier.IsModuleProvidedAccessModifier()) {
                throw new ConfigurationException("Access modifier configuration allows private, protected or public");
            }
        }
    }
} // end of namespace