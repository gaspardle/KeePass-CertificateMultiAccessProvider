using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertificateMultiAccessProvider.CertStore;

public class CertStoreCAPI : ICertStoreProvider
{

    public static X509Certificate2 SelectCertificate(string messsage, IEnumerable<string> thumbprints = null, IntPtr? hwnd = null)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        X509Certificate2Collection collection;

        if (thumbprints != null)
        {
            collection = new X509Certificate2Collection();
            foreach (var thumbprint in thumbprints)
            {
                var search = store.Certificates
                 .Find(X509FindType.FindByTimeValid, DateTime.Now, false)
                 .Find(X509FindType.FindByThumbprint, thumbprint, true)
                 .Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment, true);

                if (search.Count == 1)
                {
                    collection.Add(search[0]);
                }
            }
        }
        else
        {
            collection = store.Certificates
                .Find(X509FindType.FindByTimeValid, DateTime.Now, false)
                .Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment, true);
        }

        var selection = X509Certificate2UI.SelectFromCollection(
                collection,
                "Select a certificate",
                messsage,
                X509SelectionFlag.SingleSelection, hwnd ?? IntPtr.Zero)
            .Cast<X509Certificate2>().SingleOrDefault();

        return selection;
    }

    public byte[] Decrypt(AllowedCertificate certConfig, byte[] value)
    {
        using var fromSourceCertificate = certConfig.ReadCertificate();
        var fromStoreCertificate = CryptoHelpers.LoadCertificateFromStore(fromSourceCertificate);

        if (fromStoreCertificate == null)
        {
            throw new InvalidOperationException("The specified certificate could not be found in the store.");
        }

        if (!fromStoreCertificate.HasPrivateKey)
        {
            throw new InvalidOperationException("The specified certificate does not have a private key.");
        }

        if (certConfig is AllowedCertificateRSA rsaCertConfig)
        {
            RSA rsa;
            if ((rsa = fromStoreCertificate.GetRSAPrivateKey()) != null)
            {
                return rsa.Decrypt(value, rsaCertConfig.RSAEncryptionPadding.Value);
            }
            else
            {
                throw new NotSupportedException("Unable to use certificate RSA private key");
            }
        }
        else
        {
            throw new NotSupportedException("Certificate's key type not supported.");
        }
    }

    public byte[] Encrypt(AllowedCertificate certConfig, byte[] value)
    {
        return null;

    }

    public byte[] Sign(AllowedCertificate certConfig, byte[] value)
    {
        throw new NotImplementedException();
    }
}
