using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SSMSHelper.Options
{
    [Serializable]
    public class QueryTemplate
    {
        private string _name;
        private string[] _template;
        private string _objects = "*";
        private bool _autoexec = false;
               
        [Category("Шаблон")] 
        [DisplayName("Имя шаблона")]
        [Description("Отображаемое имя шаблона в контекстном меню элемента")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [Category("Шаблон")]
        [DisplayName("Шаблон запроса")]
        [Description("Текст шаблона, который будет использоваться для формирования текста запроса")]
        public string[] Template
        {
            get { return _template; }
            set { _template = value; }
        }

        [Category("Шаблон")]
        [DisplayName("Объекты")]
        [Description("Список объектов, для которых применяется шаблон. Указывается через запятую или *, если применяется для всех.")]
        public string Objects
        {
            get { return _objects; }
            set { _objects = value; }
        }
        [Category("Выполнение")]
        [DisplayName("Автовыполнение")]
        [Description("Автоматически выполнять запрос после формирования")]
        public bool Autoexec
        {
            get { return _autoexec; }
            set { _autoexec = value; }
        }

    }
}
