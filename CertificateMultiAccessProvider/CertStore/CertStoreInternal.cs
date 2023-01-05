using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using KeePassLib.Security;

namespace CertificateMultiAccessProvider.CertStore;
public class CertStoreInternal : ICertStoreProvider
{
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

    public byte[] Decrypt(AllowedCertificate certConfig, byte[] data)
    {
        var internalCertConfig = (AllowedCertificateRSAInternal)certConfig;
        var subject = certConfig.ReadCertificate().GetNameInfo(X509NameType.SimpleName, forIssuer: false);

        var password = AskPassword(subject);
        if (password == null) throw new InvalidOperationException("The specified password is not correct.");

        var derivative = new Rfc2898DeriveBytes(password.ReadUtf8(), internalCertConfig.PrivateKeySalt, 120000, HashAlgorithmName.SHA256);
        var privatePfxKey = new ProtectedBinary(true, derivative.GetBytes(32));
        var decryptedPfx = CryptoHelpers.DecryptAES(internalCertConfig.PrivateKey, privatePfxKey, internalCertConfig.PrivateKeyIV);

        X509Certificate2 cert;
        try
        {
            cert = new X509Certificate2(decryptedPfx.ReadData(), password.ReadString(), X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
        }
        catch (CryptographicException ex) when (ex.HResult == -2147024810)
        {
            throw new InvalidOperationException("The specified password is not correct.");
        }
        if (!cert.HasPrivateKey)
        {
            throw new InvalidOperationException("The specified certificate does not have a private key.");
        }

        RSA rsa;
        if ((rsa = cert.GetRSAPrivateKey()) != null)
        {
            return rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);
        }
        else
        {
            throw new NotSupportedException("Certificate's key type not supported.");
        }
    }

    public byte[] Encrypt(AllowedCertificate certConfig, byte[] value)
    {
        throw new NotImplementedException();
    }

    public byte[] Sign(AllowedCertificate certConfig, byte[] value)
    {
        throw new NotImplementedException();
    }

}
