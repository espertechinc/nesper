///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;


namespace com.espertech.esper.collection
{
    /// <summary>
    /// Work queue wherein items can be added to the front and to the back, wherein both front
    /// and back have a given order, with the idea that all items of the front queue get processed 
    /// before any given single item of the back queue gets processed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DualWorkQueue<T> where T : class
    {
        private readonly LinkedList<T> _frontQueue;
        private readonly LinkedList<T> _backQueue;

        /// <summary>
        /// Ctor.
        /// </summary>
        public DualWorkQueue()
        {
            _frontQueue = new LinkedList<T>();
            _backQueue = new LinkedList<T>();
        }

        /// <summary>
        /// Items to be processed first, in the order to be processed.
        /// </summary>
        /// <value>front queue</value>
        public LinkedList<T> FrontQueue
        {
            get { return _frontQueue; }
        }

        /// <summary>
        /// Items to be processed after front-queue is empty, in the order to be processed.
        /// </summary>
        /// <value>back queue</value>
        public LinkedList<T> BackQueue
        {
            get { return _backQueue; }
        }
    }
}
