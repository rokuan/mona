using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.ComponentModel;

namespace Mona
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly int SERVER_PORT = 12021;

        private static readonly int MAX_APPS = 30;

        //private SpeechRecognitionEngine reco = new SpeechRecognitionEngine();
        private bool active = true;
        private TcpListener server;
        private Thread serverThread;
        private bool connected = false;


        private MonaApplication[] runningApplications = new MonaApplication[MAX_APPS];
        private Dictionary<string, int>[] commands = new Dictionary<string, int>[MAX_APPS];
        private SpeechRecognitionEngine[] engines = new SpeechRecognitionEngine[MAX_APPS];
        private Dictionary<string, Grammar>[] grammars = new Dictionary<string, Grammar>[MAX_APPS];
        //private static readonly Choices defaultGrammarChoices = new Choices("A1B2C3");

        private int applicationsNb = 0;
        private Object numberLock = new Object();
        private Queue<int> freeIndices = new Queue<int>();
        private MonaApplication currentApplication = null;

        public MainWindow()
        {
            InitializeComponent();

            Closing += cleanClose;

            // Chargement des commandes de base

            // Initialisation du serveur

            try
            {
                server = new TcpListener(IPAddress.Any, SERVER_PORT);
                connected = true;
                serverThread = new Thread(new ThreadStart(waitForApplications));
                serverThread.Start();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("An error occurred while trying to initialize the server");
                //TODO: trouver la bonne fonction pour terminer le programme
                System.Environment.Exit(0);
            }
        }

        private void cleanClose(object sender, CancelEventArgs e)
        {
            try
            {
                connected = false;
                server.Stop();
            }
            catch (Exception ex)
            {

            }
        }

        #region SpeechRecognition

        private void recognizedWord(object sender, SpeechRecognizedEventArgs e)
        {
            if (active)
            {
                string cmd = e.Result.Text;

                //word.Text = cmd;

                //TODO: envoyer
                lock (currentApplication)
                {
                    if (currentApplication != null)
                    {
                        if (commands[currentApplication.getIndex()].ContainsKey(cmd))
                        {
                            currentApplication.sendCommand(cmd);
                            return;
                        }
                    }

                    //TODO: dans le cas contraire on applique la commande a Mona
                }
            }
        }

        private void rejectedWord(object sender, SpeechRecognitionRejectedEventArgs e)
        {

        }

        private void wordInProgress(object sender, SpeechHypothesizedEventArgs e)
        {

        }

        #endregion

        #region ServerListening

        private int getAvailableIndex()
        {
            lock (freeIndices)
            {
                if (freeIndices.Count > 0)
                {
                    return freeIndices.Dequeue();
                }
            }

            lock (numberLock)
            {
                if (applicationsNb < MAX_APPS)
                {
                    return applicationsNb;
                }
            }

            return -1;
        }

        private void waitForApplications()
        {
            try
            {
                TcpClient appSocket;
                MonaApplication app;
                int appIndex = -1;
                NetworkStream stream;

                server.Start();

                while (connected)
                {
                    appSocket = server.AcceptTcpClient();

                    appIndex = getAvailableIndex();
                    stream = appSocket.GetStream();

                    try
                    {
                        if (appIndex >= 0)
                        {
                            app = new MonaApplication(appIndex, this, appSocket);

                            addApplication(app);
                            stream.WriteByte((byte)'Y');
                            stream.Flush();

                            // Lecture des alias deja presents
                            //TOCHECK: on suppose qu'on peut avoir un maximum de 255 alias
                            //TODO: deplacer les lignes de code dans un thread
                            int aliasNb = stream.ReadByte();
                            string cmd;
                            byte[] data;
                            int dataLength;

                            for (int i = 0; i < aliasNb; i++)
                            {
                                dataLength = stream.ReadByte();
                                data = new byte[dataLength];
                                stream.Read(data, 0, dataLength);
                                cmd = Encoding.UTF8.GetString(data);

                                addCommand(cmd, appIndex);
                            }

                            switchCurrentApplication(appIndex);

                            new Thread(new ThreadStart(app.run)).Start();
                        }
                        else
                        {
                            // Le serveur a atteint le nombre max d'applications a gerer
                            // TODO
                            stream.WriteByte((byte)'N');
                            stream.Flush();
                            appSocket.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        #endregion

        #region ApplicationHandler

        public void switchCurrentApplication(int index)
        {
            MonaApplication app = runningApplications[index];

            if (currentApplication != null)
            {
                //TODO: retirer les autres parametres de l'ancienne application courante
                stopRecognition(currentApplication.getIndex());
            }

            currentApplication = app;
            startRecognition(app.getIndex());
        }

        private void startRecognition(int index)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object sender)
            {
                lock (engines)
                {
                    SpeechRecognitionEngine appReco = engines[index];
                    appReco.SetInputToDefaultAudioDevice();
                    appReco.RecognizeAsync(RecognizeMode.Multiple);
                }
            }));
        }

        private void stopRecognition(int index)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object sender)
            {
                // TODO: verifier que l'on a toujours acces a engines[index]
                lock (engines)
                {
                    if (engines[index] != null)
                    {
                        SpeechRecognitionEngine appReco = engines[index];
                        appReco.RecognizeAsyncCancel();
                        appReco.RecognizeAsyncStop();
                        appReco.SetInputToNull();
                    }
                }
            }));
        }

        public void addApplication(MonaApplication app)
        {
            SpeechRecognitionEngine appReco = new SpeechRecognitionEngine();

            lock (runningApplications)
            {
                runningApplications[app.getIndex()] = app;

                lock (commands)
                {
                    commands[app.getIndex()] = new Dictionary<string, int>();

                    lock (engines)
                    {
                        lock (grammars)
                        {
                            grammars[app.getIndex()] = new Dictionary<string, Grammar>();
                        }

                        appReco.SetInputToNull();
                        appReco.SpeechRecognized += recognizedWord;
                        appReco.MaxAlternates = 1;


                        engines[app.getIndex()] = appReco;
                    }
                }

                lock (numberLock)
                {
                    applicationsNb++;
                }
            }
        }

        public void removeApplication(MonaApplication app)
        {
            lock (runningApplications)
            {
                runningApplications[app.getIndex()] = null;
                commands[app.getIndex()] = null;

                //TODO: verifier s'il faut retirer toutes les grammaires avant
                stopRecognition(app.getIndex());
                engines[app.getIndex()] = null;

                lock (grammars)
                {
                    grammars[app.getIndex()] = null;
                }

                lock (freeIndices)
                {
                    freeIndices.Enqueue(app.getIndex());
                }

                lock (numberLock)
                {
                    applicationsNb--;
                }

                if (currentApplication == app)
                {
                    lock (numberLock)
                    {
                        if (applicationsNb == 0)
                        {
                            currentApplication = null;
                        }
                        else
                        {
                            for (int i = 0; i < runningApplications.Length; i++)
                            {
                                if (runningApplications[i] != null)
                                {
                                    switchCurrentApplication(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void addCommand(string command, int index)
        {
            lock (commands)
            {
                if (commands[index].ContainsKey(command))
                {
                    commands[index][command]++;
                }
                else
                {
                    //TODO: ajouter la commande a la reconnaissance vocale
                    Choices newCommand = new Choices(command);
                    GrammarBuilder grammarBuilder = new GrammarBuilder(newCommand);
                    Grammar grammar = new Grammar(grammarBuilder);

                    lock (engines)
                    {
                        lock (grammars)
                        {
                            grammars[index].Add(command, grammar);
                            engines[index].LoadGrammar(grammar);
                        }
                    }

                    commands[index].Add(command, 1);
                }
            }
        }

        public void removeCommand(string command, int index)
        {
            lock (commands)
            {
                commands[index][command]--;

                if (commands[index][command] == 0)
                {
                    lock (engines)
                    {
                        lock (grammars)
                        {
                            //TODO: retirer la commande de la reconnaissance vocale
                            engines[index].UnloadGrammar(grammars[index][command]);
                            grammars[index].Remove(command);
                            commands[index].Remove(command);
                        }
                    }
                }
            }
        }

        public class MonaApplication
        {
            private int index = 0;
            private MainWindow mainWindow;
            private TcpClient socket;

            public MonaApplication(int id, MainWindow mainW, TcpClient s)
            {
                index = id;
                mainWindow = mainW;
                socket = s;
            }

            public int getIndex()
            {
                return index;
            }

            public void run()
            {
                NetworkStream inStream = socket.GetStream();
                string commandType;
                byte[] cmdBuffer = new byte[3];
                byte[] data;
                int dataLength;
                string alias;
                bool connected = true;

                try
                {
                    while (connected)
                    {
                        inStream.Read(cmdBuffer, 0, cmdBuffer.Length);
                        commandType = Encoding.UTF8.GetString(cmdBuffer);

                        switch (commandType)
                        {
                            // Add an alias
                            case "ADD":
                                dataLength = inStream.ReadByte();
                                data = new byte[dataLength];
                                inStream.Read(data, 0, dataLength);
                                alias = Encoding.UTF8.GetString(data);

                                mainWindow.addCommand(alias, index);
                                break;

                            // Remove an alias
                            case "RMV":
                                dataLength = inStream.ReadByte();
                                data = new byte[dataLength];
                                inStream.Read(data, 0, dataLength);
                                alias = Encoding.UTF8.GetString(data);

                                mainWindow.removeCommand(alias, index);
                                break;

                            // Modify an existing alias
                            case "MDF":
                                break;

                            // Request for focus
                            case "RFF":
                                break;

                            // Disconnection
                            case "DCN":
                                connected = false;

                                try
                                {
                                    socket.Close();
                                }
                                catch (Exception e)
                                {

                                }

                                mainWindow.removeApplication(this);
                                break;

                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    connected = false;

                    try
                    {
                        socket.Close();
                    }
                    catch (Exception ex)
                    {

                    }
                    //TODO: voir s'il faut fermer la socket ou non
                    mainWindow.removeApplication(this);
                    return;
                }
            }

            public void sendCommand(string command)
            {
                NetworkStream outStream = socket.GetStream();
                byte[] data;

                // TOCHECK: modifier lorsque la taille est codee sur plus qu'un seul byte
                outStream.WriteByte((byte)command.Length);
                data = Encoding.UTF8.GetBytes(command);
                outStream.Write(data, 0, data.Length);
                outStream.Flush();
            }

            public override bool Equals(object obj)
            {
                if (obj is MonaApplication)
                {
                    return ((MonaApplication)obj).getIndex() == this.getIndex();
                }

                return false;
            }

            public override int GetHashCode()
            {
                return index;
            }
        }

        #endregion
    }
}
