using System.Collections.Generic;
using System.Security;
using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;
using Ecm.Utility;

namespace Ecm.CaptureCore
{
    public class PageManager : ManagerBase
    {
        private Setting _setting = new Setting();

        public PageManager(User loginUser)
            : base(loginUser)
        {
            _setting = new SettingManager(loginUser).GetSettings();
        }

        /// <summary>
        /// Get list Pages by Document
        /// </summary>
        /// <param name="docId">Id of document</param>
        /// <returns></returns>
        public List<Page> GetPagesByDocId(Guid docId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                var pageDao = new PageDao(dataContext);
                var annoDao = new AnnotationDao(dataContext);

                var pages = pageDao.GetByDoc(docId);
                // Get annotation
                foreach (var page in pages)
                {
                    if (_setting.IsSaveFileInFolder)
                    {
                        page.FileBinary = FileHelpper.ReadFile(page.FilePath, page.FileHeader);
                    }

                    page.Annotations = annoDao.GetByPage(page.Id);

                    foreach (var anno in page.Annotations)
                    {
                        anno.Content = UtilsSerializer.Serialize(anno.Content);
                    }
                }

                return pages;
            }
        }

    }
}
