using System;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.X509Store;

namespace ConsoleAppPkcs
{
    class Program
    {
        static void Main(string[] args)
        {
            // Specify the path to unmanaged PKCS#11 library provided by the cryptographic device vendor
            string pkcs11LibraryPath = @"C:\Program Files\Yubico\Yubico PIV Tool\bin\libykcs11.dll";

            // Create factories used by Pkcs11Interop library
            Pkcs11InteropFactories factories = new Pkcs11InteropFactories();

            // Load unmanaged PKCS#11 library
            using (IPkcs11Library pkcs11Library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories, pkcs11LibraryPath, AppType.MultiThreaded))
            {
                // Show general information about loaded library
                ILibraryInfo libraryInfo = pkcs11Library.GetInfo();

                Console.WriteLine("Library");
                Console.WriteLine("  Manufacturer:       " + libraryInfo.ManufacturerId);
                Console.WriteLine("  Description:        " + libraryInfo.LibraryDescription);
                Console.WriteLine("  Version:            " + libraryInfo.LibraryVersion);

                // Get list of all available slots
                foreach (ISlot slot in pkcs11Library.GetSlotList(SlotsType.WithOrWithoutTokenPresent))
                {
                    // Show basic information about slot
                    ISlotInfo slotInfo = slot.GetSlotInfo();

                    Console.WriteLine();
                    Console.WriteLine("Slot");
                    Console.WriteLine("  Manufacturer:       " + slotInfo.ManufacturerId);
                    Console.WriteLine("  Description:        " + slotInfo.SlotDescription);
                    Console.WriteLine("  Token present:      " + slotInfo.SlotFlags.TokenPresent);

                    if (slotInfo.SlotFlags.TokenPresent)
                    {
                        // Show basic information about token present in the slot
                        ITokenInfo tokenInfo = slot.GetTokenInfo();

                        Console.WriteLine("Token");
                        Console.WriteLine("  Manufacturer:       " + tokenInfo.ManufacturerId);
                        Console.WriteLine("  Model:              " + tokenInfo.Model);
                        Console.WriteLine("  Serial number:      " + tokenInfo.SerialNumber);
                        Console.WriteLine("  Label:              " + tokenInfo.Label);


                        // Show list of mechanisms (algorithms) supported by the token
                        Console.WriteLine("Supported mechanisms: ");
                        foreach (CKM mechanism in slot.GetMechanismList())
                            Console.WriteLine("  " + mechanism);
                    }
                }
            }
            Console.ReadLine();
            var pinProvider = new OurPinProvider();
            var store = new Pkcs11X509Store(@"C:\Program Files\Yubico\Yubico PIV Tool\bin\libykcs11.dll", pinProvider);
            var info = store.Info;
            var slots = store.Slots;

            foreach (var s in slots)
            {
                var s2 = s.Info;
                var tokeninfo = s.Token.Info;
                var HasProtectedAuthenticationPath = s.Token.Info.HasProtectedAuthenticationPath;
                var tokencerts = s.Token.Certificates;

                Console.WriteLine("Info.Manufacturer: " + s.Info.Manufacturer);
                Console.WriteLine("Info.Description: " + s.Info.Description);
                Console.WriteLine("Token.Info.Label: " + s.Token.Info.Label);
                Console.WriteLine("Token.Info.SerialNumber: " + s.Token.Info.SerialNumber);
                Console.WriteLine("Token.Info.Model: " + s.Token.Info.Model);
                Console.WriteLine("Token.Info.Manufacturer: " + s.Token.Info.Manufacturer);
                Console.WriteLine("Token.Info.Initialized: " + s.Token.Info.Initialized);

                foreach (Pkcs11X509Certificate t in tokencerts)
                {
                    Console.WriteLine("Label: " + t.Info.Label);
                    Console.WriteLine("Id: " + t.Info.Id);
                    Console.WriteLine("KeyType: " + t.Info.KeyType);
                    var cert = t.Info.ParsedCertificate;
                    Console.WriteLine("cert.Subject: " + cert.Subject);
                    Console.WriteLine("cert.Thumbprint: " + cert.Thumbprint);

                    //  var rsa = t.GetRSAPublicKey(); ;
                }


            }
            Console.ReadLine();
        }
    }
}
