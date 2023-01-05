using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Net.Pkcs11Interop.X509Store;

namespace CertificateMultiAccessProvider.CertStore;

public class CertStorePkcs11 : ICertStoreProvider
{
    string _pkcsLibraryPath;

    public CertStorePkcs11(string pkcsLibraryPath)
    {
        _pkcsLibraryPath = pkcsLibraryPath;
    }

    public byte[] Decrypt(AllowedCertificate certConfig, byte[] value)
    {
        using var fromSourceCertificate = certConfig.ReadCertificate();
        var fromStoreCertificate = LoadCertificateFromPkcs11(fromSourceCertificate);

        if (fromStoreCertificate == null)
        {
            throw new InvalidOperationException("The specified certificate could not be found in the pkcs11 token.");
        }

        if (!fromStoreCertificate.HasPrivateKeyObject)
        {
            throw new InvalidOperationException("The specified certificate does not have a private key.");
        }

        RSA rsa;
        if ((rsa = fromStoreCertificate.GetRSAPrivateKey()) != null)
        {
            return rsa.Decrypt(value, RSAEncryptionPadding.OaepSHA256);
        }
        else
        {
            throw new NotSupportedException("Certificate's key type not supported.");
        }
    }

    public byte[] Sign(AllowedCertificate certConfig, byte[] value)
    {
        using var fromSourceCertificate = certConfig.ReadCertificate();
        var fromStoreCertificate = LoadCertificateFromPkcs11(fromSourceCertificate);

        if (fromStoreCertificate == null)
        {
            throw new InvalidOperationException("The specified certificate could not be found in the pkcs11 token.");
        }

        if (!fromStoreCertificate.HasPrivateKeyObject)
        {
            throw new InvalidOperationException("The specified certificate does not have a private key.");
        }

        RSA rsa;
        if ((rsa = fromStoreCertificate.GetRSAPrivateKey()) != null)
        {
            return rsa.SignData(value, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        else
        {
            throw new NotSupportedException("Certificate's key type not supported.");
        }
    }
    public byte[] Encrypt(AllowedCertificate certConfig, byte[] value)
    {
        using var fromSourceCertificate = certConfig.ReadCertificate();
        var fromStoreCertificate = LoadCertificateFromPkcs11(fromSourceCertificate);

        if (fromStoreCertificate == null)
        {
            throw new InvalidOperationException("The specified certificate could not be found in the pkcs11 token.");
        }

        RSA rsa;
        if ((rsa = fromStoreCertificate.GetRSAPublicKey()) != null)
        {
            return rsa.Encrypt(value, RSAEncryptionPadding.OaepSHA256);
        }
        else
        {
            throw new NotSupportedException("Certificate's key type not supported.");
        }
    }

    private Pkcs11X509Certificate LoadCertificateFromPkcs11(X509Certificate2 publicCert)
    {
        var pinProvider = new PinProvider();
        var store = new Pkcs11X509Store(_pkcsLibraryPath, pinProvider);

        return store.Slots.First().Token.Certificates
             .Where(c => c.Info.ParsedCertificate.Thumbprint == publicCert.Thumbprint)
             .FirstOrDefault();

    }
}
