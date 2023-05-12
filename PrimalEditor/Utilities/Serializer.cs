using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// Used to serialize a given class to and from a file
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Used to serialize a given instance of a class
        /// </summary>
        /// <typeparam name="T">The datatype of the class</typeparam>
        /// <param name="instance">The instance of that particular class</param>
        /// <param name="path">The complete path to the destination file</param>
        public static void ToFile<T>(T instance, string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Create);
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(fs, instance);
                fs.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Logger.Log(MessageType.Error, $"Failed to serialize {instance} to {path}");
                throw;
            }
        }

        internal static T? FromFile<T>(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open);
                var serializer = new DataContractSerializer(typeof(T));
                T? instance = (T?)serializer.ReadObject(fs);
                fs.Close();
                return instance;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Logger.Log(MessageType.Error, $"Failed to deserialize {path}");
                throw;
            }
        }
    }
}
