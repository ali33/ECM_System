using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.CaptureCore
{
    public class PagingHelper : ManagerBase
    {
        private Setting _setting;
        private const string KEY_ITEMS_PER_PAGE = "SearchResultPageSize";
        private const int MAX_ITEMS_PER_PAGE = 50;

        public PagingHelper(User loginUser)
            : base(loginUser)
        {
            _setting = new SettingManager(loginUser).GetSettings();
        }

        /// <summary>
        /// Paging source for MVC version
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="totalPages"></param>
        /// <returns></returns>
        public List<T> PagingMvc<T>(List<T> source, ref int pageIndex, out int totalPages)
        {
            string stringItemPerPage;

            using (var dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                // Get the item per page configure
                stringItemPerPage = new SettingDao(dataContext).Get(KEY_ITEMS_PER_PAGE).Value;

            }

            int itemPerPage = 0;
            if (null != stringItemPerPage)
            {
                int.TryParse(stringItemPerPage, out itemPerPage);
            }
            if (1 > itemPerPage || 50 < itemPerPage)
            {
                itemPerPage = Constants.MAX_ITEMS_PER_PAGE;
            }

            var totalCount = source.Count;
            totalPages = totalCount / itemPerPage;
            if (0 != totalCount % itemPerPage)
            {
                totalPages++;
            }

            // Set default page index
            if (1 > pageIndex)
            {
                pageIndex = 1;
            }
            if (totalPages < pageIndex)
            {
                pageIndex = totalPages;
            }

            return source.Take(pageIndex * itemPerPage).ToList();
        }

        /// <summary>
        /// Paging source for Mobile version
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="totalPages"></param>
        /// <remarks>totalPages = -1: take all source, totalPages = -2: take all source,</remarks>
        /// <returns></returns>
        public List<T> PagingMobile<T>(List<T> source, int pageIndex, int itemPerPage)
        {
            if (pageIndex  < 0)
            {
                throw new ArgumentException("Invalid page index.");
            }

            if (0 > itemPerPage || 50 < itemPerPage)
            {
                itemPerPage = Constants.MAX_ITEMS_PER_PAGE;
            }

            var totalCount = source.Count;
            var totalPages = totalCount / itemPerPage;
            if (0 != totalCount % itemPerPage)
            {
                totalPages++;
            }

            if (pageIndex > totalPages)
            {
                return new List<T>();
            }

            return source.Skip((pageIndex - 1) * itemPerPage).Take(itemPerPage).ToList();
        }
    }
}
