///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compiler.client.option;

namespace com.espertech.esper.compiler.client
{
    /// <summary>
    ///     Callbacks and optional values for the compiler to determine modifiers, statement name,
    ///     statement user object, module name and module-uses.
    ///     All values are optional and can be null.
    /// </summary>
    public class CompilerOptions
    {
        /// <summary>
        ///     Returns the callback that determines the access modifier of a given event type
        /// </summary>
        /// <returns>callback returning an access modifier for an event type</returns>
        public AccessModifierEventTypeOption AccessModifierEventType { get; set; }

        /// <summary>
        ///     Returns the callback that determines a compiler-time statement user object for a
        ///     statement. The user object is available from EPStatement by method {@code getUserObjectCompileTime}.
        /// </summary>
        /// <returns>callback to set a compile-time statement user object</returns>
        public StatementUserObjectOption StatementUserObject { get; set; }

        /// <summary>
        ///     Returns the callback that determines whether the event type is visible in the event bus i.e.
        ///     available for use with send-event.
        /// </summary>
        /// <returns>callback to set the event type bus modifier value</returns>
        public BusModifierEventTypeOption BusModifierEventType { get; set; }

        /// <summary>
        ///     Returns the callback that determines the access modifier of a given context.
        /// </summary>
        /// <returns>callback returning an access modifier for a context</returns>
        public AccessModifierContextOption AccessModifierContext { get; set; }

        /// <summary>
        ///     Returns the callback that determines the access modifier of a given variable.
        /// </summary>
        /// <returns>callback returning an access modifier for a variable</returns>
        public AccessModifierVariableOption AccessModifierVariable { get; set; }

        /// <summary>
        ///     Returns the callback that determines the access modifier of a given declared expression.
        /// </summary>
        /// <returns>callback returning an access modifier for a declared expression</returns>
        public AccessModifierExpressionOption AccessModifierExpression { get; set; }

        /// <summary>
        ///     Returns the callback that determines the access modifier of a given declared expression.
        /// </summary>
        /// <returns>callback returning an access modifier for a declared expression</returns>
        public AccessModifierTableOption AccessModifierTable { get; set; }

        /// <summary>
        ///     Returns the callback that determines the statement name
        /// </summary>
        /// <returns>callback returning the statement name</returns>
        public StatementNameOption StatementName { get; set; }

        /// <summary>
        ///     Returns the callback that determines the access modifier of a given named window.
        /// </summary>
        /// <returns>callback returning an access modifier for an named window</returns>
        public AccessModifierNamedWindowOption AccessModifierNamedWindow { get; set; }

        /// <summary>
        ///     Returns the callback that determines the access modifier of a given script.
        /// </summary>
        /// <returns>callback returning an access modifier for a script</returns>
        public AccessModifierScriptOption AccessModifierScript { get; set; }

        /// <summary>
        ///     Returns the callback that determines the module name.
        /// </summary>
        /// <returns>callback returning the module name to use</returns>
        public ModuleNameOption ModuleName { get; set; }

        /// <summary>
        ///     Returns the callback that determines the module uses.
        /// </summary>
        /// <returns>callback returning the module uses</returns>
        public ModuleUsesOption ModuleUses { get; set; }

        /// <summary>
        /// Returns the classback for inlined-class compilation wherein the callback receives class output
        /// </summary>
        public InlinedClassInspectionOption InlinedClassInspection { get; set; }
        
        /// <summary>
        /// For internal-use-only and subject-to-change-between-versions, state-management settings
        /// </summary>
        public StateMgmtSettingOption StateMgmtSetting { get; set; }
        
        public CompilerOptions SetAccessModifierEventType(AccessModifierEventTypeOption value)
        {
            AccessModifierEventType = value;
            return this;
        }

        public CompilerOptions SetStatementUserObject(StatementUserObjectOption value)
        {
            StatementUserObject = value;
            return this;
        }

        public CompilerOptions SetBusModifierEventType(BusModifierEventTypeOption value)
        {
            BusModifierEventType = value;
            return this;
        }

        public CompilerOptions SetAccessModifierContext(AccessModifierContextOption value)
        {
            AccessModifierContext = value;
            return this;
        }

        public CompilerOptions SetAccessModifierVariable(AccessModifierVariableOption value)
        {
            AccessModifierVariable = value;
            return this;
        }

        public CompilerOptions SetAccessModifierExpression(AccessModifierExpressionOption value)
        {
            AccessModifierExpression = value;
            return this;
        }

        public CompilerOptions SetAccessModifierTable(AccessModifierTableOption value)
        {
            AccessModifierTable = value;
            return this;
        }

        public CompilerOptions SetStatementName(StatementNameOption value)
        {
            StatementName = value;
            return this;
        }

        public CompilerOptions SetAccessModifierNamedWindow(AccessModifierNamedWindowOption value)
        {
            AccessModifierNamedWindow = value;
            return this;
        }

        public CompilerOptions SetAccessModifierScript(AccessModifierScriptOption value)
        {
            AccessModifierScript = value;
            return this;
        }

        public CompilerOptions SetModuleName(ModuleNameOption value)
        {
            ModuleName = value;
            return this;
        }

        public CompilerOptions SetModuleUses(ModuleUsesOption value)
        {
            ModuleUses = value;
            return this;
        }

        public CompilerOptions SetInlinedClassInspection(InlinedClassInspectionOption value)
        {
            InlinedClassInspection = value;
            return this;
        }

        public CompilerOptions SetStateMgmtSetting(StateMgmtSettingOption value)
        {
            StateMgmtSetting = value;
            return this;
        }

    }
} // end of namespace