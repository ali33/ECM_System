namespace Ecm.LookupService
{
    public class LookupServices
    {
        public LookupServices(string dataProvider)
        {
            _dataProvider = dataProvider;
            _factory = DaoFactories.GetFactory(_dataProvider);
        }

        public ILookupDao LookupDao
        {
            get
            {
                return _factory.LookupDao;
            }
        }

        private readonly string _dataProvider;
        private readonly DaoFactory _factory;
    }
}
