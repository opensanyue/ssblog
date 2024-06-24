using System.ComponentModel.DataAnnotations.Schema;

namespace aspnetMCVBSUser1.Models.ViewModels
{
    [NotMapped]
    public class ReturnJsonInfo
    {
        public int Code { get; set; } = 0;
        public string Message { get; set; } = "";
        public object Entity { get; set; }
        public ReturnJsonInfo(int code, string message, object obj)
        {
            this.Code = code;
            this.Message = message;
            this.Entity = obj;
        }
    }
}
