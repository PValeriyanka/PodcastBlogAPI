using Microsoft.AspNetCore.Mvc;

namespace PodcastBlog.Server.Controllers
{
    [ApiController]
    [Route("error")]
    public class ErrorController : ControllerBase
    {
        [HttpGet]
        public IActionResult HandleError()
        {
            return Problem("Произошла ошибка на сервере.");
        }
    }
}