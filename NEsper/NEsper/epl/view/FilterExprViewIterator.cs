///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.view
{
    /// <summary>Iterator for reading and filtering a source event iterator.</summary>
    public class FilterExprViewIterator : IEnumerator<EventBean> {
        private readonly IEnumerator<EventBean> sourceIterator;
        private readonly ExprEvaluator filter;
        private readonly ExprEvaluatorContext exprEvaluatorContext;
        private readonly EventBean[] evalEventArr;
    
        private EventBean nextResult;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="sourceIterator">is the iterator supplying events to filter out.</param>
        /// <param name="filter">is the filter expression</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        public FilterExprViewIterator(Iterator<EventBean> sourceIterator, ExprEvaluator filter, ExprEvaluatorContext exprEvaluatorContext) {
            this.sourceIterator = sourceIterator;
            this.filter = filter;
            this.exprEvaluatorContext = exprEvaluatorContext;
            evalEventArr = new EventBean[1];
        }
    
        public bool HasNext() {
            if (nextResult != null) {
                return true;
            }
            FindNext();
            if (nextResult != null) {
                return true;
            }
            return false;
        }
    
        public EventBean Next() {
            if (nextResult != null) {
                EventBean result = nextResult;
                nextResult = null;
                return result;
            }
            FindNext();
            if (nextResult != null) {
                EventBean result = nextResult;
                nextResult = null;
                return result;
            }
            throw new NoSuchElementException();
        }
    
        private void FindNext() {
            while (sourceIterator.HasNext()) {
                EventBean candidate = sourceIterator.Next();
                evalEventArr[0] = candidate;
    
                bool? pass = (bool?) filter.Evaluate(evalEventArr, true, exprEvaluatorContext);
                if ((pass != null) && pass) {
                    nextResult = candidate;
                    break;
                }
            }
        }
    
        public void Remove() {
            throw new UnsupportedOperationException();
        }
    }
} // end of namespace
