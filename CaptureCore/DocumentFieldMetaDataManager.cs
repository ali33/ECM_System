using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Ecm.LookupDomain;
using Ecm.Utility;
using System;
using System.Collections.Generic;

namespace Ecm.CaptureCore
{
    public class DocumentFieldMetaDataManager : ManagerBase
    {

        public DocumentFieldMetaDataManager(User loginUser)
            : base(loginUser)
        {
        }

        #region Public methods

        public List<DocumentFieldMetaData> GetByDocumentType(Guid docTypeId)
        {
            using (DapperContext context = new DapperContext(LoginUser))
            {
                var dfmdDao = new DocFieldMetaDataDao(context);
                var plDao = new PicklistDao(context);
                var fields = dfmdDao.GetByDocType(docTypeId);

                foreach (var field in fields)
                {
                    if (field.DataType == "Table")
                    {
                        field.Children = dfmdDao.GetChildren(field.Id);
                    }

                    if (field.DataTypeEnum == FieldDataType.Picklist)
                    {
                        field.Picklists = plDao.GetByField(field.Id);
                    }

                    if (field.IsLookup && !string.IsNullOrEmpty(field.LookupXml))
                    {
                        field.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(field.LookupXml);
                    }

                }

                return fields;
            }
        }

        #endregion
    }
}
