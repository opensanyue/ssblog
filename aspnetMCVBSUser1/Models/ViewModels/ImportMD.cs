using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace aspnetMCVBSUser1.Models.ViewModels
{
    [NotMapped]
    public class Keyval : IEquatable<Keyval>
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";

        public bool IsExists { get; set; } = true;

        public string tostring { get { return Key + Environment.NewLine + System.Web.HttpUtility.UrlDecode(Value).Replace("/", "\\") + Environment.NewLine + IsExists.ToString(); } }
        public bool Equals(Keyval? other)
        {
            return this.Key == other!.Key;
        }
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }

    [NotMapped]
    public class PostImg
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<Keyval> imgList { get; set; } = new List<Keyval>();
    }
}
