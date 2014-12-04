using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MonaFramework.Interfaces;
using MonaFramework.VocalEngine;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MonaFramework.Controls
{
    public class MonaWindow : Window, IAliasesContainer
    {
        public static readonly int SERVER_PORT = 12021;
        //public static readonly int SERVER_PORT = 4444;

        private MonaComponentRegister __register;
        private TcpClient __socket;

        private HwndSource _hwndSource;



        public MonaWindow()
            : base()
        {
            PreviewMouseMove += OnPreviewMouseMove;
            Closing += cleanClose;
                        
            try
            {
                int answer = 0;

                __socket = new TcpClient("localhost", SERVER_PORT);
                __register = new MonaComponentRegister(__socket);

                answer = __socket.GetStream().ReadByte();

                if (answer == 'Y')
                {
                    initBasicCommands();

                    // TODO: Envoi des alias
                    List<IAliasAnswerer> answerers = __register.getAllComponents();
                    NetworkStream stream = __socket.GetStream();
                    byte[] aliasData;

                    // TOCHECK: on suppose qu'on a au plus 255 commandes de base
                    stream.WriteByte((byte)answerers.Count);
                    stream.Flush();

                    foreach (IAliasAnswerer ans in answerers)
                    {
                        aliasData = Encoding.UTF8.GetBytes(ans.getAlias());

                        stream.WriteByte((byte)aliasData.Length);
                        stream.Write(aliasData, 0, aliasData.Length);
                        stream.Flush();
                    }

                    __register.startNotify();

                    new Thread(new ThreadStart(waitForCommands)).Start();
                }
                else
                {
                    // On quitte le programme
                    System.Windows.MessageBox.Show("Server cannot handle one more application");
                    Environment.Exit(0);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Please launch Mona first");
                Environment.Exit(0);
            }            
        }

        private void initBasicCommands()
        {
            MonaWindow thisWindow = this;
            MonaButton closeButton = new MonaButton();
            MonaButton minimizeButton = new MonaButton();
            MonaButton maximizeButton = new MonaButton();
            MonaButton reduceButton = new MonaButton();
            MonaButton restoreButton = new MonaButton();
            MonaButton aliasButton = new MonaButton();

            closeButton.setAlias("Close");
            closeButton.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs args)
            {
                thisWindow.Close();
            });

            minimizeButton.setAlias("Minimize");
            minimizeButton.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs args)
            {
                // TODO: trouver comment retrecir la fenetre
                thisWindow.WindowState = System.Windows.WindowState.Normal;
            });

            maximizeButton.setAlias("Maximize");
            maximizeButton.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs args)
            {
                //TODO: trouver comment agrandir la fenetre
                thisWindow.WindowState = System.Windows.WindowState.Maximized;
            });

            reduceButton.setAlias("Reduce");
            reduceButton.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs args)
            {
                //TODO: trouver comment reduire la fenetre dans la barre des taches
                thisWindow.WindowState = System.Windows.WindowState.Minimized;
            });

            restoreButton.setAlias("Restore");
            restoreButton.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs args)
            {
                //TODO: trouver comment reduire la fenetre dans la barre des taches
                thisWindow.WindowState = System.Windows.WindowState.Normal;
            });

            aliasButton.setAlias("Alias");
            aliasButton.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs args)
            {
                foreach (IAliasAnswerer answerer in __register.getAllComponents())
                {
                    try
                    {
                        string alias = answerer.getAlias();
                        ToolTip tip = new ToolTip();
                        FrameworkElement element = (FrameworkElement)answerer;

                        tip.Content = alias;

                        ToolTipService.SetPlacementTarget(tip, element);
                        ToolTipService.SetPlacement(tip, System.Windows.Controls.Primitives.PlacementMode.Bottom);
                        /*tip.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                        ((FrameworkElement)answerer).ToolTip = tip;*/                        
                        tip.IsOpen = true;
                    }
                    catch (Exception e)
                    {

                    }
                }
            });

            __register.registerComponent(closeButton);
            __register.registerComponent(minimizeButton);
            __register.registerComponent(maximizeButton);
            __register.registerComponent(reduceButton);
            __register.registerComponent(restoreButton);
            __register.registerComponent(aliasButton);
        }

        private void cleanClose(object sender, CancelEventArgs e)
        {
            try
            {
                __register.stopNotify();

                byte[] dcnData = Encoding.UTF8.GetBytes("DCN");
                __socket.GetStream().Write(dcnData, 0, dcnData.Length);
                __socket.Close();
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            try
            {
                if (oldContent != null)
                {
                    if (oldContent is IAliasAnswerer)
                    {
                        __register.unregisterComponent((IAliasAnswerer)oldContent);
                    }
                    else if (oldContent is IAliasesContainer)
                    {
                        IAliasesContainer cont = (IAliasesContainer)oldContent;

                        foreach (IAliasAnswerer ans in cont.getAllComponents())
                        {
                            __register.unregisterComponent(ans);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            try
            {
                if (newContent != null)
                {
                    if (newContent is IAliasAnswerer)
                    {
                        __register.registerComponent((IAliasAnswerer)newContent);
                    }
                    else if (newContent is IAliasesContainer)
                    {
                        IAliasesContainer cont = (IAliasesContainer)newContent;

                        foreach (IAliasAnswerer ans in cont.getAllComponents())
                        {
                            __register.registerComponent(ans);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            base.OnContentChanged(oldContent, newContent);
        }

        public ICollection<IAliasAnswerer> getComponentsByAlias(string alias)
        {
            //TODO: voir si renvoyer null ou la valeur correcte
            return null;
        }

        public ICollection<IAliasAnswerer> getAllComponents()
        {
            //TODO: voir si renvoyer null ou la valeur correcte
            return null;
        }

        public void notifyAliasAdd(string alias, IAliasAnswerer comp)
        {
            if (alias != null && alias != string.Empty)
                __register.registerComponent(comp);
        }

        public void notifyAliasRemove(string alias, IAliasAnswerer comp)
        {
            if (alias != null && alias != string.Empty)
                __register.unregisterComponent(comp);
        }

        public void waitForCommands()
        {
            NetworkStream stream = __socket.GetStream();
            string cmd;
            byte[] cmdData;
            int cmdLength;

            try
            {
                while (__socket.Connected)
                {
                    cmdLength = stream.ReadByte();

                    if (cmdLength < 0)
                    {
                        //TODO: erreur
                        System.Windows.MessageBox.Show("An error occurred, server might have disconnected");
                        this.Close();
                        break;
                    }

                    cmdData = new byte[cmdLength];
                    stream.Read(cmdData, 0, cmdLength);
                    cmd = Encoding.UTF8.GetString(cmdData);

                    // TODO: lancer l'appel dans un thread ?
                    activate(cmd);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("An error occurred, server might have disconnected");
                this.Close();
            }
        }

        public void activate(string command)
        {
            try
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(__register.getComponent(command).defaultAction));
            }
            catch (Exception e)
            {
                
            }
        }

        static MonaWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MonaWindow),
                new FrameworkPropertyMetadata(typeof(MonaWindow)));
        }

        #region Click events
        protected void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        protected void RestoreClick(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        protected void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        public override void OnApplyTemplate()
        {
            Button minimizeButton = GetTemplateChild("minimizeButton") as Button;
            if (minimizeButton != null)
                minimizeButton.Click += MinimizeClick;

            Button restoreButton = GetTemplateChild("restoreButton") as Button;
            if (restoreButton != null)
                restoreButton.Click += RestoreClick;

            Button closeButton = GetTemplateChild("closeButton") as Button;
            if (closeButton != null)
                closeButton.Click += CloseClick;

            Grid resizeGrid = GetTemplateChild("resizeGrid") as Grid;
            if (resizeGrid != null)
            {
                foreach (UIElement element in resizeGrid.Children)
                {
                    Rectangle resizeRectangle = element as Rectangle;
                    if (resizeRectangle != null)
                    {
                        resizeRectangle.PreviewMouseDown += ResizeRectangle_PreviewMouseDown;
                        resizeRectangle.MouseMove += ResizeRectangle_MouseMove;
                    }
                }
            }

            base.OnApplyTemplate();
        }

        private void moveRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        protected void ResizeRectangle_MouseMove(Object sender, MouseEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            switch (rectangle.Name)
            {
                case "top":
                    Cursor = Cursors.SizeNS;
                    break;
                case "bottom":
                    Cursor = Cursors.SizeNS;
                    break;
                case "left":
                    Cursor = Cursors.SizeWE;
                    break;
                case "right":
                    Cursor = Cursors.SizeWE;
                    break;
                case "topLeft":
                    Cursor = Cursors.SizeNWSE;
                    break;
                case "topRight":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "bottomLeft":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "bottomRight":
                    Cursor = Cursors.SizeNWSE;
                    break;
                default:
                    break;
            }
        }

        protected void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                Cursor = Cursors.Arrow;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        protected void ResizeRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            switch (rectangle.Name)
            {
                case "top":
                    Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "bottom":
                    Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "left":
                    Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "right":
                    Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "topLeft":
                    Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "topRight":
                    Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "bottomLeft":
                    Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "bottomRight":
                    Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
                default:
                    break;
            }
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        protected override void OnInitialized(EventArgs e)
        {
            SourceInitialized += OnSourceInitialized;
            base.OnInitialized(e);
        }

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }
    }
}
