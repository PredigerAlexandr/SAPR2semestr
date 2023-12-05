using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SAPR.Controllers
{
    public class SignController : Controller
    {
        // GET: SignController
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/Sign")]
        public async Task<IActionResult> CreateCertificate(IFormFile file, string data)
        {

            byte[] sign;

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

            if (VerifyMsg(dataByte, encodedSignature))
            {
                //Console.WriteLine("\nСообщение проверено.");
                var a = 1;
            }
            else
            {
                //Console.WriteLine("\nОшибка при проверке сообщения.");
                var b = 1;
            }

            return View();
        }


        static public bool VerifyMsg(byte[] msg,
        byte[] encodedSignature)
        {
            //  Создаем объект ContentInfo по сообщению.
            //  Это необходимо для создания объекта SignedCms.
            ContentInfo contentInfo = new ContentInfo(msg);

            //  Создаем SignedCms для декодирования и проверки.
            SignedCms signedCms = new SignedCms(contentInfo, true);


            //  Декодируем подпись
            signedCms.Decode(encodedSignature);

            //  Перехватываем криптографические исключения, для 
            //  возврата о false значения при некорректности подписи.
            try
            {
                signedCms.CheckSignature(true);
            }
            catch (System.Security.Cryptography.CryptographicException e)
            {
                return false;
            }

            return true;
        }

        //static public X509Certificate2 GetSignerCert()
        //{
        //    //  Открываем хранилище My.
        //    X509Store storeMy = new X509Store(StoreName.My,
        //        StoreLocation.CurrentUser);
        //    storeMy.Open(OpenFlags.ReadOnly);

        //    //  Отображаем сертификаты для удобства работы
        //    //  с примером.
        //    Console.WriteLine("Найдены сертификаты следующих субъектов " +
        //        "в хранилище {0}:", storeMy.Name);
        //    foreach (X509Certificate2 cert in storeMy.Certificates)
        //    {
        //        Console.WriteLine("\t{0}", cert.SubjectName.Name);
        //    }

        //    //  Ищем сертификат для подписи.
        //    X509Certificate2Collection certColl =
        //        storeMy.Certificates.Find(X509FindType.FindBySubjectName,
        //        signerName, false);
        //    Console.WriteLine(
        //        "Найдено {0} сертификат(ов) в хранилище {1} для субъекта {2}",
        //        certColl.Count, storeMy.Name, signerName);

        //    //  Проверяем, что нашли требуемый сертификат
        //    if (certColl.Count == 0)
        //    {
        //        Console.WriteLine(
        //            "Сертификат для данного примера не найден " +
        //            "в хранилище. Выберите другой сертификат для подписи. ");
        //    }

        //    storeMy.Close();

        //    //  Если найдено более одного сертификата,
        //    //  возвращаем первый попавщийся.
        //    return certColl[0];
        //}

    }
}
