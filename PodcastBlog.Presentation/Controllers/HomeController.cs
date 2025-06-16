using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodcastBlog.Domain.Models;

namespace PodcastBlog.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class HomeController : ControllerBase 
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Index")] 
        public IActionResult Index()
        {
            return Ok("This is the Index page of Home API controller.");
        }

        [HttpGet("Privacy")] 
        public IActionResult Privacy()
        {
            return Ok("This is the Privacy page of Home API controller.");
        }

        [HttpGet("Error")] 
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult<ErrorViewModel> Error()
        {
            var errorModel = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            return Ok(errorModel);
        }
    }
}