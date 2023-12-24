using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Xml;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using SAPR.Models;
using SAPR.Utils;
using Microsoft.Extensions.Options;

namespace SAPR.Controllers
{
    public class SignController : Controller
    {
        private readonly DataBaseContext db;
        private readonly AppOptions _appOptions;
        public SignController(DataBaseContext context, IOptions<AppOptions> appOptions)
        {
            db = context;
            _appOptions = appOptions.Value;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Route("/Sign/Test")]
        public IActionResult IndexTest()
        {
            return View("SignTest");
        }

        [HttpPost]
        [Route("/Sign")]
        public async Task<IActionResult> SignData(IFormFile file, string data)
        {
            
            var dataXml = @$"<data><attribute>{data}</attribute></data>";
            byte[] sign;
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(dataXml);

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                sign = memoryStream.ToArray();
            }

            X509Certificate2 signerCert = new X509Certificate2(sign, (string)null);

            var dataByte = System.Text.Encoding.ASCII.GetBytes(data);

            ContentInfo contentInfo = new ContentInfo(dataByte);
            SignedCms signedCms = new SignedCms(contentInfo, true);

            //  Определяем подписывающего, объектом CmsSigner.
            CmsSigner cmsSigner = new CmsSigner(signerCert);

            signedCms.ComputeSignature(cmsSigner);

            //  Кодируем CMS/PKCS #7 подпись сообщения.
            var encodedSignature = signedCms.Encode();

            string signXml = System.Convert.ToBase64String(encodedSignature);

            XmlElement signNode = xml.CreateElement("sign");
            signNode.InnerText = signXml;
            xml.DocumentElement.AppendChild(signNode);

            MemoryStream xmlStream = new MemoryStream();
            xml.Save(xmlStream);
            xmlStream.Flush();//Adjust this if you want read your data 
            xmlStream.Position = 0;
            
            db.SignerUsers.Add(new SignerUser
            {
                Sign = signXml,
                XmlDoc = xml.OuterXml,
                subjectName = signerCert.SubjectName.Name
            });
            db.SaveChanges();

            return File(xmlStream, "application/octet-stream", @$"data.xml");
        }

        [Route("/Sign/Check")]
        public IActionResult IndexCheck()
        {
            return View("SignCheck");
        }

        [HttpPost]
        [Route("/Sign/Check")]
        public async Task<IActionResult> SignCheck(IFormFile fileXml, IFormFile fileSign)
        {

            XmlDocument xmlData = new XmlDocument();
            byte[] sign;

            using (var memoryStream = new MemoryStream())
            {
                await fileXml.CopyToAsync(memoryStream);

                xmlData.LoadXml(System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()));
            }

            using (var memoryStream = new MemoryStream())
            {
                await fileSign.CopyToAsync(memoryStream);

                sign = memoryStream.ToArray();
            }

            X509Certificate2 signerCert = new X509Certificate2(sign, (string)null);

            var dataByte = System.Text.Encoding.ASCII.GetBytes(xmlData.SelectSingleNode("//attribute").InnerText);

            ContentInfo contentInfo = new ContentInfo(dataByte);
            SignedCms signedCms = new SignedCms(contentInfo, true);

            CmsSigner cmsSigner = new CmsSigner(signerCert);

            signedCms.ComputeSignature(cmsSigner);

            var encodedSignature = signedCms.Encode();

            string signNew = System.Convert.ToBase64String(encodedSignature);

            string signInFile = xmlData.SelectSingleNode("//sign").InnerText;

            bool flag = signNew == signInFile;

            if (flag)
            {
                return View("AcceptCheck");
            }
            else
            {
                return View("ErrorCheck");
            }
        }

        [Route("/Sign/Valid")]
        public IActionResult IndexValid()
        {
            return View("SignValid");
        }


        static public bool VerifyMsg(byte[] msg,
        byte[] encodedSignature)
        {
            //  Создаем объект ContentInfo по сообщению.
            //  Это необходимо для создания объекта SignedCms.
            ContentInfo contentInfo = new ContentInfo(msg);

            //  Создаем SignedCms для декодирования и проверки.
            SignedCms signedCms = new SignedCms(contentInfo, true);

            try
            {
                signedCms.Decode(encodedSignature);
                signedCms.CheckSignature(true);
            }
            catch (System.Security.Cryptography.CryptographicException e)
            {
                return false;
            }

            return true;
        }
        
        [HttpPost]
        [Route("/Sign/Valid")]
        public async Task<IActionResult> SignValid(IFormFile fileWithSign)
        {

            XmlDocument xmlData = new XmlDocument();
            byte[] sign;

            using (var memoryStream = new MemoryStream())
            {
                await fileWithSign.CopyToAsync(memoryStream);

                xmlData.LoadXml(System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()));
            }

            var dataByte = System.Text.Encoding.ASCII.GetBytes(xmlData.SelectSingleNode("//attribute").InnerText);

            var signByte = System.Convert.FromBase64String(xmlData.SelectSingleNode("//sign").InnerText);

            bool flag = VerifyMsg(dataByte, signByte);

            if (flag)
            {
                return View("AcceptValid");
            }
            else
            {
                return View("ErrorValid");
            }
        }
    }
}
