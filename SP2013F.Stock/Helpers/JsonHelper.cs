using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SP2013F.Stock.Helpers
{
    public class JsonHelper
    {
        /// <summary>
        ///     JSON Serialization
        /// </summary>
        public static string Serialize<T>(T t)
        {
            try
            {
                var ser = new DataContractJsonSerializer(typeof(T));
                using (var ms = new MemoryStream())
                {
                    ser.WriteObject(ms, t);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to serialize object to JSON", ex);
            }
        }

        /// <summary>
        ///     JSON Deserialization
        /// </summary>
        public static T Deserialize<T>(string jsonString)
        {
            try
            {
                var ser = new DataContractJsonSerializer(typeof(T));
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
                {
                    return (T)ser.ReadObject(ms);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to deserialize object from JSON", ex);
            }
        }
    }
}
