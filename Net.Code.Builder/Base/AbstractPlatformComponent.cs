using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Build.Model;
using System.Xml;

namespace Net.Code.Builder.Base
{
    public abstract class AbstractPlatformComponent : IPlatformComponent
    {
        /// <summary>
        /// 项目GUID
        /// </summary>
        protected string _guid = "";
        public string Guid
        {
            get { return _guid; }
        }
        /// <summary>
        /// 列编码
        /// </summary>
        private string _code;
        public string Code
        {
            get { return _code; }
            set
            {
                if (_code != value)
                {
                    _code = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 列名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    this.IsChanged = true;
                }
            }
        }

        private DataState _dataState = DataState.UnChanged;
        public DataState DataState
        {
            get { return _dataState; }
            set { _dataState = value; }
        }

        private bool _isChanged = false;
        public bool IsChanged
        {
            get { return _isChanged; }
            set { _isChanged = value; }
        }

        public AbstractPlatformComponent(string guid)
        {
            this._guid = guid;
        }

        public abstract void FromXML(XmlNode node);
        public abstract void ToXML(XmlDocument xmlDoc);

        public virtual bool Validate() {
            return true;
        }

    }
}
