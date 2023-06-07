using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CertificateMultiAccessProvider.CertProvider;

namespace CertificateMultiAccessProvider.CertStore;

public interface ICertStoreProvider
{
    // public ProtectedBinary DecryptKEK(AllowedCertificate certConfig);
    public byte[] Decrypt(AllowedCertificate certConfig, byte[] value);
    public byte[] Encrypt(AllowedCertificate certConfig, byte[] value);
    public byte[] Sign(AllowedCertificate certConfig, byte[] value);
    //public byte[] Verify(AllowedCertificate certConfig, byte[] value);
}
