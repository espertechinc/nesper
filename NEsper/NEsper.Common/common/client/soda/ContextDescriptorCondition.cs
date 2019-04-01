///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// For use with overlapping or non-overlapping contexts, implementations represents a 
    /// condition for starting/initiating or ending/terminating a context.
    /// </summary>
    public interface ContextDescriptorCondition {
    
        /// <summary>Populate the EPL. </summary>
        /// <param name="writer">output</param>
        /// <param name="formatter">formatter</param>
        void ToEPL(TextWriter writer, EPStatementFormatter formatter);
    }
}
