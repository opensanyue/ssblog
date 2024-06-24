using System.ComponentModel.DataAnnotations;

namespace aspnetMCVBSUser1.Models.ViewModels
{
    public class Lic
    {
        [Display(Name = "软件名版本")]
        public string? AppVer { get; set; }
        [Display(Name = "注册日期")]
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        [Display(Name = "用户等级")]  //<0免费 1青铜 2白银 3黄金 4铂金 5钻石 
        public int Grade { get; set; } = 0;
        [Display(Name = "5级客户自定义内容")]
        public string? Content { get; set; }
      
        public string? MachineCode { get; set; }
    }
}
