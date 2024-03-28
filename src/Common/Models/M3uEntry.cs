using Prism.Mvvm;

namespace m3u_editor.Common.Models
{
    public class M3uEntry : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private string tvgname;
        public string Tvgname
        {
            get { return tvgname; }
            set { tvgname = value; }
        }
        /// <summary>
        /// ID
        /// </summary>
        private string tvgid;
        public string Tvgid
        {
            get { return tvgid; }
            set { tvgid = value; }
        }
        /// <summary>
        /// logo地址
        /// </summary>
        private string tvglogo;
        public string Tvglogo
        {
            get { return tvglogo; }
            set { tvglogo = value; }
        }
        private string grouptitle;
        /// <summary>
        /// 组名称
        /// </summary>
        public string Grouptitle
        {
            get { return grouptitle; }
            set { grouptitle = value; }
        }
        /// <summary>
        /// name2
        /// </summary>
        private string name2;
        public string Name2
        {
            get { return name2; }
            set { name2 = value; }
        }
        /// <summary>
        /// Link
        /// </summary>
        private string link;
        public string Link
        {
            get { return link; }
            set { link = value; }
        }

        private bool isHighlighted;

        public bool IsHighlighted
        {
            get { return isHighlighted; }
            set { isHighlighted = value; }
        }

    }
}
