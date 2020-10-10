namespace VA.Framework.Common.Environment.Definition
{
    public interface IEnvironmentSettings
    {
        string GetSetting(string key);
        string GetSetting(string key, string defaultValue);
    }
}
