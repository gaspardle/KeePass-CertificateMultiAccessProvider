using KeePass.App.Configuration;

namespace CertificateMultiAccessProvider;

internal class Settings
{
    private readonly AceCustomConfig _config;
    public const string ConfigPrefix = "certmultiaccess";

    public Settings(AceCustomConfig config)
    {
        _config = config;
    }

    public string Pkcs11LibPath
    {
        get { return _config.GetString("Pkcs11LibPath"); }
        set { _config.SetString("Pkcs11LibPath", value); }
    }
}
