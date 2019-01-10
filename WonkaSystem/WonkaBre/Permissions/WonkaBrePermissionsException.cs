using System;

namespace WonkaBre.Permissions
{
    /// <summary>
    /// 
    /// This exception should be used when encountering a permissions issue with using
    /// the Business Rules Engine.
    /// 
    /// </summary>
    class WonkaBrePermissionsException : Exception
    {
        public WonkaBrePermissionsException(string psErrorMessage) : base(psErrorMessage)
        {
        }
    }
}