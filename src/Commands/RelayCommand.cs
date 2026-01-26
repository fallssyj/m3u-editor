using System.Windows.Input;

namespace m3u_editor.Commands
{
    /// <summary>
    /// 通用命令实现，用于将委托绑定到按钮/菜单等控件。
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// 创建一个命令实例。
        /// </summary>
        /// <param name="execute">执行逻辑。</param>
        /// <param name="canExecute">是否允许执行的判断逻辑。</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 当可执行状态变化时触发。
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// 判断当前命令是否可以执行。
        /// </summary>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// 执行命令。
        /// </summary>
        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>
        /// 主动通知 UI 刷新可执行状态。
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
