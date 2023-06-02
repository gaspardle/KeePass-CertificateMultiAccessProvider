using KeePass.App.Configuration;

namespace CertificateMultiAccessProvider;

public class Settings
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

    public string DefaultRsaPaddingName
    {
        get { return _config.GetString("DefaultRsaPaddingName", "OAEPSHA256"); }
        set { _config.SetString("DefaultRsaPaddingName", value); }
    }

    public bool UseCAPIByDefault
    {
        get { return _config.GetBool("UseCAPIByDefault", true); }
        set { _config.SetBool("UseCAPIByDefault", value); }
    }

    public bool AllowUnsecureDeletion
    {
        get { return _config.GetBool("AllowUnsecureDeletion", false); }
        set { _config.SetBool("AllowUnsecureDeletion", value); }
    }
    
}
