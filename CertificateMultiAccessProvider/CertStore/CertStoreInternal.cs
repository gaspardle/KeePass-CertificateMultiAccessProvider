using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using KeePassLib.Cryptography.KeyDerivation;
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


        var kdf = new Argon2Kdf(Argon2Type.ID);
        var pKdf = kdf.GetDefaultParameters();
        pKdf.SetByteArray(Argon2Kdf.ParamSalt, internalCertConfig.PrivateKeySalt);
        //pKdf.SetByteArray(Argon2Kdf.ParamAssocData, pbAssoc);
        //pKdf.SetUInt64(Argon2Kdf.ParamIterations, uIt);
        //pKdf.SetUInt32(Argon2Kdf.ParamParallelism, uPar);
        //pKdf.SetUInt64(Argon2Kdf.ParamMemory, uMem);
        var derivative = kdf.Transform(password.ReadUtf8(), pKdf);
        var privatePfxKey = new ProtectedBinary(true, derivative, 0, 32);

        var decryptedPfx = CryptoHelpers.DecryptAES(internalCertConfig.PrivateKey, privatePfxKey, internalCertConfig.PrivateKeyIV);

        X509Certificate2 cert;
        try
        {
            cert = new X509Certificate2(decryptedPfx.ReadData(), password.ReadString(), X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
        }
        catch (CryptographicException ex) when ((uint)ex.HResult == 0x80070056)
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
