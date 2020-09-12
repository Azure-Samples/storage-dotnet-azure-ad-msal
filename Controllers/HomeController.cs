using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Models;
using Microsoft.Identity.Web;
using WebAppOpenIDConnectDotNet;
using Azure.Storage.Blobs;
using System.Text;
using System.IO;

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
            string message = await CreateBlob(new TokenAcquisitionTokenCredential(_tokenAcquisition));
            ViewData["Message"] = message;
            return View();
        }

        private static async Task<string> CreateBlob(TokenAcquisitionTokenCredential tokenCredential)
        {
            Uri blobUri = new Uri("https://blobstorageazuread123.blob.core.windows.net/sample-container/Blob1.txt");
            BlobClient blobClient = new BlobClient(blobUri, tokenCredential);

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
