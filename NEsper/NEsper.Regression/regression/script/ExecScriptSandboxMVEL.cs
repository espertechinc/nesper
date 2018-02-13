///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.script
{
    public class ExecScriptSandboxMVEL : RegressionExecution {
    
        // Comment-in for MVEL support custom testing.
        public override void Run(EPServiceProvider epService) {
    
            if (JavaClassHelper.GetClassInClasspath("org.mvel2.MVEL", ClassForNameProviderDefault.INSTANCE) == null) {
                return;
            }
    
            // comment-in
            /*
            string expression = "new MyImportedClass()";
    
            var inputs = new Dictionary<string, Type>();
            inputs.Put("epl", typeof(MyEPLContext));
    
            // analysis
            var analysisResult = new ParserContext();
            analysisResult.StrongTyping = true;
            analysisResult.Inputs = inputs;
            analysisResult.AddImport(typeof(MyImportedClass));
            MVEL.AnalysisCompile(expression, analysisResult);
            System.out.Println(inputs);
    
            // compile
            var compileResult = new ParserContext();
            compileResult.StrongTyping = true;
            compileResult.AddImport(typeof(MyImportedClass));
            ExecutableStatement ce = (ExecutableStatement) MVEL.CompileExpression(expression, compileResult);
    
            // execute
            IDictionary<string, Object> @params = new Dictionary<string, Object>();
            params.Put("epl", new MyEPLContext());
            System.out.Println(MVEL.ExecuteExpression(ce, @params));
    
            long start = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1; i++) {
                MVEL.ExecuteExpression(ce, @params);
            }
            long end = DateTimeHelper.CurrentTimeMillis;
            long delta = end - start;
            System.out.Println("delta=" + delta);
            */
        }
    
        public class MyEPLContext {
            public long GetVariable(string name) {
                return 50L;
            }
        }
    }
} // end of namespace
