using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class APKController : ControllerBase
    {
        [HttpGet("download")]
        public IActionResult DownloadApk()
        {
            var apkPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "skincede.apk");
            if (!System.IO.File.Exists(apkPath))
            {
                return NotFound("APK file not found.");
            }
            var stream = new FileStream(apkPath, FileMode.Open, FileAccess.Read);
            return File(
                stream,
                "application/vnd.android.package-archive",
                "skincede.apk"
            );
        }

    }
}
