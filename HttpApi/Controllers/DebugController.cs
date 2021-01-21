using HttpApi.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HttpApi.Controllers {
    [ServiceFilter(typeof(RequireManagerAttribute))]
    public class DebugController:Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
