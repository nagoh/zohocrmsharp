using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Deveel.Web.Zoho
{
    public interface IZohoCrmClient
    {
       InsertDuplicateCheck DuplicateCheck { get; set; }
       bool? InsertApproval { get; set; }
       UserContext CreateUserContext(string userEmail);
       bool DeleteRecord<T>(T record) where T : ZohoEntity;
       bool DeleteRecordById<T>(string id);
       bool DeleteRecordFile<T>(string fileId) where T : ZohoEntity;
       DownloadFile DownloadRecordFile<T>(string fileId) where T : ZohoEntity;
       ZohoEntityContext<T> GetContext<T>() where T : ZohoEntity;
       ZohoEntityCollection<T> GetMyRecords<T>(ListOptions options) where T : ZohoEntity;
       ZohoEntityCollection<ZohoAttachment> GetRecordAttachments<T>(string id) where T : ZohoEntity;
       T GetRecordById<T>(string id) where T : ZohoEntity;
       ZohoEntityCollection<T> GetRecords<T>(ListOptions options) where T : ZohoEntity;
       ZohoEntityCollection<TRelated> GetRelatedRecordsTo<TSource, TRelated>(string id)
            where TSource : ZohoEntity
            where TRelated : ZohoEntity;
       ZohoEntityCollection<TRelated> GetRelatedRecordsTo<TSource, TRelated>(string id, int? toIndex)
            where TSource : ZohoEntity
            where TRelated : ZohoEntity;
        ZohoEntityCollection<TRelated> GetRelatedRecordsTo<TSource, TRelated>(string id, int? fromIndex, int? toIndex)
            where TSource : ZohoEntity
            where TRelated : ZohoEntity;
        ZohoUsersResponse GetUsers(UserType userType);
        ZohoInsertResponse InsertRecord<T>(T record) where T : ZohoEntity;
        ZohoInsertResponse InsertRecords<T>(IEnumerable<T> records) where T : ZohoEntity;
        ZohoEntityCollection<T> Search<T>(string column, ConditionOperator @operator, object value) where T : ZohoEntity;
        ZohoEntityCollection<T> Search<T>(ZohoSearchCondition searchCondition) where T : ZohoEntity;
        ZohoEntityCollection<T> Search<T>(ZohoSearchCondition searchCondition, IEnumerable<string> selectColumns) where T : ZohoEntity;
        bool UpdateRecord<T>(T record) where T : ZohoEntity;
        bool UpdateRecord<T>(string id, T record) where T : ZohoEntity;
        string UploadFileToRecord<T>(string recordId, string fileName, string contentType, string filePath) where T : ZohoEntity;
        string UploadFileToRecord<T>(string id, string fileName, string contentType, byte[] content) where T : ZohoEntity;
        string UploadFileToRecord<T>(string recordId, string fileName, string contentType, Uri uri) where T : ZohoEntity;
        string UploadFileToRecord<T>(string id, string fileName, string contentType, Stream inputStream) where T : ZohoEntity;
        string UploadPhotoToRecord<T>(string id, string contentType, byte[] data);
        string UploadPhotoToRecord<T>(string id, string contentType, Stream inputStream) where T : ZohoEntity;
        string UploadPhotoToRecord<T>(string id, string contentType, string filePath) where T : ZohoEntity;
        string UploadPhotoToRecord<T>(string id, string contentType, Uri uri) where T : ZohoEntity;
    }
}
