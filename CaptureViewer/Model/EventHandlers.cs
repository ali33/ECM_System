namespace Ecm.CaptureViewer.Model
{
    public delegate void CollapsedEventHandler(object sender, bool collasped);

    public delegate void SaveAllEventHandler();

    public delegate void SaveEventHandler(ContentItem batchItem);

    public delegate void ApproveEventHandler(ContentItem batchItem);

    public delegate void ApproveAllEventHandler();

    public delegate void RejectEventHandler(ContentItem batchItem);

    public delegate void DeleteBatchEventHandler(ContentItem batchItem);

    public delegate void DeleteDocumentEventHandler(ContentItem documentItem);

    public delegate void SubmitBatchEventHandler();
}