///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde
{
    /// <summary>
    /// Implementations read and write objects from/to the stream.
    /// </summary>
    public interface DataInputOutputSerdeWCollation<E>
    {
        /// <summary>
        /// Write an object to the stream.
        /// </summary>
        /// <param name="object">to write</param>
        /// <param name="output">to write to</param>
        /// <param name="unitKey">the page key of the page containing the object</param>
        /// <param name="writer">the writer for events</param>
        /// <throws>IOException for io exceptions</throws>
        void Write(
            E @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer);

        /// <summary>
        /// Read an object from the stream.
        /// </summary>
        /// <param name="input">input to read</param>
        /// <param name="unitKey">the identifying key of the reader</param>
        /// <returns>object read</returns>
        /// <throws>IOException for io exceptions</throws>
        E Read(
            DataInput input,
            byte[] unitKey);
    }
} // end of namespace