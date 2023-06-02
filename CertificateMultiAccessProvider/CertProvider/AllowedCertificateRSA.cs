using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using KeePassLib.Security;
using static CertificateMultiAccessProvider.CryptoHelpers;

namespace CertificateMultiAccessProvider;

public class AllowedCertificateRSA : AllowedCertificate
{
    public const int CurrentVersion = 1;
    public string RSAEncryptionPaddingName { get; set; }


    public AllowedCertificateRSA() { }

    public static AllowedCertificateRSA Create(X509Certificate2 certificate, AllowedRSAEncryptionPadding rsaEncryptionPadding)
    {
        if (certificate == null) throw new ArgumentNullException(nameof(certificate));

        return new()
        {
            Version = CurrentVersion,
            Certificate = certificate.Export(X509ContentType.Cert),  // public part only       
            Thumbprint = certificate.Thumbprint,
            RSAEncryptionPaddingName = rsaEncryptionPadding.Name
        };
    }

    public AllowedRSAEncryptionPadding RSAEncryptionPadding => AllowedRSAEncryptionPadding.GetFromNameOrDefault(RSAEncryptionPaddingName);

    public override AllowedCertificate Clone()
    {
        return new AllowedCertificateRSA()
        {
            Certificate = Certificate,
            Thumbprint = this.Thumbprint,
            RSAEncryptionPaddingName = this.RSAEncryptionPaddingName,
        };
    }

    internal override void SetSecret(ProtectedBinary randomKey, byte[] iv, byte[] encryptedData)
    {
        RSA rsa;
        var rsaPadding = this.RSAEncryptionPadding ?? AllowedRSAEncryptionPadding.Default;
        if ((rsa = ReadCertificate().GetRSAPublicKey()) != null)
        {
            EncryptedKey = rsa.Encrypt(randomKey.ReadData(), rsaPadding.Value);
            this.IV = iv;
            this.EncryptedData = encryptedData;
        }
        else
        {
            throw new NotSupportedException("Certificate's key type not supported.");
        }
    }

    public override X509Certificate2 ReadCertificate()
    {
        return new(Certificate);
    }
}

