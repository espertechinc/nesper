using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;

namespace com.espertech.esperio.csv
{
	/// <summary>
	/// A wrapper for a Stream or a TextReader.
	/// </summary>

	public class CSVSource
	{
		private readonly AdapterInputSource _source; // source of data
		private TextReader _reader; // reader from where data comes
		private Stream _stream;     // stream from where data comes

	    private int _eMarkIndex; // end index
	    private int _wMarkIndex; // write index
	    private int _rMarkIndex; // read index
	    private int[] _markData; // backing store for reading data when marked

	    private IContainer _container;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="source">the AdapterInputSource from which to obtain the underlying resource</param>

        public CSVSource(IContainer container, AdapterInputSource source)
        {
            _container = container;

            _rMarkIndex = -1; // not reading
            _wMarkIndex = -1; // not writing
            _eMarkIndex = -1;
            _markData = new int[2];

			_stream = source.GetAsStream(_container) ;
			if ( _stream == null )
			{
				_reader = source.GetAsReader() ;
			}

			this._source = source;
		}
		
		/// <summary>Close the underlying resource.</summary>
		/// <throws>IOException to indicate an io error</throws>

		public void Close()
		{
			if (_stream != null)
			{
				_stream.Close();
			}
			else
			{
				_reader.Close();
			}
		}

        private int ReadFromBase()
        {
            if (_stream != null)
            {
                return _stream.ReadByte();
            }
            else
            {
                return _reader.Read();
            }
        }

        private void WriteToMark(int value)
        {
            // Do we need to store the value for a future read?
            if (_wMarkIndex >= 0)
            {
                if (_wMarkIndex < _markData.Length)
                {
                    _markData[_wMarkIndex] = value; // Write the value to buffer
                    _wMarkIndex++; // Increment the write mark
                }
            }
        }

        private void IncrementWriteMark()
        {
            // Do we need to store the value for a future read?
            if (_wMarkIndex >= 0)
            {
                if (_wMarkIndex < _markData.Length)
                {
                    _wMarkIndex++;
                }
            }
        }

	    /// <summary>Read from the underlying resource.</summary>
		/// <returns>the result of the read</returns>
		/// <throws>IOException for io errors</throws>

        public int Read()
		{
            int value;

            // Check to see if we are supposed to be reading from
            // somewhere within the markData.  If we are, make sure
            // that we return data from the markData buffer.
		    if (_rMarkIndex >= 0)
		    {
		        int index = _rMarkIndex++;
		        if (_eMarkIndex > index)
		        {
		            value = _markData[index];
		            IncrementWriteMark();
		        }
                else
		        {
		            value = ReadFromBase();

                    // Can this value be written to the pushback buffer or have
                    // we consumed more data that was specified for the lookahead?

                    if (_wMarkIndex < _markData.Length)
                    {
                        // More space is available in the pushback buffer
                        WriteToMark(value);
                    }
                    else
                    {
                        // Once we have to read past the end of the marked data, then
                        // the marked data is no longer retrievable because we can not
                        // recreate the character that has been consumed off the wire.

                        _eMarkIndex = -1;
                        _wMarkIndex = -1;
                        _rMarkIndex = -1;
                    }
		        }
            }
            // We are not reading from the mark buffer which means we
            // need to read one value.  If we are marking, then we need
            // to write the data into the markData buffer.
            else
            {
                value = ReadFromBase();
                WriteToMark(value);
            }
            
		    return value;
		}

	    /// <summary>Return true if the underlying resource is resettable.</summary>
		/// <returns>true if resettable, false otherwise</returns>

		public bool IsResettable
		{
			get { return _source.IsResettable; }
		}
		
		/// <summary>Reset to the last mark position.</summary>
		/// <throws>IOException for io errors</throws>

		public void ResetToMark()
		{
		    _rMarkIndex = 0;
		    _eMarkIndex =
		        _eMarkIndex > _wMarkIndex
		            ? _eMarkIndex
		            : _wMarkIndex;

		    _wMarkIndex = 0;
		}

        /// <summary>Set the mark position.</summary>
		/// <param name="readAheadLimit">is the maximum number of read-ahead events</param>
		/// <throws>IOException when an io error occurs</throws>

		public void Mark(int readAheadLimit)
		{
            // Set a new mark ...
            // - Have we consumed data that is currently in the pushback buffer?

            switch( _rMarkIndex )
            {
                case -1:
                case 0:
                    _wMarkIndex = 0;
                    break;
                case 1:
                    _markData[0] = _markData[1];
                    _markData[1] = 0;
                    _rMarkIndex = 0;
                    _eMarkIndex--;
                    _wMarkIndex = 0;
                    break;
                case 2:
                    _rMarkIndex = -1;
                    _wMarkIndex = 0;
                    _eMarkIndex = 0;
                    break;
            }
		}
		
		/// <summary>Reset to the beginning of the resource.</summary>

		public void Reset()
		{
			if(!IsResettable)
			{
				throw new UnsupportedOperationException("Reset not supported: underlying source cannot be reset");
			}
			
			if(_stream != null)
			{
				_stream = _source.GetAsStream(_container);
			}
			else
			{
				_reader = _source.GetAsReader();
			}

		    _rMarkIndex = -1;
		    _wMarkIndex = -1;
		    _eMarkIndex = -1;
		    _markData[0] = 0;
		    _markData[1] = 0;
		}
	}
}
