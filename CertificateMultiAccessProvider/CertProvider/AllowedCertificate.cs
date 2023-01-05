using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using KeePassLib.Security;

namespace CertificateMultiAccessProvider;

//public enum CertType
//{
//    Certificate,
//    InternalCertificate
//}

[XmlType("CertificateMultiAccessProvider")]
public class CertProviderConfig
{
    public List<AllowedCertificate> AllowedCertificates { get; private set; } = new List<AllowedCertificate>();

    public CertProviderConfig Copy()
    {
        return new CertProviderConfig()
        {
            AllowedCertificates = new(this.AllowedCertificates) { }
        };
    }
}

[XmlType("AllowedCertificate")]
[XmlInclude(typeof(AllowedCertificateRSA))]
[XmlInclude(typeof(AllowedCertificateRSAInternal))]
public abstract class AllowedCertificate
{

    [XmlAttribute]
    public int Version { get; set; }

    public byte[] Certificate { get; set; }
    public string Thumbprint { get; set; }

    /// <summary>
    /// Random key encrypted with the certificate.
    /// </summary>
    public byte[] EncryptedKey { get; set; }
    public byte[] IV { get; set; }
    /// <summary>
    /// User passphrase encrypted with the random key.
    /// </summary>
    public byte[] EncryptedData { get; set; }
    public abstract AllowedCertificate Clone();

    public abstract byte[] Decrypt(byte[] data);

    public abstract byte[] Encrypt(byte[] data);

    /// <summary>
    /// Public certificate only
    /// </summary>
    /// <returns></returns>
    public abstract X509Certificate2 ReadCertificate();

    internal void SetSecret(ProtectedBinary randomKey, byte[] iv, byte[] encryptedData)
    {
        RSA rsa;
        var rsaPadding = RSAEncryptionPadding.OaepSHA256;
        if ((rsa = ReadCertificate().GetRSAPublicKey()) != null)
        {
            EncryptedKey = rsa.Encrypt(randomKey.ReadData(), rsaPadding);
            this.IV = iv;
            this.EncryptedData = encryptedData;
        }
        else
        {
            throw new NotSupportedException("Certificate's key type not supported.");
        }
    }
}

public interface ICertStoreProvider
{
    // public ProtectedBinary DecryptKEK(AllowedCertificate certConfig);
    public byte[] Decrypt(AllowedCertificate certConfig, byte[] value);
    public byte[] Encrypt(AllowedCertificate certConfig, byte[] value);
    public byte[] Sign(AllowedCertificate certConfig, byte[] value);
    //public byte[] Verify(AllowedCertificate certConfig, byte[] value);
}
