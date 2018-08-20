using System;
using System.Collections.Generic;
using System.Linq;
using asdf.Models;
using Microsoft.AspNetCore.Mvc;

namespace asdf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerApiBase
    {
        [HttpGet("")]
        public JsonResult Get()
        {
            return Output(new App
            {
                project = new Project()
                {
                    name = "ng-alain"
                },
                menu = new List<Menu>()
                {
                    new Menu()
                    {
                        text = "主导航",
                        group = true,
                        children = new List<Menu>()
                        {
                            new Menu()
                            {
                                text = "仪表盘",
                                link = "/dashboard",
                                icon = "anticon anticon-appstore-o"
                            },
                            new Menu()
                            {
                                text = "快捷菜单",
                                icon = "anticon anticon-rocket",
                                shortcut_root = true
                            }
                        }
                    },
                    new Menu()
                    {
                        text = "业务",
                        group = true,
                        children = new List<Menu>()
                        {
                            new Menu()
                            {
                                text = "CMS",
                                icon = "anticon anticon-skin",
                                children = new List<Menu>()
                                {
                                    new Menu()
                                    {
                                        text = "CMS列表",
                                        link = "/cms/list"
                                    },
                                    new Menu()
                                    {
                                        text = "模块列表",
                                        link = "/cms/module"
                                    }
                                }
                            }
                        }
                    }
                },
                user = new User()
                {
                    id = 1,
                    name = "cipchk"
                }
            });
        }
    }
}