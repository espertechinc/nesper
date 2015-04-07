///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.regression.dataflow
{
    [DataFlowOperator]
    [OutputTypeAttribute("line", typeof(int))]
    [OutputTypeAttribute("wordCount", typeof(int))]
    [OutputTypeAttribute("charCount", typeof(int))]
    public class MyTokenizerCounter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        [DataFlowContext]
        private EPDataFlowEmitter graphContext;
    
        public void OnInput(String line)
        {
            var tokens = line.Split('\t', ' ');
            var wordCount = tokens.Length;
            var charCount = tokens.Sum(token => token.Length);

            Log.Debug("Submitting stat words[" + wordCount + "] chars[" + charCount + "] for line '" + line + "'");
            graphContext.Submit(new Object[] {1, wordCount, charCount});
        }
    }
}
