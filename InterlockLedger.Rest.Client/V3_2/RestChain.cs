using System.IO;
using System.Web;
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
        bool IMultiDocumentApp.AddItem(string transactionId, string name, string comment, string contentType, Stream source)
            => _rest.PostStream($"/multiDocuments/add/{transactionId}?name={HttpUtility.UrlEncode(name)}&comment={HttpUtility.UrlEncode(comment)}", source, contentType);

        MultiDocumentTransactionModel IMultiDocumentApp.BeginTransaction(MultiDocumentBeginTransactionModel transactionStart)
            => _rest.Post<MultiDocumentTransactionModel>("/multiDocuments/begin", transactionStart);

        string IMultiDocumentApp.CommitTransaction(string transactionId)
            => _rest.Post<string>($"/multiDocuments/commit/{transactionId}", null);

        FileInfo IMultiDocumentApp.RetrieveBlob(string multiDocumentStorageLocator, DirectoryInfo folderToStore)
            => _rest.GetFile(folderToStore, $"/multiDocuments/{multiDocumentStorageLocator}/zip", "application/zip");

        MultiDocumentMetadataModel IMultiDocumentApp.RetrieveMetadata(string multiDocumentStorageLocator)
            => _rest.Get<MultiDocumentMetadataModel>($"/multiDocuments/{multiDocumentStorageLocator}");

        FileInfo IMultiDocumentApp.RetrieveSingle(string multiDocumentStorageLocator, int index, DirectoryInfo folderToStore)
            => _rest.GetFile(folderToStore, $"/multiDocuments/{multiDocumentStorageLocator}/{index}", " */*");

        internal RestChain(RestNode rest, ChainIdModel chainId) : base(rest, chainId) {
        }
    }
}