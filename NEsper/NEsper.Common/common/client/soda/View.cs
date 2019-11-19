///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// A view provides a projection upon a stream, such as a data window, grouping or unique.
    /// For views, the namespace is an optional value and can be null for any-namespace.
    /// </summary>
    [Serializable]
    public class View : EPBaseNamedObject
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public View()
        {
        }

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="namespace">is thie view namespace, i.e. "win" for data windows</param>
        /// <param name="name">is the view name, i.e. "length" for length window</param>
        /// <returns>view</returns>
        public static View Create(
            string @namespace,
            string name)
        {
            return new View(@namespace, name, new List<Expression>());
        }

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="name">is the view name, i.e. "length" for length window</param>
        /// <returns>view</returns>
        public static View Create(string name)
        {
            return new View(null, name, new List<Expression>());
        }

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="namespace">is thie view namespace, i.e. "win" for data windows</param>
        /// <param name="name">is the view name, i.e. "length" for length window</param>
        /// <param name="parameters">is a list of view parameters, or empty if there are no parameters for the view</param>
        /// <returns>view</returns>
        public static View Create(
            string @namespace,
            string name,
            IList<Expression> parameters)
        {
            return new View(@namespace, name, parameters);
        }

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="name">is the view name, i.e. "length" for length window</param>
        /// <param name="parameters">is a list of view parameters, or empty if there are no parameters for the view</param>
        /// <returns>view</returns>
        public static View Create(
            string name,
            IList<Expression> parameters)
        {
            return new View(null, name, parameters);
        }

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="namespace">is thie view namespace, i.e. "win" for data windows</param>
        /// <param name="name">is the view name, i.e. "length" for length window</param>
        /// <param name="parameters">is a list of view parameters, or empty if there are no parameters for the view</param>
        /// <returns>view</returns>
        public static View Create(
            string @namespace,
            string name,
            params Expression[] parameters)
        {
            if (parameters != null)
            {
                return new View(@namespace, name, parameters);
            }
            else
            {
                return new View(@namespace, name, new List<Expression>());
            }
        }

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="name">is the view name, i.e. "length" for length window</param>
        /// <param name="parameters">is a list of view parameters, or empty if there are no parameters for the view</param>
        /// <returns>view</returns>
        public static View Create(
            string name,
            params Expression[] parameters)
        {
            if (parameters != null)
            {
                return new View(null, name, parameters);
            }
            else
            {
                return new View(null, name, new List<Expression>());
            }
        }

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="namespace">is thie view namespace, i.e. "win" for data windows</param>
        /// <param name="name">is the view name, i.e. "length" for length window</param>
        /// <param name="parameters">is a list of view parameters, or empty if there are no parameters for the view</param>
        public View(
            string @namespace,
            string name,
            IList<Expression> parameters)
            : base(@namespace, name, parameters)
        {
        }

        /// <summary>
        /// Render view.
        /// </summary>
        /// <param name="writer">to render to</param>
        public void ToEPLWithHash(TextWriter writer)
        {
            writer.Write(Name);
            if (!Parameters.IsEmpty())
            {
                writer.Write('(');
                ExpressionBase.ToPrecedenceFreeEPL(Parameters, writer);
                writer.Write(')');
            }
        }
    }
} // end of namespace