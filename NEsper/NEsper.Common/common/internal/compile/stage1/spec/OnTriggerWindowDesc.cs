///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification for the on-select, on-delete and on-Update (via subclass) (no split-stream) statement.
    /// </summary>
    public class OnTriggerWindowDesc : OnTriggerDesc
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="windowName">the window name</param>
        /// <param name="optionalAsName">the optional name</param>
        /// <param name="onTriggerType">for indicationg on-delete, on-select or on-Update</param>
        /// <param name="deleteAndSelect">if set to <c>true</c> [delete and select].</param>
        public OnTriggerWindowDesc(
            string windowName,
            string optionalAsName,
            OnTriggerType onTriggerType,
            bool deleteAndSelect)
            : base(onTriggerType)
        {
            WindowName = windowName;
            OptionalAsName = optionalAsName;
            IsDeleteAndSelect = deleteAndSelect;
        }

        /// <summary>
        /// Returns the window name.
        /// </summary>
        /// <value>The name of the window.</value>
        public string WindowName { get; private set; }

        /// <summary>
        /// Returns the name, or null if none defined.
        /// </summary>
        /// <value>The name of the optional as.</value>
        public string OptionalAsName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [delete and select].
        /// </summary>
        /// <value><c>true</c> if [delete and select]; otherwise, <c>false</c>.</value>
        public bool IsDeleteAndSelect { get; private set; }
    }
} // End of namespace