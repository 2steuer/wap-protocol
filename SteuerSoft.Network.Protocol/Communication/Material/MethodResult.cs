namespace SteuerSoft.Network.Protocol.Communication.Material
{
    public class MethodResult<T>
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public T Result { get; set; }

        public static MethodResult<T> FromResult(T result)
        {
            return new MethodResult<T>()
            {
                Success = true,
                Error = string.Empty,
                Result = result
            };
        }

        public static MethodResult<T> FromError(string error)
        {
            return new MethodResult<T>()
            {
                Success = false,
                Error = error,
                Result = default(T)
            };
        }
    }
}
