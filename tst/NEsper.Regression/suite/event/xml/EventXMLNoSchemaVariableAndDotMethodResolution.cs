///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaVariableAndDotMethodResolution : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmtTextOne =
                "select var, xpathAttrNum.after(xpathAttrNumTwo) from TestXMLNoSchemaTypeWNum#length(100)";
            env.CompileDeploy(stmtTextOne).UndeployAll();
        }
    }
} // end of namespace