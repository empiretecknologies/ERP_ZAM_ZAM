using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class EmpRegistrationController : BaseController
    {
        public IEmpRegistrationService _empRegistrationService { get; set; }
        private readonly IWebHostEnvironment _hostingEnvironment;
        public EmpRegistrationController(IEmpRegistrationService empRegistrationService, IMenuService menuService, IWebHostEnvironment hostingEnvironment, IBaseService baseService) : base(menuService, baseService)
        {
            _empRegistrationService = empRegistrationService;
            _hostingEnvironment = hostingEnvironment;
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
                var data = _empRegistrationService.QuickSearch(CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public JsonResult GetEmpRegistrationById(int id)
        {
            var data = _empRegistrationService.GetEmpRegistrationById(id, CommonHelper.GetValues(HttpContext));
            return Json(data);
        }

        [HttpPost]
        public JsonResult Save(EmpRegistration model)
        {
            try
            {
                var data = _empRegistrationService.Save(model, CommonHelper.GetValues(HttpContext));
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
                var data = _empRegistrationService.Delete(id, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public JsonResult GetDepartments()
        {
            try
            {
                var data = DropdownService.EmpDepartmentDropdown();
                return Json(new { data = data, msgType = 1 });
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

        [HttpGet]
        public JsonResult GetDesignations()
        {
            try
            {
                var data = DropdownService.DesignationDropdown();
                return Json(new { data = data, msgType = 1 });
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

        [HttpPost]
        public async Task<IActionResult> SaveImage()
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            var uniqueFileName = "";
            try
            {
                IFormFile Image = Request.Form.Files[0];
                if (Image != null && Image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images", "upload", "empregistration");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string extension = Path.GetExtension(Image.FileName);
                    uniqueFileName = Guid.NewGuid().ToString().Substring(0, 25) + extension;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }
                }
                response.msg = "File uploaded successfully.";
                response.msgType = 1;
                response.data = uniqueFileName;
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                response.msg = _catchMessage;
                response.msgType = 2;
            }
            return Json(response);
        }
    }
}
