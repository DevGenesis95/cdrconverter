using ApiConvertorv3.DataContext;
using ApiConvertorv3.Helpers;
using ApiConvertorv3.Models;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace ApiConvertorv3.Controllers
{
    public class ConvertorController : ApiController
    {
        private readonly string ParentDirectory = ConfigurationManager.AppSettings["UploadsDirectory"].ToString();
        private readonly string RequestKey = ConfigurationManager.AppSettings["RequestKey"].ToString();
        private readonly string AuthToken = ConfigurationManager.AppSettings["Token"].ToString();


        //file status
        string processingStatus = Status.processingFileStatus;
        string successStatus = Status.successFileStatus;
        string failedStatus = Status.failedFileStatus;



        #region Dummy Methods for pinging purpose
        public string Get(int id)
        {

            return "value";
        }

        public IEnumerable<string> Get()
        {

            //  return new string[] { "value1", "value2" };
            int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            var dat = DateTime.Now;

            return new string[] { epoch.ToString(), dat.ToString(), DateTime.UtcNow.ToString() };

        }
        #endregion

        public IEnumerable<string> RecreateTable()
        {
            DbContext.RecreateTable();
            return new string[] { "Table Recreated" };

        }

        #region Initial apis return the physical folder path c:/user/convertedFile.cdr

        // GET api/values/folderName
       
        [HttpPost]
        public IHttpActionResult GetPdfFile()
        {
            try
            {
                var file = HttpContext.Current.Request.Files.Count > 0 ?
                    HttpContext.Current.Request.Files[0] : null;


                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                    string filePath = SaveFile(file, fileName);

                    string pdfPath = PdfConvertor.ConvertToPdf(filePath);
                    string pdfFileName = Path.GetFileName(pdfPath);

                    //converting Pdf file into bytes array  
                    var dataBytes = File.ReadAllBytes(pdfPath);

                    //adding bytes to memory stream   
                    var dataStream = new MemoryStream(dataBytes);
                    return new convertorResult(dataStream, Request, pdfFileName);

                }
            }
            catch (Exception ex)
            {
                return Ok(ex);
            }
            return BadRequest();
            //  return file != null ? "/uploads/" + file.FileName : null;
        }


        [HttpPost]
        public IHttpActionResult GetPdfPath()
        {
            try
            {
                var file = HttpContext.Current.Request.Files.Count > 0 ?
                    HttpContext.Current.Request.Files[0] : null;


                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                    string filePath = SaveFile(file, fileName);

                    string pdfPath = PdfConvertor.ConvertToPdf(filePath);
                    string pdfFileName = Path.GetFileName(pdfPath);

                    return Ok(pdfPath);

                }
            }
            catch (Exception ex)
            {
                return Ok(ex);

            }
            return BadRequest();

        }




        private string SaveFile(HttpPostedFile file, string fileName)
        {
            try
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var extn = Path.GetExtension(fileName);

                //create dir and save file
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                string dirName = timestamp + "_" + fileNameWithoutExtension;

                string dir = ParentDirectory + "/" + dirName;
                string dirPath = HttpContext.Current.Server.MapPath(dir);


                if (!Directory.Exists(dirPath))  // if it doesn't exist, create
                    Directory.CreateDirectory(dirPath);


                var path = Path.Combine(
                    HttpContext.Current.Server.MapPath(dir),
                    fileName
                );

                file.SaveAs(path);
                return path;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion


        /// <summary>
        /// Api to upload cdr file and return folder name
        /// </summary>
        /// <returns></returns>

        #region Api v2


        [HttpPost]
        public IHttpActionResult UploadCdrFile()
        {
            try
            {

                var file = HttpContext.Current.Request.Files.Count > 0 ?
                   HttpContext.Current.Request.Files[0] : null;

                //  var dir = new RequestModel();


                if (file != null && file.ContentLength > 0)
                {
                    var keyName = HttpContext.Current.Request.Files.AllKeys[0].ToString();

                    if (keyName != RequestKey)
                        return BadRequest(Status.invalidFileKey);

                    var fileName = Path.GetFileName(file.FileName);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    //   var getFullPathWithoutExtn = Path.Combine(Path.GetDirectoryName("~/uploads"), Path.GetFileNameWithoutExtension(fileName));

                    string filePath = "";
                    try
                    {
                        filePath = SaveCdrFile(file, fileName);

                    }
                    catch (Exception ex)
                    {
                        var errResponse = GetUploadResponse(filePath, failedStatus,ex.Message);
                        return Content(HttpStatusCode.BadRequest,errResponse);
                    }

                    var recordId = DbContext.InsertRecord(filePath, processingStatus);
                    var response = GetUploadResponse(filePath, successStatus);
                  //  dir.folderName = filePath;


                    var virtualFilePath = filePath + "/" + fileNameWithoutExtension;
                    var absoluteFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath(ParentDirectory), virtualFilePath);
                    new Thread(() =>
                    {
                        // Thread.CurrentThread.IsBackground = true;
                        /* run your code here */
                        try
                        {

                            var pdfPath = PdfConvertor.ConvertToPdf(absoluteFilePath);

                            DbContext.UpdateRecord(recordId, successStatus);
                        }
                        catch (Exception ex)
                        {
                            DbContext.UpdateRecord(recordId, failedStatus, ex.Message);

                        }


                    }).Start();

                    return Ok(response);
                }

            }
            catch (Exception ex)
            {
              
                throw ex;
                     
            }
            return BadRequest();
        }

        [HttpPost]
        public IHttpActionResult GetPdfFileStatus([FromBody] RequestModel dirName)
        {
            try
            {
                string folderName = dirName.folderName;
                var fetchedRecord = DbContext.FetchRecords(folderName);

                if (fetchedRecord != null)
                {
                    var response = new ResponseModel();
                    response.status = fetchedRecord.status;
                    // return Ok(fetchRecords.Status);
                    return Ok(response);
                }

                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        public IHttpActionResult DownloadPdfFile([FromBody] RequestModel dirName)
        {
            try
            {

                string dir = ParentDirectory + "/" + dirName.folderName;
                string dirPath = HttpContext.Current.Server.MapPath(dir);


                // string[] parts = dirPath.Split('_');

                var fileName = GetPdfFileName(dirPath);

                var path = Path.Combine(
                  HttpContext.Current.Server.MapPath(dir),
                  fileName
              );


                // string pdfPath = ConvertToPdf(path);
                string pdfFileName = Path.GetFileName(path);

                //converting Pdf file into bytes array  
                var dataBytes = File.ReadAllBytes(path);
                //adding bytes to memory stream   
                var dataStream = new MemoryStream(dataBytes);
                return new convertorResult(dataStream, Request, pdfFileName);


            }
            catch (Exception ex)
            {
                throw ex;
            }
            //  return BadRequest();
        }

        [HttpPost]
        public IHttpActionResult GetRecords([FromBody] RequestModel dirName)
        {

            try
            {
                string folderName = dirName.folderName;
                var fetchedRecord = DbContext.FetchRecords(folderName);

                if (fetchedRecord != null)
                    return Ok(fetchedRecord);

                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public IHttpActionResult DeleteGeneratedPdf([FromBody] DeletePdfModel request)
        {
            try
            {
                var token = request.token;
                var daysToDeleteBefore = -request.days;

                if (token != AuthToken)
                    return Unauthorized();

                var directoryPath = HttpContext.Current.Server.MapPath(ParentDirectory);
                var directoryInfo = new DirectoryInfo(directoryPath);

             

                var directories = directoryInfo.EnumerateDirectories();  //directoryInfo.GetDirectories();
                int count = 0;

                new Thread(()=>
                {
                    try
                    {
                        foreach (var dirs in directories)
                        {

                            if (dirs.CreationTime < DateTime.Now.AddDays(daysToDeleteBefore))
                            {
                                count++;
                                Directory.Delete(dirs.FullName, true);

                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                }).Start();
               
                return Ok(successStatus);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetPdfFileName(string directoryPath)
        {
          

            var array = directoryPath.Split('_');
            string firstElem = array.First();

            string restOfArray = string.Join("_", array.Skip(1));

            var fileName = restOfArray + ".pdf";

            return fileName;
        }

        private string SaveCdrFile(HttpPostedFile file, string fileName)
        {
            try
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var extn = Path.GetExtension(fileName);

                //create dir and save file
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                string dirName = timestamp + "_" + fileNameWithoutExtension;

                string dir = ParentDirectory + "/" + dirName;
                string dirPath = HttpContext.Current.Server.MapPath(dir);


                if (!Directory.Exists(dirPath))  // if it doesn't exist, create
                    Directory.CreateDirectory(dirPath);


                var path = Path.Combine(
                    HttpContext.Current.Server.MapPath(dir),
                    fileName
                );
               
                file.SaveAs(path);
                return dirName;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private RequestModel GetUploadResponse(string folder, string status, string errMessage = "")
        {
            var response = new RequestModel
            {
                folderName = folder,
                uploadStatus = status,
                errorMessage = errMessage
            };

            return response;
        }


        #endregion



    }

    public class convertorResult : IHttpActionResult
    {
        MemoryStream responseStream;
        string PdfFileName;
        HttpRequestMessage httpRequestMessage;
        HttpResponseMessage httpResponseMessage;
        public convertorResult(MemoryStream data, HttpRequestMessage request, string filename)
        {
            responseStream = data;
            httpRequestMessage = request;
            PdfFileName = filename;
        }
        public System.Threading.Tasks.Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            httpResponseMessage = httpRequestMessage.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StreamContent(responseStream);
            //httpResponseMessage.Content = new ByteArrayContent(bookStuff.ToArray());  
            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = PdfFileName;
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            return System.Threading.Tasks.Task.FromResult(httpResponseMessage);
        }
    }



}
