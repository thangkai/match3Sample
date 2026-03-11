using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace SaveLib
{
    /// <summary>
    /// AES-256 encryption cho save file.
    /// Key được derive từ một secret string + device identifier.
    /// </summary>
    public static class EncryptionHelper
    {
        // ⚠️ Đổi chuỗi này thành secret riêng của game bạn
        private const string SECRET = "YOUR_GAME_SECRET_KEY_CHANGE_THIS";
        private const int KEY_SIZE  = 32; // 256-bit
        private const int IV_SIZE   = 16; // 128-bit

        /// <summary>Encrypt plain text → Base64 string</summary>
        public static string Encrypt(string plainText)
        {
            try
            {
                byte[] key = DeriveKey();
                byte[] iv  = GenerateIV();

                using var aes       = Aes.Create();
                aes.Key             = key;
                aes.IV              = iv;
                aes.Mode            = CipherMode.CBC;
                aes.Padding         = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                using var ms        = new MemoryStream();

                // Prepend IV vào đầu để decrypt sau này
                ms.Write(iv, 0, iv.Length);

                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs, Encoding.UTF8))
                    sw.Write(plainText);

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception e)
            {
                Debug.LogError($"[EncryptionHelper] Encrypt failed: {e.Message}");
                return plainText; // Fallback: lưu plain nếu lỗi
            }
        }

        /// <summary>Decrypt Base64 string → plain text</summary>
        public static string Decrypt(string cipherText)
        {
            try
            {
                byte[] fullBytes = Convert.FromBase64String(cipherText);
                byte[] key       = DeriveKey();

                // Tách IV từ đầu
                byte[] iv        = new byte[IV_SIZE];
                byte[] cipher    = new byte[fullBytes.Length - IV_SIZE];
                Array.Copy(fullBytes, 0, iv, 0, IV_SIZE);
                Array.Copy(fullBytes, IV_SIZE, cipher, 0, cipher.Length);

                using var aes       = Aes.Create();
                aes.Key             = key;
                aes.IV              = iv;
                aes.Mode            = CipherMode.CBC;
                aes.Padding         = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                using var ms        = new MemoryStream(cipher);
                using var cs        = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr        = new StreamReader(cs, Encoding.UTF8);

                return sr.ReadToEnd();
            }
            catch (Exception e)
            {
                Debug.LogError($"[EncryptionHelper] Decrypt failed: {e.Message}");
                return cipherText; // Fallback: trả về raw nếu lỗi
            }
        }

        // ─── Private ─────────────────────────────────────────────

        private static byte[] DeriveKey()
        {
            // Kết hợp secret + device ID để key khác nhau trên mỗi thiết bị
            string combined = SECRET + SystemInfo.deviceUniqueIdentifier;
            using var sha   = SHA256.Create();
            byte[] hash     = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));

            // Đảm bảo đúng 32 bytes
            byte[] key = new byte[KEY_SIZE];
            Array.Copy(hash, key, Math.Min(hash.Length, KEY_SIZE));
            return key;
        }

        private static byte[] GenerateIV()
        {
            byte[] iv = new byte[IV_SIZE];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(iv);
            return iv;
        }
    }
    
    

}
