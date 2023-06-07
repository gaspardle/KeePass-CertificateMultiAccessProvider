using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using KeePassLib.Security;
using static CertificateMultiAccessProvider.CryptoHelpers;

namespace CertificateMultiAccessProvider.CertProvider;

public record AllowedCertificateRSA : AllowedCertificate
{
    public const int CurrentVersion = 1;

    public string RSAEncryptionPaddingName { get; /*private*/ set; }


    [XmlIgnore]
    internal AllowedRSAEncryptionPadding RSAEncryptionPadding
    {
        get { return AllowedRSAEncryptionPadding.GetFromNameOrDefault(RSAEncryptionPaddingName); }
        init
        {
            RSAEncryptionPaddingName = value.Name;
        }
    }

    public AllowedCertificateRSA() { }

    public static AllowedCertificateRSA Create(X509Certificate2 certificate, AllowedRSAEncryptionPadding rsaEncryptionPadding)
    {
        if (certificate == null) throw new ArgumentNullException(nameof(certificate));

        return new()
        {
            Version = CurrentVersion,
            Certificate = certificate.Export(X509ContentType.Cert),  // public part only       
            Thumbprint = certificate.Thumbprint,
            RSAEncryptionPadding = rsaEncryptionPadding
        };
    }

    public override AllowedCertificate Copy()
    {
        return this with { };
    }

    [Obsolete]
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

