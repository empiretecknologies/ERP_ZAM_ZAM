using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class EmpManualAttController : BaseController
    {
        public IEmpManualAttService _empManualAttService { get; set; }
        public EmpManualAttController(IEmpManualAttService empManualAttService, IMenuService menuService, IBaseService baseService) : base(menuService, baseService)
        {
            _empManualAttService = empManualAttService;
        }

        public IActionResult Index()
        {
            ViewBag.Permissions = CommonHelper.GetValues(HttpContext).RoleType == "A"
                ? "Admin"
                : CommonHelper.GetPermissionByMenueID(CommonHelper.GetValues(HttpContext).RoleID, CommonHelper.GetValues(HttpContext).MenuID);
            return View();
        }

        [HttpGet]
        public JsonResult QuickSearch()
        {
            try
            {
                var data = _empManualAttService.QuickSearch(CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public JsonResult GetEmpManualAttById(int id)
        {
            var data = _empManualAttService.GetEmpManualAttById(id, CommonHelper.GetValues(HttpContext));
            return Json(data);
        }

        [HttpPost]
        public JsonResult Save(EmpManualAtt model)
        {
            try
            {
                var data = _empManualAttService.Save(model, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var data = _empManualAttService.Delete(id, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public JsonResult GetEmployees()
        {
            try
            {
                List<object> dropdown = new List<object>();
                using (SqlConnection conn = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT ID, LTRIM(RTRIM(ISNULL(FIRST_NAME,'') + ' ' + ISNULL(LAST_NAME,''))) AS EMPLOYEE_NAME, MACHINE_ID " +
                                   "FROM TBL_EMP_REG WHERE DLT = 'T' AND ASTATUS = 'Y'";
                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dropdown.Add(new
                                {
                                    key = Convert.ToInt32(reader["ID"]),
                                    value = Convert.ToString(reader["EMPLOYEE_NAME"]),
                                    machineId = reader["MACHINE_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MACHINE_ID"])
                                });
                            }
                        }
                    }
                }
                return Json(new { data = dropdown, msgType = 1 });
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(new { data = _catchMessage, msgType = 2 });
            }
        }
    }
}
