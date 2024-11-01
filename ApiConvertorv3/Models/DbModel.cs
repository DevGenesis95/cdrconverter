using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiConvertorv3.Models
{
    public class DbModel
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FolderName { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}