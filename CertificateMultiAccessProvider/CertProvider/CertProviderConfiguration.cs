using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;
using System.Xml.Schema;

namespace CertificateMultiAccessProvider.CertProvider;

[XmlRoot]
[XmlType("CertificateMultiAccessProvider")]
public class CertProviderConfiguration
{

    public List<AllowedCertificate> AllowedCertificates { get; set; } = new();

    public CertProviderConfiguration Copy()
    {
        return new CertProviderConfiguration()
        {
            AllowedCertificates = new(this.AllowedCertificates)
        };
    }

}
