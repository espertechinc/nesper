///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.spec;
using com.espertech.esper.script;
using com.espertech.esper.util;

#if true
using Jurassic;
#endif

namespace NEsper.Scripting.Jurassic
{
    public class ScriptingEngineJurassic : ScriptingEngine
    {
        public ScriptingEngineJurassic()
        {
            Language = "javascript";
            LanguagePrefix = "js";
        }

        public string Language { get; private set; }

        public string LanguagePrefix { get; private set; }

        public Func<ScriptArgs, Object> Compile(ExpressionScriptProvided expressionScript)
        {
#if true
            var script = new StringScriptSource(expressionScript.Expression);
            return args => ExecuteWithScriptArgs(script, args);
#else
            throw new UnsupportedOperationException("jurassic engine is not supported outside of x86 and x64 builds");
#endif
        }

#if true
        private object ExecuteWithScriptArgs(ScriptSource script, ScriptArgs args)
        {
            var engine = new ScriptEngine();
            engine.EnableExposedClrTypes = true;
            engine.CompatibilityMode = CompatibilityMode.Latest;

            foreach (var binding in args.Bindings)
            {
                engine.SetGlobalValue(binding.Key, binding.Value);
            }

            engine.SetGlobalValue("clr", new ClrBinding());
            engine.SetGlobalFunction("print", new Action<object>(value => Console.Out.WriteLine(value)));
            engine.SetGlobalFunction("render", new Action<object>(value => CompatExtensions.RenderAny(value)));

            return engine.Evaluate(script);
        }
#endif

        public void Verify(ExpressionScriptProvided expressionScript)
        {
#if true
            var script = new StringScriptSource(expressionScript.Expression);
            ScopeTestHelper.AssertNotNull(script);
#else
            throw new UnsupportedOperationException("jurassic engine is not supported outside of x86 and x64 builds");
#endif
        }

        public class TypeWrapper
        {
            public Type Type { get; set; }

            public TypeWrapper(Type type)
            {
                Type = type;
            }

            public object New(object[] args)
            {
                return ConstructorHelper.InvokeConstructor(Type, args);
            }
        }

        public class ClrBinding
        {
            public TextReader Stdin
            {
                get { return Console.In; }
            }

            public TextWriter Stdout
            {
                get { return Console.Out; }
            }

            public TextWriter Stderr
            {
                get { return Console.Error; }
            }

            public AppDomain CurrentAppDomain
            {
                get { return AppDomain.CurrentDomain; }
            }

            public object New(string typeName, object[] args)
            {
                var type = TypeHelper.ResolveType(typeName, true);
                return ConstructorHelper.InvokeConstructor(type, args);
            }

            public object ImportClass(string typeName)
            {
                var type = TypeHelper.ResolveType(typeName, true);
                return new TypeWrapper(type);
            }
        }
    }
}
