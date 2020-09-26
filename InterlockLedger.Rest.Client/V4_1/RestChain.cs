using System.IO;
using System.Web;
using InterlockLedger.Rest.Client.Abstractions;

namespace InterlockLedger.Rest.Client.V4_1
{
    public interface IDocumentsApp
    {
        FileInfo RetrieveBlob(string locator, DirectoryInfo folderToStore);

        DocumentsMetadataModel RetrieveMetadata(string locator);

        FileInfo RetrieveSingle(string locator, int index, DirectoryInfo folderToStore);

        bool TransactionAddItem(string transactionId, string name, string comment, string contentType, Stream source);

        bool TransactionAddItem(string transactionId, FileInfo file, string comment, string contentType)
            => TransactionAddItem(transactionId, file.Name, comment, contentType, file.OpenRead());

        DocumentsTransactionModel TransactionBegin(DocumentsBeginTransactionModel transactionStart);

        string TransactionCommit(string transactionId);

        DocumentsTransactionModel TransactionStatus(string transactionId);
    }

    public class RestChain : RestAbstractChain, IDocumentsApp
    {
        FileInfo IDocumentsApp.RetrieveBlob(string locator, DirectoryInfo folderToStore)
            => _rest.GetFile(FromLocator(locator, "zip"), accept: "application/zip", folderToStore: folderToStore);

        DocumentsMetadataModel IDocumentsApp.RetrieveMetadata(string locator)
            => _rest.Get<DocumentsMetadataModel>(FromLocator(locator, "metadata"));

        FileInfo IDocumentsApp.RetrieveSingle(string locator, int index, DirectoryInfo folderToStore)
            => _rest.GetFile(FromLocator(locator, index), accept: "*/*", folderToStore: folderToStore);

        bool IDocumentsApp.TransactionAddItem(string transactionId, string name, string comment, string contentType, Stream source)
            => _rest.PostStream($"/documents/transaction/{transactionId}?name={HttpUtility.UrlEncode(name)}&comment={HttpUtility.UrlEncode(comment)}", source, contentType);

        DocumentsTransactionModel IDocumentsApp.TransactionBegin(DocumentsBeginTransactionModel transactionStart)
            => _rest.Post<DocumentsTransactionModel>("/documents/transaction", transactionStart);

        string IDocumentsApp.TransactionCommit(string transactionId)
            => _rest.Post<string>($"/documents/transaction/{transactionId}/commit", null);

        DocumentsTransactionModel IDocumentsApp.TransactionStatus(string transactionId)
            => _rest.Get<DocumentsTransactionModel>($"/documents/transaction/{transactionId}");

        internal RestChain(RestNode rest, ChainIdModel chainId) : base(rest, chainId) {
        }

        private static string FromLocator<T>(string locator, T selector) => $"/documents/{HttpUtility.UrlEncode(locator)}/{selector}";
    }
}