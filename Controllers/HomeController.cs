using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Azure.Core;
using Azure.Storage.Blobs;
using WebApp_OpenIDConnect_DotNet.Models;
using Azure.Identity;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        ITokenAcquisition _tokenAcquisition;

        public HomeController(ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [AuthorizeForScopes(Scopes = new string[] { "https://storage.azure.com/user_impersonation" })]
        public async Task<IActionResult> Blob()
        {
            var scopes = new string[] { "https://storage.azure.com/user_impersonation" };
            var accessToken =
                await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            ViewData["Message"] = await CreateBlob(accessToken);
            return View();
        }

        private static async Task<string> CreateBlob(string accessToken)
        {
            // Replace the URL below with the URL to your blob.
            Uri blobUri = new Uri("https://storagesamples.blob.core.windows.net/sample-container/blob1.txt");
            BlobClient blobClient = new BlobClient(blobUri, new AuthorizationCodeCredential(accessToken)); // what kind of credential do we want here???

            // Create a blob on behalf of the user.
            string blobContents = "Blob created by Azure AD authenticated user.";
            byte[] byteArray = Encoding.ASCII.GetBytes(blobContents);

            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                await blobClient.UploadAsync(stream);
            }
            return "Blob successfully created";
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
