using System.Collections.Generic;
using System.Linq;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Dapper;
using System;

namespace Ecm.CaptureDAO
{
    public class BarcodeConfigurationDao
    {
        private readonly DapperContext _context;

        public BarcodeConfigurationDao(DapperContext context)
        {
            _context = context;
        }

        public void SetBarcodeConfiguration(string xml, Guid batchTypeId)
        {
            const string query = @"UPDATE [BatchType] 
                                   SET
                                   [BarcodeConfigurationXml] = @Xml
                                   WHERE Id = @Id";
            _context.Connection.Execute(query,
                                        new
                                            {
                                                Xml = xml,
                                                Id = batchTypeId
                                            },
                                        _context.CurrentTransaction);
        }
    }
}