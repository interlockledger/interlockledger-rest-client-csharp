using System.IO;
using InterlockLedger.Rest.Client.Abstractions;

namespace InterlockLedger.Rest.Client.V3_2
{
    public interface IMultiDocumentApp
    {
        bool AddItem(string transactionId, string name, string comment, string contentType, Stream source);

        bool AddItem(string transactionId, FileInfo file, string comment, string contentType)
            => AddItem(transactionId, file.Name, comment, contentType, file.OpenRead());

        MultiDocumentTransactionModel BeginTransaction(MultiDocumentBeginTransactionModel transactionStart);

        string CommitTransaction(string transactionId);

        FileInfo RetrieveBlob(string multiDocumentStorageLocator, DirectoryInfo folderToStore);

        MultiDocumentMetadataModel RetrieveMetadata(string multiDocumentStorageLocator);

        FileInfo RetrieveSingle(string multiDocumentStorageLocator, int index, DirectoryInfo folderToStore);
    }

    public class RestChain : RestAbstractChain, IMultiDocumentApp
    {
        internal RestChain(RestNode rest, ChainIdModel chainId) : base(rest, chainId) {
        }

        bool IMultiDocumentApp.AddItem(string transactionId, string name, string comment, string contentType, Stream source) {
            throw new System.NotImplementedException();
        }

        MultiDocumentTransactionModel IMultiDocumentApp.BeginTransaction(MultiDocumentBeginTransactionModel transactionStart) {
            return _rest.Get<MultiDocumentTransactionModel>();
        }

        string IMultiDocumentApp.CommitTransaction(string transactionId)
            => _rest.Get<string>($"/commit/{transactionId}");

        FileInfo IMultiDocumentApp.RetrieveBlob(string multiDocumentStorageLocator, DirectoryInfo folderToStore) {
            return _rest.GetFile(folderToStore, null/* How to build the request properly */);
        }

        MultiDocumentMetadataModel IMultiDocumentApp.RetrieveMetadata(string multiDocumentStorageLocator) {
            throw new System.NotImplementedException();
        }

        FileInfo IMultiDocumentApp.RetrieveSingle(string multiDocumentStorageLocator, int index, DirectoryInfo folderToStore) {
            throw new System.NotImplementedException();
        }
    }
}