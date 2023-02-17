
# Cryptec

Cryptec is a simple Windows CMD application made to quickly and easily encrypt/decrypt files.

It also supports recursively encrypting/decrypting folders in one command.


## Usage / Examples

### Drag/drop
You can drag/drop files and folders onto the exe to begin the process of encryption/decryption.

### Launch arguments
When opening Cryptec in a folder, you can call 'cryptec here' to make cryptec automatically begin encrypting/decrypting every file recursively in the working folder.

Of course, you can use 'cryptec [path of file or directory]' to make Cryptec focus on a single file or directory.

Example: `cryptec here` or `cryptec secret-file.txt`

### Mixed folders
In the case of a folder mixed with encrypted/unencrypted files, Cryptec automatically recognizes which files need to be decrypted or encrypted.

If there is more than one password across the files, files which don't match the supplied password will simply not decrypt.