namespace MagazineAPI
{

    public class ApiResponse
    {
        public string Token { get; set; }
        public bool Success { get; set; }

    }
    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }
}