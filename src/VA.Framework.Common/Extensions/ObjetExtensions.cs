namespace VA.Framework.Common.Extensions
{
    public static class ObjetExtensions
    {
        public static string ToJson(this object value) => Newtonsoft.Json.JsonConvert.SerializeObject(value);



    }
}
