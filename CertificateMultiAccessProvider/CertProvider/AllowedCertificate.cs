using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Serialization;
using KeePassLib.Security;

namespace CertificateMultiAccessProvider.CertProvider;

[XmlType("AllowedCertificate")]
[XmlInclude(typeof(AllowedCertificateRSA))]
public abstract record AllowedCertificate
{

    [XmlAttribute]
    public int Version { get; init; }

    public byte[] Certificate { get; init; }
    public string Thumbprint { get; init; }

    /// <summary>
    /// Random key encrypted with the certificate.
    /// </summary>
    public byte[] EncryptedKey { get; set; }
    public byte[] IV { get; set; }

    /// <summary>
    /// User passphrase encrypted with the random key.
    /// </summary>
    public byte[] EncryptedData { get; set; }

    public abstract AllowedCertificate Copy();

    /// <summary>
    /// Public certificate only
    /// </summary>
    /// <returns></returns>
    public abstract X509Certificate2 ReadCertificate();

    internal abstract void SetSecret(ProtectedBinary randomKey, byte[] iv, byte[] encryptedData);
}
