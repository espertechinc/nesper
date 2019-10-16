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
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptServiceCompileTimeImpl : ScriptServiceCompileTime
    {
        /// <summary>
        /// Scripting engines indexed by language prefix
        /// </summary>
        private readonly IDictionary<string, ScriptingEngine> _scriptingEngines =
            new Dictionary<string, ScriptingEngine>();

        /// <summary>
        /// Attempts to discover engine instances in the AppDomain.
        /// </summary>
        public void DiscoverEngines(IContainer container)
        {
            //if (IsEngineType(type) && isEngine(type)) {
            //    AddScriptingEngine((ScriptingEngine) Activator.CreateInstance(type));
            //}
        }

        /// <summary>
        /// Adds the scripting engine.
        /// </summary>
        /// <param name="scriptingEngine">The scripting engine.</param>
        /// <exception cref="ScriptingEngineException">duplicate language prefix \"{scriptingEngine.LanguagePrefix}\" detected</exception>

        public void AddScriptingEngine(
            ScriptingEngine scriptingEngine)
        {
            var scriptingEngineCurr = _scriptingEngines.Get(scriptingEngine.LanguagePrefix);
            if (scriptingEngineCurr != null)
            {
                if (Equals(scriptingEngineCurr, scriptingEngine)) {
                    return;
                }

                throw new ScriptingEngineException(
                    $"duplicate language prefix \"{scriptingEngine.LanguagePrefix}\" detected");
            }

            _scriptingEngines.Add(scriptingEngine.LanguagePrefix, scriptingEngine);
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

        /// <summary>
        /// Determines whether the type is a valid scripting engine type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if [is engine type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEngineType(Type type)
        {
            return type.IsImplementsInterface<ScriptingEngine>();
            //return type.IsSubclassOrImplementsInterface<ScriptingEngine>();
        }

        public static IEnumerable<Type> FindEngineTypesInAssembly(
            Assembly assembly,
            Predicate<Type> typeMatchingPredicate)
        {
            Type[] types = null;
            try {
                types = assembly.GetTypes();
            }
            catch {
            }

            if (types != null) {
                foreach (var type in types) {
                    if (type.IsInterface || type.IsAbstract || type.IsValueType || type.IsEnum) {
                        continue;
                    }

                    if (IsEngineType(type) && typeMatchingPredicate(type)) {
                        yield return type;
                        //AddScriptingEngine((ScriptingEngine) Activator.CreateInstance(type));
                    }
                }
            }
        }
    }
}