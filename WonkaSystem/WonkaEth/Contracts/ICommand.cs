using System;
using System.Collections.Generic;
using System.Reflection;

using WonkaRef;

namespace WonkaEth.Contracts
{
    public interface ICommand
    {
        PropertyInfo[] GetProperties();

        Dictionary<PropertyInfo, WonkaRefAttr> GetPropertyMap();
    }
}

