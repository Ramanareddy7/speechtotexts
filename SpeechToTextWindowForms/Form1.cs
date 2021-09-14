using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using Microsoft.CognitiveServices.Speech.Transcription;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.AspNetCore.SignalR.Client;

namespace SpeechToTextWindowForms
{
    public partial class Form1 : Form
    {

        SqlConnection sqlconnection = new SqlConnection(ConfigurationManager.ConnectionStrings["key"].ConnectionString);
        private bool _dragging = false;
        private Point _start_point=new Point(0,0);
        HubConnection connection;
        public Form1()
        {
           
            InitializeComponent();
            Conversation.BackColor = Color.Beige;
            Conversation.DrawMode = DrawMode.OwnerDrawFixed;
            Conversation.DrawItem += new DrawItemEventHandler(listBox1_SetColor);
            //FindAgents();
            label19.Text = Environment.GetEnvironmentVariable("USERNAME");
            //WindowState = FormWindowState.Maximized;
            this.Dock = DockStyle.Fill;
            Getdata();
            connection = new HubConnectionBuilder()
              .WithUrl("https://signalrchat20210312211311.azurewebsites.net/ChatHub")
              .Build();
            ConnectToHub();
        }
        private async void ConnectToHub()
        {
            #region snippet_ConnectionOn
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
               
                var newMessage = $"{user}: {message}";
                messagesList.Items.Add(newMessage);
                listBox1.Text = newMessage;
                
            });
            #endregion

