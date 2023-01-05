using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using KeePassLib.Security;

namespace CertificateMultiAccessProvider;

public class AllowedCertificateRSAInternal : AllowedCertificateRSA
{
    public new const int CurrentVersion = 1;

    public AllowedCertificateRSAInternal() { }

    public byte[] PrivateKey { get; set; }
    public byte[] PrivateKeySalt { get; set; }
    public byte[] PrivateKeyIV { get; set; }


    public static new AllowedCertificateRSAInternal Create(X509Certificate2 certificate, ProtectedString password)
    {
        if (certificate == null) throw new ArgumentNullException(nameof(certificate));

        //var password = AskPassword(certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false));
        //if (password == null) throw new InvalidOperationException("The specified password is not correct.");

        byte[] privatePfxSalt = new byte[16];
        using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
        {
            // Fill the array with a random value.
            rngCsp.GetBytes(privatePfxSalt);
        }

        var derivative = new Rfc2898DeriveBytes(password.ReadUtf8(), privatePfxSalt, 120000, HashAlgorithmName.SHA256);
        var privatePfxKey = new ProtectedBinary(true, derivative.GetBytes(32));
        var privatePfx = new ProtectedBinary(true, certificate.Export(X509ContentType.Pfx, password.ReadString()));
        var encryptedPfx = CryptoHelpers.EncryptAES(privatePfx, privatePfxKey, out var privatePfxIV);

        return new()
        {
            Version = CurrentVersion,
            PrivateKey = encryptedPfx,
            PrivateKeySalt = privatePfxSalt,
            PrivateKeyIV = privatePfxIV,
            Certificate = certificate.Export(X509ContentType.Cert),
            Thumbprint = certificate.Thumbprint,
        };
    }

    private static ProtectedString AskPassword(string subject)
    {
        var form = new PasswordDialog(subject, false);
        var dialogResult = form.ShowDialog();
        var passphrase = form.Passphrase;

        if (dialogResult == DialogResult.OK)
        {
            return passphrase;
        }
        else
        {
            return null;
        }
    }

    public override byte[] Decrypt(byte[] data)
    {
        return null;
    }


    public override AllowedCertificate Clone()
    {
        return new AllowedCertificateRSAInternal()
        {
            Certificate = Certificate,
            Thumbprint = this.Thumbprint,
            PrivateKey = PrivateKey,
            PrivateKeyIV = PrivateKeyIV,
            PrivateKeySalt = this.PrivateKeySalt,
        };
    }
}


