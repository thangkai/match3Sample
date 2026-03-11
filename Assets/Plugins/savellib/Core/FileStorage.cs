using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace SaveLib
{
    /// <summary>
    /// Xử lý đọc/ghi file xuống disk.
    /// Hỗ trợ encryption tuỳ chọn.
    /// </summary>
    public class FileStorage
    {
        private readonly string _saveDirectory;
        private readonly bool _useEncryption;

        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            TypeNameHandling     = TypeNameHandling.Auto,
            NullValueHandling    = NullValueHandling.Ignore,
            Formatting           = Formatting.Indented,
        };

        public FileStorage(bool useEncryption = true)
        {
            _useEncryption = useEncryption;
            _saveDirectory = Path.Combine(Application.persistentDataPath, "saves");
            Directory.CreateDirectory(_saveDirectory);
        }

        // ─── Public API ──────────────────────────────────────────

        public void Write<T>(string fileName, T data)
        {
            try
            {
                string json    = JsonConvert.SerializeObject(data, JsonSettings);
                string content = _useEncryption ? EncryptionHelper.Encrypt(json) : json;
                string path    = GetPath(fileName);

                // Ghi vào file tạm trước, sau đó rename → tránh corrupt nếu crash
                string tempPath = path + ".tmp";
                File.WriteAllText(tempPath, content);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tempPath, path);

                Debug.Log($"[FileStorage] Saved: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileStorage] Write failed ({fileName}): {e.Message}");
            }
        }

        public T Read<T>(string fileName) where T : class
        {
            string path = GetPath(fileName);
            if (!File.Exists(path)) return null;

            try
            {
                string content = File.ReadAllText(path);
                string json    = _useEncryption ? EncryptionHelper.Decrypt(content) : content;
                return JsonConvert.DeserializeObject<T>(json, JsonSettings);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileStorage] Read failed ({fileName}): {e.Message}");
                return null;
            }
        }

        public bool Exists(string fileName) => File.Exists(GetPath(fileName));

        public void Delete(string fileName)
        {
            string path = GetPath(fileName);
            if (File.Exists(path)) File.Delete(path);
        }

        public string GetPath(string fileName) => Path.Combine(_saveDirectory, fileName + ".sav");
    }
}
