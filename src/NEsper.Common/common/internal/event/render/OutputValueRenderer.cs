///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

namespace com.espertech.esper.common.@internal.@event.render
{
    /// <summary>
    ///     For rendering an output value returned by a property.
    /// </summary>
    public interface OutputValueRenderer
    {
        /// <summary>
        ///     Renders the value to the buffer.
        /// </summary>
        /// <param name="o">object to render</param>
        /// <param name="buf">buffer to populate</param>
        void Render(
            object o,
            StringBuilder buf);
    }
}