using System;
using System.Collections.Generic;
using System.Reflection;

using Wonka.MetaData;

namespace Wonka.Eth.Contracts
{
    public interface ICommand
    {
        PropertyInfo[] GetProperties();

        Dictionary<PropertyInfo, WonkaRefAttr> GetPropertyMap();
    }
}

