///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.supportunit.bean.word
{
    public class SentenceEvent
    {
        private readonly string sentence;

        public SentenceEvent(string sentence)
        {
            this.sentence = sentence;
        }

        public WordEvent[] GetWords()
        {
            var split = sentence.Split(' ');
            var words = new WordEvent[split.Length];
            for (var i = 0; i < split.Length; i++)
            {
                words[i] = new WordEvent(split[i]);
            }

            return words;
        }
    }
} // end of namespace
