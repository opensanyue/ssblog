using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aspnetMCVBSUser1.Models.ViewModels
{
    [NotMapped]
    public class ConfigModel
    {
        [Display(Name = "网站名")]
        public string? AppName { get; set; }
        [Display(Name = "作者")]
        public string? Author { get; set; }
        [Display(Name = "分页每页数量")]
        public int PageSize { get; set; }
        [Display(Name = "License")]
        public string? Lic { get; set; }
        [Display(Name = "默认编辑器")]
        public bool DefultEditorIsMarkdown { get; set; }
        public string? DefaultLanguage { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public Center? Center { get; set; }
    }



    public class Center
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }

}
