///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.compiler.client.option;

namespace com.espertech.esper.compiler.client
{
	/// <summary>
	/// Callbacks and optional values for the compiler to determine modifiers, statement name,
	/// statement user object, module name and module-uses.
	/// All values are optional and can be null.
	/// </summary>
	public class CompilerOptions {
		public CompilerOptions() {
	    }

	    public BusModifierEventTypeOption BusModifierEventType { get; set; }

	    public AccessModifierContextOption AccessModifierContext { get; set; }

	    public AccessModifierEventTypeOption AccessModifierEventType { get; set; }

	    public AccessModifierExpressionOption AccessModifierExpression { get; set; }

	    public AccessModifierNamedWindowOption AccessModifierNamedWindow { get; set; }

	    public AccessModifierScriptOption AccessModifierScript { get; set; }

	    public AccessModifierTableOption AccessModifierTable { get; set; }

	    public AccessModifierVariableOption AccessModifierVariable { get; set; }

	    public AccessModifierInlinedClassOption AccessModifierInlinedClass { get; set; }

	    public StatementUserObjectOption StatementUserObject { get; set; }

	    public StatementNameOption StatementName { get; set; }

	    public ModuleNameOption ModuleName { get; set; }

	    public ModuleUsesOption ModuleUses { get; set; }

	    public InlinedClassInspectionOption InlinedClassInspection { get; set; }

	    public StateMgmtSettingOption StateMgmtSetting { get; set; }

	    public CompilerPathCache PathCache { get; set; }

	    public CompilerHookOption CompilerHook { get; set; }

	    /// <summary>
	    /// Sets the callback that determines the access modifier of a given event type.
	    /// </summary>
	    /// <param name="accessModifierEventType">callback returning an access modifier for an event type</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetAccessModifierEventType(AccessModifierEventTypeOption accessModifierEventType) {
	        AccessModifierEventType = accessModifierEventType;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines a compiler-time statement user object for a
	    /// statement. The user object is available from EPStatement by method {@code getUserObjectCompileTime}.
	    /// </summary>
	    /// <param name="statementUserObject">callback to set a compile-time statement user object</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetStatementUserObject(StatementUserObjectOption statementUserObject) {
	        StatementUserObject = statementUserObject;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines whether the event type is visible in the event bus i.e.
	    /// available for use with send-event.
	    /// </summary>
	    /// <param name="busModifierEventType">callback to set the event type bus modifier value</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetBusModifierEventType(BusModifierEventTypeOption busModifierEventType) {
	        BusModifierEventType = busModifierEventType;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines the access modifier of a given context.
	    /// </summary>
	    /// <param name="accessModifierContext">callback returning an access modifier for a context</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetAccessModifierContext(AccessModifierContextOption accessModifierContext) {
	        AccessModifierContext = accessModifierContext;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines the access modifier of a given variable.
	    /// </summary>
	    /// <param name="accessModifierVariable">callback returning an access modifier for a variable</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetAccessModifierVariable(AccessModifierVariableOption accessModifierVariable) {
	        AccessModifierVariable = accessModifierVariable;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines the access modifier of a given inlined-class.
	    /// </summary>
	    /// <param name="accessModifierInlinedClass">callback returning an access modifier for an inlined-class</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetAccessModifierInlinedClass(AccessModifierInlinedClassOption accessModifierInlinedClass) {
	        AccessModifierInlinedClass = accessModifierInlinedClass;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines the access modifier of a given declared expression.
	    /// </summary>
	    /// <param name="accessModifierExpression">callback returning an access modifier for a declared expression</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetAccessModifierExpression(AccessModifierExpressionOption accessModifierExpression) {
	        AccessModifierExpression = accessModifierExpression;
	        return this;
	    }

	    /// <summary>
	    /// Returns the callback that determines the access modifier of a given table.
	    /// </summary>
	    /// <param name="accessModifierTable">callback returning an access modifier for a table</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetAccessModifierTable(AccessModifierTableOption accessModifierTable) {
	        AccessModifierTable = accessModifierTable;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines the statement name
	    /// </summary>
	    /// <param name="statementName">callback returning the statement name</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetStatementName(StatementNameOption statementName) {
	        StatementName = statementName;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines the access modifier of a given named window.
	    /// </summary>
	    /// <param name="accessModifierNamedWindow">callback returning an access modifier for an named window</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetAccessModifierNamedWindow(AccessModifierNamedWindowOption accessModifierNamedWindow) {
	        AccessModifierNamedWindow = accessModifierNamedWindow;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines the access modifier of a given script.
	    /// </summary>
	    /// <param name="accessModifierScript">callback returning an access modifier for a script</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetAccessModifierScript(AccessModifierScriptOption accessModifierScript) {
	        AccessModifierScript = accessModifierScript;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines the module name.
	    /// </summary>
	    /// <param name="moduleName">callback returning the module name to use</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetModuleName(ModuleNameOption moduleName) {
	        ModuleName = moduleName;
	        return this;
	    }

	    /// <summary>
	    /// Sets the callback that determines the module uses.
	    /// </summary>
	    /// <param name="moduleUses">callback returning the module uses</param>
	    /// <returns>itself</returns>
	    public CompilerOptions SetModuleUses(ModuleUsesOption moduleUses) {
	        ModuleUses = moduleUses;
	        return this;
	    }

	    /// <summary>
	    /// Sets the classback for inlined-class compilation wherein the callback receives class output
	    /// </summary>
	    /// <param name="inlinedClassInspection">callback</param>
	    public void SetInlinedClassInspection(InlinedClassInspectionOption inlinedClassInspection) {
	        InlinedClassInspection = inlinedClassInspection;
	    }

	    /// <summary>
	    /// For internal-use-only and subject-to-change-between-versions, state-management settings
	    /// </summary>
	    /// <param name="stateMgmtSetting">settings option</param>
	    public void SetStateMgmtSetting(StateMgmtSettingOption stateMgmtSetting) {
	        StateMgmtSetting = stateMgmtSetting;
	    }

	    /// <summary>
	    /// Sets the cache, or null if not using a cache, that retains for each <seealso cref="EPCompiled" />
	    /// the EPL objects that the <seealso cref="EPCompiled" /> provides.
	    /// </summary>
	    /// <param name="pathCache">or null if not using a cache</param>
	    public void SetPathCache(CompilerPathCache pathCache) {
	        PathCache = pathCache;
	    }

	    /// <summary>
	    /// Experimental API: Sets the provider of the compiler to use
	    /// <para />NOTE: Experimental API and not supported
	    /// </summary>
	    /// <param name="compilerHook">provider of the compiler to that replaces the default compiler</param>
	    public void SetCompilerHook(CompilerHookOption compilerHook) {
	        CompilerHook = compilerHook;
	    }
	}
} // end of namespace
