///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    ///     Provides a range as a start and end value, for use as a parameter to the lookup values passed to the
    ///     <seealso cref="VirtualDataWindowLookup" /> lookup method.
    ///     <para />
    ///     Consult <seealso cref="VirtualDataWindowLookupOp" /> for information on the type of range represented (open,
    ///     closed, inverted etc.) .
    /// </summary>
    public class VirtualDataWindowKeyRange
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="start">range start</param>
        /// <param name="end">range end</param>
        public VirtualDataWindowKeyRange(
            object start,
            object end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        ///     Returns the start value of the range.
        /// </summary>
        /// <returns>start value</returns>
        public object Start { get; }

        /// <summary>
        ///     Returns the end value of the range.
        /// </summary>
        /// <returns>end value</returns>
        public object End { get; }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (VirtualDataWindowKeyRange)o;

            if (!End?.Equals(that.End) ?? that.End != null) {
                return false;
            }

            if (!Start?.Equals(that.Start) ?? that.Start != null) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return CompatExtensions.HashAll(Start, End);
        }

        public override string ToString()
        {
            return "VirtualDataWindowKeyRange{" +
                   "start=" +
                   Start +
                   ", end=" +
                   End +
                   '}';
        }
    }
} // end of namespace