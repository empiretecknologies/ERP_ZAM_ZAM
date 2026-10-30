using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Empire_ERP.Core.Services
{
    public class DepartmentsService : IDepartmentsService
    {
        public IDepartmentsRepository _departmentsRepository { get; set; }
        public DepartmentsService(IDepartmentsRepository departmentsRepository)
        {
            _departmentsRepository = departmentsRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            return _departmentsRepository.QuickSearch(common);
        }

        public MyHttpResponseMessage Save(Departments model, Common common)
        {
            return _departmentsRepository.Save(model, common);
        }

        public string GenerateNextId(Common common)
        {
            return _departmentsRepository.GenerateNextId(common);
        }

        public MyHttpResponseMessage GetDepartmentsById(int id, Common common)
        {
            return _departmentsRepository.GetDepartmentsById(id, common);
        }

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            return _departmentsRepository.Delete(id, common);
        }
    }
}
