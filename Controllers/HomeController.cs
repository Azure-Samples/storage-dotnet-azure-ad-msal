using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Models;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Identity.Web;

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
            // create a blob on behalf of the user
            TokenCredential tokenCredential = new TokenCredential(accessToken);
            StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);
            // replace the URL below with your storage account URL
            Uri blobUri = new Uri("https://blobstorageazuread123.blob.core.windows.net/sample-container/Blob1.txt");
            CloudBlockBlob blob = new CloudBlockBlob(blobUri, storageCredentials);
            await blob.UploadTextAsync("Blob created by Azure AD authenticated user.");
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
