using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertificateMultiAccessProvider.CertProvider;
using CertificateMultiAccessProvider.CertStore;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace CertificateMultiAccessProvider;


public static class CryptoHelpers
{

    public record AllowedRSAEncryptionPadding(RSAEncryptionPadding Value, string Name, string DisplayName)
    {
        public AllowedRSAEncryptionPadding() : this(RSAEncryptionPadding.OaepSHA256, "OAEPSHA256", "OAEP SHA256") { }

        public static AllowedRSAEncryptionPadding[] List { get; } =
            new[]
            {
                new AllowedRSAEncryptionPadding(RSAEncryptionPadding.OaepSHA256, "OAEPSHA256", "OAEP SHA256"),
                new AllowedRSAEncryptionPadding(RSAEncryptionPadding.OaepSHA384, "OAEPSHA384", "OAEP SHA384"),
                new AllowedRSAEncryptionPadding(RSAEncryptionPadding.OaepSHA512, "OAEPSHA512", "OAEP SHA512"),
                new AllowedRSAEncryptionPadding(RSAEncryptionPadding.OaepSHA1, "OAEPSHA1", "OAEP SHA1"),
                new AllowedRSAEncryptionPadding(RSAEncryptionPadding.Pkcs1, "PKCS1", "PKCS #1 (Legacy)")
            };

        public static AllowedRSAEncryptionPadding Default => List[0];
        public static AllowedRSAEncryptionPadding GetFromNameOrDefault(string name) => List.SingleOrDefault(x => x.Name == name) ?? Default;
        public static AllowedRSAEncryptionPadding GetFromName(string name) => List.Single(x => x.Name == name);
    }

    internal static (X509Certificate2, CertProvType) ShowCertificateSelection(string message, CertProviderConfiguration certProviderConfig, Settings settings, Form parent = null)
    {
        using var form = new KeySelectionForm(certProviderConfig, settings, message);
        form.Parent = parent;

        if (form.ShowDialog() != DialogResult.OK)
        {
            return (null, CertProvType.None);
        }

        return (form.SelectedCertificate, form.SelectedType);
    }

    public static string GetSha2Thumbprint(X509Certificate2 cert)
    {
        byte[] hashBytes;
        using (var hasher = new SHA256Managed())
        {
            hashBytes = hasher.ComputeHash(cert.RawData);
        }

        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
    }

    public static X509Certificate2 GenerateSelfSignedCertificate(string subject)
    {
        var distinguishedName = new X500DistinguishedName($"CN={subject}");

        using var rsa = RSA.Create(4096);
        var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DataEncipherment
                | X509KeyUsageFlags.KeyEncipherment
                | X509KeyUsageFlags.DigitalSignature, false));

        var certificate = request.CreateSelfSigned(
            new DateTimeOffset(DateTime.UtcNow),
            new DateTimeOffset(DateTime.UtcNow.AddYears(50)));

        certificate.FriendlyName = $"{subject}";

        return certificate;
    }

    public static void SetSecretKeyFromNewUserKey(AllowedCertificate certInfo, ProtectedBinary secretKey)
    {
        // Instead of directly encrypting the passphrase with the certificate,
        // we use an intermediate random symmetric key.
        // The passphrase is encrypted with the symmetric key, and the symmetric key is encrypted with the certificate.
        // (asymmetric encryption is not suited to encrypt a lot of data)

        // symmetric encryption:
        var randomKey = new ProtectedBinary(true, CryptoRandom.Instance.GetRandomBytes(32));
        var encryptedData = EncryptAES(secretKey, randomKey, out var iv);

        // now we asymmetrically encrypt the random key.
        certInfo.SetSecret(randomKey, iv, encryptedData);
    }

    public static ProtectedBinary DecryptSecretFromConfig(AllowedCertificate certInfo, CertProvType providerType, Settings settings)
    {
        ICertStoreProvider certStoreProv;
        if (providerType == CertProvType.Pkcs11)
        {
            certStoreProv = new CertStorePkcs11(settings.Pkcs11LibPath);
        }
        else
        {
            certStoreProv = new CertStoreCAPI();
        }

        // First we decrypt the symmetric key:
        var decryptedKey = new ProtectedBinary(true, certStoreProv.Decrypt(certInfo, certInfo.EncryptedKey));

        // Then we use the symmetric key to decrypt the passphrase:
        var decryptedData = DecryptAES(certInfo.EncryptedData, decryptedKey, certInfo.IV);

        return decryptedData;
    }

    public static X509Certificate2 LoadCertificateFromStore(X509Certificate2 certificate)
    {
        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        using (store)
        {
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            var result = store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, false);
            return result.Cast<X509Certificate2>().SingleOrDefault();
        }
    }

    public static byte[] EncryptAES(ProtectedBinary secret, ProtectedBinary key, out byte[] iv)
    {
        iv = CryptoRandom.Instance.GetRandomBytes(16); // AES 256 uses 128bits blocks
        using var ms = new MemoryStream();
        using (var encryptionStream = new StandardAesEngine().EncryptStream(ms, key.ReadData(), iv))
        {
            MemUtil.Write(encryptionStream, secret.ReadData());
        }
        return ms.ToArray();
    }

    public static ProtectedBinary DecryptAES(byte[] encryptedSecret, ProtectedBinary key, byte[] iv)
    {
        using var ms = new MemoryStream(encryptedSecret);
        using var decryptionStream = new StandardAesEngine().DecryptStream(ms, key.ReadData(), iv);
        return new ProtectedBinary(true, ReadToEnd(decryptionStream));
    }

    private static byte[] ReadToEnd(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);

        return ms.ToArray();
    }
}

