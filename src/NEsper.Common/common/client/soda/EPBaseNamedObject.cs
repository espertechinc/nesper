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

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Base class for named runtime objects such as views, patterns guards and observers.
    /// </summary>
    [Serializable]
    public abstract class EPBaseNamedObject
    {
        private string @namespace;
        private string name;
        private IList<Expression> parameters;

        /// <summary>
        /// Ctor.
        /// </summary>
        protected EPBaseNamedObject()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="namespace">is the namespace of the object, i.e. view namespace or pattern object namespace</param>
        /// <param name="name">is the name of the object, such as the view name</param>
        /// <param name="parameters">is the optional parameters to the view or pattern object, or empty list for no parameters</param>
        protected EPBaseNamedObject(
            string @namespace,
            string name,
            IList<Expression> parameters)
        {
            this.@namespace = @namespace;
            this.name = name;
            this.parameters = parameters;
        }

        /// <summary>
        /// Returns the object namespace name.
        /// </summary>
        /// <returns>namespace name</returns>
        public string Namespace
        {
            get => @namespace;
            set => @namespace = value;
        }

        /// <summary>
        /// Returns the object name.
        /// </summary>
        /// <returns>object name</returns>
        public string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// Returns the object parameters.
        /// </summary>
        /// <returns>parameters for object, empty list for no parameters</returns>
        public IList<Expression> Parameters
        {
            get => parameters;
            set => parameters = value;
        }

        /// <summary>
        /// Writes the object in EPL-syntax in the format "namespace:name(parameter, parameter, ..., parameter)"
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write(@namespace);
            writer.Write(':');
            writer.Write(name);
            writer.Write('(');
            ExpressionBase.ToPrecedenceFreeEPL(Parameters, writer);
            writer.Write(')');
        }
    }
} // end of namespace