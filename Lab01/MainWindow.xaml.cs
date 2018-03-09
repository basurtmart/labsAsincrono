using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //CreateTask();
            //RunTaskGroup();
            //ReturnTaskValue();
        }

        void CreateTask()
        {
            Task T;
            var Code = new Action(ShowMessage);

            T = new Task(Code);

            Task T2 = new Task(delegate { MessageBox.Show("Ejecutando una tarea en un delegado anónimo"); });

            Task T3 = new Task(() => ShowMessage());

            Task T4 = new Task(() => MessageBox.Show("Ejecutando la Tarea 4"));

            Task T5 = new Task(() =>
            {
                DateTime CurrentDate = DateTime.Today;
                DateTime StarDate = CurrentDate.AddDays(30);
                MessageBox.Show($"Tarea 5. Fecha calculada: {StarDate}");
            });

            Task T6 = new Task((message) => MessageBox.Show(message.ToString()), "Expresión lamba con parámetros.");

            Task T7 = new Task(() => AddMessage("Ejecutando la tarea"));
            T7.Start();

            AddMessage("En el hilo principal");

            var T8 = Task.Factory.StartNew(() => AddMessage("Tarea iniciada con TaskFactory"));

            var T9 = Task.Run(() => AddMessage("Tarea ejecutada con Task.Run"));

            var T10 = Task.Run(() =>
            {
                WriteToOutput("Iniciando tarea 10...");
                // Simular un proceso de dura 10 segundos
                Thread.Sleep(10000); // El hilo es suspendido por 10000 milisegundos
                WriteToOutput("Fin de la tarea 10.");
            });
            WriteToOutput("Esperando a la tarea 10.");
            T10.Wait();
            WriteToOutput("La tarea finalizó su ejecución");
        }

        void ShowMessage()
        {
            MessageBox.Show("Ejecutando el método ShowMessage");
        }

        void AddMessage(string message)
        {
            int CurrentThreadId = Thread.CurrentThread.ManagedThreadId;

            this.Dispatcher.Invoke(() =>
            {
                lblMessages.Content += $"Mensaje: {message}, " +
                                       $"Hilo Actual {CurrentThreadId}\n";
            });
        }

        void WriteToOutput(string message)
        {
            System.Diagnostics.Debug.WriteLine($"Mensaje: {message}, Hilo actual: {Thread.CurrentThread.ManagedThreadId}");
        }

        void RunTask(byte taskNumber)
        {
            WriteToOutput($"Iniciando tarea {taskNumber}.");
            //Simular un proceso de dura 10 segundo 
            Thread.Sleep(10000); // El hilo es suspendido por 10000 milisegundos
            WriteToOutput($"Finalizando tarea {taskNumber}.");
        }

        void RunTaskGroup()
        {
            Task[] TaskGroup = new Task[]
            {
                Task.Run(() => RunTask(1)),
                Task.Run(() => RunTask(2)),
                Task.Run(() => RunTask(3)),
                Task.Run(() => RunTask(4)),
                Task.Run(() => RunTask(5)),
            };

            WriteToOutput("Esperando a que finalicen al menos una tareas...");
            Task.WaitAny(TaskGroup);
            WriteToOutput("Al menos una tarea finalizó.");
        }

        void ReturnTaskValue()
        {
            Task<int> T;
            T = Task.Run<int>(() => new Random().Next(1000));
            WriteToOutput($"Valor devuelto por la tarea: {T.Result}");

            Task<int> T2 = Task.Run<int>(() =>
            {
                WriteToOutput("Obtener el número aleatorio...");
                Thread.Sleep(10000); // Simular un proceso largo;
                return new Random().Next(1000);
            });

            WriteToOutput("Eperar el resultado de la tarea...");
            WriteToOutput($"La tarea devolvió el valor {T2.Result}");
            WriteToOutput("Fin de la ejecución del método ReturnTaskValue.");
        }

        private CancellationTokenSource CTS;
        private CancellationToken CT;
        private Task LongRunningTask;

        private void BtnStartTask_OnClick(object sender, RoutedEventArgs e)
        {
            CTS = new CancellationTokenSource();
            CT = CTS.Token;
            Task.Run(() =>
            {
                LongRunningTask = Task.Run(() => { DoLongRunningTask(CT); }, CT);
                try
                {
                    LongRunningTask.Wait();
                }
                catch (AggregateException ae)
                {
                    foreach (var Inner in ae.InnerExceptions)
                    {
                        if (Inner is TaskCanceledException)
                        {
                            AddMessage("Tarea cancelada y TaskCanceledException manejado.");
                        }
                        else
                        {
                            // Procesamo excepciones distintas a cancelación.
                            AddMessage(Inner.Message);
                        }
                    }
                }
            });
        }

        private void DoLongRunningTask(CancellationToken ct)
        {
            int[] IDs = {1, 3, 4, 7, 11, 18, 29, 47, 76, 100};
            for (int i = 0; i < IDs.Length && !ct.IsCancellationRequested; i++)
            {
                AddMessage($"Procesando ID: {IDs[i]}");
                Thread.Sleep(2000); // Simular un proceso largo.
            }
            if (ct.IsCancellationRequested)
            {
                // Finaliza el procesameinto
                AddMessage("Proceso cancelado");
                ct.ThrowIfCancellationRequested();
            }
        }

        private void CancelTask_OnClick(object sender, RoutedEventArgs e)
        {
            CTS.Cancel();
        }

        private void BtnShowStatus_OnClick(object sender, RoutedEventArgs e)
        {
            AddMessage($"Estatus de la tarea: {LongRunningTask.Status}");
        }
    }
}
