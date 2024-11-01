using ApiConvertorv3.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace ApiConvertorv3.DataContext
{
    public  class DbContext
    {
        static string databaseName = ConfigurationManager.AppSettings["DatabaseName"].ToString();
        static string databasePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/SqliteDB"), databaseName);


        #region DB Methods

        public static void CreateTable()
        {
            using (var db = GetSqlConnection())
            {
                db.CreateTable<DbModel>();
            }

        }
        public static void DropTable()
        {
            using (var db = GetSqlConnection())
            {
                db.DropTable<DbModel>();
            }
           
        }

        public static void  RecreateTable()
        {
            DropTable();
            CreateTable();
         
        }



        public static int InsertRecord(string folderName, string status)
        {
            using (var db = GetSqlConnection())
            {
                try
                {
                    var folderContext = new DbModel()
                    {
                        FolderName = folderName,
                        Status = status,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now

                    };
                    db.Insert(folderContext);
                    return folderContext.Id;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

        }

        public static FetchRecordsModel FetchRecords(string folderName)
        {

            using (var db = GetSqlConnection())
            {
                try
                {
                    var record = db.Table<DbModel>().Where(v => v.FolderName == folderName).FirstOrDefault();


                    if (record != null)
                    {
                        //  DbModel fetchedRecord = new DbModel();
                        FetchRecordsModel fetchedRecord = new FetchRecordsModel();


                        fetchedRecord.id = record.Id;
                        fetchedRecord.folderName = record.FolderName;
                        fetchedRecord.status = record.Status;
                        fetchedRecord.createdDate = record.CreatedDate;
                        fetchedRecord.modifiedDate = record.ModifiedDate;

                        //epoch for ModifiedDate
                        int epoch = (int)(fetchedRecord.modifiedDate - new DateTime(1970, 1, 1)).TotalSeconds;
                        fetchedRecord.epochSeconds = epoch;

                        fetchedRecord.isDeleted = record.IsDeleted;
                        fetchedRecord.errorMessage = record.ErrorMessage;

                        return fetchedRecord;
                    }
                    else
                        return null;


                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static int UpdateRecord(int id, string status, string errorMessage = "")
        {
            using (var db = GetSqlConnection())
            {
                try
                {
                    var existingRecord = db.Query<DbModel>("Select * from DbModel where Id = ?", id).FirstOrDefault();
                    if (existingRecord != null)
                    {
                        existingRecord.Status = status;
                        existingRecord.ErrorMessage = errorMessage;
                        existingRecord.ModifiedDate = DateTime.Now;

                        db.RunInTransaction(() =>
                        {
                            db.Update(existingRecord);
                        });



                        return existingRecord.Id;




                    }

                    return -1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


        }

        public static SQLiteConnection GetSqlConnection()
        {

            return new SQLiteConnection(databasePath);

        }

        #endregion
    }
}