using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace NHExt.Runtime.Util
{
    public static class EncryptHelper
    {
        //默认密钥向量 
        private static byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        private static string _keys = "IWEHAVE.ERP.GAIA";//密钥,128位   
        /// <summary>
        /// AES加密算法
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <param name="strKey">密钥</param>
        /// <returns>返回加密后的密文字节数组</returns>
        public static string Encrypt(string plainText)
        {
            //分组加密算法
            SymmetricAlgorithm des = Rijndael.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText.Trim());//得到需要加密的字节数组	
            //设置密钥及密钥向量
            des.Key = Encoding.UTF8.GetBytes(_keys);
            des.IV = _key1;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputBytes, 0, inputBytes.Length);
            cs.FlushFinalBlock();
            byte[] outputBytes = ms.ToArray();//得到加密后的字节数组
            cs.Close();
            ms.Close();
            return Convert.ToBase64String(outputBytes);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText">密文字节数组</param>
        /// <param name="strKey">密钥</param>
        /// <returns>返回解密后的字符串</returns>
        public static string Decrypt(string plainText)
        {
            SymmetricAlgorithm des = Rijndael.Create();
            des.Key = Encoding.UTF8.GetBytes(_keys);
            des.IV = _key1;
            byte[] inputBytes = Convert.FromBase64String(plainText);
            byte[] outputBytes = new byte[inputBytes.Length];
            MemoryStream ms = new MemoryStream(inputBytes);
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
            cs.Read(outputBytes, 0, outputBytes.Length);
            cs.Close();
            ms.Close();
            return Encoding.UTF8.GetString(outputBytes).Trim('\0');
        }

    }
}
