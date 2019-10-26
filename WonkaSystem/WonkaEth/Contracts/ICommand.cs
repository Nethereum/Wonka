using System;
using System.Collections.Generic;
using System.Reflection;

using WonkaRef;

namespace Wonka.Eth.Contracts
{
    public interface ICommand
    {
        PropertyInfo[] GetProperties();

        Dictionary<PropertyInfo, WonkaRefAttr> GetPropertyMap();
    }
}

