using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.CaptureDomain;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDAO;

namespace Ecm.CaptureCore
{
    public class CustomActivitySettingManager : ManagerBase
    {
        public CustomActivitySettingManager(User loginUser)
            : base(loginUser)
        {
            
        }

        public CustomActivitySetting GetCustomActivitySetting(Guid wfDefinitionId, Guid activityId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                CustomActivitySettingDao customActivityDao = new CustomActivitySettingDao(dataContext);
                return customActivityDao.GetCustomActivitySetting(wfDefinitionId, activityId);
            }
        }

        public List<CustomActivitySetting> GetCustomActivitySettings(Guid wfDefinitionId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser.CaptureConnectionString))
            {
                CustomActivitySettingDao customActivityDao = new CustomActivitySettingDao(dataContext);
                return customActivityDao.GetCustomActivitySettingByWorkflow(wfDefinitionId);
            }
        }
    }
}
