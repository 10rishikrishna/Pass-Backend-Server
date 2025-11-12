using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CIAL.Shared.DigitalSignature
{
    /// <summary>
    /// Digital Signature Model - contains all signature information
    /// </summary>
    public class DigitalSignatureData
    {
        [JsonPropertyName("signatureId")]
        public string SignatureId { get; set; }

        [JsonPropertyName("signerName")]
        public string SignerName { get; set; }

        [JsonPropertyName("signerTitle")]
        public string SignerTitle { get; set; }

        [JsonPropertyName("signerOrganization")]
        public string SignerOrganization { get; set; }

        [JsonPropertyName("signedDate")]
        public DateTime SignedDate { get; set; }

        [JsonPropertyName("documentHash")]
        public string DocumentHash { get; set; }

        [JsonPropertyName("signatureValue")]
        public string SignatureValue { get; set; }

        [JsonPropertyName("publicKey")]
        public string PublicKey { get; set; }

        /// <summary>
        /// Get display text for the signature badge
        /// </summary>
        public string GetDisplayText()
        {
            return $"✅ DIGITALLY SIGNED\n" +
                   $"{SignerName}\n" +
                   $"{SignerTitle}\n" +
                   $"{SignerOrganization}\n" +
                   $"Date: {SignedDate:dd-MMM-yyyy HH:mm:ss}\n" +
                   $"Signature ID: {SignatureId}";
        }
    }

    /// <summary>
    /// Digital Signature Service - handles signing and verification
    /// </summary>
    public class DigitalSignatureService
    {
        private RSA _rsa;
        private string _publicKeyXml;
        private string _privateKeyXml;

        public DigitalSignatureService()
        {
            InitializeKeys();
        }

        /// <summary>
        /// Initialize RSA key pair (2048-bit)
        /// </summary>
        private void InitializeKeys()
        {
            _rsa = RSA.Create(2048);

            // Export keys as XML strings
            _publicKeyXml = _rsa.ToXmlString(false);  // Public key only
            _privateKeyXml = _rsa.ToXmlString(true);  // Public + Private key
        }

        /// <summary>
        /// Load keys from stored XML strings
        /// </summary>
        public void LoadKeys(string publicKeyXml, string privateKeyXml = null)
        {
            _publicKeyXml = publicKeyXml;
            _privateKeyXml = privateKeyXml;

            if (!string.IsNullOrEmpty(privateKeyXml))
            {
                _rsa.FromXmlString(privateKeyXml);
            }
            else
            {
                _rsa.FromXmlString(publicKeyXml);
            }
        }

        /// <summary>
        /// Get public key for sharing
        /// </summary>
        public string GetPublicKey()
        {
            return _publicKeyXml;
        }

        /// <summary>
        /// Create a digital signature for pass data
        /// </summary>
        public DigitalSignatureData SignPassData(
            object passData,
            string signerName,
            string signerTitle,
            string signerOrganization)
        {
            // Serialize pass data to JSON for hashing
            string jsonData = JsonSerializer.Serialize(passData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            // Create SHA-256 hash of the document
            string documentHash = ComputeSHA256Hash(jsonData);

            // Sign the hash with private key
            byte[] hashBytes = Convert.FromBase64String(documentHash);
            byte[] signatureBytes = _rsa.SignHash(hashBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            string signatureValue = Convert.ToBase64String(signatureBytes);

            // Generate unique signature ID
            string signatureId = GenerateSignatureId();

            return new DigitalSignatureData
            {
                SignatureId = signatureId,
                SignerName = signerName,
                SignerTitle = signerTitle,
                SignerOrganization = signerOrganization,
                SignedDate = DateTime.Now,
                DocumentHash = documentHash,
                SignatureValue = signatureValue,
                PublicKey = _publicKeyXml
            };
        }

        /// <summary>
        /// Verify a digital signature
        /// </summary>
        public bool VerifySignature(object passData, DigitalSignatureData signature)
        {
            try
            {
                // Recreate the hash from current pass data
                string jsonData = JsonSerializer.Serialize(passData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                string currentHash = ComputeSHA256Hash(jsonData);

                // Check if hash matches (document hasn't been tampered)
                if (currentHash != signature.DocumentHash)
                {
                    return false;
                }

                // Load public key from signature
                using (RSA rsaVerify = RSA.Create())
                {
                    rsaVerify.FromXmlString(signature.PublicKey);

                    // Verify signature
                    byte[] hashBytes = Convert.FromBase64String(signature.DocumentHash);
                    byte[] signatureBytes = Convert.FromBase64String(signature.SignatureValue);

                    return rsaVerify.VerifyHash(hashBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Signature verification failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Compute SHA-256 hash and return as Base64
        /// </summary>
        private string ComputeSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Generate unique signature ID
        /// </summary>
        private string GenerateSignatureId()
        {
            byte[] randomBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return $"SIG-{BitConverter.ToString(randomBytes).Replace("-", "").Substring(0, 12)}";
        }

        /// <summary>
        /// Export keys for storage (IMPORTANT: Keep private key secure!)
        /// </summary>
        public (string publicKey, string privateKey) ExportKeys()
        {
            return (_publicKeyXml, _privateKeyXml);
        }
    }

    /// <summary>
    /// Key Storage Service - handles secure key persistence
    /// WARNING: This is a simple implementation. In production, use secure key storage!
    /// </summary>
    public static class KeyStorageService
    {
        private const string PUBLIC_KEY_FILENAME = "cial_public_key.xml";
        private const string PRIVATE_KEY_FILENAME = "cial_private_key.xml";

        /// <summary>
        /// Save keys to local storage
        /// </summary>
        public static void SaveKeys(string publicKey, string privateKey, string folderPath)
        {
            string publicKeyPath = System.IO.Path.Combine(folderPath, PUBLIC_KEY_FILENAME);
            string privateKeyPath = System.IO.Path.Combine(folderPath, PRIVATE_KEY_FILENAME);

            System.IO.Directory.CreateDirectory(folderPath);
            System.IO.File.WriteAllText(publicKeyPath, publicKey);
            System.IO.File.WriteAllText(privateKeyPath, privateKey);
        }

        /// <summary>
        /// Load keys from local storage
        /// </summary>
        public static (string publicKey, string privateKey) LoadKeys(string folderPath)
        {
            string publicKeyPath = System.IO.Path.Combine(folderPath, PUBLIC_KEY_FILENAME);
            string privateKeyPath = System.IO.Path.Combine(folderPath, PRIVATE_KEY_FILENAME);

            if (!System.IO.File.Exists(publicKeyPath) || !System.IO.File.Exists(privateKeyPath))
            {
                throw new System.IO.FileNotFoundException("Keys not found. Please generate new keys.");
            }

            string publicKey = System.IO.File.ReadAllText(publicKeyPath);
            string privateKey = System.IO.File.ReadAllText(privateKeyPath);

            return (publicKey, privateKey);
        }

        /// <summary>
        /// Check if keys exist
        /// </summary>
        public static bool KeysExist(string folderPath)
        {
            string publicKeyPath = System.IO.Path.Combine(folderPath, PUBLIC_KEY_FILENAME);
            string privateKeyPath = System.IO.Path.Combine(folderPath, PRIVATE_KEY_FILENAME);

            return System.IO.File.Exists(publicKeyPath) && System.IO.File.Exists(privateKeyPath);
        }
    }
}