///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


/*
 * Created on Apr 22, 2006
 *
 */

using System;

namespace NEsper.Examples.Transaction.sim
{
    /** Just so we can swap between Random and SecureRandom.
     * 
     * @author Hans Gilde
     *
     */
    public class RandomUtil
    {
        public static Random GetNewInstance()
        {
            return new Random();
        }
    }
}
