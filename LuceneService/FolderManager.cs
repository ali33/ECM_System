using System.Configuration;
using System.IO;
using Ecm.Domain;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using System;

namespace Ecm.LuceneService
{
    public class FolderManager
    {
        public string RootIndexFolder
        {
            get { return ConfigurationManager.AppSettings["LuceneIndexRootDir"]; }
        }

        public Directory GetDirectory(string uniqueId)
        {
            string indexFolder = Path.Combine(RootIndexFolder, uniqueId);
            return FSDirectory.Open(new DirectoryInfo(indexFolder));
        }

        public string GetDirectoryPath(string uniqueId)
        {
            return Path.Combine(RootIndexFolder, uniqueId);
        }

        public Directory GetDirectory(Guid uniqueId)
        {
            string indexFolder = Path.Combine(RootIndexFolder, uniqueId.ToString());
            return FSDirectory.Open(new DirectoryInfo(indexFolder));
        }

        public string GetDirectoryPath(Guid uniqueId)
        {
            return Path.Combine(RootIndexFolder, uniqueId.ToString());
        }
    }
}
