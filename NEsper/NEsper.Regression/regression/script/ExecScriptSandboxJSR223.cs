///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.script.jsr223;
using com.espertech.esper.supportregression.execution;

using javax.script;

using NUnit.Framework;

namespace com.espertech.esper.regression.script
{
    public class ExecScriptSandboxJSR223 : RegressionExecution {
    
        /// <summary>
        /// MVEL does not support JSR 223.
        /// Making MVEL an Esper compile-time dependency is not desired.
        /// Script and MVEL performance comparison is not close and MVEL is faster.
        /// </summary>
        public override void Run(EPServiceProvider epService) {
            var manager = new ScriptEngineManager();
            ScriptEngine engine = manager.GetEngineByName("js");
            string expressionFib = "Fib(num); function Fib(n) { If(n <= 1) return n; return Fib(n-1) + Fib(n-2); };";
            string expressionTwo = "var words = new Java.util.List();\n" +
                    "words.Add('wordOne');\n" +
                    "words.Add('wordTwo');\n" +
                    "words;\n";
            Compilable compilingEngine = (Compilable) engine;
            CompiledScript script = null;
            try {
                script = compilingEngine.Compile(expressionTwo);
            } catch (ScriptException ex) {
                throw new EPRuntimeException("Script compiler exception: " + JSR223Helper.GetScriptCompileMsg(ex), ex);
            }
    
            Bindings bindings = engine.CreateBindings();
            bindings.Put("epl", new MyEPLContext());
    
            Object result = script.Eval(bindings);
            System.out.Println(result + " typed " + (result != null ? Result.Class : "null"));
    
            long start = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1; i++) {
                script.Eval(bindings);
            }
            long end = DateTimeHelper.CurrentTimeMillis;
            long delta = end - start;
            System.out.Println("delta=" + delta);
        }
    
        private static class MyEPLContext {
            public long GetVariable(string name) {
                return 50L;
            }
        }
    
        private static class MyScriptContext : ScriptContext {
    
            public void SetBindings(Bindings bindings, int scope) {
                System.out.Println("setBindings " + bindings);
            }
    
            public Bindings GetBindings(int scope) {
                System.out.Println("getBindings scope=" + scope);
                return null;
            }
    
            public void SetAttribute(string name, Object value, int scope) {
                System.out.Println("setAttribute name=" + name);
            }
    
            public Object GetAttribute(string name, int scope) {
                System.out.Println("getAttribute name=" + name);
                return null;
            }
    
            public Object RemoveAttribute(string name, int scope) {
                System.out.Println("removeAttribute name=" + name);
                return null;
            }
    
            public Object GetAttribute(string name) {
                System.out.Println("getAttribute name=" + name);
                return null;
            }
    
            public int GetAttributesScope(string name) {
                System.out.Println("getAttributesScope name=" + name);
                return 0;
            }
    
            public Writer GetWriter() {
                System.out.Println("getWriter");
                return null;
            }
    
            public Writer GetErrorWriter() {
                System.out.Println("getErrorWriter");
                return null;
            }
    
            public void SetWriter(Writer writer) {
                System.out.Println("setWriter");
            }
    
            public void SetErrorWriter(Writer writer) {
                System.out.Println("setErrorWriter");
            }
    
            public Reader GetReader() {
                System.out.Println("getReader");
                return null;
            }
    
            public void SetReader(Reader reader) {
                System.out.Println("setReader");
            }
    
            public List<int?> GetScopes() {
                System.out.Println("getScopes");
                return null;
            }
        }
    
        public class MyBindings : Bindings {
            public Object Put(string name, Object value) {
                System.out.Println("put");
                return null;
            }
    
            public void PutAll(Map<? : string, ? : Object> toMerge) {
                System.out.Println("putAll");
            }
    
            public bool ContainsKey(Object key) {
                System.out.Println("containsKey");
                return false;
            }
    
            public Object Get(Object key) {
                System.out.Println("get");
                return null;
            }
    
            public Object Remove(Object key) {
                System.out.Println("remove");
                return null;
            }
    
            public int Size() {
                System.out.Println("size");
                return 0;
            }
    
            public bool IsEmpty() {
                System.out.Println("empty");
                return false;
            }
    
            public bool ContainsValue(Object value) {
                System.out.Println("containsValue");
                return false;
            }
    
            public void Clear() {
                System.out.Println("clear");
            }
    
            public Set<string> KeySet() {
                System.out.Println("keySet");
                return null;
            }
    
            public ICollection<Object> Values() {
                System.out.Println("values");
                return null;
            }
    
            public Set<Entry<string, Object>> EntrySet() {
                System.out.Println("entrySet");
                return null;
            }
        }
    }
} // end of namespace
