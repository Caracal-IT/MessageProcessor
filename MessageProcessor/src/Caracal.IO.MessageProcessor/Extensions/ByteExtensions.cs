namespace Caracal.IO.MessageProcessor.Extensions; 

public static class ByteExtensions {
  public static long GetOffset(this byte[] bytes) => bytes.Aggregate(0L, (s, a) => s * 256 + a);

  public static DateTime GetDateFromEpoch(this byte[] bytes) {
    var  dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    return dtDateTime.AddSeconds(bytes.GetOffset());
  }
}