using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace sitewithimage.Models
{
    public class ImageService
    {
        private readonly string _imageRootPath;
        private readonly string _containerName;
        private readonly string _blobStorageConnectionString;

        public ImageService()
        {
            _imageRootPath = ConfigurationManager.AppSettings["ImageRootPath"];
            _containerName = ConfigurationManager.AppSettings["ImagesContainer"];
            _blobStorageConnectionString = ConfigurationManager.ConnectionStrings["BlobStorageConnectionString"].ConnectionString;
        }
        public CloudBlobContainer GetImagesBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(_blobStorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerName);
            container.CreateIfNotExists();
            container.SetPermissions(
            new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
            return container;
        }
        public async Task<UploadImage> CreateUploadedImage(HttpPostedFileBase file)
        {
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {


                byte[] fileBytes = new byte[file.ContentLength];
                await file.InputStream.ReadAsync(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                return new UploadImage
                {
                    ContentType = file.ContentType,
                    Data = fileBytes,
                    Name = file.FileName,
                    Url = String.Format("data:image/jpeg;base64,{0}", Convert.ToBase64String(fileBytes))
                };
            }
            return null;
        }
        public CloudTable CreateTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_blobStorageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("imagetable");
            table.CreateIfNotExists();
            return table;
        }
        public CloudQueue CreateQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_blobStorageConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("imagequeue");
            queue.CreateIfNotExists();
            return queue;
        }
        public async Task AddImageToBlobStorageAsync(UploadImage image)
        {
            var container = GetImagesBlobContainer();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(image.Name);
            blockBlob.Properties.ContentType = image.ContentType;
            var fileBytes = image.Data;
            await blockBlob.UploadFromByteArrayAsync(fileBytes, 0, fileBytes.Length);
            string blockurl = (blockBlob.Uri).ToString();
            string name = image.Name;
            AddToTable(blockurl,name);
        }
        public void AddToTable(string blockurl,string name)
        {
            CloudTable table = CreateTable();
            BaseImage bi = new Models.BaseImage();
            bi.URLcore = blockurl;
            bi.Name = name;
            TableOperation insertOperation = TableOperation.Insert(bi);
            table.Execute(insertOperation);
            string rowkey = bi.RowKey;
            AddToQueue(rowkey);
        }
        public void AddToQueue(string rowkey)
        {
            CloudQueue queue = CreateQueue();
            CloudQueueMessage message = new CloudQueueMessage(rowkey);
            queue.AddMessage(message);
        }
        public List<BaseImage> RetrieveFromTable()
        {
            CloudTable table = CreateTable();
            var entities = table.ExecuteQuery(new TableQuery<BaseImage>()).ToList();
            return entities;
        }
        public Image GetImageFromUrl(string url)
        {
            WebRequest requestPic = WebRequest.Create(url);
            WebResponse responsePic = requestPic.GetResponse();
            Image webImage = Image.FromStream(responsePic.GetResponseStream());
            return webImage;
        }
        public List<Image> GetImages(List <BaseImage> images)
        {
            List<Image> allimages = new List<Image>();
            foreach(BaseImage image in images)
            {
                string urlcore = image.URLcore;
                string urlcrop = image.URLcrop;
                Image core = GetImageFromUrl(urlcore);
                Image crop= GetImageFromUrl(urlcrop);
                allimages.Add(core);
                allimages.Add(crop);
            }
            return allimages;
        }
    }
}