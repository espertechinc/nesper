///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Base class for named engine objects such as views, patterns guards and observers.
    /// </summary>
    [Serializable]
    public abstract class EPBaseNamedObject
    {
        private String _namespace;
        private String _name;
        private IList<Expression> _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="EPBaseNamedObject"/> class.
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
        protected EPBaseNamedObject(String @namespace, String name, IList<Expression> parameters)
        {
            _namespace = @namespace;
            _name = name;
            _parameters = parameters;
        }

        /// <summary>Gets or sets the object namespace name.</summary>
        public String Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        /// <summary>Gets or sets the object name.</summary>
        /// <returns>object name</returns>
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>Gets or sets the object parameters.</summary>
        /// <returns>parameters for object, empty list for no parameters</returns>
        public IList<Expression> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        /// <summary>
        /// Writes the object in EPL-syntax in the format "namespace:name(parameter, parameter, ..., parameter)"
        /// </summary>
        /// <param name="writer">to output to</param>
        public virtual void ToEPL(TextWriter writer)
        {
            writer.Write(_namespace);
            writer.Write(':');
            writer.Write(_name);

            writer.Write('(');
            ExpressionBase.ToPrecedenceFreeEPL(Parameters, writer);
            writer.Write(')');
        }
    }
} // End of namespace
