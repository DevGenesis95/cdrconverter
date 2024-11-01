using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiConvertorv3.Helpers
{
    public static class Status
    {

        //file status
        public const string
              processingFileStatus = "processing",
              successFileStatus = "success",
              failedFileStatus = "failed",
              invalidFileKey = "invalid file key",
              notfound = "resource not found";
    }
}