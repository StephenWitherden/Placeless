using System;
using System.IO;
using System.Threading.Tasks;

namespace Placeless.BlobStore.FileSystem
{
    public class FileSystemBlobStore : IBlobStore
    {
        public static string BLOB_ROOT_PATH = "Blob:Root";

        private readonly IPlacelessconfig _config;
        private readonly string _root;
        public FileSystemBlobStore(IPlacelessconfig config)
        {
            _config = config;
            _root = _config.GetValue(BLOB_ROOT_PATH);
        }

        private string GetPath(int id)
        {
            // 002/147/483/647
            // make 9-digit string, pad left with 0
            string s = id.ToString().PadLeft(12, '0');

            // insert backslashes
            string path= s.Substring(0, 3) + "\\" +
                        s.Substring(3, 3) + "\\" +
                        s.Substring(6, 3) + "\\" +
                        s.Substring(9, 3);

            return Path.Combine(_root, path);
        }

        public Stream Get(int id)
        {
            return System.IO.File.OpenRead(GetPath(id));
        }

        public async Task PutAsync(Stream contents, int id)
        {
            string path = GetPath(id);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            using (var stream = System.IO.File.OpenWrite(path))
            {
                await contents.CopyToAsync(stream);
            }
        }
    }
}
