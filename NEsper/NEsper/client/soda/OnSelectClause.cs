///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// A clause to delete from a named window based on a triggering event arriving and
    /// correlated to the named window events to be deleted.
    /// </summary>
    public class OnSelectClause : OnClause
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnSelectClause"/> class.
        /// </summary>
        public OnSelectClause()
        {
        }

        /// <summary>
        /// Creates an on-select clause.
        /// </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="optionalAsName">is the optional name</param>
        /// <returns>
        /// on-select clause
        /// </returns>
        public static OnSelectClause Create(String windowName, String optionalAsName)
        {
            return new OnSelectClause(windowName, optionalAsName);
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="windowName">is the named window name</param>
        /// <param name="optionalAsName">is the name of the named window</param>
        public OnSelectClause(String windowName, String optionalAsName)
        {
            WindowName = windowName;
            OptionalAsName = optionalAsName;
        }
    
        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write(WindowName);
            if (OptionalAsName != null)
            {
                writer.Write(" as ");
                writer.Write(OptionalAsName);
            }
        }

        /// <summary>
        /// Returns the name of the named window to delete from.
        /// </summary>
        /// <returns>
        /// named window name
        /// </returns>
        public string WindowName { get; set; }

        /// <summary>
        /// Returns the as-provided name for the named window.
        /// </summary>
        /// <returns>
        /// name
        /// </returns>
        public string OptionalAsName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether select-and-delete or just select.
        /// </summary>
        /// <value><c>true</c> if [select and delete]; otherwise, <c>false</c>.</value>
        public bool IsDeleteAndSelect { get; set; }
    }
}
