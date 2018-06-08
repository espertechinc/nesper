///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view.internals;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// : a method for pre-loading (initializing) join that does not return any events.
    /// </summary>
    public class JoinPreloadMethodNull : JoinPreloadMethod
    {
        /// <summary>Ctor. </summary>
        public JoinPreloadMethodNull()
        {
        }
    
        public void PreloadFromBuffer(int stream, ExprEvaluatorContext exprEvaluatorContext)
        {
        }
    
        public void PreloadAggregation(ResultSetProcessor resultSetProcessor)
        {
        }
    
        public void SetBuffer(BufferView buffer, int i)
        {        
        }
    
        public bool IsPreloading
        {
            get { return false; }
        }
    }
}
