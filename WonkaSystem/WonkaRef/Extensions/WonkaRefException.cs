using System;

namespace WonkaRef.Extensions
{
    /// <summary>
    /// 
    /// This exception should be used when encountering any issue with using
    /// extensions regarding the WonkaRef library.
    /// 
    /// </summary>
    public class WonkaRefException : Exception
    {
        public WonkaRefException(string psErrorMessage) : base(psErrorMessage)
        { }
    }
}
