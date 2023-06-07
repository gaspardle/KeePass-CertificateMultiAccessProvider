using KeePass.App.Configuration;

namespace CertificateMultiAccessProvider;

public class Settings
{
    private readonly AceCustomConfig _config;
    public const string ConfigPrefix = "CertMultiAccess";

    public Settings(AceCustomConfig config)
    {
        _config = config;
    }

    public string Pkcs11LibPath
    {
        get { return _config.GetString($"{ConfigPrefix}_Pkcs11LibPath"); }
        set { _config.SetString($"{ConfigPrefix}_Pkcs11LibPath", value); }
    }

    public string DefaultRsaPaddingName
    {
        get { return _config.GetString($"{ConfigPrefix}_DefaultRsaPaddingName", "OAEPSHA256"); }
        set { _config.SetString($"{ConfigPrefix}_DefaultRsaPaddingName", value); }
    }

    public bool UseCAPIByDefault
    {
        get { return _config.GetBool($"{ConfigPrefix}_UseCAPIByDefault", true); }
        set { _config.SetBool($"{ConfigPrefix}_UseCAPIByDefault", value); }
    }

    public bool AllowUnsecureDeletion
    {
        get { return _config.GetBool($"{ConfigPrefix}_AllowUnsecureDeletion", false); }
        set { _config.SetBool($"{ConfigPrefix}_AllowUnsecureDeletion", value); }
    }

    public bool AllowKeyExport
    {
        get { return _config.GetBool($"{ConfigPrefix}_AllowKeyExport", true); }
        set { _config.SetBool($"{ConfigPrefix}_AllowKeyExport", value); }
    }

    public bool AllowConfigurationRemoval
    {
        get { return _config.GetBool($"{ConfigPrefix}_AllowConfigurationRemoval", false); }
        set { _config.SetBool($"{ConfigPrefix}_AllowConfigurationRemoval", value); }
    }

}
