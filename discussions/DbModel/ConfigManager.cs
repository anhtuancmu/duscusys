using Microsoft.Win32;

namespace Discussions
{
    public class ConfigManager
    {
        public static string ConnStr
        {
            get
            {
                RegistryKey k = getDiscusysKey();
                if (k != null)
                    return (string) getDiscusysKey().GetValue("DbConnStr");
                else
                    return
                        "metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=\"Data Source=tcp:123.108.5.30,8080;Initial Catalog=disc;user id=sa;password=sa\"";
            }
        }

        public static string PhotonSrv
        {
            get
            {
                RegistryKey k = getDiscusysKey();
                if (k != null)
                    return (string) getDiscusysKey().GetValue("PhotonServer");
                else
                    return "123.108.5.30:5055";
            }
        }

        public static string ServiceServer
        {
            get { return PhotonSrv.Substring(0, PhotonSrv.Length - 5); }
        }

        private static RegistryKey hkDiscusystem = null;

        private static RegistryKey getDiscusysKey()
        {
            if (hkDiscusystem == null)
            {
                RegistryKey hklm = Registry.LocalMachine;
                RegistryKey hkSoftware = hklm.OpenSubKey("Software");
                RegistryKey hkTohokuUniv = hkSoftware.OpenSubKey("Tohoku University");
                if (hkTohokuUniv == null)
                {
                    RegistryKey wowNode = hkSoftware.OpenSubKey("Wow6432Node");
                    hkTohokuUniv = wowNode.OpenSubKey("Tohoku University");
                }
                hkDiscusystem = hkTohokuUniv.OpenSubKey("Discusystem");
            }
            return hkDiscusystem;
        }
    }
}