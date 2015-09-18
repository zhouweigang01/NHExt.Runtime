using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.BEBuild;
using Net.Code.Builder.Build.BPBuild;
using System.IO;

namespace Net.Code.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("请录入元数据路径");
                return;
            }
            try
            {
                Net.Code.Builder.Util.OutPut.OutPutMsgHandler = DisplayMsg;
                Net.Code.Builder.Util.OutPut.OutPutMsg("开始读入文件，文件地址:" + args[0]);
                IProject project = Net.Code.Builder.Build.Model.ProjectFactory.BuildProj(args[0]);
                Net.Code.Builder.Util.OutPut.OutPutMsg("开始生成代码，项目名称" + project.ProjName);
                GenerateProjCode(project);
                Net.Code.Builder.Util.OutPut.OutPutMsg("开始项目代码成功，项目名称" + project.ProjName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        private static void DisplayMsg(string msg)
        {
            Console.WriteLine(msg);
        }
        private static void GenerateProjCode(IProject proj)
        {
            proj.Load();

            if (proj is BEProj)
            {
                BuildEntityProj bp = new Net.Code.Builder.Build.BEBuild.BuildEntityProj(proj as BEProj);
                bp.BuildCode();
                bp.BuildMSSQL();
                bp.BuildMetaData();
            }
            else
            {
                BuildBPProj bp = new BuildBPProj(proj as BPProj);
                bp.BuildCode();
                bp.BuildMetaData();
            }
            //   compilerCode(proj);
        }

        /// <summary>
        /// 自动编译代码
        /// </summary>
        /// <param name="proj"></param>
        private static void compilerCode(IProject proj)
        {
            string baseDir = "C:\\Windows\\Microsoft.NET\\Framework\\";
            string[] dirArray = Directory.GetDirectories(baseDir, "v4.0.*");
            if (dirArray == null || dirArray.Length == 0)
            {
                Net.Code.Builder.Util.OutPut.OutPutMsg("没有找到v4.0的msbuild,编译代码失败");
                return;
            }
            Net.Code.Builder.Util.OutPut.OutPutMsg("####################开始编译工程" + proj.ProjName + ".sln");
            string msbuildDir = dirArray[0] + "\\MSBuild.exe";
            string command = msbuildDir + " " + proj.CodePath + proj.Namespace + "\\" + proj.Namespace + ".sln";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";//要执行的程序名称 
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;//可能接受来自调用程序的输入信息 
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息 
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口 
            p.Start();//启动程序 
            p.StandardInput.WriteLine(command); //10秒后重启（C#中可不好做哦） 
            p.StandardInput.WriteLine("exit");        //不过要记得加上Exit
            //获取CMD窗口的输出信息： 
            string msg = p.StandardOutput.ReadToEnd();
            Net.Code.Builder.Util.OutPut.OutPutMsg(msg);
            Net.Code.Builder.Util.OutPut.OutPutMsg("####################编译工程完成" + proj.ProjName + ".sln");

        }
    }
}
