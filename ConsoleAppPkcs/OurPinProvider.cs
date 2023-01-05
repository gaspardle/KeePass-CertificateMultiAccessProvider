using System.Text;
using Net.Pkcs11Interop.X509Store;

namespace ConsoleAppPkcs
{
    internal class OurPinProvider : IPinProvider
    {

        public GetPinResult GetKeyPin(Pkcs11X509StoreInfo storeInfo, Pkcs11SlotInfo slotInfo, Pkcs11TokenInfo tokenInfo, Pkcs11X509CertificateInfo certificateInfo)
        {
            var pin = "132";
            if (true)
                return new GetPinResult(false, Encoding.UTF8.GetBytes(pin));
            else
                return new GetPinResult(true, null);
        }

        public GetPinResult GetTokenPin(Pkcs11X509StoreInfo storeInfo, Pkcs11SlotInfo slotInfo, Pkcs11TokenInfo tokenInfo)
        {
            var pin = "132";
            if (true)
                return new GetPinResult(false, Encoding.UTF8.GetBytes(pin));
            else
                return new GetPinResult(true, null);
        }
    }
}
