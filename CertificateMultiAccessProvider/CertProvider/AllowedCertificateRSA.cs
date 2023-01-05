using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertificateMultiAccessProvider;

public class AllowedCertificateRSA : AllowedCertificate
{
    public const int CurrentVersion = 1;

    public AllowedCertificateRSA() { }

    public static AllowedCertificateRSA Create(X509Certificate2 certificate)
    {
        if (certificate == null) throw new ArgumentNullException(nameof(certificate));

        return new()
        {
            Version = CurrentVersion,
            // public part only        
            Certificate = certificate.Export(X509ContentType.Cert),
            Thumbprint = certificate.Thumbprint,
        };
    }

    public override AllowedCertificate Clone()
    {
        return new AllowedCertificateRSA()
        {
            Certificate = Certificate,
            Thumbprint = this.Thumbprint,
        };
    }

    public override byte[] Decrypt(byte[] data)
    {
        return null;
    }

    public override byte[] Encrypt(byte[] data)
    {
        RSA rsa;
        var rsaPadding = RSAEncryptionPadding.OaepSHA256;
        if ((rsa = this.ReadCertificate().GetRSAPublicKey()) != null)
        {
            return rsa.Encrypt(data, rsaPadding);
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

