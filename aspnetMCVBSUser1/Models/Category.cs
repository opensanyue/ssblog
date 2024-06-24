using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace aspnetMCVBSUser1.Models
{
    [Table("Category")]
    public class Category
    {
        public Category()
        {
            this.Posts = new List<Post>();
        }
        [Key]
        public int Id { get; set; }
        [DisplayName("类名")]
        public string Name { get; set; } = "";

        public int PID { get; set; }
        public int Leven { get; set; }
        public bool CStatus { get; set; } = false;
        public int CSort { get; set; }
        public int SortType { get; set; } =  0; //0：更新日期 1：标题 2：排序序号
        public int SortDirection { get; set; } =  0;  //0:倒序"321", 1：正序 "123"

        public int IsOnlyUserSee { get; set; } =  0; //0 要登陆,默认 非0：不要登陆。

        public bool DelStatus { get; set; } = false; //是否删除 true删除 false没删除
        /// <summary>
        /// 该分类下的内容集合
        /// </summary>
        public virtual ICollection<Post> Posts { get; set; }


        public virtual List<Category> ChildCategory { get; set; } = new List<Category>();


    }
}
