using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sitewithimage.Models
{
    public class BaseImage : TableEntity
    {
        public BaseImage()
        {
            this.PartitionKey = "image";
            this.RowKey = Guid.NewGuid().ToString();
        }
        public string URLcore { get; set; }
        public string URLcrop { get; set; }
        public string Name { get; set; }
    }
}