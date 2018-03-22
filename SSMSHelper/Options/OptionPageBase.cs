using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using SSMSHelper.Helpers;

namespace SSMSHelper.Options
{
    public class OptionPageBase : DialogPage
    {
        public static MemoryStream SerializeToStream(object o)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new PreMergeToMergedDeserializationBinder();
            formatter.Serialize(stream, o);
            return stream;
        }

        public static object DeserializeFromStream(MemoryStream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new PreMergeToMergedDeserializationBinder();
            object o = formatter.Deserialize(stream);

            //IFormatter formatter = new BinaryFormatter();
            //stream.Seek(0, SeekOrigin.Begin);
            //object o = formatter.Deserialize(stream);
            return o;
        }


        protected static WritableSettingsStore GetWritableSettingsStore(string collectionName)
        {
            return GetWritableSettingsStore(collectionName,false);
        }
        protected static WritableSettingsStore GetWritableSettingsStore(string collectionName, bool CreateIfNotExists)
        {
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            if (settingsManager == null)
            {
                return null;
            }
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            if (!userSettingsStore.CollectionExists(collectionName) && CreateIfNotExists)
                userSettingsStore.CreateCollection(collectionName);

            return userSettingsStore;
        }

        protected static object GetPropertyIfExists(WritableSettingsStore settingsStore, string collectionName, string propertyName)
        {
            Object obj =null;
            if (settingsStore == null || !(collectionName.Length > 0) || !(propertyName.Length > 0)) {
                return null;
            }

            if (settingsStore.PropertyExists(collectionName, propertyName)) {
                MemoryStream mstream = settingsStore.GetMemoryStream(collectionName, propertyName);
                obj = DeserializeFromStream(mstream);
            }
            return obj;
        }

        protected static void SavePropertyIfExists(WritableSettingsStore settingsStore, string collectionName, string propertyName, Object obj)
        {
            
            if (settingsStore == null || !(collectionName.Length > 0) || !(propertyName.Length > 0) || obj==null) {
                throw new ArgumentNullException();
            }
            var mstream = SerializeToStream(obj);
            settingsStore.SetMemoryStream(collectionName, nameof(propertyName), mstream);

        }
        
    }
}
