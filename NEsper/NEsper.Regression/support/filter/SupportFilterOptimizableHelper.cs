///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.support.filter
{
    public class SupportFilterOptimizableHelper
    {
        public static bool HasFilterIndexPlanBasic(RegressionEnvironment env)
        {
            return env.Configuration.Compiler.Execution.FilterIndexPlanning == ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC;
        }

        public static bool HasFilterIndexPlanBasicOrMore(RegressionEnvironment env)
        {
            return env.Configuration.Compiler.Execution.FilterIndexPlanning >= ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC;
        }

        public static bool HasFilterIndexPlanAdvanced(RegressionEnvironment env)
        {
            return env.Configuration.Compiler.Execution.FilterIndexPlanning == ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED;
        }
    }
}