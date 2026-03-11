using NUnit.Framework;
using SaveLib;
using UnityEngine;

namespace SaveLib.Tests
{
    public class EncryptionTests
    {
        [Test]
        public void Encrypt_ThenDecrypt_ReturnsSameString()
        {
            string original  = "{\"coin\": 999}";
            string encrypted = EncryptionHelper.Encrypt(original);
            string decrypted = EncryptionHelper.Decrypt(encrypted);

            Debug.Log($"Original:  {original}");
            Debug.Log($"Encrypted: {encrypted}");
            Debug.Log($"Decrypted: {decrypted}");

            Assert.AreEqual(original, decrypted);
        }
        
        [Test]
        public void Encrypt_SameInput_ReturnsDifferentOutput()
        {
            // IV random nên mỗi lần encrypt phải ra kết quả khác nhau
            string input = "test data";
            string enc1  = EncryptionHelper.Encrypt(input);
            string enc2  = EncryptionHelper.Encrypt(input);

            Assert.AreNotEqual(enc1, enc2);
        }
    }
}