namespace Game.Archive
{
    /// <summary>
    /// 加密解密接口
    /// </summary>
    public interface IEncryptorHelper
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] Decrypt(byte[] data);
    }
}