            try
            {
                await connection.StartAsync();
                messagesList.Items.Add("Connection started");
                
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        }
        private void Getdata()
        {
            if(sqlconnection.State==ConnectionState.Closed)
            {
                sqlconnection.Open();
            }

            SqlCommand cmd = new SqlCommand("select * from [dbo].[User]", sqlconnection);
            SqlDataReader sdr;
            sdr = cmd.ExecuteReader();
            while(sdr.Read())
            {
                listView1.Items.Add(sdr[1].ToString());
            }

        }
        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await ContinuousRecognitionAutoDetectLanguageEng();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

       

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            timer1.Start();
            if (panel3.Visible == true)
            {
                panel3.Visible = false;
            }
            else
            {
                panel3.Visible = true;
            }
        }

        private int _count = 0;
        delegate void SetTextCallback(string text);
        private void SetText(string text)
        {

            if (this.Conversation.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.Conversation.Items.Add(text);
               
            }
        }

        private void customertext(string text)
        {
            if (this.listBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(customertext);
                this.Invoke(d, new object[] { text });
            }
            else
            {

                //this.textBox2.Text += text;
                this.listBox1.Items.Add(text);
            }
        }
        void listBox1_SetColor(object sender, DrawItemEventArgs e)
        {
            try
            {
                e.DrawBackground();
                Brush myBrush = Brushes.White;

                var sayi = ((ListBox)sender).Items[e.Index].ToString();
                if (sayi.Contains("Customer:"))
                {
                    myBrush = Brushes.Red;

                }
                else
                {
                    myBrush = Brushes.Green;
                }

                e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(),
                e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);

                e.DrawFocusRectangle();
            }
            catch
            {

            }
        }

        StringBuilder sb = new StringBuilder();
        LoginInfo obj = new LoginInfo();
        StringBuilder customersb = new StringBuilder();
        public async Task ContinuousRecognitionAutoDetectLanguageEng()
        {
            obj.UserID = "Hello";
            //var client = EsClient();
            var config = SpeechConfig.FromSubscription("c2733300c04e4a68884c220da5a4d848", "westeurope");

            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US" });

            var stopMicRecognition = new TaskCompletionSource<int>();
            var stopSpeakerRecognition = new TaskCompletionSource<int>();
            var pushStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(48000, 16, 2));
            var speakerInput = AudioConfig.FromStreamInput(pushStream);
            

            var micInput = AudioConfig.FromDefaultMicrophoneInput();
            var micrecognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig, micInput);
            micrecognizer.Recognizing += (s, e) =>
            {
                //Console.WriteLine($"Agent:{e.Result.Text}");
            };

            micrecognizer.Recognized += async (s, e) =>
            {
                
                if (e.Result.Reason == ResultReason.RecognizedSpeech && obj.UserID == "Bye")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Agent: {e.Result.Text}");
                    sb.Append($"Agent: {e.Result.Text}");
                    //listBox1.Items.Add($"Agent: {e.Result.Text}");
                    SetText($"Agent: {e.Result.Text}");
                    obj.UserID = e.Result.Text;
                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
            };

            micrecognizer.Canceled += (s, e) =>
            {
                Console.WriteLine($"CANCELED: Reason={e.Reason}");

                if (e.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you update the subscription info?");
                }

                stopMicRecognition.TrySetResult(0);
            };

            micrecognizer.SessionStarted += (s, e) =>
            {
                Console.WriteLine("\n    Session started event.");
            };

            micrecognizer.SessionStopped += (s, e) =>
            {
                Console.WriteLine("\n    Session stopped event.");
                Console.WriteLine("\nStop recognition.");
                stopMicRecognition.TrySetResult(0);
            };

            await micrecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            var speakerRecognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig, speakerInput);
            speakerRecognizer.Recognizing += (s, e) =>
            {
                //Console.WriteLine($"Client: Text={e.Result.Text}");
            };

            speakerRecognizer.Recognized += async (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"Customer: {e.Result.Text}");
                    sb.Append($"Customer: {e.Result.Text}");
                    //listBox1.Items.Add($"Customer: {e.Result.Text}");
                    SetText($"Customer: {e.Result.Text}");
                    customersb.Append($"{e.Result.Text}");
                    customertext($" {e.Result.Text}");
                    obj.UserID = "Bye";

                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                        Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        timer1.Enabled = true;
                        timer1.Interval = 10000;
                        await speakerRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                        await micrecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                   
                    
                }
            };

            speakerRecognizer.Canceled += (s, e) =>
            {
                Console.WriteLine($"CANCELED: Reason={e.Reason}");

                if (e.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you update the subscription info?");
                }

                stopSpeakerRecognition.TrySetResult(0);
            };

            speakerRecognizer.SessionStarted += (s, e) =>
            {
                Console.WriteLine("\nSession started event.");
            };

            speakerRecognizer.SessionStopped += (s, e) =>
            {
                Console.WriteLine("\nSession stopped event.");
                Console.WriteLine("\nStop recognition.");
                stopSpeakerRecognition.TrySetResult(0);
            };
            await speakerRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            var capture = new WasapiLoopbackCapture();

            capture.DataAvailable += async (s, e) =>
            {
                if (_count == 0)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(capture.WaveFormat));
                    _count++;
                }
                var resampledByte = ToPCM16(e.Buffer, e.BytesRecorded, capture.WaveFormat); //ResampleWasapi(s, e);
                pushStream.Write(resampledByte); // try to push buffer here
            };
            capture.RecordingStopped += (s, e) =>
            {

                capture.Dispose();
            };
            capture.StartRecording();
            Console.WriteLine("Record Started, Press Any key to stop the record");
            Console.ReadLine();
            capture.StopRecording();

            pushStream.Close();

            Task.WaitAny(new[] { stopSpeakerRecognition.Task, stopMicRecognition.Task });
            await speakerRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            await micrecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            
        }

        public byte[] ToPCM16(byte[] inputBuffer, int length, WaveFormat format)
        {
            if (length == 0)
                return new byte[0]; // No bytes recorded, return empty array.

            // Create a WaveStream from the input buffer.
            using (var memStream = new MemoryStream(inputBuffer, 0, length))
            {
                using (var inputStream = new RawSourceWaveStream(memStream, format))
                {

                    // Convert the input stream to a WaveProvider in 16bit PCM format with sample rate of 48000 Hz.
                    var convertedPCM = new SampleToWaveProvider16(
                        new WdlResamplingSampleProvider(
                            new WaveToSampleProvider(inputStream),
                            48000)
                            );

                    byte[] convertedBuffer = new byte[length];

                    using (var stream = new MemoryStream())
                    {
                        int read;

                        // Read the converted WaveProvider into a buffer and turn it into a Stream.
                        while ((read = convertedPCM.Read(convertedBuffer, 0, length)) > 0)
                            stream.Write(convertedBuffer, 0, read);


                        // Return the converted Stream as a byte array.
                        return stream.ToArray();
                    }
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await ContinuousRecognitionAutoDetectLanguageEng();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                SendStatus(sb.ToString());
                Conversation.Items.Clear(); 
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        private async void SendStatus(string message)
        {

            #region snippet_ErrorHandling
            try
            {
                #region snippet_InvokeAsync
                var name = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                if (name.Contains('\\'))
                    name = name.Split('\\')[1];
                await connection.InvokeAsync("SendMessage",
                    name, message);
                #endregion
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
            #endregion
        }
      
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _start_point = new Point(e.X, e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this._start_point.X, p.Y - this._start_point.Y);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        public string webGetMethod(string URL)
        {
            string jsonString = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "GET";
            request.Credentials = CredentialCache.DefaultCredentials;
            ((HttpWebRequest)request).UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)";
            request.Accept = "/";
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            request.ContentType = "application/x-www-form-urlencoded";

            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            jsonString = sr.ReadToEnd();
            sr.Close();
            return jsonString;
        }

        private  void button3_Click_1(object sender, EventArgs e)
        {
           

        }

        public class LoginInfo
        {
            public string UserID;
        }

        private void Analyse_Click(object sender, EventArgs e)
        {
            string URL = "https://text2emotion.azurewebsites.net/emotion?text=" + customersb.ToString();
            var data = webGetMethod(URL);
            listBox2.Items.Add(data);
        }
    }
}
