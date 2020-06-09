using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class DelfusTemplate
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string UploadedById { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedOn { get; set; }
        public bool IsRemoved { get; set; }
        public string RemovedById { get; set; }
        public string RemovedBy { get; set; }
        public DateTime? RemovedOn { get; set; }
    }
    public class DelfusResult
    {
        public bool Success { get; set; }
        public object Object { get; set; }
        public string ErrorMessage { get; set; }
    }
}
