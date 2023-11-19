///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Context condition that starts/initiates immediately.</summary>
    public class ContextDescriptorConditionNever : ContextDescriptorCondition
    {
        /// <summary>Ctor.</summary>
        public ContextDescriptorConditionNever()
        {
        }

        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
        }
    }
} // end of namespace