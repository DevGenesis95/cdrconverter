using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiConvertorv3.Models
{
    public class RequestModel
    {
        public string folderName { get; set; }
        public string uploadStatus { get; set; }
        public string errorMessage { get; set; }
    }

    public class ResponseModel
    {
        public string status { get; set; }
    }

    // response mapper for the table DbModel
    public class FetchRecordsModel
    {
        public int id { get; set; }
        public string folderName { get; set; }
        public string status { get; set; }
        public string errorMessage { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime modifiedDate { get; set; }
        public int epochSeconds { get; set; }
        public bool isDeleted { get; set; } = false;

    }

    public class DeletePdfModel
    {
      
        public int days { get; set; }
        public string token { get; set; }
    }

}