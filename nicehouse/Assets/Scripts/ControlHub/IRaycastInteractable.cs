namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 供第一人称射线交互系统使用的接口。
    /// </summary>
    public interface IRaycastInteractable
    {
        /// <summary>
        /// 当准星射线首次悬停在对象上时调用。
        /// </summary>
        void OnHoverEnter(FPRaycastInteractor interactor);

        /// <summary>
        /// 当准星离开对象时调用。
        /// </summary>
        void OnHoverExit(FPRaycastInteractor interactor);

        /// <summary>
        /// 当玩家按下交互键（默认鼠标左键）时调用。
        /// </summary>
        void OnRaycastClick(FPRaycastInteractor interactor);

        /// <summary>
        /// 返回浮现在准星旁的提示文本。
        /// </summary>
        string HoverHint { get; }
    }
}


