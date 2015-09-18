using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
namespace NHExt.WebUtil
{

    /// <summary>
    ///VerificationCodeHelper 的摘要说明
    /// </summary>
    public class VerificationCodeHelper
    {

        public static string GetVerCode(ref byte[] buffer)
        {
            string code = GenerateCheckCode();
            buffer = CreateCheckCodeImage(code);
            return code;
        }
        private static string GenerateCheckCode()            //绘制4个随机数或字母
        {
            int number;
            char code;
            string checkCode = String.Empty;

            Random random = new Random();

            for (int i = 0; i < 4; i++)
            {
                number = random.Next();

                if (number % 2 == 0)
                    code = (char)('0' + (char)(number % 10));
                else
                    code = (char)('A' + (char)(number % 26));

                checkCode += code.ToString();
            }
            return checkCode;
        }
        private static byte[] CreateCheckCodeImage(string checkCode)
        {
            byte[] tmpImg = null;

            if (checkCode == null || checkCode.Trim() == String.Empty)
                return tmpImg;

            System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)Math.Ceiling((checkCode.Length * 12.5)), 22);
            Graphics g = Graphics.FromImage(image);

            try
            {
                //生成随机生成器
                Random random = new Random();

                //清空图片背景色
                g.Clear(Color.FromArgb(215, 215, 215));

                //画图片的背景噪音线
                for (int i = 0; i < 2; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);

                    g.DrawLine(new Pen(Color.Black), x1, y1, x2, y2);
                }

                Font font = new System.Drawing.Font("Arial", 12, (System.Drawing.FontStyle.Bold));
                System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(checkCode, font, brush, 2, 2);

                //画图片的前景噪音点
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);

                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }

                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                tmpImg = ms.ToArray();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
            return tmpImg;
        }

    }
}