using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace aspnetMCVBSUser1.Models
{
    [Table("Post")]
    public class Post
    {

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置内容标题
        /// </summary>
        [Required]
        [DisplayName("标题")]
        //[DataType(DataType.MultilineText)]   //如在视图中使用强类类型的辅助方法@Html.EditorFor(model =>model),则此字段将被渲染成<textarea>文本域标签。
        public string? Title { get; set; }
        /// <summary>
        /// 获取或设置内容
        /// </summary>
        [Display(Name = "内容")]
        public string? Content { get; set; }
        /// <summary>
        /// 获取或设置内容发布日期
        /// </summary>
        [Display(Name = "创建日期")]
        [DataType(DataType.Date)]
        public DateTime? CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 获取或设置内容更新日期
        /// </summary>
        [Display(Name = "更新日期")]
        [DataType(DataType.Date)]
        public DateTime? UpdataDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 获取或设置点击数
        /// </summary>
        [Display(Name = "点击数")]
        public int? hits { get; set; }
        /// <summary>
        /// 获取或设置分类ID
        /// </summary>
        [Required]
        [Display(Name = "类别")]
        public int CategoryId { get; set; }
        /// <summary>
        /// 获取或设置分类
        /// </summary>
        [Display(Name = "类别")]
        public virtual Category? Category { get; set; }

        public bool DelStatus { get; set; } = false; //是否删除 true删除 false没删除

        public int CSort { get; set; }
        public int PostType { get; set; } =  0;
        public int IsOnlyUserSee { get; set; } =  0; //0 要登陆,默认 非0：不要登陆。

    }
}
