using System;
using System.Collections.Generic;
using System.Text;

namespace Wonka.Import
{
    public class WonkaImportException : Exception
    {
        public WonkaImportException(string psErrorMessage) : base(psErrorMessage)
        {
            // NOTE: We might add more later
        }
    }
}


