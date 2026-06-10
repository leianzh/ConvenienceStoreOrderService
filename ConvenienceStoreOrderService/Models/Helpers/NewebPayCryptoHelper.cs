using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.IO;


namespace ConvenienceStoreOrderService.Models.Helpers
{
    public class NewebPayCryptoHelper
    {

        /// <summary>
        /// 使用 AES-256-CBC + PKCS7 Padding 加密，產生 TradeInfo
        /// </summary>
        public static string EncryptTradeInfo(string plainText, string hashKey, string hashIV)
        {
            //字串轉byte
            byte[] keybyte = Encoding.UTF8.GetBytes(hashKey);
            byte[] ivbyte = Encoding.UTF8.GetBytes(hashIV);
            byte[] plainbyte = Encoding.UTF8.GetBytes(plainText);
            //AES 規格
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.Key = keybyte;
            aes.IV = ivbyte;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            //aes加密
            MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(
               ms,
               aes.CreateEncryptor(),
               CryptoStreamMode.Write))
            {

                cs.Write(plainbyte, 0, plainbyte.Length);
                cs.FlushFinalBlock();

                //加密後 byte[]
                byte[] encryptedBytes = ms.ToArray();

                //轉成 16 進位字串
                return BitConverter
                    .ToString(encryptedBytes)
                    .Replace("-", "")
                    .ToLower();
            }
        }
        /// <summary>
        /// 使用 SHA256 產生 TradeSha轉大寫
        /// </summary>
        public static string GenerateTradeSha(string tradeInfo, string hashKey, string hashIV)
        {
            
            string rawData = $"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}";

            //轉成 byte[]
            byte[] rawbytes = Encoding.UTF8.GetBytes(rawData);

            //做 SHA256
            SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(rawbytes);
            //轉成 16 進位字串，並轉大寫
            return BitConverter
                .ToString(hashBytes)
                .Replace("-","")
                .ToUpper();


        }
        /// <summary>
        /// AES-256-CBC + PKCS7 解密藍新回傳的 TradeInfo
        /// </summary>
        public static string DecryptTradeInfo(string encryptedHexText, string hashKey, string hashIV)
        {
            // HashKey、HashIV 轉 byte[]
            byte[] keyBytes = Encoding.UTF8.GetBytes(hashKey);
            byte[] ivBytes = Encoding.UTF8.GetBytes(hashIV);

            //字串轉回 byte[]
            byte[] encryptedBytes = HexStringToByteArray(encryptedHexText);

            //aes規格
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();         
            aes.Key = keyBytes;              
            aes.IV = ivBytes;                
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(
                    ms,
                    aes.CreateDecryptor(),
                    CryptoStreamMode.Write))
            {
                   
                    cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                    cs.FlushFinalBlock();

                    // 解密後的 byte[]
                    byte[] decryptedBytes = ms.ToArray();

                    //轉回字串
                    return Encoding.UTF8.GetString(decryptedBytes);
            }
            
        }
        /// <summary>
        /// 16 進位字串轉 byte[]
        /// </summary>
        private static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex 字串長度必須是偶數。");
            }

            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}
