using System;
using System.Collections.Generic;
using System.Linq;
using asdf.Models;
using Microsoft.AspNetCore.Mvc;

namespace asdf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CMSController : ControllerApiBase
    {
        private IList<CMS> DATA;

        public CMSController()
        {
            DATA = new List<CMS>();
            for (var i = 1; i < 100; i++)
            {
                DATA.Add(new CMS()
                {
                    id = i,
                    title = $"title {i}",
                    content = $"content {i}",
                });
            }
        }

        [HttpGet("")]
        public JsonResult Get()
        {
            return Output(DATA);
        }

        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            var item = DATA.FirstOrDefault(w => w.id == id);
            if (item == null)
                throw new Exception("无效编码");
            return Output(item);
        }
    }
}