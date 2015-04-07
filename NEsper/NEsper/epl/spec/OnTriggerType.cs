///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.spec
{
    /// <summary>Enum for the type of on-trigger statement.</summary>
	public enum OnTriggerType
	{
	    /// <summary>
	    /// For on-delete triggers that delete from a named window when a triggering event arrives.
	    /// </summary>
	    ON_DELETE,

	    /// <summary>
	    /// For on-select triggers that selected from a named window when a triggering event arrives.
	    /// </summary>
	    ON_SELECT,

        /// <summary>
        /// For the on-insert split-stream syntax allowing multiple insert-into streams.
        /// </summary>
        ON_SPLITSTREAM,

	    /// <summary>
	    /// For on-set triggers that set variable values when a triggering event arrives.
	    /// </summary>
	    ON_SET,

        /// <summary>
        /// For on-Update triggers that Update an event in a named window when a
        /// triggering event arrives.
        /// </summary>
        ON_UPDATE,

        /// <summary>
        /// For on-merge triggers that insert/Update an event in a named window when a
        /// triggering event arrives.
        /// </summary>
        ON_MERGE
	}

    public static class OnTriggerTypeExtensions
    {
        public static string GetTextual(this OnTriggerType triggerType)
        {
            switch (triggerType)
            {
                case OnTriggerType.ON_DELETE:
                    return "on-delete";
                case OnTriggerType.ON_SELECT:
                    return "on-select";
                case OnTriggerType.ON_SPLITSTREAM:
                    return "on-insert-multiple";
                case OnTriggerType.ON_SET:
                    return "on-set";
                case OnTriggerType.ON_UPDATE:
                    return "on-Update";
                case OnTriggerType.ON_MERGE:
                    return "on-merge";
            }

            throw new ArgumentException();
        }
    }
} // End of namespace
