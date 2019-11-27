namespace Ringer.ViewModels
{
    #region CamaraAction Class
    class CameraAction
    {
        public string Title { get; } = "작업을 선택하세요.";
        public string Cancle { get; } = "취소";
        public string Destruction { get; } = "파파괴";
        public string TakingPhoto { get; } = "사진 촬영";
        public string AttachingPhoto { get; } = "사진 불러오기";
        public string TakingVideo { get; } = "동영상 촬영";
        public string AttachingVideo { get; } = "동영상 불러오기";
    }
    #endregion
}