using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace ClientSide;


public partial class MainWindow : Window
{
    public UdpClient client;
    public IPAddress remoteIP;
    public int remotePort;
    public IPEndPoint remoteEP;

    public bool isCheck = false;
    public bool isFirst = true;

    bool isBreak = false;

    bool isRecord = false;
    public static BigInteger counter = 0;

    public MainWindow()
    {
        InitializeComponent();
        remoteIP = IPAddress.Parse("127.0.0.1");
        remotePort = 27001;
        remoteEP = new IPEndPoint(remoteIP, remotePort);

        client = new UdpClient();
    }

    //private async void Button_Click(object sender, RoutedEventArgs e)
    //{
    //    if (!isCheck)
    //    {
    //        isCheck = true;
    //        var buffer = new byte[ushort.MaxValue - 29];


    //        if (isFirst)
    //        {
    //            await client.SendAsync(buffer, buffer.Length, remoteEP);
    //            isFirst = false;
    //        }

    //        var list = new List<byte>();
    //        var maxLen = buffer.Length;
    //        var len = 0;

    //        while (true)
    //        {
    //            if (isBreak)
    //                break;


    //            do
    //            {
    //                try
    //                {
    //                    var result = await client.ReceiveAsync();

    //                    buffer = result.Buffer;
    //                    len = buffer.Length;
    //                    list.AddRange(buffer);
    //                }
    //                catch (Exception ex)
    //                {
    //                    System.Windows.MessageBox.Show(ex.Message);
    //                }

    //            } while (len == maxLen);

    //            var image = await ByteToImageAsync(list.ToArray());
    //            if (image is not null)
    //                ScreenImage.Source = image;

    //            list.Clear();
    //        }
    //        isBreak = false;
    //    }
    //}


    public async Task<BitmapImage> ByteToImageAsync(byte[] bytes)
    {
        var image = new BitmapImage();
        image.BeginInit();


        //if (isRecord)
        //{
        //    var ms = new MemoryStream(bytes);
        //    var imagePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Step\\image (" + counter + ").jpeg";

        //    FileStream file = new FileStream(imagePath, FileMode.Create, FileAccess.Write);
        //    await ms.CopyToAsync(file);
        //    counter++;
        //}


        image.StreamSource = new MemoryStream(bytes);
        image.CacheOption = BitmapCacheOption.OnLoad;

        image.EndInit();
        return image;
    }


    private async void ConnectToServer(object sender, RoutedEventArgs e)
    {
        var buffer = new byte[ushort.MaxValue - 29];
        await client.SendAsync(buffer, buffer.Length, remoteEP);


    }

    private async void SendToScreenshot(object sender, RoutedEventArgs e)
    {
        while (true)
        {
            await Task.Delay(1);

            var screen = await TakeScreenShotAsync();
            var imageBytes = await ImageToByteAsync(screen);

            var chunks = imageBytes.Chunk(ushort.MaxValue - 29);

            foreach (var chunk in chunks)
                await client.SendAsync(chunk, chunk.Length, remoteEP);
        }


    }

    private async void ShowScreen(object sender, RoutedEventArgs e)
    {
        if (!isCheck)
        {
            isCheck = true;
            var buffer = new byte[ushort.MaxValue - 29];


            //if (isFirst)
            //{
            //    await client.SendAsync(buffer, buffer.Length, remoteEP);
            //    isFirst = false;
            //}

            var list = new List<byte>();
            var maxLen = buffer.Length;
            var len = 0;

            while (true)
            {
                //if (isBreak)
                //    break;


                do
                {
                    try
                    {
                        var result = await client.ReceiveAsync();

                        buffer = result.Buffer;
                        len = buffer.Length;
                        list.AddRange(buffer);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }

                } while (len == maxLen);

                var image = await ByteToImageAsync(list.ToArray());
                if (image is not null)
                    ScreenImage.Source = image;

                list.Clear();
            }
            // isBreak = false;
        }
    }

    async Task<Image> TakeScreenShotAsync()
    {
        var width = Screen.PrimaryScreen.Bounds.Width;
        var height = Screen.PrimaryScreen.Bounds.Height;
        Bitmap bitmap = new Bitmap(width, height);

        using Graphics? g = Graphics.FromImage(bitmap);
        g?.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

        return bitmap;
    }


    async Task<byte[]> ImageToByteAsync(Image image)
    {
        using MemoryStream ms = new MemoryStream();
        image.Save(ms, ImageFormat.Jpeg);

        return ms.ToArray();
    }
}

