namespace BasicAuthentication.Models
{
  public class User
  {
    public int Id { get; set; }
    public string Username { get; set; }
    // In a real-world application, never store plain passwords
    public string Password { get; set; }
  }
}