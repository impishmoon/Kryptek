// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using System.Text;

void Encrypt(string password, FileStream originalFileStream, FileStream tempFileStream)
{
    using (SymmetricAlgorithm crypt = Aes.Create())
    using (HashAlgorithm hash = SHA256.Create())
    {
        crypt.Key = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
        // This is really only needed before you call CreateEncryptor the second time,
        // since it starts out random.  But it's here just to show it exists.
        crypt.GenerateIV();

        crypt.Padding = PaddingMode.ANSIX923;

        for (int i = 0; i < 16; i++)
        {
            tempFileStream.WriteByte(crypt.IV[i]);
        }

        using (CryptoStream cryptoStream = new CryptoStream(
            originalFileStream, crypt.CreateEncryptor(), CryptoStreamMode.Read))
        {
            cryptoStream.CopyTo(tempFileStream);
        }
    }
}

bool Decrypt(string password, FileStream originalFileStream, FileStream tempFileStream)
{
    using (SymmetricAlgorithm crypt = Aes.Create())
    using (HashAlgorithm hash = SHA256.Create())
    {
        crypt.Key = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
        var iv = new byte[16];
        originalFileStream.Read(iv, 0, 16);
        crypt.IV = iv;

        crypt.Padding = PaddingMode.ANSIX923;

        using (CryptoStream cryptoStream = new CryptoStream(
            originalFileStream, crypt.CreateDecryptor(), CryptoStreamMode.Read))
        {
            try
            {
                cryptoStream.CopyTo(tempFileStream);
            }
            catch
            {
                return false;
            }
        }
    }

    return true;
}

void EncryptTarget(string targetPath, string password)
{
    var tempPath = targetPath + ".tmp";

    using (var originalStream = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
    using (var tempStream = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
    {
        File.SetAttributes(tempPath, FileAttributes.Hidden & FileAttributes.Temporary);

        Encrypt(password, originalStream, tempStream);
    }

    var encryptedPath = targetPath + ".enc";

    File.Delete(targetPath);
    File.Move(tempPath, encryptedPath);

    File.SetAttributes(encryptedPath, 0);

    Console.WriteLine($"Encrypted '{Path.GetFileName(targetPath)}'");
}

void DecryptTarget(string targetPath, string password)
{
    var tempPath = targetPath + ".tmp";

    bool decryptSuccess = true;

    using (var originalStream = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
    using (var tempStream = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
    {
        File.SetAttributes(tempPath, FileAttributes.Hidden & FileAttributes.Temporary);

        decryptSuccess = Decrypt(password, originalStream, tempStream);
    }

    if (decryptSuccess)
    {
        var finalPath = targetPath.Substring(0, targetPath.Length - 4);

        File.Delete(targetPath);
        File.Move(tempPath, finalPath);

        File.SetAttributes(finalPath, 0);

        Console.WriteLine($"Decrypted '{Path.GetFileName(targetPath)}'");
    }
    else
    {
        File.Delete(tempPath);

        Console.WriteLine($"Failed to decrypt '{Path.GetFileName(targetPath)}' - wrong password?");
    }
}

if (args.Length == 0)
{
    Console.WriteLine("- There are currently no launch arguments for cryptec");
    Console.WriteLine("- To use, drag your target file/folder onto cryptec and provide a password");
    Console.WriteLine("- To decrypt, target any file that ends with '.enc'");
    Console.WriteLine("- Additionally, you can simply execute 'cryptec here' in any folder and cryptec will recursively encrypt all files in the folder you launched it from.");
    Console.WriteLine("- When providing a folder target, cryptec will encrypt/decrypt each file in the folder recursively based on the individual file's encryption status (does it end with .enc or not?)");
}
else if(args.Length == 1)
{
    var targetPath = args[0].Replace(@"\", "/");

    if(targetPath == "here")
    {
        targetPath = Environment.CurrentDirectory;
    }

    FileAttributes attributes = 0;
    try
    {
        attributes = File.GetAttributes(targetPath);
    }
    catch { }

    if(attributes != 0)
    {
        Console.Write("Please enter passphrase: ");
        var password = Console.ReadLine();

        if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
        {
            //is a directory
            foreach(string filePath in Directory.EnumerateFiles(targetPath, "*.*", SearchOption.AllDirectories))
            {
                if (filePath.EndsWith(".enc"))
                {
                    DecryptTarget(filePath, password);
                }
                else
                {
                    EncryptTarget(filePath, password);
                }
            }
        }
        else
        {
            //is a file
            if (targetPath.EndsWith(".enc"))
            {
                DecryptTarget(targetPath, password);
            }
            else
            {
                EncryptTarget(targetPath, password);
            }
        }
    }
}
