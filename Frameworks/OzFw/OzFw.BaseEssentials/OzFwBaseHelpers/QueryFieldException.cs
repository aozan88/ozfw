namespace OzFwBaseHelpers
{
    /// <summary>
    /// Filtre/sıralama isteğindeki alan, operatör ya da değer geçersiz olduğunda fırlatılır.
    /// İstemci hatasıdır (HTTP 400).
    /// </summary>
    public class QueryFieldException : ArgumentException
    {
        public QueryFieldException(string message, Exception? inner = null) : base(message, inner) { }
    }
}
