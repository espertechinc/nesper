///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace NEsper.Scripting.ClearScript
{
    public class ScriptingEngineJavascriptV8 : ScriptingEngine
    {
        public ScriptingEngineJavascriptV8()
        {
            Language = "javascript";
            LanguagePrefix = "js";
        }

        public string Language { get; private set; }

        public string LanguagePrefix { get; private set; }

        public void Initialize(
            IContainer container,
            ConfigurationCommonScripting scriptingConfiguration)
        {
        }

        public void Verify(ExpressionScriptProvided expressionScript)
        {
            try
            {
                using (var engine = new V8ScriptEngine())
                {
                    ExposeTypesToEngine(engine);

                    engine.AddHostObject("host", new XHostFunctions(null));
                    engine.AddHostObject("debug", new DebugFunctions(this));

                    var writer = new StringWriter();
                    WritePolyfills(writer);
                    writer.WriteLine("var __result = function() {");
                    writer.WriteLine(expressionScript.Expression);
                    writer.WriteLine("};");

                    engine.Execute(writer.ToString());
                }
            }
            catch (ScriptEngineException ex)
            {
                throw new ExprValidationException(ex.Message, ex);
            }
        }

        public Func<ScriptArgs, object> Compile(
            ExpressionScriptProvided expressionScript,
            ImportService importService)
        {
            return args => ExecuteWithScriptArgs(expressionScript, args);
        }

        private bool IsPrimitive(object value)
        {
            if (value == null)
                return true;
            if (value.GetType().IsEnum)
                return true;

            return
                (value is string) ||
                (value is DateTime) ||
                (value is DateTimeOffset) ||
                (value is bool) ||
                (value is byte) ||
                (value is char) ||
                (value is short) ||
                (value is int) ||
                (value is long) ||
                (value is float) ||
                (value is double) ||
                (value is decimal);
        }

        private object ExecuteWithScriptArgs(
            ExpressionScriptProvided expressionScript,
            ScriptArgs args)
        {
            using (var engine = new V8ScriptEngine()) {
                var primitives = new ExpandoObject();
                var primitivesAsDictionary = (IDictionary<string, object>)primitives;

                foreach (var binding in args.Bindings)
                {
                    if (IsPrimitive(binding.Value))
                    {
                        primitivesAsDictionary.Add(binding.Key, binding.Value);
                    }
                    else
                    {
                        engine.AddHostObject(binding.Key, binding.Value);
                    }
                }

                ExposeTypesToEngine(engine);

                engine.AddHostObject("__variables", primitives);
                engine.AddHostObject("host", new XHostFunctions(args.Context.TypeResolver));
                engine.AddHostObject("debug", new DebugFunctions(this));

                var writer = new StringWriter();
                WritePolyfills(writer);
                writer.WriteLine("var __result = (function() {");

                foreach (var binding in primitivesAsDictionary)
                {
                    writer.WriteLine("var {0} = __variables.{0};", binding.Key);
                }

                writer.WriteLine(expressionScript.Expression);
                writer.WriteLine("})();");

                engine.Execute(writer.ToString());

                var result = engine.Script.__result;
                if ((result is VoidResult) || (result is Undefined))
                {
                    return null;
                }

                return result;
            }
        }

        private void WritePolyfills(StringWriter writer)
        {
            WriteStartsWithPolyfill(writer);
        }

        private void WriteStartsWithPolyfill(StringWriter writer)
        {
            writer.WriteLine("if (!String.prototype.startsWith) {");
            writer.WriteLine("    String.prototype.startsWith = function(searchString, position) {");
            writer.WriteLine("        return this.substr(position || 0, searchString.length) === searchString;");
            writer.WriteLine("    }");
            writer.WriteLine("}");
        }

        private void ExposeTypesToEngine(Microsoft.ClearScript.ScriptEngine engine)
        {
            //var typeCollection = new HostTypeCollection(
            //    typeof(com.espertech.esper.client.EPAdministrator).Assembly);

            //engine.AddHostObject("esper", typeCollection);
            engine.AddHostType("Object", typeof(Object));
            engine.AddHostType("typeHelper", typeof(TypeHelper));
            engine.AddHostType("Collections", typeof(com.espertech.esper.compat.collections.Collections));
            engine.AddHostType("EventBean", typeof(EventBean));
            engine.AddHostType("Console", typeof(Console));
            engine.AddHostType("Compat", typeof(CompatExtensions));
            //engine.AddHostType("EPRuntime", typeof(com.espertech.esper.client.EPRuntime));
        }
        
        private int _debugCounter = 0;

        public class XHostFunctions : ExtendedHostFunctions
        {
            private readonly TypeResolver typeResolver;

            public XHostFunctions(TypeResolver typeResolver)
            {
                this.typeResolver = typeResolver;
            }

            public object resolveType(string typeName)
            {
                return type(typeResolver.ResolveType(typeName, false));
            }

            public void throwException(string exceptionTypeName, string message)
            {
                var exceptionType = typeResolver.ResolveType(exceptionTypeName, true);
                if (exceptionType.IsInstanceOfType(typeof(Exception)))
                {
                    // attempt to find a constructor with a single argument
                    var ctor = exceptionType.GetConstructor(new Type[] { typeof(string) });
                    if (ctor == null)
                    {
                        throw new EPException("unable to find constructor for object of type \"" + exceptionType.FullName + "\"");
                    }

                    var exception = ctor.Invoke(new object[] { message }) as Exception;
                    if (exception == null)
                    {
                        throw new EPException("unable to construct exception of type \"" + exceptionType.FullName + "\"");
                    }

                    throw exception;
                }
            }
        }

        public class DebugFunctions
        {
            public ScriptingEngineJavascriptV8 Parent { get; private set; }

            public DebugFunctions(ScriptingEngineJavascriptV8 parent)
            {
                this.Parent = parent;
            }

            public void Print(object value)
            {
                Console.Out.WriteLine(value);
            }

            public string Render(object value)
            {
                return value.RenderAny();
            }

            public object Debug(object value)
            {
                Parent._debugCounter++;
                System.Diagnostics.Debug.WriteLine(Convert.ToString(value));
                return value;
            }
        }
    }
}
