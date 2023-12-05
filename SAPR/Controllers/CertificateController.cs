using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SAPR.Models;
using SAPR.Utils;
using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace SAPR.Controllers
{
    public class CertificateController : Controller
    {
        // GET: CertificateController
        private readonly AppOptions _appOptions;
        public CertificateController(DataBaseContext context, IOptions<AppOptions> appOptions)
        {
            _appOptions = appOptions.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/Certificate/Create")]
        public IActionResult CreateCertificate(string fio, string other)
        {
            var rsaKey = RSA.Create(2048);

            byte[] serialNumber = BitConverter.GetBytes(DateTime.Now.ToBinary());
            string friendlySerialNumber = ("00" + BitConverter.ToString(serialNumber).Replace("-", "")).ToLower();

            var expirate = DateTimeOffset.Now.AddYears(4);

            string subject = @$"CN={fio};S={friendlySerialNumber};E={other}";

            var certReq = new CertificateRequest(subject, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            
            certReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            certReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation, false));
            certReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certReq.PublicKey, false));

            X509Certificate2 serverCert = new X509Certificate2(@$"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.Certificate}\server.pfx");

            var caCert = certReq.Create(serverCert, DateTimeOffset.Now, expirate, serialNumber);

            var exportCert = new X509Certificate2(caCert.Export(X509ContentType.Cert), (string)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet).CopyWithPrivateKey(rsaKey);

            MemoryStream ms = new MemoryStream(exportCert.Export(X509ContentType.Pfx));

            return File(ms, "application/octet-stream", @$"{fio}.pfx");
        }

        [HttpPost]
        [Route("/Certificate/CreateAdmin")]
        public IActionResult CreateCertificateAdmin()
        {
            if(System.IO.File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.Certificate}\server.pfx")) 
            {
                return Redirect("~/Admin/AdminIndex");
            }

            var rsaKey = RSA.Create(2048);
            string subject = "CN=ourserver.ru";

            var certReq = new CertificateRequest(subject, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            certReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            certReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certReq.PublicKey, false));

            var expirate = DateTimeOffset.MaxValue;

            X509Certificate2 caCert = certReq.CreateSelfSigned(DateTimeOffset.Now, expirate);

            var exportCert = new X509Certificate2(caCert.Export(X509ContentType.Cert), (string)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet).CopyWithPrivateKey(rsaKey);

            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.Certificate}"))
            {
                Directory.CreateDirectory(@$"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.Certificate}");
            }

            System.IO.File.WriteAllBytes(@$"{AppDomain.CurrentDomain.BaseDirectory}\{_appOptions.Directories.Certificate}\server.pfx", exportCert.Export(X509ContentType.Pfx));

            return Redirect("~/Admin/AdminIndex");
        }
    }
}
