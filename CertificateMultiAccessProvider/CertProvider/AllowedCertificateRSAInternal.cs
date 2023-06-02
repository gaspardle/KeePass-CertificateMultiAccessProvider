using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using KeePassLib.Security;
using static CertificateMultiAccessProvider.CryptoHelpers;

namespace CertificateMultiAccessProvider;

public class AllowedCertificateRSAInternal : AllowedCertificateRSA
{
    public new const int CurrentVersion = 1;
    public byte[] PrivateKey { get; set; }
    public byte[] PrivateKeySalt { get; set; }
    public byte[] PrivateKeyIV { get; set; }


    public AllowedCertificateRSAInternal() { }

    public static AllowedCertificateRSAInternal Create(X509Certificate2 certificate, ProtectedString password)
    {
        if (certificate == null) throw new ArgumentNullException(nameof(certificate));


        var privatePfxSalt = new byte[16];
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            rngCsp.GetBytes(privatePfxSalt);
        }

        var derivative = new Rfc2898DeriveBytes(password.ReadUtf8(), privatePfxSalt, 300000, HashAlgorithmName.SHA512);
        var privatePfxKey = new ProtectedBinary(true, derivative.GetBytes(32));
        var privatePfx = new ProtectedBinary(true, certificate.Export(X509ContentType.Pfx, password.ReadString()));
        var encryptedPfx = CryptoHelpers.EncryptAES(privatePfx, privatePfxKey, out var privatePfxIV);

        return new()
        {
            Version = CurrentVersion,
            Certificate = certificate.Export(X509ContentType.Cert),
            Thumbprint = certificate.Thumbprint,
            RSAEncryptionPaddingName = AllowedRSAEncryptionPadding.GetFromName("OAEPSHA256").Name,
            PrivateKey = encryptedPfx,
            PrivateKeySalt = privatePfxSalt,
            PrivateKeyIV = privatePfxIV,
        };
    }

    public override AllowedCertificate Clone()
    {
        return new AllowedCertificateRSAInternal()
        {
            Certificate = Certificate,
            Thumbprint = this.Thumbprint,
            RSAEncryptionPaddingName = this.RSAEncryptionPaddingName,
            PrivateKey = PrivateKey,
            PrivateKeyIV = PrivateKeyIV,
            PrivateKeySalt = this.PrivateKeySalt,
        };
    }
}


