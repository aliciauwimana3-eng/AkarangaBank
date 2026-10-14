using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AkarangaBank.Web.Controllers;

namespace AkarangaBank.Web.Tests.UnitTests;

public class CultureControllerTests
{
    [Fact]
    public void SetLanguage_EnregistreCookieCultureEtRetourneVersUrlLocale()
    {
        var controller = new CultureController
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = controller.SetLanguage("rn-BI", "/Clients");

        Assert.IsType<LocalRedirectResult>(result);
        Assert.Contains(".AspNetCore.Culture", controller.Response.Headers.SetCookie.ToString());
        Assert.Contains("rn-BI", controller.Response.Headers.SetCookie.ToString());
        Assert.Contains("expires=", controller.Response.Headers.SetCookie.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SetLanguage_RefuseCultureNonSupportee()
    {
        var controller = new CultureController
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        Assert.IsType<BadRequestObjectResult>(controller.SetLanguage("xx-XX", "/"));
    }
}
