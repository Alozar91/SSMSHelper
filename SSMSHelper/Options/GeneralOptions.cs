using System.ComponentModel;
using System.IO;

namespace SSMSHelper.Options
{
    public class GeneralOptions : OptionPageBase
    {

        const string collectionName = "SSMSHelperVSIX";

        private bool _enabled = true;
        private QueryTemplate[] _serverTemplates = new QueryTemplate[0];
        private QueryTemplate[] _databaseTemplates = new QueryTemplate[0];
        private QueryTemplate[] _tableTemplates = new QueryTemplate[0];
        private QueryTemplate[] _columnTemplates = new QueryTemplate[0];
        

        [Category("Основное")]
        [DisplayName("Включено")]
        [Description("Включить/выключить функционал SSMSHelper")]
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        [Category("Шаблоны")]
        [DisplayName("Серверы")]
        [Description("Шаблоны запросов, используемые для серверов")]
        public QueryTemplate[] ServerTemplates
        {
            get { return _serverTemplates; }
            set { _serverTemplates = value; }
        }

        [Category("Шаблоны")]
        [DisplayName("Базы данных")]
        [Description("Шаблоны запросов, используемые для баз данных")]
        public QueryTemplate[] DatabaseTemplates
        {
            get { return _databaseTemplates; }
            set { _databaseTemplates = value; }
        }

        [Category("Шаблоны")]
        [DisplayName("Таблицы")]
        [Description("Шаблоны запросов, используемые для таблицы")]
        public QueryTemplate[] TableTemplates
        {
            get { return _tableTemplates; }
            set { _tableTemplates = value; }
        }

        [Category("Шаблоны")]
        [DisplayName("Столбцы")]
        [Description("Шаблоны запросов, используемые для столбцов")]
        public QueryTemplate[] ColumnTemplates
        {
            get { return _columnTemplates; }
            set { _columnTemplates = value; }
        }

        public override void SaveSettingsToStorage()
        {
            MemoryStream mstream;
            base.SaveSettingsToStorage();

            var userSettingsStore = GetWritableSettingsStore(collectionName, true);
            userSettingsStore.SetBoolean(collectionName, nameof(Enabled), Enabled);
            if (ServerTemplates != null && ServerTemplates.Length > 0) {
                mstream = SerializeToStream(ServerTemplates);
                userSettingsStore.SetMemoryStream(collectionName, nameof(ServerTemplates), mstream);
            }
            if (DatabaseTemplates != null && DatabaseTemplates.Length > 0) {
                mstream = SerializeToStream(DatabaseTemplates);
                userSettingsStore.SetMemoryStream(collectionName, nameof(DatabaseTemplates), mstream);
            }
            if (TableTemplates != null && TableTemplates.Length > 0) {
                mstream = SerializeToStream(TableTemplates);
                userSettingsStore.SetMemoryStream(collectionName, nameof(TableTemplates), mstream);
            }
            if (ColumnTemplates != null && ColumnTemplates.Length > 0) {
                mstream = SerializeToStream(ColumnTemplates);
                userSettingsStore.SetMemoryStream(collectionName, nameof(ColumnTemplates), mstream);
            }
        }

        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();

            var userSettingsStore = GetWritableSettingsStore(collectionName);
            if (userSettingsStore.PropertyExists(collectionName, nameof(Enabled))) {
                Enabled = userSettingsStore.GetBoolean(collectionName, nameof(Enabled));
            }
            ServerTemplates = (QueryTemplate[])GetPropertyIfExists(userSettingsStore, collectionName, nameof(ServerTemplates));
            DatabaseTemplates = (QueryTemplate[])GetPropertyIfExists(userSettingsStore, collectionName, nameof(DatabaseTemplates));
            TableTemplates = (QueryTemplate[])GetPropertyIfExists(userSettingsStore, collectionName, nameof(TableTemplates));
            ColumnTemplates = (QueryTemplate[])GetPropertyIfExists(userSettingsStore, collectionName, nameof(ColumnTemplates));

        }
        
    }
}
