using KeePassLib.Security;
using Net.Pkcs11Interop.X509Store;

namespace CertificateMultiAccessProvider;

internal class PinProvider : IPinProvider
{
    private static ProtectedString AskPassword(string title)
    {
        var form = new PinDialog(title);
        var dialogResult = form.ShowDialog();

        if (dialogResult == DialogResult.OK)
        {
            return form.Pin;
        }
        else
        {
            return null;
        }
    }

    public GetPinResult GetKeyPin(Pkcs11X509StoreInfo storeInfo, Pkcs11SlotInfo slotInfo, Pkcs11TokenInfo tokenInfo, Pkcs11X509CertificateInfo certificateInfo)
    {
        var pin = AskPassword(certificateInfo.Label);
        if (pin != null)
            return new GetPinResult(false, pin.ReadUtf8());
        else
            return new GetPinResult(true, null);
    }

    public GetPinResult GetTokenPin(Pkcs11X509StoreInfo storeInfo, Pkcs11SlotInfo slotInfo, Pkcs11TokenInfo tokenInfo)
    {
        var pin = AskPassword($"{tokenInfo.Label}");
        if (pin != null)
            return new GetPinResult(false, pin.ReadUtf8());
        else
            return new GetPinResult(true, null);
    }
}
