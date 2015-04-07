///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Exception list populated in a deployment operation.
    /// </summary>
    [Serializable]
    public class DeploymentActionException : DeploymentException
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">deployment error message</param>
        /// <param name="exceptions">that occured deploying</param>
        public DeploymentActionException(String message, IList<DeploymentItemException> exceptions)
            : base(message)
        {
            Exceptions = exceptions;
        }

        /// <summary>Returns the exception list. </summary>
        /// <value>exceptions</value>
        public IList<DeploymentItemException> Exceptions { get; private set; }

        /// <summary>Returns a detail print of all exceptions and messages line-separated. </summary>
        /// <returns>exception list</returns>
        public String GetDetail()
        {
            var detail = new StringWriter();
            var count = 0;
            var delimiter = "";
            for (int ii = 0; ii < Exceptions.Count; ii++)
            {
                var item = Exceptions[ii];
                detail.Write(delimiter);
                detail.Write("Exception #");
                detail.Write(Convert.ToString(count));
                detail.Write(" : ");
                detail.Write(item.InnerException.Message);
                delimiter = Environment.NewLine + Environment.NewLine;
                count++;
            }
            return detail.ToString();
        }
    }
}
