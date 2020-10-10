using System;
using VA.Framework.Common.Environment.Definition;

namespace VA.Framework.Common.Environment.Implementation
{
    public class EnvironmentSettings : IEnvironmentSettings
    {
        public string GetSetting(string key) =>
            System.Environment.GetEnvironmentVariable(key) ?? throw new ArgumentException($"{key} was not found as an environment variable");

        public string GetSetting(string key, string defaultValue) => System.Environment.GetEnvironmentVariable(key) ?? defaultValue;
    }
}
