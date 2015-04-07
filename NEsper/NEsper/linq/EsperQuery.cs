///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.soda;

namespace com.espertech.esper.linq
{
    /// <summary>
    /// Each variation of the EsperQuery must result in a statement that can be submitted
    /// to the administrator for compilation and execution.
    /// </summary>

    public class EsperQuery<T>
    {
        /// <summary>
        /// Gets the service provider associated with this request.
        /// </summary>
        /// <value>The service provider.</value>
        public EPServiceProvider ServiceProvider { get; private set; }
        /// <summary>
        /// Gets the statement associated with this request.
        /// </summary>
        /// <value>The statement.</value>
        public EPStatement Statement { get; private set; }
        /// <summary>
        /// Gets the statement object model.
        /// </summary>
        /// <value>The object model.</value>
        public EPStatementObjectModel ObjectModel { get; private set;}

        /// <summary>
        /// Returns true if the query has been compiled into a statement.
        /// </summary>
        public bool IsCompiled
        {
            get { return Statement != null; }
        }

        /// <summary>
        /// Compiles the statement object model into a true statement.
        /// </summary>
        public void Compile()
        {
            if (Statement == null) {
                Statement = ServiceProvider.EPAdministrator.Create(ObjectModel);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EsperQuery&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="objectModel">The object model.</param>
        public EsperQuery(EPServiceProvider serviceProvider, EPStatementObjectModel objectModel)
        {
            ServiceProvider = serviceProvider;
            ObjectModel = objectModel;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("ServiceProvider: {0}, Statement: {1}, ObjectModel: {2}", ServiceProvider, Statement, ObjectModel.ToEPL());
        }
    }
}
