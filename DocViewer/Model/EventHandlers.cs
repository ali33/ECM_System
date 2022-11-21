using System;

namespace Ecm.DocViewer.Model
{
    public delegate void CollapsedEventHandler(object sender, bool collasped);

    public delegate void SaveAllEventHandler();

    public delegate void SaveEventHandler(ContentItem batchItem);

    public delegate void DeleteBatchEventHandler(ContentItem batchItem);

    public delegate void DeleteDocumentEventHandler(ContentItem documentItem);

    public delegate void OpenLinkDocumentEvent(Guid docId);
}