using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.EntityAttribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnDescriptionAttribute : Attribute
    {
        private string _code = string.Empty;

        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }

        private string _description;
        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        private bool _entityRefrence = false;
        /// <summary>
        /// 是否启用标识
        /// </summary>
        public bool EntityRefrence
        {
            get { return _entityRefrence; }
            set { _entityRefrence = value; }
        }

        private bool _isViewer = true;
        /// <summary>
        /// 是否可浏览
        /// </summary>
        public bool IsViewer
        {
            get
            {
                return _isViewer;
            }
            set
            {
                _isViewer = value;
            }
        }

        private bool _isBizKey = false;
        /// <summary>
        /// 是否业务主键
        /// </summary>
        public bool IsBizKey
        {
            get { return _isBizKey; }
            set { _isBizKey = value; }
        }

        private bool _isNull = false;
        /// <summary>
        /// 字段是否可空
        /// </summary>
        public bool IsNull
        {
            get
            {
                return this._isNull;
            }
            set
            {
                this._isNull = value;
            }
        }

    }
}
