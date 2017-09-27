///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.script;
using com.espertech.esper.util;

using Microsoft.ClearScript;
using Microsoft.ClearScript.Windows;

namespace NEsper.Scripting.ClearScript
{
    public class ScriptingEngineJScript: ScriptingEngine
    {
        public ScriptingEngineJScript()
        {
            Language = "jscript.net";
            LanguagePrefix = "jscript";
        }

        public string Language { get; private set; }

        public string LanguagePrefix { get; private set; }

        public void Verify(ExpressionScriptProvided expressionScript)
        {
            try
            {
                using (var engine = new JScriptEngine())
                {
                    ExposeTypesToEngine(engine);

                    engine.AddHostObject("host", new XHostFunctions());
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

        public Func<ScriptArgs, Object> Compile(ExpressionScriptProvided expressionScript)
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

        private object ExecuteWithScriptArgs(ExpressionScriptProvided expressionScript, ScriptArgs args)
        {
            using (var engine = new Microsoft.ClearScript.Windows.JScriptEngine())
            {
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
                engine.AddHostObject("host", new XHostFunctions());
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
            var typeCollection = new HostTypeCollection(
                typeof(com.espertech.esper.client.EPAdministrator).Assembly);

            engine.AddHostObject("esper", typeCollection);
            engine.AddHostType("Object", typeof(Object));
            engine.AddHostType("typeHelper", typeof(com.espertech.esper.util.TypeHelper));
            engine.AddHostType("Collections", typeof(com.espertech.esper.compat.collections.Collections));
            engine.AddHostType("EventBean", typeof(com.espertech.esper.client.EventBean));
            engine.AddHostType("EventBean", typeof(com.espertech.esper.client.EventBean));
            engine.AddHostType("EPAdministrator", typeof(com.espertech.esper.client.EPAdministrator));
            engine.AddHostType("EPRuntime", typeof(com.espertech.esper.client.EPRuntime));
        }
        
        private int _debugCounter = 0;

        public class XHostFunctions : ExtendedHostFunctions
        {
            public object resolveType(string typeName)
            {
                return type(TypeHelper.ResolveType(typeName, false));
            }

            public void throwException(string exceptionTypeName, string message)
            {
                var exceptionType = TypeHelper.ResolveType(exceptionTypeName, true);
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
            public ScriptingEngineJScript Parent { get; private set; }

            public DebugFunctions(ScriptingEngineJScript parent)
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
