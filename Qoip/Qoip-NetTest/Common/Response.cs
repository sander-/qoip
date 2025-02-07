namespace Qoip.ZeroTrustNetwork.Common
{
    public class Response<T>
    {
        public ResponseStatus Status { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; }

        public Response(ResponseStatus status, T? data, string message)
        {
            Status = status;
            Data = data;
            Message = message;
        }

        public Response<T> WithStatus(ResponseStatus status)
        {
            Status = status;
            return this;
        }

        public Response<T> WithData(T data)
        {
            Data = data;
            return this;
        }

        public Response<T> WithMessage(string message)
        {
            Message = message;
            return this;
        }
    }
}



