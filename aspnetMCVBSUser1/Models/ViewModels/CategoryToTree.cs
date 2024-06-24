using System.ComponentModel.DataAnnotations.Schema;

namespace aspnetMCVBSUser1.Models.ViewModels
{
    [NotMapped]
    public class CategoryToTree
    {
        public int id { get; set; }

        public string value { get; set; } = "";
        public string label { get; set; } = "";
        public bool expand { get; set; } = false;
        public int parentID { get; set; } = 0;
        public int postCount { get; set; } = 0;
        public int SortType { get; set; } = 0; //0：更新日期 1：标题 2：排序序号
        public int SortDirection { get; set; } = 0;  //0:倒序"321", 1：正序 "123"
        public int IsOnlyUserSee { get; set; } = 0; //0 要登陆,默认 非0：不要登陆。

        public List<CategoryToTree> children { get; set; } = null;

    }
}
