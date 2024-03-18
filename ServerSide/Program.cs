using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Text;



var ip = IPAddress.Parse("127.0.0.1");
var port = 27001;

var listenerEP = new IPEndPoint(ip, port);
//var remoteEP = new IPEndPoint(IPAddress.Any, 0);

var listener = new UdpClient(listenerEP);
// --------------------------------------------------------------------------------------

List<UdpClient> Clients = new List<UdpClient>();

while (true)
{

    var resultt = await listener.ReceiveAsync();
    Console.WriteLine($"{resultt.RemoteEndPoint} Connected...");

    if (Database.RemoteEPS.Contains(resultt.RemoteEndPoint) == false)
    {

        Database.RemoteEPS.Add(resultt.RemoteEndPoint);
    }

    Console.WriteLine(Database.RemoteEPS.Count);


    while (true)
    {

        var buffer = new byte[ushort.MaxValue - 29];
        var list = new List<byte>();
        var maxLen = buffer.Length;
        var len = 0;

        do
        {
            try
            {
                var result = await listener.ReceiveAsync();

                buffer = result.Buffer;
                len = buffer.Length;
                list.AddRange(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        } while (len == maxLen);

        if (Database.RemoteEPS.Count > 0)
        {
            foreach (var item in Database.RemoteEPS)
            {
                //Console.WriteLine(item.ToString());
                var chunks = list.ToArray().Chunk(maxLen);
                foreach (var chunk in chunks)
                {
                    await listener.SendAsync(chunk, chunk.Length, item);
                }
            }
            list.Clear();

        }

    }


}

public static class Database
{
    public static List<IPEndPoint> RemoteEPS = new List<IPEndPoint>();
}



//var result = await listener.ReceiveAsync();
//Console.WriteLine($"{result.RemoteEndPoint} .. Connected ...");
//Clients.Add(listener);

//_ = Task.Run(async () =>
//{
//    foreach (var item in Clients)
//    {
//        var remoteEP = result.RemoteEndPoint;

//        while (true)
//        {
//            var screen = await TakeScreenShotAsync();
//            var imageBytes = await ImageToByteAsync(screen);

//            var chunks = imageBytes.Chunk(ushort.MaxValue - 29);

//            foreach (var chunk in chunks)
//                await listener.SendAsync(chunk, chunk.Length, remoteEP);
//        }

//    }
//});
//// --------------------------------------------------------------------------------------
//// Ekranin Screen edilme hissesi.

//async Task<Image> TakeScreenShotAsync()
//{
//    var width = Screen.PrimaryScreen.Bounds.Width;
//    var height = Screen.PrimaryScreen.Bounds.Height;
//    Bitmap bitmap = new Bitmap(width, height);

//    using Graphics? g = Graphics.FromImage(bitmap);
//    g?.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

//    return bitmap;
//}


//async Task<byte[]> ImageToByteAsync(Image image)
//{
//    using MemoryStream ms = new MemoryStream();
//    image.Save(ms, ImageFormat.Jpeg);

//    return ms.ToArray();
//}

