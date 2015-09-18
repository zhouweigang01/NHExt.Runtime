using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Net.Code.Builder.Enums;

namespace Net.Code.Generator.Win
{
    class ComponentTreeNode : TreeNode
    {
        private NodeType _nodeType;
        internal NodeType NodeType
        {
            get { return _nodeType; }
            set { _nodeType = value; }
        }

        private string _guid;
        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
        #region 引用的节点需要字段
        private RefType _refType;
        public RefType RefType
        {
            get { return _refType; }
            set { _refType = value; }
        }
        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
        #endregion
        public ComponentTreeNode(NodeType nodeType, string guid)
            : base()
        {
            this._guid = guid;
            this._nodeType = nodeType;
        }
    }

    class MetaDataTreeNode : TreeNode
    {

        public RefType RefType { get; private set; }

        public string FileName { get; private set; }

        public string FilePath { get; private set; }

        public MetaDataTreeNode(RefType refType, string filePath)
            : base()
        {
            this.RefType = refType;
            this.FilePath = filePath;
            this.FileName = System.IO.Path.GetFileName(filePath);

        }
    }
}
