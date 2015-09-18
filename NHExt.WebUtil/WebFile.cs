using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Web.UI.WebControls;

namespace NHExt.WebUtil
{
    /// <summary>
    ///FileManage 的摘要说明
    /// </summary>
    public class WebFile
    {
        public static string UploadFile(FileUpload Object, string PicSize, string DelFileName)
        {
            string strFileName = "";
            if (Object.HasFile)
            {
                string fileContentType = Object.PostedFile.ContentType;
                if (fileContentType == "image/bmp" || fileContentType == "image/gif" || fileContentType == "image/pjpeg")
                {
                    //客户端文件路径
                    string name = Object.PostedFile.FileName;
                    FileInfo file = new FileInfo(name);
                    //上传路径设置
                    string uploadPath = string.Format(@"{0}\{1}\{2}\", StrHelp.GetPhysicalPath(), ConfigurationManager.AppSettings["UploadPath"], PicSize);
                    //按年月生成文件夹
                    string folderName = StrHelp.MakeFolderName();
                    //生成新文件名
                    string fileName = StrHelp.MakeFileRndName() + file.Extension.ToLower();
                    //服务器端文件路径
                    string webFilePath = uploadPath + folderName + "\\" + fileName;
                    //用于存储在数据库的文件路径+文件名
                    strFileName = folderName + "/" + fileName;

                    //开始上传到服务器
                    if (!Directory.Exists(uploadPath + folderName))
                    {
                        Directory.CreateDirectory(uploadPath + folderName);
                    }
                    if (!File.Exists(webFilePath))
                    {
                        try
                        {
                            Object.SaveAs(webFilePath);//保存
                        }
                        catch (Exception e)
                        {
                            throw new ApplicationException(e.Message);
                        }
                    }
                    //删除原有文件
                    if (File.Exists(uploadPath + "\\" + DelFileName))
                    {
                        FileInfo delFile = new FileInfo(uploadPath + "\\" + DelFileName);
                        try
                        {
                            delFile.Delete();
                        }
                        catch (Exception e)
                        {
                            throw new ApplicationException(e.Message);
                        }
                    }
                }
                else
                {
                    //格式不正常
                }
            }
            else
            {
                //空文件
            }

            return strFileName;
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImagePath">源图路径（物理路径）</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>    
        public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, string mode)
        {

            System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath);

            int towidth = width;
            int toheight = height;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            switch (mode)
            {
                case "HW"://指定高宽缩放（可能变形）                
                    break;
                case "W"://指定宽，高按比例
                    if (originalImage.Width < width)
                    {
                        towidth = originalImage.Width;
                        toheight = originalImage.Height;
                    }
                    else
                    {
                        toheight = originalImage.Height * width / originalImage.Width;
                    }
                    break;
                case "H"://指定高，宽按比例
                    if (originalImage.Height < height)
                    {
                        toheight = originalImage.Height;
                        towidth = originalImage.Width;
                    }
                    else
                    {
                        towidth = originalImage.Width * height / originalImage.Height;
                    }
                    break;
                case "Cut"://指定高宽裁减（不变形）                
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
                case "AHW"://自动宽高缩放，不变形
                    ow = originalImage.Width;
                    oh = originalImage.Height;
                    float o = (float)ow / (float)oh;
                    float c = (float)width / (float)height;
                    x = y = 0;
                    //当前宽高比原始图大，按照高度缩放
                    if (o <= c)
                    {
                        if (oh > height)
                        {
                            toheight = height;
                            towidth = (int)((float)ow * ((float)height / (float)oh));
                        }
                        else
                        {
                            toheight = oh;
                            towidth = ow;
                        }

                    }
                    else
                    {
                        if (ow > width)
                        {
                            towidth = width;
                            toheight = (int)((float)oh * ((float)width / (float)ow));
                        }
                        else
                        {
                            toheight = oh;
                            towidth = ow;
                        }
                    }
                    break;
                default:
                    break;
            }
            int posX = (width - towidth) / 2;
            int posY = (height - toheight) / 2;

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(width, height);

            //新建一个画板
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(System.Drawing.Color.White);

            if (posX > 0 || posY > 0)
            {
                g.TranslateTransform(posX, posY);
            }
            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                new System.Drawing.Rectangle(x, y, ow, oh),
                System.Drawing.GraphicsUnit.Pixel);

            try
            {
                string Path = thumbnailPath.Substring(0, thumbnailPath.LastIndexOf('\\'));
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }

                //以jpg格式保存缩略图
                bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileInfo delFile = new FileInfo(filePath);
                try
                {
                    delFile.Delete();
                }
                catch (Exception e)
                {
                    throw new ApplicationException(e.Message);
                }
            }
        }
    }
}