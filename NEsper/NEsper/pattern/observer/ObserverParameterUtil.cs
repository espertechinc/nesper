///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.pattern.observer
{
    public class ObserverParameterUtil
    {
        public static void ValidateNoNamedParameters(string name, IList<ExprNode> parameter)
        {
            foreach (ExprNode node in parameter)
            {
                if (node is ExprNamedParameterNode)
                {
                    throw new ObserverParameterException(name + " does not allow named parameters");
                }
            }
        }
    }
} // end of namespace