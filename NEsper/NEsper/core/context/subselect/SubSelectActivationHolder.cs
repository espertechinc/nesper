///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.epl.spec;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.subselect
{
    /// <summary>
    /// Record holding lookup resource references for use by <seealso cref="SubSelectActivationCollection" />. 
    /// </summary>
    public class SubSelectActivationHolder
    {
        public SubSelectActivationHolder(int streamNumber, EventType viewableType, ViewFactoryChain viewFactoryChain, ViewableActivator activator, StreamSpecCompiled streamSpecCompiled)
        {
            StreamNumber = streamNumber;
            ViewableType = viewableType;
            ViewFactoryChain = viewFactoryChain;
            Activator = activator;
            StreamSpecCompiled = streamSpecCompiled;
        }

        /// <summary>Returns lookup stream number. </summary>
        /// <value>stream num</value>
        public int StreamNumber { get; private set; }

        public EventType ViewableType { get; private set; }

        /// <summary>Returns the lookup view factory chain </summary>
        /// <value>view factory chain</value>
        public ViewFactoryChain ViewFactoryChain { get; private set; }

        public ViewableActivator Activator { get; private set; }

        public StreamSpecCompiled StreamSpecCompiled { get; private set; }
    }
}
