///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptingServiceImpl : ScriptingService
    {
        /// <summary>
        /// Scripting engines indexed by language prefix
        /// </summary>
        private readonly IDictionary<string, ScriptingEngine> _scriptingEngines =
            new Dictionary<string, ScriptingEngine>();

        /// <summary>
        /// Attempts to discover engine instances in the AppDomain.
        /// </summary>
        public void DiscoverEngines()
        {
            DiscoverEngines(type => true);
        }

        /// <summary>
        /// Attempts to discover engine instances in the AppDomain.
        /// </summary>
        public void DiscoverEngines(Predicate<Type> isEngine)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                DiscoverEngines(assembly, isEngine);
            }
        }

        /// <summary>
        /// Discovers the engines.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="isEngine">The is engine.</param>
        public void DiscoverEngines(
            Assembly assembly,
            Predicate<Type> isEngine)
        {
            Type[] types;
            try {
                types = assembly.GetTypes();
            }
            catch {
                // ignore assemblies that cannot be loaded
                return;
            }

            foreach (var type in types) {
                if (type.IsInterface || type.IsAbstract) {
                    continue;
                }

                if (IsEngineType(type) && isEngine(type)) {
                    var scriptingEngine = (ScriptingEngine) Activator.CreateInstance(type);
                    var scriptingEngineCurr = _scriptingEngines.Get(scriptingEngine.LanguagePrefix);
                    if (scriptingEngineCurr != null) {
                        if (scriptingEngineCurr.GetType() == type) {
                            continue;
                        }

                        throw new ScriptingEngineException(
                            string.Format(
                                "duplicate language prefix \"{0}\" detected",
                                scriptingEngine.LanguagePrefix));
                    }

                    _scriptingEngines.Add(scriptingEngine.LanguagePrefix, scriptingEngine);
                }
            }
        }

        /// <summary>
        /// Determines whether the type is a valid scripting engine type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if [is engine type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsEngineType(Type type)
        {
            return type.IsSubclassOrImplementsInterface<ScriptingEngine>();
        }

        /// <summary>
        /// Compiles the specified language prefix.
        /// </summary>
        /// <param name="dialect">The language prefix.</param>
        /// <param name="script">The script.</param>
        /// <returns></returns>
        public Func<ScriptArgs, object> Compile(
            string dialect,
            ExpressionScriptProvided script)
        {
            var scriptingEngine = _scriptingEngines.Get(dialect);
            if (scriptingEngine == null) {
                throw new ExprValidationException(
                    "Failed to obtain script engine for dialect '" + dialect + "' for script '" + script.Name + "'");
            }

            return scriptingEngine.Compile(script);
        }

        public void VerifyScript(
            string dialect,
            ExpressionScriptProvided script)
        {
            var scriptingEngine = _scriptingEngines.Get(dialect);
            if (scriptingEngine == null) {
                throw new ExprValidationException(
                    "Failed to obtain script engine for dialect '" + dialect + "' for script '" + script.Name + "'");
            }

            scriptingEngine.Verify(script);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}