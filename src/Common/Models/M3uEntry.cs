using Prism.Mvvm;
using System;
using System.ComponentModel.DataAnnotations;

namespace m3u_editor.Common.Models
{
    public class M3uEntry : BindableBase
    {
        private string _tvgname = string.Empty;
        private string _tvgid = string.Empty;
        private string _tvglogo = string.Empty;
        private string _grouptitle = string.Empty;
        private string _name2 = string.Empty;
        private string _link = string.Empty;
        private bool _isHighlighted;

        /// <summary>
        /// 频道名称
        /// </summary>
        [MaxLength(500)]
        public string Tvgname
        {
            get => _tvgname;
            set => SetProperty(ref _tvgname, value?.Trim() ?? string.Empty);
        }

        /// <summary>
        /// 频道ID
        /// </summary>
        [MaxLength(100)]
        public string Tvgid
        {
            get => _tvgid;
            set => SetProperty(ref _tvgid, value?.Trim() ?? string.Empty);
        }

        /// <summary>
        /// 频道Logo地址
        /// </summary>
        [Url]
        [MaxLength(500)]
        public string Tvglogo
        {
            get => _tvglogo;
            set => SetProperty(ref _tvglogo, value?.Trim() ?? string.Empty);
        }

        /// <summary>
        /// 分组名称
        /// </summary>
        [MaxLength(200)]
        public string Grouptitle
        {
            get => _grouptitle;
            set => SetProperty(ref _grouptitle, value?.Trim() ?? string.Empty);
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Name2
        {
            get => _name2;
            set => SetProperty(ref _name2, value?.Trim() ?? string.Empty);
        }

        /// <summary>
        /// 播放链接
        /// </summary>
        [Required]
        [Url]
        [MaxLength(2000)]
        public string Link
        {
            get => _link;
            set => SetProperty(ref _link, value?.Trim() ?? string.Empty);
        }

        /// <summary>
        /// 是否高亮显示（用于搜索）
        /// </summary>
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetProperty(ref _isHighlighted, value);
        }

        /// <summary>
        /// 验证条目是否有效
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name2) && 
                   !string.IsNullOrWhiteSpace(Link) &&
                   System.Uri.TryCreate(Link, UriKind.Absolute, out _);
        }

        /// <summary>
        /// 创建条目的浅拷贝
        /// </summary>
        public M3uEntry Clone()
        {
            return new M3uEntry
            {
                Tvgname = Tvgname,
                Tvgid = Tvgid,
                Tvglogo = Tvglogo,
                Grouptitle = Grouptitle,
                Name2 = Name2,
                Link = Link,
                IsHighlighted = IsHighlighted
            };
        }
    }
}
