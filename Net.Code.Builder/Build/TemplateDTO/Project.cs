using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.TemplateDTO
{
    [Serializable]
    public class Project
    {
        public Project()
        {
            this.RefrenceList = new List<string>();
            this.CommonRefList = new List<string>();
            this.FileList = new List<string>();
            this.CfgFileList = new List<string>();
        }
        public Project(Model.BEProj proj, ProjType type)
            : this()
        {
            this.Guid = proj.Guid;
            this.Namespace = proj.Namespace;
            this.Type = type;
            this.CommonRefList = new List<string>();
            foreach (Model.BEEntity entity in proj.EntityList)
            {
                if (this.Type == ProjType.BE)
                {
                    this.FileList.Add(entity.Code);
                    this.CfgFileList.Add(entity.Code);
                }
                else
                {
                    this.FileList.Add(entity.Code + Net.Code.Builder.Build.Model.DTOEntity.AttrEndTag);

                }
            }
            foreach (Model.EnumEntity enumEntity in proj.EnumList)
            {
                if (this.Type != ProjType.Deploy)
                {
                    this.FileList.Add(enumEntity.Code);
                }
                else
                {
                    this.FileList.Add(enumEntity.Code + Net.Code.Builder.Build.Model.DTOEntity.AttrEndTag);
                }
            }
            if (this.Type == ProjType.Deploy)
            {
                foreach (Model.DTOEntity entity in proj.DTOList)
                {
                    this.FileList.Add(entity.Code);
                }
            }

            this.CommonRefList.Add("NHExt.Runtime");
            if (this.Type == ProjType.BE)
            {
                this.CommonRefList.Add("log4net");
                this.CommonRefList.Add("NHibernate");
            }

            foreach (Net.Code.Builder.Base.AbstractPlatformComponent refrence in proj.RefrenceList)
            {
                Model.ProjectRefrence pr = refrence as Model.ProjectRefrence;
                if (this.Type == ProjType.Deploy)
                {
                    if (pr.RefrenceType == Net.Code.Builder.Enums.RefType.BEEntity) continue;

                    if (pr.RefProjName == proj.FileName)
                    {
                        continue;
                    }
                }
                else if (this.Type == ProjType.BE)
                {
                    if (pr.RefProjName == proj.FileName)
                    {
                        if (pr.RefrenceType == Net.Code.Builder.Enums.RefType.BEEntity) continue;
                    }
                }
                this.RefrenceList.Add((refrence as Model.ProjectRefrence).AssemblyName);
            }

            if (this.Type == ProjType.Deploy)
            {
                this.DllName = this.Namespace + "." + Net.Code.Builder.Build.Model.DTOEntity.AssemblyEndTag;
            }
            else
            {
                this.DllName = this.Namespace;
            }

        }
        public Project(Model.BPProj proj, ProjType type)
            : this()
        {
            this.Guid = proj.Guid;
            this.Namespace = proj.Namespace;
            this.CommonRefList = new List<string>();
            this.Type = type;
            if (this.Type == ProjType.Agent)
            {
                this.DllName = this.Namespace + ".Agent";

            }
            if (this.Type == ProjType.Deploy)
            {
                this.DllName = this.Namespace + ".Deploy";
            }
            foreach (Model.BPEntity entity in proj.EntityList)
            {
                if (this.Type != ProjType.Deploy)
                {
                    if (this.Type == ProjType.Agent)
                    {
                        this.FileList.Add(entity.Code + "Agent");
                    }
                    else
                    {
                        this.FileList.Add(entity.Code);
                    }
                }
            }
            foreach (Model.EnumEntity enumEntity in proj.EnumList)
            {
                if (this.Type == ProjType.Deploy)
                {
                    this.FileList.Add(enumEntity.Code + Net.Code.Builder.Build.Model.DTOEntity.AttrEndTag);
                }
            }
            if (this.Type == ProjType.Deploy)
            {
                foreach (Model.DTOEntity entity in proj.DTOList)
                {
                    this.FileList.Add(entity.Code);
                }
            }
            this.CommonRefList.Add("NHExt.Runtime");
            this.CommonRefList.Add("log4net");
            if (this.Type == ProjType.BP)
            {
                this.CommonRefList.Add("NHibernate");
            }


            foreach (Net.Code.Builder.Base.AbstractPlatformComponent refrence in proj.RefrenceList)
            {
                Model.ProjectRefrence pr = refrence as Model.ProjectRefrence;
                if (this.Type == ProjType.Deploy)
                {
                    if (pr.RefrenceType == Net.Code.Builder.Enums.RefType.BEEntity) continue;
                    if (pr.RefrenceType == Enums.RefType.Agent) continue;
                    if (pr.RefrenceType == Enums.RefType.BPEntity) continue;
                    if (pr.RefProjName == proj.FileName)
                    {
                        if (pr.RefrenceType == Net.Code.Builder.Enums.RefType.Deploy) continue;
                    }
                }
                else if (this.Type == ProjType.Agent)
                {
                    if (pr.RefrenceType != Net.Code.Builder.Enums.RefType.Deploy) continue;
                }
                this.RefrenceList.Add((refrence as Model.ProjectRefrence).AssemblyName);
            }


            if (this.Type == ProjType.Deploy)
            {
                this.DllName = this.Namespace + "." + Net.Code.Builder.Build.Model.DTOEntity.AssemblyEndTag;
            }
            else if (this.Type == ProjType.Agent)
            {
                this.DllName = this.Namespace + "." + Net.Code.Builder.Build.Model.BPEntity.AssemblyEndTag;
            }
            else
            {
                this.DllName = this.Namespace;
            }

        }
        public ProjType Type { get; set; }
        public string DllName { get; set; }
        public string Guid { get; set; }
        public string Namespace { get; set; }
        public List<string> RefrenceList { get; set; }
        public List<string> FileList { get; set; }
        public List<string> CommonRefList { get; set; }
        public List<string> CfgFileList { get; set; }
 
    }
    [Serializable]
    public enum ProjType
    {
        BE = 1,
        BP = 2,
        Deploy = 3,
        Agent = 4
    }
}
