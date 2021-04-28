using cloudscribe.Web.Navigation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavigationDemo.Web.Areas.Area51.Controllers
{
    [Area("Area51")]
    [NavNodeController(KeyPrefix = "Area51")]
    public class ReflectionController : NavigationDemo.Web.Controllers.ReflectionController
    {
    }
}
