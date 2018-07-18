using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace asdf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassportController : ControllerBase
    {
        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            if (id != 1) throw new Exception("无效用户");
            return new JsonResult(new { msg = "ok", data = "asdf" });
        }
    }
}
