namespace CheckTranslationWidthAPP.model
{
    /// <summary>
    /// 用于保存基本的设置参数，或者控制台传入的参数
    /// </summary>
    public  class Argument
    {
        public static string FilePath { set; get; }

        public static string OutPutType { set; get; }

        public static string OutPutDiretory { set; get; }

        public static int TargetColumn { set; get; }
    }
}
