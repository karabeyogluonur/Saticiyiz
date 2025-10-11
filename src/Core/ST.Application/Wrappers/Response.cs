using System.Collections.Generic;

namespace ST.Application.Wrappers
{
    public sealed class Response<T>
    {
        private Response(bool succeeded, T data, string message, IEnumerable<string> errors)
        {
            Succeeded = succeeded;
            Data = data;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        public bool Succeeded { get; init; }
        public string Message { get; init; }
        public IEnumerable<string> Errors { get; init; }
        public T Data { get; init; }

        public static Response<T> Success(T data, string message = null) => new Response<T>(true, data, message, null);

        public static Response<T> Success(string message) => new Response<T>(true, default, message, null);

        public static Response<T> Error(string message, IEnumerable<string> errors = null) => new Response<T>(false, default, message, errors);
        public static Response<T> ErrorWithData(T data) => new Response<T>(false, data, "", null);


    }
}
