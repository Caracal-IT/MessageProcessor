namespace Caracal.IO.MessageProcessor.Extensions; 

public static class ByteExtensions {
  public static DateTime GetDateFromEpoch(this byte[] bytes) {
    var  dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    return dtDateTime.AddSeconds(bytes.ToInt64());
  }
  
  private static long ToInt64(this byte[] bytes) => bytes.Aggregate(0L, (s, a) => s * 256 + a);
}